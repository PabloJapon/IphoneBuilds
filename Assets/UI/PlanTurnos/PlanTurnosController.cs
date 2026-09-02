using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

// ── Modelos ──

[Serializable]
public class Empleado
{
    public int id_empleado;
    public string nombre;
    public float horas_contrato_semana;
    public int activo;
}

[Serializable]
public class Turno
{
    public int id;
    public int id_empleado;
    public string nombre_empleado;
    public string fecha;       // "yyyy-MM-dd"
    public string hora_inicio; // "HH:mm"
    public string hora_fin;    // "HH:mm"
    public string puesto;      // cocina | sala | barra | repartidor
    public string notas;
}

[Serializable]
public class TurnosResponse
{
    public Turno[] turnos;
}

[Serializable]
public class TurnoNuevo
{
    public string restaurant_id;
    public int id_empleado;
    public string fecha;
    public string hora_inicio;
    public string hora_fin;
    public string puesto;
    public string notas;
    public string creado_por;
}

[Serializable]
public class TurnoEdicion
{
    public int id_empleado;
    public string fecha;
    public string hora_inicio;
    public string hora_fin;
    public string puesto;
    public string notas;
}

// ErrorResponse y JsonHelper ya existen en el proyecto (ver ReservasController.cs
// y la utilidad JsonHelper compartida), asi que PlanTurnosController los reutiliza
// en lugar de volver a declararlos aqui.

