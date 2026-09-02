using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

// Empleado y JsonHelper ya existen en el proyecto (ver PlanTurnosController.cs),
// asi que ControlHorarioController los reutiliza en lugar de volver a declararlos aqui.
// Empleado trae de sobra los campos que necesitamos (id_empleado, nombre, activo).

[Serializable]
public class Fichaje
{
    public int id;
    public int id_empleado;
    public string nombre;         // viene incluido por si el empleado ya no esta activo
    public string fecha_hora;     // "yyyy-MM-dd HH:mm:ss"
    public string tipo;           // ENTRADA | SALIDA
    public string observaciones;
}

[Serializable]
public class ControlHorarioTpvRequest { public string id; public string codigo; }

public class ControlHorarioController : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private string apiBase = "https://tu-api.com";
    [SerializeField] private string restaurantId = "1";
    [SerializeField] private string codigoEmpleado;

    private const float INTERVALO_REFRESCO = 60f; // segundos, igual que el setInterval de la web

    // ── Referencias UI ──
    private VisualElement root;

    private VisualElement tabDia, tabInforme;
    private Button tabBtnDia, tabBtnInforme;

    private TextField campoFechaDia;
    private Button btnHoyDia;
    private VisualElement tablaBodyDia;
    private Label statTrabajandoValue, statHorasValue, statIncompletosValue, statSinficharValue;

    private DropdownField campoEmpleadoInforme;
    private Button segMesBtn, segRangoBtn;
    private VisualElement grupoMes, grupoDesde, grupoHasta;
    private TextField campoMes, campoDesde, campoHasta;
    private Label informeError;

    private VisualElement informeResultado;
    private Label informeVacio, informeTitulo, informeSub, thEmpleadoInforme;
    private Label irStatHoras, irStatDias, irStatMedia, irStatTurnos;
    private VisualElement irTablaBody;

    private VisualElement modalOverlay;
    private Label mdNombre, mdFecha;
    private ScrollView mdBody;

    // ── Estado ──
    private List<Empleado> personal = new();
    private List<Fichaje> fichajesTodos = new();
    private readonly Dictionary<string, int> empleadoNombreAId = new();
    private string fechaActual;
    private string modoPeriodo = "mes";
    private Informe informeActual;
    private float refreshTimer = 0f;

    private void OnEnable()
    {
        root = document.rootVisualElement;
        fechaActual = DateTime.Today.ToString("yyyy-MM-dd");
        restaurantId = SesionEmpleado.RestaurantId;
        codigoEmpleado = SesionEmpleado.Codigo;

        CachearReferencias();
        ConectarEventos();
        CambiarTab("dia");
        CambiarModoPeriodo("mes");
        CerrarDetalle();

        campoFechaDia.SetValueWithoutNotify(fechaActual);
        ActualizarVisibilidadBtnHoy();
        campoMes.SetValueWithoutNotify(fechaActual.Substring(0, 7));
        informeResultado.style.display = DisplayStyle.None;

        _ = CargarTodo();
    }

    private void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= INTERVALO_REFRESCO)
        {
            refreshTimer = 0f;
            _ = RefrescarFichajes();
        }
    }

    public void SetCodigoEmpleado(string codigo)
    {
        codigoEmpleado = codigo;
    }

    private void CachearReferencias()
    {
        tabDia = root.Q<VisualElement>("ch-tab-dia");
        tabInforme = root.Q<VisualElement>("ch-tab-informe");
        tabBtnDia = root.Q<Button>("ch-tab-btn-dia");
        tabBtnInforme = root.Q<Button>("ch-tab-btn-informe");

        campoFechaDia = root.Q<TextField>("ch-campo-fecha");
        btnHoyDia = root.Q<Button>("ch-btn-hoy");
        tablaBodyDia = root.Q<VisualElement>("ch-tabla-body");

        statTrabajandoValue = root.Q<Label>("ch-stat-trabajando-value");
        statHorasValue = root.Q<Label>("ch-stat-horas-value");
        statIncompletosValue = root.Q<Label>("ch-stat-incompletos-value");
        statSinficharValue = root.Q<Label>("ch-stat-sinfichar-value");

        campoEmpleadoInforme = root.Q<DropdownField>("ch-ir-empleado");
        segMesBtn = root.Q<Button>("ch-seg-mes");
        segRangoBtn = root.Q<Button>("ch-seg-rango");
        grupoMes = root.Q<VisualElement>("ch-grupo-mes");
        grupoDesde = root.Q<VisualElement>("ch-grupo-desde");
        grupoHasta = root.Q<VisualElement>("ch-grupo-hasta");
        campoMes = root.Q<TextField>("ch-ir-mes");
        campoDesde = root.Q<TextField>("ch-ir-desde");
        campoHasta = root.Q<TextField>("ch-ir-hasta");
        informeError = root.Q<Label>("ch-informe-error");

        informeResultado = root.Q<VisualElement>("ch-informe-resultado");
        informeVacio = root.Q<Label>("ch-informe-vacio");
        informeTitulo = root.Q<Label>("ch-informe-titulo");
        informeSub = root.Q<Label>("ch-informe-sub");
        thEmpleadoInforme = root.Q<Label>("ch-ir-th-empleado");

        irStatHoras = root.Q<Label>("ch-ir-stat-horas");
        irStatDias = root.Q<Label>("ch-ir-stat-dias");
        irStatMedia = root.Q<Label>("ch-ir-stat-media");
        irStatTurnos = root.Q<Label>("ch-ir-stat-turnos");
        irTablaBody = root.Q<VisualElement>("ch-ir-tabla-body");

        modalOverlay = root.Q<VisualElement>("ch-modal-overlay");
        mdNombre = root.Q<Label>("ch-md-nombre");
        mdFecha = root.Q<Label>("ch-md-fecha");
        mdBody = root.Q<ScrollView>("ch-md-body");
    }

    private void ConectarEventos()
    {
        tabBtnDia.clicked += () => CambiarTab("dia");
        tabBtnInforme.clicked += () => CambiarTab("informe");

        root.Q<Button>("ch-btn-dia-prev").clicked += () => CambiarDia(-1);
        root.Q<Button>("ch-btn-dia-next").clicked += () => CambiarDia(1);
        btnHoyDia.clicked += IrHoy;
        campoFechaDia.RegisterValueChangedCallback(evt =>
        {
            if (EsFechaValida(evt.newValue))
            {
                fechaActual = evt.newValue;
                ActualizarVisibilidadBtnHoy();
                RenderDia();
            }
        });

        segMesBtn.clicked += () => CambiarModoPeriodo("mes");
        segRangoBtn.clicked += () => CambiarModoPeriodo("rango");
        root.Q<Button>("ch-btn-consultar").clicked += ConsultarInforme;
        root.Q<Button>("ch-btn-pdf-informe").clicked += DescargarInformeCsv;

        root.Q<Button>("ch-modal-close").clicked += CerrarDetalle;
        modalOverlay.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == modalOverlay) CerrarDetalle();
        });
    }

    // ── Pestañas ──
    private void CambiarTab(string tab)
    {
        SetDisplay(tabDia, tab == "dia");
        SetDisplay(tabInforme, tab == "informe");
        ToggleActiva(tabBtnDia, tab == "dia", "ch-tab-btn--active");
        ToggleActiva(tabBtnInforme, tab == "informe", "ch-tab-btn--active");
    }

    private void SetDisplay(VisualElement el, bool visible) =>
        el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

    private void ToggleActiva(Button b, bool activa, string claseActiva)
    {
        b.RemoveFromClassList(claseActiva);
        if (activa) b.AddToClassList(claseActiva);
    }

    // ── Carga de datos ──
    private async Task CargarTodo()
    {
        personal = (await ObtenerPersonal()).ToList();
        fichajesTodos = (await ObtenerFichajes()).ToList();
        PoblarSelectEmpleados();
        RenderDia();
    }

    private async Task RefrescarFichajes()
    {
        if (string.IsNullOrEmpty(restaurantId)) return;
        fichajesTodos = (await ObtenerFichajes()).ToList();
        if (fechaActual == DateTime.Today.ToString("yyyy-MM-dd")) RenderDia();
    }

    private async Task<Empleado[]> ObtenerPersonal()
    {
        string url = $"{apiBase}/personal/restaurant_tpv/{restaurantId}";
        string json = JsonUtility.ToJson(new ControlHorarioTpvRequest { id = restaurantId, codigo = codigoEmpleado });
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
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

    private async Task<Fichaje[]> ObtenerFichajes()
    {
        string url = $"{apiBase}/fichajes/restaurant_tpv/{restaurantId}";
        string json = JsonUtility.ToJson(new ControlHorarioTpvRequest { id = restaurantId, codigo = codigoEmpleado });
        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudieron cargar los fichajes: " + req.error);
            return Array.Empty<Fichaje>();
        }

        return JsonHelper.FromJson<Fichaje>(req.downloadHandler.text);
    }

    private void PoblarSelectEmpleados()
    {
        var valorPrevio = campoEmpleadoInforme.value;
        var choices = new List<string> { "Todos los empleados" };
        empleadoNombreAId.Clear();

        foreach (var p in personal.Where(p => p.activo != 0))
        {
            // por si dos empleados comparten nombre, se distingue con el id
            string etiqueta = empleadoNombreAId.ContainsKey(p.nombre) ? $"{p.nombre} ({p.id_empleado})" : p.nombre;
            choices.Add(etiqueta);
            empleadoNombreAId[etiqueta] = p.id_empleado;
        }

        campoEmpleadoInforme.choices = choices;
        campoEmpleadoInforme.value = choices.Contains(valorPrevio) ? valorPrevio : "Todos los empleados";
    }

    // ── Construccion de turnos (equivalente a construirTurnosPorEmpleado en la web) ──
    // Empareja cada ENTRADA con su SALIDA para todo el historial de un empleado, asi un
    // turno que cruza la medianoche se calcula bien. Cada turno se asigna al dia de SU
    // ENTRADA. Si el ultimo turno de un empleado se quedo sin SALIDA:
    //  - Si la entrada fue HOY -> sigue fichado ahora mismo (turno "abierta", horas en vivo).
    //  - Si la entrada fue ANTES -> incidencia sin resolver ("incompleta"), no se calculan
    //    horas en vivo para no falsear los totales del informe.
    private struct TurnoCalculado
    {
        public string entrada;
        public string salida;
        public double horas;
        public bool abierta;
        public bool incompleta;
        public string fechaTurno;
    }

    private Dictionary<int, List<TurnoCalculado>> ConstruirTurnosPorEmpleado(List<Fichaje> fichajes)
    {
        var porEmpleado = new Dictionary<int, List<Fichaje>>();
        foreach (var f in fichajes)
        {
            if (!porEmpleado.TryGetValue(f.id_empleado, out var lista))
            {
                lista = new List<Fichaje>();
                porEmpleado[f.id_empleado] = lista;
            }
            lista.Add(f);
        }

        string hoyStr = DateTime.Today.ToString("yyyy-MM-dd");
        var resultado = new Dictionary<int, List<TurnoCalculado>>();

        foreach (var kv in porEmpleado)
        {
            var ordenada = kv.Value.OrderBy(f => ParseFechaHora(f.fecha_hora)).ToList();
            string entradaAbierta = null;
            var turnos = new List<TurnoCalculado>();

            foreach (var f in ordenada)
            {
                if (f.tipo == "ENTRADA")
                {
                    if (entradaAbierta != null)
                    {
                        // Ya habia una ENTRADA sin cerrar (incidencia sin resolver). No se
                        // descarta la nueva: se cierra la anterior como turno incompleto para
                        // que la ENTRADA de hoy si abra un turno y el empleado aparezca "Trabajando".
                        turnos.Add(new TurnoCalculado
                        {
                            entrada = entradaAbierta,
                            salida = null,
                            horas = 0,
                            abierta = false,
                            incompleta = true,
                            fechaTurno = entradaAbierta.Split(' ')[0],
                        });
                    }
                    entradaAbierta = f.fecha_hora;
                }
                else if (f.tipo == "SALIDA")
                {
                    if (entradaAbierta != null)
                    {
                        double horas = (ParseFechaHora(f.fecha_hora) - ParseFechaHora(entradaAbierta)).TotalHours;
                        turnos.Add(new TurnoCalculado
                        {
                            entrada = entradaAbierta,
                            salida = f.fecha_hora,
                            horas = horas,
                            abierta = false,
                            incompleta = false,
                            fechaTurno = entradaAbierta.Split(' ')[0],
                        });
                        entradaAbierta = null;
                    }
                    // una SALIDA sin ENTRADA previa (dato suelto) se ignora
                }
            }

            if (entradaAbierta != null)
            {
                string fechaEntrada = entradaAbierta.Split(' ')[0];
                bool esHoy = fechaEntrada == hoyStr;
                double horas = esHoy ? Math.Max((DateTime.Now - ParseFechaHora(entradaAbierta)).TotalHours, 0) : 0;
                turnos.Add(new TurnoCalculado
                {
                    entrada = entradaAbierta,
                    salida = null,
                    horas = horas,
                    abierta = esHoy,
                    incompleta = !esHoy,
                    fechaTurno = fechaEntrada,
                });
            }

            resultado[kv.Key] = turnos;
        }

        return resultado;
    }

    // ── Vista diaria ──
    private static readonly Dictionary<string, int> ORDEN_ESTADO = new()
    {
        { "working", 0 }, { "incompleta", 1 }, { "out", 2 }, { "none", 3 },
    };

    private class FilaDia
    {
        public Empleado empleado;
        public List<TurnoCalculado> turnosDia;
        public double horas;
        public string estado;
        public List<Fichaje> listaDia;
    }

    private void CambiarDia(int delta)
    {
        var d = DateTime.ParseExact(fechaActual, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        fechaActual = d.AddDays(delta).ToString("yyyy-MM-dd");
        campoFechaDia.SetValueWithoutNotify(fechaActual);
        ActualizarVisibilidadBtnHoy();
        RenderDia();
    }

    private void IrHoy()
    {
        fechaActual = DateTime.Today.ToString("yyyy-MM-dd");
        campoFechaDia.SetValueWithoutNotify(fechaActual);
        ActualizarVisibilidadBtnHoy();
        RenderDia();
    }

    private void ActualizarVisibilidadBtnHoy()
    {
        btnHoyDia.style.display = (fechaActual == DateTime.Today.ToString("yyyy-MM-dd")) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void RenderDia()
    {
        var turnosPorEmpleado = ConstruirTurnosPorEmpleado(fichajesTodos);

        int trabajando = 0, sinFichar = 0, incompletos = 0;
        double horasTotales = 0;
        var filas = new List<FilaDia>();

        foreach (var emp in personal.Where(p => p.activo != 0))
        {
            var turnosDia = (turnosPorEmpleado.TryGetValue(emp.id_empleado, out var t) ? t : new List<TurnoCalculado>())
                .Where(x => x.fechaTurno == fechaActual).ToList();

            var listaDia = fichajesTodos
                .Where(f => f.id_empleado == emp.id_empleado && (f.fecha_hora ?? "").Split(' ')[0] == fechaActual)
                .OrderBy(f => ParseFechaHora(f.fecha_hora))
                .ToList();

            double horas = turnosDia.Sum(x => x.horas);
            string estado = "none";
            if (turnosDia.Count > 0)
            {
                var ultimo = turnosDia[turnosDia.Count - 1];
                estado = ultimo.abierta ? "working" : (ultimo.incompleta ? "incompleta" : "out");
            }

            if (estado == "working") trabajando++;
            else if (estado == "incompleta") incompletos++;
            else if (estado == "none") sinFichar++;
            horasTotales += horas;

            filas.Add(new FilaDia { empleado = emp, turnosDia = turnosDia, horas = horas, estado = estado, listaDia = listaDia });
        }

        filas = filas
            .OrderBy(f => ORDEN_ESTADO[f.estado])
            .ThenBy(f => f.empleado.nombre, StringComparer.Ordinal)
            .ToList();

        statTrabajandoValue.text = trabajando.ToString();
        statHorasValue.text = FmtHoras(horasTotales);
        statSinficharValue.text = sinFichar.ToString();
        statIncompletosValue.text = incompletos.ToString();

        tablaBodyDia.Clear();

        if (filas.Count == 0)
        {
            var vacio = new Label("No hay empleados dados de alta todavia.");
            vacio.AddToClassList("ch-empty");
            tablaBodyDia.Add(vacio);
            return;
        }

        int idx = 0;
        foreach (var fila in filas)
        {
            tablaBodyDia.Add(CrearFilaDia(fila.empleado, fila.turnosDia, fila.horas, fila.estado, fila.listaDia, idx));
            idx++;
        }
    }

    private VisualElement CrearFilaDia(Empleado empleado, List<TurnoCalculado> turnosDia, double horas, string estado, List<Fichaje> listaDia, int avatarIdx)
    {
        var fila = new VisualElement();
        fila.AddToClassList("ch-table-row");

        var tdEmp = new VisualElement();
        tdEmp.AddToClassList("ch-td");
        tdEmp.AddToClassList("ch-col-empleado");
        tdEmp.Add(CrearEmpBox(empleado.nombre, avatarIdx));
        fila.Add(tdEmp);

        var tdEstado = new VisualElement();
        tdEstado.AddToClassList("ch-td");
        tdEstado.AddToClassList("ch-col-estado");
        tdEstado.Add(CrearBadge(estado));
        fila.Add(tdEstado);

        var tdTurnos = new VisualElement();
        tdTurnos.AddToClassList("ch-td");
        tdTurnos.AddToClassList("ch-col-turnos");
        string turnosTxt = turnosDia.Count > 0
            ? string.Join(" . ", turnosDia.Select(t => $"{FmtHora(t.entrada)} -> {(t.abierta ? "en curso" : (t.incompleta ? "sin cerrar" : FmtHora(t.salida)))}"))
            : "-";
        var turnosLbl = new Label(turnosTxt);
        turnosLbl.AddToClassList("ch-mono");
        if (turnosDia.Count == 0) turnosLbl.AddToClassList("ch-td-muted");
        tdTurnos.Add(turnosLbl);
        fila.Add(tdTurnos);

        var tdHoras = new VisualElement();
        tdHoras.AddToClassList("ch-td");
        tdHoras.AddToClassList("ch-col-horas");
        string horasTxt = turnosDia.Count > 0 && turnosDia.All(t => t.incompleta) ? "-" : FmtHoras(horas);
        var horasLbl = new Label(horasTxt);
        horasLbl.AddToClassList("ch-td-hours");
        tdHoras.Add(horasLbl);
        fila.Add(tdHoras);

        var tdAccion = new VisualElement();
        tdAccion.AddToClassList("ch-td");
        tdAccion.AddToClassList("ch-col-accion");
        if (listaDia.Count > 0)
        {
            var btnDetalle = new Button(() => AbrirDetalle(empleado.nombre, fechaActual, listaDia)) { text = "Ver detalle" };
            btnDetalle.AddToClassList("btn-detalle");
            tdAccion.Add(btnDetalle);
        }
        fila.Add(tdAccion);

        return fila;
    }

    // ── Informe por periodo ──
    private class FilaInforme
    {
        public string nombre;
        public string fecha;
        public string entrada;
        public string salida;
        public double horas;
        public bool abierta;
        public bool incompleta;
        public List<Fichaje> lista;
    }

    private class Informe
    {
        public int? empleadoId;
        public string nombreEmpleado;
        public string desde;
        public string hasta;
        public List<FilaInforme> filas;
        public double horasTotales;
        public int diasTrabajados;
        public int turnosTotales;
        public double mediaHorasDia;
    }

    private void CambiarModoPeriodo(string modo)
    {
        modoPeriodo = modo;
        ToggleActiva(segMesBtn, modo == "mes", "ch-seg-btn--active");
        ToggleActiva(segRangoBtn, modo == "rango", "ch-seg-btn--active");
        SetDisplay(grupoMes, modo == "mes");
        SetDisplay(grupoDesde, modo == "rango");
        SetDisplay(grupoHasta, modo == "rango");
    }

    private Informe CalcularInforme(int? empleadoId, string desde, string hasta)
    {
        var turnosPorEmpleado = ConstruirTurnosPorEmpleado(fichajesTodos);
        var filas = new List<FilaInforme>();

        foreach (var kv in turnosPorEmpleado)
        {
            if (empleadoId.HasValue && kv.Key != empleadoId.Value) continue;

            var empleado = personal.FirstOrDefault(p => p.id_empleado == kv.Key);
            var primerFichaje = fichajesTodos.FirstOrDefault(f => f.id_empleado == kv.Key);
            string nombre = empleado != null ? empleado.nombre : (primerFichaje?.nombre ?? "Empleado");

            foreach (var t in kv.Value.Where(x => string.CompareOrdinal(x.fechaTurno, desde) >= 0 && string.CompareOrdinal(x.fechaTurno, hasta) <= 0))
            {
                var listaDia = fichajesTodos
                    .Where(f => f.id_empleado == kv.Key && (f.fecha_hora ?? "").Split(' ')[0] == t.fechaTurno)
                    .OrderBy(f => ParseFechaHora(f.fecha_hora))
                    .ToList();

                filas.Add(new FilaInforme
                {
                    nombre = nombre,
                    fecha = t.fechaTurno,
                    entrada = t.entrada,
                    salida = t.salida,
                    horas = t.horas,
                    abierta = t.abierta,
                    incompleta = t.incompleta,
                    lista = listaDia,
                });
            }
        }

        filas = filas
            .OrderBy(f => f.fecha, StringComparer.Ordinal)
            .ThenBy(f => f.nombre, StringComparer.Ordinal)
            .ThenBy(f => f.entrada != null ? ParseFechaHora(f.entrada) : DateTime.MinValue)
            .ToList();

        // Los turnos incompletos (incidencia sin resolver) aportan 0 horas para no
        // inflar el informe con tiempo que en realidad no se sabe si se trabajo.
        double horasTotales = filas.Sum(f => f.horas);
        int diasTrabajados = filas.Select(f => f.fecha).Distinct().Count();
        int turnosTotales = filas.Count;
        double media = diasTrabajados > 0 ? horasTotales / diasTrabajados : 0;

        var empleadoObj = empleadoId.HasValue ? personal.FirstOrDefault(p => p.id_empleado == empleadoId.Value) : null;

        return new Informe
        {
            empleadoId = empleadoId,
            nombreEmpleado = empleadoObj != null ? empleadoObj.nombre : "Todos los empleados",
            desde = desde,
            hasta = hasta,
            filas = filas,
            horasTotales = horasTotales,
            diasTrabajados = diasTrabajados,
            turnosTotales = turnosTotales,
            mediaHorasDia = media,
        };
    }

    private void ConsultarInforme()
    {
        OcultarErrorInforme();

        int? empleadoId = null;
        if (campoEmpleadoInforme.value != "Todos los empleados" && empleadoNombreAId.TryGetValue(campoEmpleadoInforme.value, out var id))
            empleadoId = id;

        string desde, hasta;

        if (modoPeriodo == "mes")
        {
            if (string.IsNullOrWhiteSpace(campoMes.value)) { MostrarErrorInforme("Escribe un mes en formato AAAA-MM."); return; }
            if (!DateTime.TryParseExact(campoMes.value.Trim() + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var primerDia))
            { MostrarErrorInforme("Formato de mes invalido. Usa AAAA-MM, por ejemplo 2026-08."); return; }

            desde = primerDia.ToString("yyyy-MM-dd");
            hasta = new DateTime(primerDia.Year, primerDia.Month, DateTime.DaysInMonth(primerDia.Year, primerDia.Month)).ToString("yyyy-MM-dd");
        }
        else
        {
            desde = campoDesde.value?.Trim();
            hasta = campoHasta.value?.Trim();
            if (string.IsNullOrWhiteSpace(desde) || string.IsNullOrWhiteSpace(hasta)) { MostrarErrorInforme("Escribe la fecha de inicio y la de fin (AAAA-MM-DD)."); return; }
            if (!EsFechaValida(desde) || !EsFechaValida(hasta)) { MostrarErrorInforme("Formato de fecha invalido. Usa AAAA-MM-DD."); return; }
            if (string.CompareOrdinal(desde, hasta) > 0) { MostrarErrorInforme("La fecha \"desde\" debe ser anterior a la fecha \"hasta\"."); return; }
        }

        informeActual = CalcularInforme(empleadoId, desde, hasta);
        RenderInforme(informeActual);
    }

    private void RenderInforme(Informe informe)
    {
        informeVacio.style.display = DisplayStyle.None;
        informeResultado.style.display = DisplayStyle.Flex;

        informeTitulo.text = informe.nombreEmpleado;
        informeSub.text = $"Del {FmtFechaCorta(informe.desde)} al {FmtFechaCorta(informe.hasta)}";

        irStatHoras.text = FmtHoras(informe.horasTotales);
        irStatDias.text = informe.diasTrabajados.ToString();
        irStatMedia.text = FmtHoras(informe.mediaHorasDia);
        irStatTurnos.text = informe.turnosTotales.ToString();

        bool mostrarEmpleado = !informe.empleadoId.HasValue;
        SetDisplay(thEmpleadoInforme, mostrarEmpleado);

        irTablaBody.Clear();

        if (informe.filas.Count == 0)
        {
            var vacio = new Label("No hay fichajes registrados en este periodo.");
            vacio.AddToClassList("ch-empty");
            irTablaBody.Add(vacio);
            return;
        }

        int idx = 0;
        foreach (var f in informe.filas)
        {
            irTablaBody.Add(CrearFilaInforme(f, idx, mostrarEmpleado));
            idx++;
        }
    }

    private VisualElement CrearFilaInforme(FilaInforme f, int avatarIdx, bool mostrarEmpleado)
    {
        var fila = new VisualElement();
        fila.AddToClassList("ch-table-row");

        var tdFecha = new VisualElement();
        tdFecha.AddToClassList("ch-td");
        tdFecha.AddToClassList("ch-col-fecha");
        var fechaLbl = new Label(FmtFechaCorta(f.fecha));
        fechaLbl.AddToClassList("ch-mono");
        tdFecha.Add(fechaLbl);
        fila.Add(tdFecha);

        var tdEmp = new VisualElement();
        tdEmp.AddToClassList("ch-td");
        tdEmp.AddToClassList("ch-col-empleado");
        SetDisplay(tdEmp, mostrarEmpleado);
        tdEmp.Add(CrearEmpBox(f.nombre, avatarIdx));
        fila.Add(tdEmp);

        var tdEntrada = new VisualElement();
        tdEntrada.AddToClassList("ch-td");
        tdEntrada.AddToClassList("ch-col-entrada");
        var entradaLbl = new Label(FmtHora(f.entrada));
        entradaLbl.AddToClassList("ch-mono");
        if (string.IsNullOrEmpty(f.entrada)) entradaLbl.AddToClassList("ch-td-muted");
        tdEntrada.Add(entradaLbl);
        fila.Add(tdEntrada);

        var tdSalida = new VisualElement();
        tdSalida.AddToClassList("ch-td");
        tdSalida.AddToClassList("ch-col-salida");
        string salidaTxt = f.abierta ? "en curso" : (f.incompleta ? "sin fichar" : FmtHora(f.salida));
        var salidaLbl = new Label(salidaTxt);
        salidaLbl.AddToClassList("ch-mono");
        if (string.IsNullOrEmpty(f.salida) && !f.abierta) salidaLbl.AddToClassList("ch-td-muted");
        tdSalida.Add(salidaLbl);
        fila.Add(tdSalida);

        var tdHoras = new VisualElement();
        tdHoras.AddToClassList("ch-td");
        tdHoras.AddToClassList("ch-col-horas");
        var horasLbl = new Label(f.incompleta ? "-" : FmtHoras(f.horas));
        horasLbl.AddToClassList("ch-td-hours");
        tdHoras.Add(horasLbl);
        fila.Add(tdHoras);

        var tdAccion = new VisualElement();
        tdAccion.AddToClassList("ch-td");
        tdAccion.AddToClassList("ch-col-accion");
        var btnDetalle = new Button(() => AbrirDetalle(f.nombre, f.fecha, f.lista)) { text = "Ver detalle" };
        btnDetalle.AddToClassList("btn-detalle");
        tdAccion.Add(btnDetalle);
        fila.Add(tdAccion);

        return fila;
    }

    private void MostrarErrorInforme(string mensaje)
    {
        informeError.text = mensaje;
        informeError.AddToClassList("ch-informe-error--visible");
    }

    private void OcultarErrorInforme()
    {
        informeError.text = "";
        informeError.RemoveFromClassList("ch-informe-error--visible");
    }

    // ── Elementos compartidos: avatar/badge ──
    private VisualElement CrearEmpBox(string nombre, int avatarIdx)
    {
        var empBox = new VisualElement();
        empBox.AddToClassList("ch-emp");

        var avatar = new VisualElement();
        avatar.AddToClassList("ch-avatar");
        avatar.Add(new Label(Iniciales(nombre)));

        var nombreLbl = new Label(nombre);
        nombreLbl.AddToClassList("ch-name");

        empBox.Add(avatar);
        empBox.Add(nombreLbl);
        return empBox;
    }

    private VisualElement CrearBadge(string estado)
    {
        var mapa = new Dictionary<string, (string cls, string txt)>
        {
            { "working", ("badge-working", "Trabajando") },
            { "incompleta", ("badge-incompleta", "Sin fichar salida") },
            { "out", ("badge-out", "Jornada finalizada") },
            { "none", ("badge-none", "Sin fichar") },
        };
        var (cls, txt) = mapa.TryGetValue(estado, out var v) ? v : mapa["none"];

        var badge = new VisualElement();
        badge.AddToClassList("ch-badge");
        badge.AddToClassList(cls);

        var dot = new VisualElement();
        dot.AddToClassList("ch-badge-dot");
        badge.Add(dot);
        badge.Add(new Label(txt));

        return badge;
    }

    private static string Iniciales(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "?";
        var partes = nombre.Trim().Split(' ');
        return string.Concat(partes.Take(2).Select(p => char.ToUpper(p[0])));
    }

    // ── Modal detalle (compartido) ──
    private void AbrirDetalle(string nombre, string fechaStr, List<Fichaje> lista)
    {
        mdNombre.text = nombre;
        mdFecha.text = FmtFechaLarga(fechaStr);

        mdBody.Clear();
        foreach (var f in lista)
            mdBody.Add(CrearTimelineItem(f));

        modalOverlay.style.display = DisplayStyle.Flex;
    }

    private VisualElement CrearTimelineItem(Fichaje f)
    {
        bool esEntrada = f.tipo == "ENTRADA";

        var item = new VisualElement();
        item.AddToClassList("timeline-item");

        var icon = new VisualElement();
        icon.AddToClassList("timeline-icon");
        icon.AddToClassList(esEntrada ? "tl-entrada" : "tl-salida");
        icon.Add(new Label(esEntrada ? "E" : "S"));
        item.Add(icon);

        var info = new VisualElement();
        info.AddToClassList("timeline-info");

        var label = new Label(esEntrada ? "Entrada" : "Salida");
        label.AddToClassList("timeline-label");
        info.Add(label);

        var time = new Label(FmtHora(f.fecha_hora));
        time.AddToClassList("timeline-time");
        info.Add(time);

        if (!string.IsNullOrEmpty(f.observaciones))
        {
            var obs = new Label(f.observaciones);
            obs.AddToClassList("timeline-obs");
            info.Add(obs);
        }

        item.Add(info);
        return item;
    }

    private void CerrarDetalle() => modalOverlay.style.display = DisplayStyle.None;

    // ── Exportacion del informe ──
    // Unity no trae un generador de PDF de fabrica (a diferencia de jsPDF en el navegador).
    // Para no anadir una dependencia externa, el informe se exporta como CSV, que se abre
    // directamente en Excel/Sheets con el mismo contenido que el PDF de la web. Si mas
    // adelante quieres un PDF con el mismo diseno, se puede integrar una libreria como
    // PdfSharp o iText.
    private void DescargarInformeCsv()
    {
        if (informeActual == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("Fecha;Empleado;Entrada;Salida;Horas");
        foreach (var f in informeActual.filas)
        {
            string salida = f.abierta ? "en curso" : (f.incompleta ? "sin fichar" : FmtHora(f.salida));
            string horas = f.incompleta ? "-" : FmtHoras(f.horas);
            sb.AppendLine($"{FmtFechaCorta(f.fecha)};{f.nombre};{FmtHora(f.entrada)};{salida};{horas}");
        }

        string nombreSlug = System.Text.RegularExpressions.Regex.Replace(
            (informeActual.nombreEmpleado ?? "todos").ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        string nombreArchivo = $"informe-control-horario_{nombreSlug}_{informeActual.desde}_a_{informeActual.hasta}.csv";
        string ruta = System.IO.Path.Combine(Application.persistentDataPath, nombreArchivo);

        try
        {
            System.IO.File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
            Debug.Log($"Informe exportado a: {ruta}");
        }
        catch (Exception e)
        {
            Debug.LogError("No se pudo exportar el informe: " + e.Message);
        }
    }

    // ── Helpers de fecha/hora/formato ──
    private static DateTime ParseFechaHora(string fechaHora)
    {
        string[] formatos = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm" };
        if (DateTime.TryParseExact(fechaHora, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
            return f;
        return DateTime.Parse(fechaHora, CultureInfo.InvariantCulture);
    }

    private static bool EsFechaValida(string fechaStr) =>
        DateTime.TryParseExact(fechaStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static string FmtHora(string fechaHora)
    {
        if (string.IsNullOrEmpty(fechaHora)) return "-";
        var partes = fechaHora.Split(' ');
        return partes.Length > 1 ? partes[1].Substring(0, Math.Min(5, partes[1].Length)) : fechaHora;
    }

    private static readonly string[] DIAS_CORTOS = { "dom", "lun", "mar", "mie", "jue", "vie", "sab" };

    private static string FmtFechaCorta(string fechaStr)
    {
        var partes = fechaStr.Split('-');
        var fecha = DateTime.ParseExact(fechaStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"{DIAS_CORTOS[(int)fecha.DayOfWeek]} {partes[2]}/{partes[1]}/{partes[0]}";
    }

    private static string FmtFechaLarga(string fechaStr)
    {
        var fecha = DateTime.ParseExact(fechaStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var ci = new CultureInfo("es-ES");
        string texto = fecha.ToString("dddd, d 'de' MMMM 'de' yyyy", ci);
        return char.ToUpper(texto[0]) + texto.Substring(1);
    }

    private static string FmtHoras(double h)
    {
        if (double.IsNaN(h)) return "-";
        int horas = (int)Math.Floor(h);
        int min = (int)Math.Round((h - horas) * 60);
        if (min == 60) { horas += 1; min = 0; }
        return $"{horas}h {min:00}m";
    }
}