public class PlanTurnosController : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UIToolkitKeyboardBridge keyboardBridge;
    [SerializeField] private string apiBase = "https://tu-api.com";
    [SerializeField] private string restaurantId = "1";
    [SerializeField] private DataBasePersonalizacion dbPersonalizacion;
    private Color colorPpalBotones = new Color(245f / 255f, 168f / 255f, 60f / 255f); // fallback = naranja (igual que Reservas)
    private Color colorSecBotones = new Color(164f / 255f, 35f / 255f, 63f / 255f);   // fallback = vino (igual que Reservas)

    private static readonly string[] Puestos = { "cocina", "sala", "barra", "repartidor" };
    private static readonly Dictionary<string, string> PuestoLabel = new()
    {
        { "cocina", "Cocina" }, { "sala", "Sala" }, { "barra", "Barra" }, { "repartidor", "Repartidor" }
    };

    private static readonly string[] MesesLargos =
    {
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    };
    private static readonly string[] DiasSemanaCal = { "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB", "DOM" };

    private VisualElement root;
    private VisualElement tablaBody;
    private ScrollView tablaScroll;
    private Vector2 scrollGuardado = Vector2.zero;
    private VisualElement modalOverlay;
    private VisualElement modalBox;
    private Label semanaTexto;
    private Label modalTitulo;
    private Label modalSubtitulo;
    private DropdownField campoEmpleado;
    private DropdownField campoPuesto;
    private TextField campoNotas;
    private Button btnFecha, btnHoraInicio, btnHoraFin;
    private Label errorLabel;
    private Button btnEliminar;
    private Button btnGuardar;
    private Button btnHoy;

    private List<Empleado> personal = new();
    private List<Turno> turnosSemana = new();
    private DateTime lunesActual;
    private Turno turnoEnEdicion;
    private int solicitudActual = 0; // evita pisar resultados si el usuario cambia de semana rapido

    private DateTime modalFecha;
    private string modalHoraInicio = "";
    private string modalHoraFin = "";
    private bool editandoHoraInicio = true;
    private DateTime calMesVisible;

    // ── Portapapeles de turnos (copiar/pegar) ──
    private string copiadoPuesto;
    private string copiadoHoraInicio;
    private string copiadoHoraFin;
    private string copiadoNotas;
    private bool guardandoPegado = false;
    private bool HayTurnoCopiado => !string.IsNullOrEmpty(copiadoPuesto);

    private void OnEnable()
    {
        root = document.rootVisualElement;
        lunesActual = ObtenerLunes(DateTime.Today);

        CargarColoresPersonalizacion();
        if (dbPersonalizacion != null)
            dbPersonalizacion.OnDataLoaded += OnPersonalizacionCargada;

        CachearReferencias();
        ConectarEventos();
        CerrarModal();

        FijarColoresBotones();

        _ = CargarTodo();
    }

    private void OnDisable()
    {
        if (dbPersonalizacion != null)
            dbPersonalizacion.OnDataLoaded -= OnPersonalizacionCargada;
    }

    private void CachearReferencias()
    {
        tablaBody = RequireQ<VisualElement>("pt-tabla-body");
        tablaScroll = RequireQ<ScrollView>("pt-tabla-body-scroll");
        tablaScroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;

        var horaScroll = RequireQ<ScrollView>("pt-hora-picker-scroll");
        horaScroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;

        var modalBodyScroll = RequireQ<ScrollView>("pt-modal-body");
        modalBodyScroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
        modalOverlay = RequireQ<VisualElement>("pt-modal-overlay");
        modalBox = RequireQ<VisualElement>("pt-modal-box");
        semanaTexto = RequireQ<Label>("pt-semana-texto");
        modalTitulo = RequireQ<Label>("pt-modal-titulo");
        modalSubtitulo = RequireQ<Label>("pt-modal-subtitulo");
        campoEmpleado = RequireQ<DropdownField>("pt-campo-empleado");
        campoPuesto = RequireQ<DropdownField>("pt-campo-puesto");
        campoNotas = RequireQ<TextField>("pt-campo-notas");
        btnFecha = RequireQ<Button>("pt-campo-fecha-btn");
        btnHoraInicio = RequireQ<Button>("pt-campo-hora-inicio-btn");
        btnHoraFin = RequireQ<Button>("pt-campo-hora-fin-btn");
        errorLabel = RequireQ<Label>("pt-error");
        btnEliminar = RequireQ<Button>("pt-btn-eliminar");
        btnGuardar = RequireQ<Button>("pt-btn-guardar");
        btnHoy = RequireQ<Button>("pt-btn-hoy");

        campoPuesto.choices = Puestos.Select(p => PuestoLabel[p]).ToList();

        if (keyboardBridge != null) keyboardBridge.Bind(campoNotas);

        ConfigurarFechaPicker();
        ConfigurarHoraPicker();
    }

    private T RequireQ<T>(string name) where T : VisualElement
    {
        var el = root.Q<T>(name);
        if (el == null)
            Debug.LogError($"[PlanTurnos] No se encontro el elemento '{name}' en el UXML. Revisa que el UIDocument apunte al archivo correcto y que el nombre coincida.");
        return el;
    }

    private void ConectarEventos()
    {
        root.Q<Button>("pt-btn-semana-prev").clicked += () => CambiarSemana(-1);
        root.Q<Button>("pt-btn-semana-next").clicked += () => CambiarSemana(1);
        root.Q<Button>("pt-btn-hoy").clicked += () => { lunesActual = ObtenerLunes(DateTime.Today); _ = RecargarSemana(); };

        root.Q<Button>("pt-btn-nuevo-turno").clicked += () => AbrirModalNuevo();
        root.Q<Button>("pt-modal-close").clicked += CerrarModal;
        root.Q<Button>("pt-btn-cancelar").clicked += CerrarModal;
        root.Q<Button>("pt-btn-guardar").clicked += () => _ = GuardarTurno();
        btnEliminar.clicked += () => _ = EliminarTurnoEnEdicion();

        root.Q<Button>("pt-btn-duplicar-semana").clicked += () => _ = DuplicarSemanaAnterior();
        root.Q<Button>("pt-btn-cancelar-copia").clicked += CancelarCopia;

        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Escape && HayTurnoCopiado) CancelarCopia();
        }, TrickleDown.TrickleDown);

        // ── Selector de fecha ──
        btnFecha.clicked += () =>
        {
            calMesVisible = new DateTime(modalFecha.Year, modalFecha.Month, 1);
            RenderCalendarioTurno();
            root.Q<VisualElement>("pt-fecha-picker-overlay").style.display = DisplayStyle.Flex;
        };
        root.Q<Button>("pt-cal-mes-anterior").clicked += () => { calMesVisible = calMesVisible.AddMonths(-1); RenderCalendarioTurno(); };
        root.Q<Button>("pt-cal-mes-siguiente").clicked += () => { calMesVisible = calMesVisible.AddMonths(1); RenderCalendarioTurno(); };
        root.Q<VisualElement>("pt-fecha-picker-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("pt-fecha-picker-overlay"))
                root.Q<VisualElement>("pt-fecha-picker-overlay").style.display = DisplayStyle.None;
        });

        // ── Selector de hora (compartido entre inicio y fin) ──
        btnHoraInicio.clicked += () => AbrirPickerHora(esInicio: true);
        btnHoraFin.clicked += () => AbrirPickerHora(esInicio: false);
        root.Q<VisualElement>("pt-hora-picker-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("pt-hora-picker-overlay"))
                root.Q<VisualElement>("pt-hora-picker-overlay").style.display = DisplayStyle.None;
        });
    }

    // ── Semana ──
    private static DateTime ObtenerLunes(DateTime fecha)
    {
        int diff = (7 + (fecha.DayOfWeek - DayOfWeek.Monday)) % 7;
        return fecha.AddDays(-diff).Date;
    }

    private void CambiarSemana(int deltaSemanas)
    {
        lunesActual = lunesActual.AddDays(7 * deltaSemanas);
        _ = RecargarSemana();
    }

    private void ActualizarCabecerasSemana()
    {
        string[] nombres = { "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom" };
        for (int i = 0; i < 7; i++)
        {
            var fecha = lunesActual.AddDays(i);
            var th = root.Q<Label>($"pt-th-{i}");
            if (th != null) th.text = $"{nombres[i]} {fecha.Day}";
        }
        var domingo = lunesActual.AddDays(6);
        semanaTexto.text = $"{lunesActual:dd MMM} - {domingo:dd MMM}";

        // "Esta semana" solo aparece si te has ido de la semana actual — si ya estas en ella, sobra.
        btnHoy.style.display = (lunesActual == ObtenerLunes(DateTime.Today)) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    // ── Carga de datos ──
    private async Task CargarTodo()
    {
        ActualizarCabecerasSemana();
        MostrarCargandoTabla();
        personal = (await ObtenerPersonal()).ToList();
        turnosSemana = (await ObtenerTurnosSemana(lunesActual)).ToList();
        PintarTabla();
    }

    private async Task RecargarSemana()
    {
        int miSolicitud = ++solicitudActual;
        scrollGuardado = tablaScroll.scrollOffset;
        ActualizarCabecerasSemana();
        MostrarCargandoTabla();
        var nuevos = await ObtenerTurnosSemana(lunesActual);
        if (miSolicitud != solicitudActual) return; // el usuario ya cambio de semana otra vez
        turnosSemana = nuevos.ToList();
        PintarTabla();
    }

    private async Task<Empleado[]> ObtenerPersonal()
    {
        string url = $"{apiBase}/personal/restaurant/{restaurantId}";
        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo cargar el personal: " + req.error);
            return Array.Empty<Empleado>();
        }

        var todos = JsonHelper.FromJson<Empleado>(req.downloadHandler.text);
        return todos.Where(e => e.activo != 0).ToArray();
    }

    private async Task<Turno[]> ObtenerTurnosSemana(DateTime lunes)
    {
        string inicio = lunes.ToString("yyyy-MM-dd");
        string fin = lunes.AddDays(6).ToString("yyyy-MM-dd");
        string url = $"{apiBase}/turnos?restaurant_id={restaurantId}&fecha_inicio={inicio}&fecha_fin={fin}";

        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudieron cargar los turnos: " + req.error);
            return Array.Empty<Turno>();
        }

        var data = JsonUtility.FromJson<TurnosResponse>(req.downloadHandler.text);
        return data?.turnos ?? Array.Empty<Turno>();
    }

    // ── Render de la tabla semanal ──
    private void MostrarCargandoTabla()
    {
        tablaBody.Clear();
        var msg = new Label("Cargando turnos...");
        msg.AddToClassList("pt-tabla-mensaje");
        tablaBody.Add(msg);
    }

    private void PintarTabla()
    {
        tablaBody.Clear();

        if (personal.Count == 0)
        {
            var msg = new Label("No hay empleados dados de alta todavia.");
            msg.AddToClassList("pt-tabla-mensaje");
            tablaBody.Add(msg);
            return;
        }

        foreach (var emp in personal)
        {
            var fila = new VisualElement();
            fila.AddToClassList("pt-table-row");

            var turnosEmp = turnosSemana.Where(t => t.id_empleado == emp.id_empleado).ToList();
            float horasPlanificadas = turnosEmp.Sum(t => DiferenciaHoras(t.hora_inicio, t.hora_fin));

            var tdEmpleado = new VisualElement();
            tdEmpleado.AddToClassList("pt-td");
            tdEmpleado.AddToClassList("pt-td-empleado");

            var empBox = new VisualElement();
            empBox.AddToClassList("pt-emp");

            var avatar = new VisualElement();
            avatar.AddToClassList("pt-avatar");
            avatar.Add(new Label(Iniciales(emp.nombre)));

            var infoBox = new VisualElement();
            infoBox.AddToClassList("pt-emp-info");

            var nombreLbl = new Label(emp.nombre);
            nombreLbl.AddToClassList("pt-nombre");
            infoBox.Add(nombreLbl);

            var contrato = emp.horas_contrato_semana > 0 ? emp.horas_contrato_semana : 40f;
            var horasLbl = new Label($"{horasPlanificadas:0.0} / {contrato:0}h");
            horasLbl.AddToClassList("pt-horas-contrato");
            if (horasPlanificadas > contrato) horasLbl.AddToClassList("pt-horas-contrato--exceso");
            infoBox.Add(horasLbl);

            var barra = new VisualElement();
            barra.AddToClassList("pt-contrato-barra");
            var fill = new VisualElement();
            fill.AddToClassList("pt-contrato-barra-fill");
            float ratio = contrato > 0 ? Mathf.Clamp01(horasPlanificadas / contrato) : 0;
            fill.style.width = new Length(ratio * 100, LengthUnit.Percent);
            barra.Add(fill);
            infoBox.Add(barra);

            empBox.Add(avatar);
            empBox.Add(infoBox);
            tdEmpleado.Add(empBox);
            fila.Add(tdEmpleado);

            for (int i = 0; i < 7; i++)
            {
                var fechaCelda = lunesActual.AddDays(i).Date;
                var td = new VisualElement();
                td.AddToClassList("pt-td");

                string fechaCeldaStr = fechaCelda.ToString("yyyy-MM-dd");
                var turnosDia = turnosEmp.Where(t => t.fecha == fechaCeldaStr).ToList();
                foreach (var t in turnosDia)
                    td.Add(CrearChipTurno(t));

                var masBtn = new VisualElement();
                bool esVacia = turnosDia.Count == 0;
                masBtn.AddToClassList(esVacia ? "celda-vacia" : "celda-mas");

                if (HayTurnoCopiado)
                {
                    masBtn.AddToClassList(esVacia ? "celda-vacia--pegar" : "celda-mas--pegar");
                    masBtn.Add(new Label("Pegar"));
                    masBtn.RegisterCallback<ClickEvent>(evt => { _ = PegarTurno(emp.id_empleado, fechaCelda); });
                }
                else
                {
                    masBtn.Add(new Label("+"));
                    masBtn.RegisterCallback<ClickEvent>(_ => AbrirModalNuevo(emp.id_empleado, fechaCelda));
                }
                td.Add(masBtn);

                fila.Add(td);
            }

            tablaBody.Add(fila);
        }

        RestaurarScrollTabla();
    }

    private void RestaurarScrollTabla()
    {
        int intentosRestantes = 6;
        IVisualElementScheduledItem item = null;
        item = tablaScroll.schedule.Execute(() =>
        {
            tablaScroll.scrollOffset = scrollGuardado;
            intentosRestantes--;
            if (intentosRestantes <= 0) item.Pause();
        }).Every(16);
    }

    private VisualElement CrearChipTurno(Turno t)
    {
        var chip = new VisualElement();
        chip.AddToClassList("turno-chip");
        chip.AddToClassList($"p-{t.puesto}");

        var infoBox = new VisualElement();
        infoBox.AddToClassList("turno-chip-info");

        var puestoLbl = new Label(PuestoLabel.TryGetValue(t.puesto, out var lbl) ? lbl : t.puesto);
        puestoLbl.AddToClassList("turno-chip-puesto");
        var horarioLbl = new Label($"{t.hora_inicio}-{t.hora_fin}");
        horarioLbl.AddToClassList("turno-chip-horario");

        infoBox.Add(puestoLbl);
        infoBox.Add(horarioLbl);
        chip.Add(infoBox);

        var btnCopiar = new Button { text = "Copiar" };
        btnCopiar.AddToClassList("turno-chip-copiar");
        btnCopiar.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            CopiarTurno(t);
        });
        chip.Add(btnCopiar);

        chip.RegisterCallback<ClickEvent>(_ => AbrirModalEdicion(t));
        return chip;
    }

    private static string Iniciales(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "?";
        var partes = nombre.Trim().Split(' ');
        return string.Concat(partes.Take(2).Select(p => char.ToUpper(p[0])));
    }

    private static float DiferenciaHoras(string inicio, string fin)
    {
        if (!TimeSpan.TryParse(inicio, out var i) || !TimeSpan.TryParse(fin, out var f)) return 0f;
        var diff = f - i;
        if (diff.TotalMinutes < 0) diff += TimeSpan.FromHours(24);
        return (float)diff.TotalHours;
    }

    // ── Modal ──
    private void AbrirModalNuevo(int? idEmpleado = null, DateTime? fecha = null)
    {
        turnoEnEdicion = null;
        modalTitulo.text = "Nuevo turno";
        modalBox.RemoveFromClassList("modal-box--editar");
        modalSubtitulo.RemoveFromClassList("pt-modal-subtitulo--visible");

        campoEmpleado.choices = personal.Select(p => p.nombre).ToList();
        var empPorDefecto = idEmpleado.HasValue
            ? personal.FirstOrDefault(p => p.id_empleado == idEmpleado.Value)
            : personal.FirstOrDefault();
        campoEmpleado.value = empPorDefecto?.nombre;

        modalFecha = fecha ?? lunesActual;
        ActualizarLabelFechaModal();

        modalHoraInicio = "";
        modalHoraFin = "";
        btnHoraInicio.text = "Elegir hora";
        btnHoraFin.text = "Elegir hora";
        btnHoraInicio.RemoveFromClassList("hora-picker-btn--error");
        btnHoraFin.RemoveFromClassList("hora-picker-btn--error");

        campoPuesto.value = PuestoLabel["cocina"];
        campoNotas.value = "";
        btnEliminar.style.display = DisplayStyle.None;
        errorLabel.style.display = DisplayStyle.None;
        modalOverlay.style.display = DisplayStyle.Flex;
    }

    private void AbrirModalEdicion(Turno t)
    {
        turnoEnEdicion = t;
        modalTitulo.text = "Editar turno";
        modalBox.AddToClassList("modal-box--editar");
        modalSubtitulo.text = $"Editando el turno de {t.nombre_empleado}";
        modalSubtitulo.AddToClassList("pt-modal-subtitulo--visible");

        campoEmpleado.choices = personal.Select(p => p.nombre).ToList();
        campoEmpleado.value = t.nombre_empleado;

        modalFecha = DateTime.TryParseExact(t.fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)
            ? f
            : lunesActual;
        ActualizarLabelFechaModal();

        modalHoraInicio = t.hora_inicio;
        modalHoraFin = t.hora_fin;
        btnHoraInicio.text = string.IsNullOrEmpty(t.hora_inicio) ? "Elegir hora" : t.hora_inicio;
        btnHoraFin.text = string.IsNullOrEmpty(t.hora_fin) ? "Elegir hora" : t.hora_fin;
        btnHoraInicio.RemoveFromClassList("hora-picker-btn--error");
        btnHoraFin.RemoveFromClassList("hora-picker-btn--error");

        campoPuesto.value = PuestoLabel.TryGetValue(t.puesto, out var lbl) ? lbl : t.puesto;
        campoNotas.value = t.notas;
        btnEliminar.style.display = DisplayStyle.Flex;
        errorLabel.style.display = DisplayStyle.None;
        modalOverlay.style.display = DisplayStyle.Flex;
    }

    private void CerrarModal() => modalOverlay.style.display = DisplayStyle.None;

    private void CancelarCopia()
    {
        copiadoPuesto = null;
        copiadoHoraInicio = null;
        copiadoHoraFin = null;
        copiadoNotas = null;
        root.Q<VisualElement>("pt-clipboard-bar").style.display = DisplayStyle.None;
        PintarTabla();
    }

    private void CopiarTurno(Turno t)
    {
        copiadoPuesto = t.puesto;
        copiadoHoraInicio = t.hora_inicio;
        copiadoHoraFin = t.hora_fin;
        copiadoNotas = t.notas;
        ActualizarBarraPortapapeles();
        PintarTabla();
    }

    private void ActualizarBarraPortapapeles()
    {
        var barra = root.Q<VisualElement>("pt-clipboard-bar");
        var preview = root.Q<Label>("pt-clipboard-preview");

        if (!HayTurnoCopiado)
        {
            barra.style.display = DisplayStyle.None;
            return;
        }

        string label = PuestoLabel.TryGetValue(copiadoPuesto, out var lbl) ? lbl : copiadoPuesto;
        preview.text = $"{label} - {copiadoHoraInicio} a {copiadoHoraFin}";
        preview.RemoveFromClassList("p-cocina");
        preview.RemoveFromClassList("p-sala");
        preview.RemoveFromClassList("p-barra");
        preview.RemoveFromClassList("p-repartidor");
        preview.AddToClassList($"p-{copiadoPuesto}");
        barra.style.display = DisplayStyle.Flex;
    }

    private async Task PegarTurno(int idEmpleado, DateTime fecha)
    {
        if (!HayTurnoCopiado || guardandoPegado) return;
        guardandoPegado = true;

        try
        {
            var payload = new TurnoNuevo
            {
                restaurant_id = restaurantId,
                id_empleado = idEmpleado,
                fecha = fecha.ToString("yyyy-MM-dd"),
                hora_inicio = copiadoHoraInicio ?? "",
                hora_fin = copiadoHoraFin ?? "",
                puesto = copiadoPuesto ?? "",
                notas = copiadoNotas ?? "",
                creado_por = SesionEmpleado.Codigo
            };
            string json = JsonUtility.ToJson(payload);
            Debug.Log("Payload pegar turno: " + json);

            using var req = new UnityWebRequest($"{apiBase}/turnos", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string mensaje = "No se ha podido pegar el turno";
                try
                {
                    var err = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);
                    if (err != null && !string.IsNullOrEmpty(err.error)) mensaje = err.error;
                }
                catch { /* respuesta no era JSON */ }
                Debug.LogError($"{mensaje} (HTTP {req.responseCode}): {req.downloadHandler.text}");
                return;
            }

            await RecargarSemana();
        }
        finally
        {
            guardandoPegado = false;
        }
    }

    private void MostrarError(string mensaje)
    {
        errorLabel.text = mensaje;
        errorLabel.style.display = DisplayStyle.Flex;
    }

    // ── Selector de fecha ──
    private void ConfigurarFechaPicker()
    {
        var diasSemanaRow = root.Q<VisualElement>("pt-cal-dias-semana");
        foreach (var dia in DiasSemanaCal)
        {
            var lbl = new Label(dia);
            lbl.AddToClassList("cal-dia-semana-label");
            diasSemanaRow.Add(lbl);
        }
    }

    private void RenderCalendarioTurno()
    {
        root.Q<Label>("pt-cal-mes-label").text = $"{MesesLargos[calMesVisible.Month - 1]} de {calMesVisible.Year}";

        var grid = root.Q<VisualElement>("pt-cal-grid-fechas");
        grid.Clear();

        var minFecha = DateTime.Today.AddDays(-60);
        var maxFecha = DateTime.Today.AddDays(120);

        int primerDiaSemana = ((int)new DateTime(calMesVisible.Year, calMesVisible.Month, 1).DayOfWeek + 6) % 7;
        int totalDias = DateTime.DaysInMonth(calMesVisible.Year, calMesVisible.Month);

        for (int i = 0; i < primerDiaSemana; i++)
        {
            var vacio = new Button();
            vacio.AddToClassList("cal-day-chip");
            vacio.AddToClassList("cal-day-chip--empty");
            vacio.SetEnabled(false);
            grid.Add(vacio);
        }

        for (int dia = 1; dia <= totalDias; dia++)
        {
            var fecha = new DateTime(calMesVisible.Year, calMesVisible.Month, dia);
            var btn = new Button { text = dia.ToString() };
            btn.AddToClassList("cal-day-chip");

            bool fueraDeRango = fecha < minFecha || fecha > maxFecha;
            if (fueraDeRango) btn.SetEnabled(false);
            else btn.clicked += () => SeleccionarFechaTurno(fecha);

            if (fecha.Date == modalFecha.Date)
                btn.AddToClassList("cal-day-chip--selected");

            grid.Add(btn);
        }

        root.Q<Button>("pt-cal-mes-anterior").SetEnabled(new DateTime(calMesVisible.Year, calMesVisible.Month, 1) > new DateTime(minFecha.Year, minFecha.Month, 1));
        root.Q<Button>("pt-cal-mes-siguiente").SetEnabled(calMesVisible.Year != maxFecha.Year || calMesVisible.Month != maxFecha.Month);
    }

    private void SeleccionarFechaTurno(DateTime fecha)
    {
        modalFecha = fecha;
        ActualizarLabelFechaModal();
        root.Q<VisualElement>("pt-fecha-picker-overlay").style.display = DisplayStyle.None;
    }

    private void ActualizarLabelFechaModal()
    {
        if (modalFecha.Date == DateTime.Today) btnFecha.text = "Hoy";
        else if (modalFecha.Date == DateTime.Today.AddDays(1)) btnFecha.text = "Mañana";
        else btnFecha.text = $"{modalFecha:dd/MM/yyyy}";
    }

    // ── Selector de hora (compartido entre inicio y fin) ──
    private void ConfigurarHoraPicker()
    {
        var grid = root.Q<VisualElement>("pt-hora-picker-grid");
        for (var t = TimeSpan.Zero; t < TimeSpan.FromDays(1); t += TimeSpan.FromMinutes(15))
        {
            string hora = $"{(int)t.TotalHours:00}:{t.Minutes:00}";
            var chip = new Button { text = hora, name = $"pt-hora-chip-{hora}" };
            chip.AddToClassList("hora-chip");
            chip.clicked += () => SeleccionarHoraTurno(hora);
            grid.Add(chip);
        }
    }

    private void AbrirPickerHora(bool esInicio)
    {
        editandoHoraInicio = esInicio;
        string valorActual = esInicio ? modalHoraInicio : modalHoraFin;

        foreach (var chip in root.Q<VisualElement>("pt-hora-picker-grid").Children())
        {
            chip.RemoveFromClassList("hora-chip--active");
            if (chip is Button b && b.text == valorActual) chip.AddToClassList("hora-chip--active");
        }

        root.Q<VisualElement>("pt-hora-picker-overlay").style.display = DisplayStyle.Flex;
    }

    private void SeleccionarHoraTurno(string hora)
    {
        if (editandoHoraInicio)
        {
            modalHoraInicio = hora;
            btnHoraInicio.text = hora;
            btnHoraInicio.RemoveFromClassList("hora-picker-btn--error");
        }
        else
        {
            modalHoraFin = hora;
            btnHoraFin.text = hora;
            btnHoraFin.RemoveFromClassList("hora-picker-btn--error");
        }

        foreach (var chip in root.Q<VisualElement>("pt-hora-picker-grid").Children())
        {
            chip.RemoveFromClassList("hora-chip--active");
            if (chip is Button b && b.text == hora) chip.AddToClassList("hora-chip--active");
        }

        root.Q<VisualElement>("pt-hora-picker-overlay").style.display = DisplayStyle.None;
    }

    private async Task GuardarTurno()
    {
        btnHoraInicio.RemoveFromClassList("hora-picker-btn--error");
        btnHoraFin.RemoveFromClassList("hora-picker-btn--error");

        var empSeleccionado = personal.FirstOrDefault(p => p.nombre == campoEmpleado.value);
        if (empSeleccionado == null) { MostrarError("Selecciona un empleado."); return; }

        bool faltaInicio = string.IsNullOrWhiteSpace(modalHoraInicio);
        bool faltaFin = string.IsNullOrWhiteSpace(modalHoraFin);
        if (faltaInicio) btnHoraInicio.AddToClassList("hora-picker-btn--error");
        if (faltaFin) btnHoraFin.AddToClassList("hora-picker-btn--error");
        if (faltaInicio || faltaFin)
        {
            MostrarError("Rellena la hora de inicio y de fin.");
            return;
        }

        string fechaValor = modalFecha.ToString("yyyy-MM-dd");
        string puestoValor = Puestos.FirstOrDefault(p => PuestoLabel[p] == campoPuesto.value) ?? "cocina";

        btnGuardar.SetEnabled(false);
        errorLabel.style.display = DisplayStyle.None;

        try
        {
            UnityWebRequest req;
            bool esEdicion = turnoEnEdicion != null;

            if (esEdicion)
            {
                var payload = new TurnoEdicion
                {
                    id_empleado = empSeleccionado.id_empleado,
                    fecha = fechaValor,
                    hora_inicio = modalHoraInicio,
                    hora_fin = modalHoraFin,
                    puesto = puestoValor,
                    notas = campoNotas.value
                };
                string json = JsonUtility.ToJson(payload);
                req = new UnityWebRequest($"{apiBase}/turnos/{turnoEnEdicion.id}", "PATCH");
                req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
            }
            else
            {
                var payload = new TurnoNuevo
                {
                    restaurant_id = restaurantId,
                    id_empleado = empSeleccionado.id_empleado,
                    fecha = fechaValor,
                    hora_inicio = modalHoraInicio,
                    hora_fin = modalHoraFin,
                    puesto = puestoValor,
                    notas = campoNotas.value,
                    creado_por = SesionEmpleado.Codigo
                };
                string json = JsonUtility.ToJson(payload);
                req = new UnityWebRequest($"{apiBase}/turnos", "POST");
                req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
            }

            using (req)
            {
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    string mensaje = esEdicion ? "No se ha podido guardar el turno" : "No se ha podido crear el turno";
                    try
                    {
                        var err = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);
                        if (err != null && !string.IsNullOrEmpty(err.error)) mensaje = err.error;
                    }
                    catch { /* respuesta no era JSON */ }
                    MostrarError(mensaje);
                    return;
                }
            }

            CerrarModal();
            await RecargarSemana();
        }
        finally
        {
            btnGuardar.SetEnabled(true);
        }
    }

    private async Task EliminarTurnoEnEdicion()
    {
        if (turnoEnEdicion == null) return;

        using var req = UnityWebRequest.Delete($"{apiBase}/turnos/{turnoEnEdicion.id}");
        req.downloadHandler = new DownloadHandlerBuffer();
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            MostrarError("No se ha podido eliminar el turno");
            return;
        }

        CerrarModal();
        await RecargarSemana();
    }

    private async Task DuplicarSemanaAnterior()
    {
        var btnDuplicar = root.Q<Button>("pt-btn-duplicar-semana");
        string textoOriginal = btnDuplicar.text;
        btnDuplicar.SetEnabled(false);
        btnDuplicar.text = "Duplicando...";

        try
        {
            var semanaAnterior = lunesActual.AddDays(-7);
            var payload = new DuplicarSemanaPayload
            {
                restaurant_id = restaurantId,
                fecha_origen_lunes = semanaAnterior.ToString("yyyy-MM-dd"),
                fecha_destino_lunes = lunesActual.ToString("yyyy-MM-dd")
            };
            string json = JsonUtility.ToJson(payload);

            using var req = new UnityWebRequest($"{apiBase}/turnos/duplicar_semana", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("No se pudo duplicar la semana: " + req.error);
                return;
            }

            await RecargarSemana();
        }
        finally
        {
            btnDuplicar.text = textoOriginal;
            btnDuplicar.SetEnabled(true);
        }
    }

    private void CargarColoresPersonalizacion()
    {
        if (dbPersonalizacion == null || !dbPersonalizacion.IsLoaded) return;

        if (DataBasePersonalizacion.col_ppal_botones != null && DataBasePersonalizacion.col_ppal_botones.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out var cPpal))
            colorPpalBotones = cPpal;

        if (DataBasePersonalizacion.col_sec_botones != null && DataBasePersonalizacion.col_sec_botones.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_botones[0], out var cSec))
            colorSecBotones = cSec;
    }

    private void OnPersonalizacionCargada()
    {
        CargarColoresPersonalizacion();
        FijarColoresBotones();
    }

    private void FijarColoresBotones()
    {
        void FijarPrimario(string nombre)
        {
            var b = root.Q<Button>(nombre);
            if (b != null) b.style.backgroundColor = colorPpalBotones;
        }

        // "pt-btn-hoy" y "pt-btn-duplicar-semana" ya NO se tintan con el color de
        // personalizacion del restaurante: se quedan con el gris/blanco neutro
        // definido en el USS (texto siempre negro/blanco, nunca un color random
        // segun lo que tenga configurado el restaurante).
        FijarPrimario("pt-btn-nuevo-turno");
        FijarPrimario("pt-btn-guardar");
    }

    [Serializable]
    private class DuplicarSemanaPayload
    {
        public string restaurant_id;
        public string fecha_origen_lunes;
        public string fecha_destino_lunes;
    }
}