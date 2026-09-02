using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[Serializable]
public class Reserva
{
    public int id;
    public string nombre;
    public string telefono;
    public string personas;
    public string fecha;
    public string hora;
    public string notas;
    public string estado;
}

[Serializable]
public class ReservasResponse
{
    public Reserva[] reservas;
}

[Serializable]
public class ErrorResponse
{
    public string error;
}

[Serializable]
public class ReservaNueva
{
    public string restaurant_id;
    public string nombre;
    public string telefono;
    public string personas;
    public string fecha;
    public string hora;
    public string notas;
    public string origen;
    public string estado;
}

[Serializable]
public class ReservaEdicion
{
    public string nombre;
    public string telefono;
    public string personas;
    public string fecha;
    public string hora;
    public string notas;
}

[Serializable]
public class Tramo
{
    public string hora_inicio;
    public string hora_fin;
}

[Serializable]
public class ReservaConfig
{
    public int activo;
    public Tramo[] tramos;
    public int min_personas;
    public int max_personas;
    public int antelacion_minima_horas;
    public int antelacion_maxima_dias;
    public string dias_cerrados;
}

public class ReservasController : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private OnScreenKeyboardController keyboard;
    [SerializeField] private UIToolkitKeyboardBridge keyboardBridge;
    [SerializeField] private string apiBase = "https://tu-api.com";
    [SerializeField] private string restaurantId = "1";

    [SerializeField] private DataBasePersonalizacion dbPersonalizacion;
    private Color colorPpalBotones = new Color(245f / 255f, 168f / 255f, 60f / 255f); // fallback = orange original
    private Color colorSecBotones = new Color(164f / 255f, 35f / 255f, 63f / 255f);   // fallback = maroon original

    private static readonly string[] Meses =
    {
        "ene", "feb", "mar", "abr", "may", "jun",
        "jul", "ago", "sep", "oct", "nov", "dic"
    };

    private static readonly string[] MesesLargos =
    {
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    };
    private static readonly string[] DiasSemana = { "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB", "DOM" };

    private ListView listView;
    private Reserva[] reservasActuales = Array.Empty<Reserva>();
    private Reserva[] reservasFiltradas = Array.Empty<Reserva>();
    private string filtroEstado = "todas";
    private string filtroTexto = "";
    private int reservaSeleccionadaId = -1;
    private bool confirmandoCancelacion = false;
    private Coroutine confirmCancelCoroutine;
    private readonly Dictionary<int, float> confirmacionCancelarFilas = new Dictionary<int, float>();
    private const float VentanaConfirmacionCancelarFila = 3f;
    private int reservaEditandoId = -1;
    private Reserva reservaSeleccionada;
    private int personasModal = 2;
    private DateTime modalFecha = DateTime.Today;
    private string modalHora = "";
    private ReservaConfig reservaConfig;
    private DateTime calMesVisible;
    private DateTime fechaActual = DateTime.Today;
    private bool vistaPendientes = true;
    private Coroutine toastCoroutine;
    private Coroutine autoRefreshCoroutine;
    private int solicitudActual = 0;
    private const float IntervaloAutoRefresco = 20f;

    void OnEnable()
    {
        vistaPendientes = true;
        var root = document.rootVisualElement;
        CargarColoresPersonalizacion();
        if (dbPersonalizacion != null)
            dbPersonalizacion.OnDataLoaded += OnPersonalizacionCargada;
        listView = root.Q<ListView>("reservas-list");
        listView.selectionType = SelectionType.None;

        listView.makeItem = () =>
        {
            var row = new VisualElement();
            row.focusable = false;
            row.AddToClassList("reserva-row");

            var hora = new Label { name = "hora" }; hora.AddToClassList("col-hora"); row.Add(hora);
            var fecha = new Label { name = "fecha" }; fecha.AddToClassList("col-fecha"); row.Add(fecha);
            var nombre = new Label { name = "nombre" }; nombre.AddToClassList("col-nombre"); row.Add(nombre);
            var personas = new Label { name = "personas" }; personas.AddToClassList("col-personas"); row.Add(personas);
            var estado = new Label { name = "estado" }; estado.AddToClassList("col-estado"); row.Add(estado);

            var acciones = new VisualElement { name = "acciones-rapidas" };
            acciones.AddToClassList("row-acciones-rapidas");

            var btnOk = new Button { name = "accion-confirmar", text = "✓" };
            btnOk.focusable = false;
            btnOk.AddToClassList("btn-accion-rapida");
            btnOk.AddToClassList("btn-accion-confirmar");

            var btnNo = new Button { name = "accion-cancelar", text = "✕" };
            btnNo.focusable = false;
            btnNo.AddToClassList("btn-accion-rapida");
            btnNo.AddToClassList("btn-accion-cancelar");

            acciones.Add(btnOk);
            acciones.Add(btnNo);
            row.Add(acciones);

            btnOk.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                if (row.userData is Reserva rOk) _ = CambiarEstadoReserva(rOk.id, "confirmada");
            });
            btnNo.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                if (!(row.userData is Reserva rNo)) return;

                if (confirmacionCancelarFilas.TryGetValue(rNo.id, out var vence) && Time.time < vence)
                {
                    confirmacionCancelarFilas.Remove(rNo.id);
                    _ = CambiarEstadoReserva(rNo.id, "cancelada");
                    return;
                }

                confirmacionCancelarFilas[rNo.id] = Time.time + VentanaConfirmacionCancelarFila;
                listView.RefreshItems();
                StartCoroutine(RevertirFilaCancelarTrasDelay(rNo.id));
            });

            row.RegisterCallback<ClickEvent>(evt =>
            {
                if (row.userData is Reserva rSel) MostrarDetalle(rSel);
            });
            return row;
        };

        listView.bindItem = (element, i) =>
        {
            var r = reservasFiltradas[i];
            element.userData = r;
            element.Q<Label>("hora").text = r.hora;
            element.Q<Label>("nombre").text = r.nombre;
            element.Q<Label>("personas").text = r.personas;

            var fechaLabel = element.Q<Label>("fecha");
            fechaLabel.text = FormatearFechaCorta(r.fecha);
            fechaLabel.style.display = vistaPendientes ? DisplayStyle.Flex : DisplayStyle.None;

            var estadoLabel = element.Q<Label>("estado");
            estadoLabel.text = r.estado == "confirmada" ? "Confirmada" : (r.estado == "cancelada" ? "Cancelada" : "Pendiente");
            estadoLabel.AddToClassList("estado-badge");
            estadoLabel.RemoveFromClassList("estado--confirmada");
            estadoLabel.RemoveFromClassList("estado--pendiente");
            estadoLabel.RemoveFromClassList("estado--cancelada");
            estadoLabel.AddToClassList(r.estado == "confirmada" ? "estado--confirmada" : (r.estado == "cancelada" ? "estado--cancelada" : "estado--pendiente"));

            element.RemoveFromClassList("reserva-row--pendiente");
            element.RemoveFromClassList("reserva-row--confirmada");
            element.RemoveFromClassList("reserva-row--cancelada");
            element.AddToClassList(r.estado == "confirmada" ? "reserva-row--confirmada" : (r.estado == "cancelada" ? "reserva-row--cancelada" : "reserva-row--pendiente"));

            var esPendiente = r.estado == "pendiente";
            element.Q<Button>("accion-confirmar").style.visibility = esPendiente ? Visibility.Visible : Visibility.Hidden;

            var btnCancelarFila = element.Q<Button>("accion-cancelar");
            btnCancelarFila.style.visibility = esPendiente ? Visibility.Visible : Visibility.Hidden;

            bool pendienteConfirmarCancelacion = confirmacionCancelarFilas.TryGetValue(r.id, out var venceFila) && Time.time < venceFila;
            if (pendienteConfirmarCancelacion)
            {
                btnCancelarFila.style.backgroundColor = new Color(190f / 255f, 40f / 255f, 40f / 255f);
                btnCancelarFila.style.color = Color.white;
            }
            else
            {
                btnCancelarFila.style.backgroundColor = new Color(255f / 255f, 225f / 255f, 225f / 255f);
                btnCancelarFila.style.color = new Color(190f / 255f, 40f / 255f, 40f / 255f);
            }
        };

        listView.Q<ScrollView>().touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
        HabilitarArrastreScroll(listView.Q<ScrollView>());

        root.Q<Button>("btn-tab-pendientes").clicked += () => CambiarVista(true);
        root.Q<Button>("btn-tab-fecha").clicked += () => CambiarVista(false);

        root.Q<Button>("btn-dia-anterior").clicked += () => CambiarDia(-1);
        root.Q<Button>("btn-dia-siguiente").clicked += () => CambiarDia(1);

        ConfigurarPlaceholderBusqueda(root);

        ConfigurarChips(root);

        root.Q<Button>("btn-nueva").clicked += ShowModal;

        keyboardBridge.Bind(root.Q<TextField>("modal-nombre"));
        root.Q<TextField>("modal-nombre").RegisterValueChangedCallback(evt =>
            root.Q<TextField>("modal-nombre").RemoveFromClassList("campo-error"));
        keyboardBridge.Bind(root.Q<TextField>("modal-telefono"));
        keyboardBridge.Bind(root.Q<TextField>("modal-notas"));
        ConfigurarPersonasStepper(root);

        ConfigurarFechaPicker(root);
        ConfigurarHoraPicker(root);
        _ = CargarConfigReservas();

        root.Q<Button>("modal-cancelar").clicked += HideModal;
        root.Q<Button>("modal-guardar").clicked += () => _ = CrearReserva();
        root.Q<VisualElement>("modal-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("modal-overlay")) HideModal();
        });

        root.Q<Button>("detail-close").clicked += OcultarDetalle;
        root.Q<VisualElement>("detail-modal-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("detail-modal-overlay")) OcultarDetalle();
        });

        root.Q<Button>("btn-editar").clicked += () =>
        {
            if (reservaSeleccionada != null) AbrirEdicion(reservaSeleccionada);
        };

        root.Q<Button>("btn-confirmar").clicked += () =>
        {
            if (reservaSeleccionadaId != -1) _ = CambiarEstadoReserva(reservaSeleccionadaId, "confirmada");
        };
        root.Q<Button>("btn-cancelar").clicked += () => ManejarClickCancelar();

        ActualizarLabelFecha();
        ActualizarTabsActivos(root);
        root.Q<VisualElement>("date-nav-wrap").style.display = DisplayStyle.None;
        root.Q<VisualElement>("filtro-chips-wrap").style.display = DisplayStyle.None;

        _ = CargarPendientes();

        FijarColoresBotones(root);

        autoRefreshCoroutine = StartCoroutine(AutoRefrescoPeriodico());
    }

    void OnDisable()
    {
        if (autoRefreshCoroutine != null) StopCoroutine(autoRefreshCoroutine);
    }

    IEnumerator AutoRefrescoPeriodico()
    {
        var espera = new WaitForSeconds(IntervaloAutoRefresco);
        while (true)
        {
            yield return espera;
            _ = RefrescarSilencioso();
        }
    }

    async Task RefrescarSilencioso()
    {
        int miSolicitud = ++solicitudActual;
        Reserva[] nuevas = vistaPendientes
            ? await ObtenerPendientesGlobal()
            : await ObtenerReservasDeFecha(fechaActual);

        if (miSolicitud != solicitudActual) return;

        reservasActuales = nuevas;
        if (vistaPendientes) ActualizarBadgePendientes(reservasActuales.Length);
        else _ = ActualizarBadgePendientesSilencioso();
        ActualizarStats();
        AplicarFiltros();
    }

    private const string PlaceholderBusqueda = "Buscar por nombre...";

    void ConfigurarPlaceholderBusqueda(VisualElement root)
    {
        var campo = root.Q<TextField>("search-field");
        var colorPlaceholder = new Color(150f / 255f, 150f / 255f, 155f / 255f);
        var colorNormal = new Color(40f / 255f, 40f / 255f, 44f / 255f);

        void MostrarPlaceholder()
        {
            campo.SetValueWithoutNotify(PlaceholderBusqueda);
            campo.style.color = colorPlaceholder;
        }

        void QuitarPlaceholderSiCorresponde()
        {
            if (campo.value == PlaceholderBusqueda)
            {
                campo.SetValueWithoutNotify("");
                campo.style.color = colorNormal;
            }
        }

        MostrarPlaceholder();
        campo.RegisterCallback<FocusInEvent>(evt => QuitarPlaceholderSiCorresponde());
        campo.RegisterCallback<FocusOutEvent>(evt =>
        {
            if (string.IsNullOrEmpty(campo.value)) MostrarPlaceholder();
        });
        campo.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == PlaceholderBusqueda) return;
            filtroTexto = evt.newValue;
            AplicarFiltros();
        });
    }

    void CargarColoresPersonalizacion()
    {
        if (dbPersonalizacion == null || !dbPersonalizacion.IsLoaded) return;

        if (DataBasePersonalizacion.col_ppal_botones != null && DataBasePersonalizacion.col_ppal_botones.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out var cPpal))
            colorPpalBotones = cPpal;

        if (DataBasePersonalizacion.col_sec_botones != null && DataBasePersonalizacion.col_sec_botones.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_botones[0], out var cSec))
            colorSecBotones = cSec;
    }

    void OnPersonalizacionCargada()
    {
        CargarColoresPersonalizacion();
        var root = document.rootVisualElement;
        FijarColoresBotones(root);
        ActualizarTabsActivos(root);
        PoblarHoraPicker();
    }

    void FijarColoresBotones(VisualElement root)
    {
        void Fijar(string nombre, Color color)
        {
            var el = root.Q<VisualElement>(nombre);
            if (el != null) el.style.backgroundColor = color;
        }

        Fijar("btn-nueva", colorSecBotones);
        Fijar("modal-guardar", colorPpalBotones);
        Fijar("modal-cancelar", new Color(240f / 255f, 240f / 255f, 242f / 255f));
        Fijar("btn-confirmar", new Color(30f / 255f, 130f / 255f, 76f / 255f));
        Fijar("btn-cancelar", Color.white);
        Fijar("detail-close", new Color(240f / 255f, 240f / 255f, 242f / 255f));
        Fijar("chip-todas", colorPpalBotones);
        Fijar("chip-confirmadas", Color.white);
        Fijar("chip-pendientes", Color.white);

        var nuevaReservaBox = root.Q<VisualElement>("nueva-reserva-modal-box");
        if (nuevaReservaBox != null)
            nuevaReservaBox.style.borderLeftColor = colorSecBotones;
    }

    void HabilitarArrastreScroll(ScrollView scrollView)
    {
        var contenedor = scrollView.contentContainer;
        Vector2 inicioPuntero = Vector2.zero;
        Vector2 inicioScroll = Vector2.zero;
        bool arrastrando = false;
        bool umbralSuperado = false;

        void Detener()
        {
            arrastrando = false;
            umbralSuperado = false;
        }

        contenedor.RegisterCallback<PointerDownEvent>(evt =>
        {
            inicioPuntero = evt.position;
            inicioScroll = scrollView.scrollOffset;
            arrastrando = true;
            umbralSuperado = false;
        });

        contenedor.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!arrastrando || evt.pressedButtons == 0)
            {
                Detener();
                return;
            }
            Vector2 delta = (Vector2)evt.position - inicioPuntero;
            if (!umbralSuperado && delta.magnitude > 8f)
            {
                umbralSuperado = true;
                contenedor.CapturePointer(evt.pointerId);
            }
            if (umbralSuperado)
            {
                var nuevoOffset = inicioScroll - delta;
                float maxY = Mathf.Max(0, scrollView.contentContainer.layout.height - scrollView.contentViewport.layout.height);
                nuevoOffset.y = Mathf.Clamp(nuevoOffset.y, 0, maxY);
                nuevoOffset.x = 0;
                scrollView.scrollOffset = nuevoOffset;
            }
        });

        contenedor.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (umbralSuperado) contenedor.ReleasePointer(evt.pointerId);
            Detener();
        });

        contenedor.RegisterCallback<PointerCaptureOutEvent>(evt => Detener());
    }

    // ---------- View switching (new) ----------

    void CambiarVista(bool pendientes)
    {
        vistaPendientes = pendientes;
        var root = document.rootVisualElement;

        if (pendientes) filtroEstado = "todas";

        root.Q<VisualElement>("date-nav-wrap").style.display = pendientes ? DisplayStyle.None : DisplayStyle.Flex;
        root.Q<VisualElement>("filtro-chips-wrap").style.display = pendientes ? DisplayStyle.None : DisplayStyle.Flex;
        ActualizarTabsActivos(root);
        root.Q<Label>("col-fecha-header").style.display = pendientes ? DisplayStyle.Flex : DisplayStyle.None;

        if (pendientes)
        {
            _ = CargarPendientes();
        }
        else
        {
            _ = CargarReservas(fechaActual);
            _ = ActualizarBadgePendientesSilencioso();
        }
    }

    void ActualizarTabsActivos(VisualElement root)
    {
        var btnPend = root.Q<Button>("btn-tab-pendientes");
        var btnFecha = root.Q<Button>("btn-tab-fecha");
        var activo = colorPpalBotones;
        var inactivo = Color.white;

        if (vistaPendientes)
        {
            btnPend.AddToClassList("segment--active");
            btnFecha.RemoveFromClassList("segment--active");
            btnPend.style.backgroundColor = activo;
            btnFecha.style.backgroundColor = inactivo;
        }
        else
        {
            btnFecha.AddToClassList("segment--active");
            btnPend.RemoveFromClassList("segment--active");
            btnFecha.style.backgroundColor = activo;
            btnPend.style.backgroundColor = inactivo;
        }
    }

    // ---------- Date navigation (for "Por fecha") ----------

    void CambiarDia(int delta)
    {
        fechaActual = fechaActual.AddDays(delta);
        ActualizarLabelFecha();
        _ = CargarReservas(fechaActual);
    }

    void ActualizarLabelFecha()
    {
        var root = document.rootVisualElement;
        var label = root.Q<Label>("label-fecha");
        if (fechaActual.Date == DateTime.Today) label.text = "Hoy";
        else if (fechaActual.Date == DateTime.Today.AddDays(1)) label.text = "Mañana";
        else label.text = FormatearFechaCorta(fechaActual.ToString("yyyy-MM-dd"));
    }

    string FormatearFechaCorta(string fechaIso)
    {
        if (DateTime.TryParseExact(fechaIso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
            return $"{f.Day:00} {Meses[f.Month - 1]}";
        return fechaIso;
    }

    // ---------- Data fetching ----------

    async Task<Reserva[]> ObtenerReservasDeFecha(DateTime fecha)
    {
        string url = $"{apiBase}/reservas?restaurant_id={restaurantId}&fecha={fecha:yyyy-MM-dd}";
        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            return Array.Empty<Reserva>();
        }

        var data = JsonUtility.FromJson<ReservasResponse>(req.downloadHandler.text);
        return data.reservas ?? Array.Empty<Reserva>();
    }

    async Task<Reserva[]> ObtenerPendientesGlobal()
    {
        string url = $"{apiBase}/reservas/pendientes?restaurant_id={restaurantId}";
        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            return Array.Empty<Reserva>();
        }

        var data = JsonUtility.FromJson<ReservasResponse>(req.downloadHandler.text);
        return data.reservas ?? Array.Empty<Reserva>();
    }

    async Task CargarReservas(DateTime fecha)
    {
        fechaActual = fecha;
        int miSolicitud = ++solicitudActual;
        var root = document.rootVisualElement;
        var listState = root.Q<Label>("list-state");
        listView.style.display = DisplayStyle.None;
        listState.style.display = DisplayStyle.Flex;
        listState.text = "Cargando reservas...";

        var nuevas = await ObtenerReservasDeFecha(fecha);
        if (miSolicitud != solicitudActual) return;

        reservasActuales = nuevas;
        ActualizarStats();
        AplicarFiltros();
    }

    async Task CargarPendientes()
    {
        int miSolicitud = ++solicitudActual;
        var root = document.rootVisualElement;
        var listState = root.Q<Label>("list-state");
        listView.style.display = DisplayStyle.None;
        listState.style.display = DisplayStyle.Flex;
        listState.text = "Cargando pendientes...";

        var nuevas = await ObtenerPendientesGlobal();
        if (miSolicitud != solicitudActual) return;

        reservasActuales = nuevas;
        ActualizarBadgePendientes(reservasActuales.Length);
        ActualizarStats();
        AplicarFiltros();
    }

    async Task ActualizarBadgePendientesSilencioso()
    {
        var pendientes = await ObtenerPendientesGlobal();
        ActualizarBadgePendientes(pendientes.Length);
    }

    void ActualizarBadgePendientes(int total)
    {
        var boton = document.rootVisualElement.Q<Button>("btn-tab-pendientes");
        boton.text = total > 0 ? $"Pendientes ({total})" : "Pendientes";
    }

    void AplicarFiltros()
    {
        IEnumerable<Reserva> query = reservasActuales;

        if (filtroEstado != "todas")
            query = query.Where(r => r.estado == filtroEstado);

        if (!string.IsNullOrEmpty(filtroTexto))
            query = query.Where(r => !string.IsNullOrEmpty(r.nombre) && r.nombre.ToLower().Contains(filtroTexto.ToLower()));

        reservasFiltradas = query
            .OrderBy(r => r.estado == "pendiente" ? 0 : (r.estado == "confirmada" ? 1 : 2))
            .ThenBy(r => r.hora)
            .ToArray();
        listView.itemsSource = reservasFiltradas;
        listView.RefreshItems();

        var root = document.rootVisualElement;
        var listState = root.Q<Label>("list-state");
        if (reservasFiltradas.Length == 0)
        {
            listState.text = vistaPendientes
                ? "No hay reservas pendientes. Todo al día."
                : "No hay reservas que coincidan con este filtro";
            listState.style.display = DisplayStyle.Flex;
            listView.style.display = DisplayStyle.None;
        }
        else
        {
            listState.style.display = DisplayStyle.None;
            listView.style.display = DisplayStyle.Flex;
        }
    }

    void ActualizarStats()
    {
        var root = document.rootVisualElement;
        var proximaLabel = root.Q<Label>("stat-proxima");

        if (vistaPendientes)
        {
            int total = reservasActuales.Length;
            root.Q<Label>("stat-total").text = total == 1 ? "1 reserva pendiente" : $"{total} reservas pendientes";

            if (total > 0)
            {
                var primera = reservasActuales[0];
                proximaLabel.text = $"Más urgente: {FormatearFechaCorta(primera.fecha)} · {primera.hora} · {primera.nombre}";
                proximaLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                proximaLabel.style.display = DisplayStyle.None;
            }
            return;
        }

        int totalDia = reservasActuales.Length;
        string etiquetaDia = fechaActual.Date == DateTime.Today ? "hoy" : (fechaActual.Date == DateTime.Today.AddDays(1) ? "mañana" : FormatearFechaCorta(fechaActual.ToString("yyyy-MM-dd")));
        root.Q<Label>("stat-total").text = $"{totalDia} reservas {etiquetaDia}";

        var ahora = DateTime.Now.TimeOfDay;
        Reserva proxima = null;
        double mejorDelta = double.MaxValue;
        foreach (var r in reservasActuales)
        {
            if (r.estado == "cancelada") continue;
            if (!TimeSpan.TryParse(r.hora, out var t)) continue;
            var delta = (t - ahora).TotalMinutes;
            if (delta >= 0 && delta < mejorDelta)
            {
                mejorDelta = delta;
                proxima = r;
            }
        }

        if (proxima != null && fechaActual.Date == DateTime.Today)
        {
            proximaLabel.text = $"Próxima: {proxima.hora} · {proxima.nombre}";
            proximaLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            proximaLabel.style.display = DisplayStyle.None;
        }
    }

    void ConfigurarChips(VisualElement root)
    {
        var chipTodas = root.Q<Button>("chip-todas");
        var chipConfirmadas = root.Q<Button>("chip-confirmadas");
        var chipPendientes = root.Q<Button>("chip-pendientes");

        chipTodas.clicked += () => SetFiltroEstado("todas", chipTodas, chipConfirmadas, chipPendientes);
        chipConfirmadas.clicked += () => SetFiltroEstado("confirmada", chipConfirmadas, chipTodas, chipPendientes);
        chipPendientes.clicked += () => SetFiltroEstado("pendiente", chipPendientes, chipTodas, chipConfirmadas);
    }

    void ConfigurarPersonasStepper(VisualElement root)
    {
        root.Q<Button>("personas-menos").clicked += () =>
        {
            int min = reservaConfig?.min_personas ?? 1;
            if (personasModal > min) personasModal--;
            ActualizarPersonasStepperUI();
        };

        root.Q<Button>("personas-mas").clicked += () =>
        {
            int max = reservaConfig?.max_personas ?? 20;
            if (personasModal < max) personasModal++;
            ActualizarPersonasStepperUI();
        };
    }

    void ActualizarPersonasStepperUI()
    {
        var root = document.rootVisualElement;
        root.Q<Label>("personas-valor").text = personasModal.ToString();
        int min = reservaConfig?.min_personas ?? 1;
        int max = reservaConfig?.max_personas ?? 20;
        root.Q<Button>("personas-menos").SetEnabled(personasModal > min);
        root.Q<Button>("personas-mas").SetEnabled(personasModal < max);
    }

    void ConfigurarFechaPicker(VisualElement root)
    {
        var diasSemanaRow = root.Q<VisualElement>("cal-dias-semana");
        foreach (var dia in DiasSemana)
        {
            var lbl = new Label(dia);
            lbl.AddToClassList("cal-dia-semana-label");
            diasSemanaRow.Add(lbl);
        }

        root.Q<Button>("modal-fecha-btn").clicked += () =>
        {
            calMesVisible = new DateTime(modalFecha.Year, modalFecha.Month, 1);
            RenderCalendarioModal(root);
            root.Q<VisualElement>("date-picker-overlay").style.display = DisplayStyle.Flex;
        };

        root.Q<Button>("cal-mes-anterior").clicked += () => { calMesVisible = calMesVisible.AddMonths(-1); RenderCalendarioModal(root); };
        root.Q<Button>("cal-mes-siguiente").clicked += () => { calMesVisible = calMesVisible.AddMonths(1); RenderCalendarioModal(root); };

        root.Q<VisualElement>("date-picker-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("date-picker-overlay"))
                root.Q<VisualElement>("date-picker-overlay").style.display = DisplayStyle.None;
        });
    }

    void RenderCalendarioModal(VisualElement root)
    {
        root.Q<Label>("cal-mes-label").text = $"{MesesLargos[calMesVisible.Month - 1]} de {calMesVisible.Year}";

        var grid = root.Q<VisualElement>("cal-grid-fechas");
        grid.Clear();

        var minFecha = DateTime.Today;
        var maxFecha = reservaConfig != null ? DateTime.Today.AddDays(reservaConfig.antelacion_maxima_dias) : DateTime.Today.AddDays(30);
        var diasCerrados = reservaConfig != null && !string.IsNullOrEmpty(reservaConfig.dias_cerrados)
            ? reservaConfig.dias_cerrados.Split(',').Select(s => s.Trim()).ToArray()
            : Array.Empty<string>();

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

            string diaSemanaIso = ((int)fecha.DayOfWeek == 0 ? 7 : (int)fecha.DayOfWeek).ToString();
            bool cerrado = diasCerrados.Contains(diaSemanaIso);
            bool fueraDeRango = fecha < minFecha || fecha > maxFecha;

            if (cerrado || fueraDeRango) btn.SetEnabled(false);
            else btn.clicked += () => SeleccionarFechaModal(fecha, root);

            if (fecha.Date == modalFecha.Date)
            {
                btn.AddToClassList("cal-day-chip--selected");
                btn.style.backgroundColor = colorPpalBotones;
            }

            grid.Add(btn);
        }

        root.Q<Button>("cal-mes-anterior").SetEnabled(new DateTime(calMesVisible.Year, calMesVisible.Month, 1) > new DateTime(minFecha.Year, minFecha.Month, 1));
        root.Q<Button>("cal-mes-siguiente").SetEnabled(calMesVisible.Year != maxFecha.Year || calMesVisible.Month != maxFecha.Month);
    }

    void SeleccionarFechaModal(DateTime fecha, VisualElement root)
    {
        modalFecha = fecha;
        ActualizarLabelFechaModal();
        root.Q<VisualElement>("date-picker-overlay").style.display = DisplayStyle.None;
    }

    void ActualizarLabelFechaModal()
    {
        var btn = document.rootVisualElement.Q<Button>("modal-fecha-btn");
        if (modalFecha.Date == DateTime.Today) btn.text = "Hoy";
        else if (modalFecha.Date == DateTime.Today.AddDays(1)) btn.text = "Mañana";
        else btn.text = $"{modalFecha:dd/MM/yyyy}";
    }

    void ConfigurarHoraPicker(VisualElement root)
    {
        var horaScroll = root.Q<ScrollView>("hora-picker-scroll");
        horaScroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
        HabilitarArrastreScroll(horaScroll);

        root.Q<Button>("modal-hora-btn").clicked += () =>
        {
            root.Q<VisualElement>("hora-picker-overlay").style.display = DisplayStyle.Flex;
        };

        root.Q<VisualElement>("hora-picker-overlay").RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == root.Q<VisualElement>("hora-picker-overlay"))
                root.Q<VisualElement>("hora-picker-overlay").style.display = DisplayStyle.None;
        });
    }

    async Task CargarConfigReservas()
    {
        string url = $"{apiBase}/reservas/config?restaurant_id={restaurantId}";
        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo cargar la configuración de horarios: " + req.error);
            reservaConfig = ConfigPorDefecto();
        }
        else
        {
            reservaConfig = JsonUtility.FromJson<ReservaConfig>(req.downloadHandler.text);
            if (reservaConfig == null || reservaConfig.tramos == null || reservaConfig.tramos.Length == 0)
                reservaConfig = ConfigPorDefecto();
        }

        PoblarHoraPicker();
    }

    ReservaConfig ConfigPorDefecto()
    {
        return new ReservaConfig
        {
            activo = 1,
            tramos = new[] { new Tramo { hora_inicio = "13:00", hora_fin = "23:30" } },
            min_personas = 1,
            max_personas = 10,
            antelacion_minima_horas = 0,
            antelacion_maxima_dias = 30,
            dias_cerrados = ""
        };
    }

    void PoblarHoraPicker()
    {
        var root = document.rootVisualElement;
        var grid = root.Q<VisualElement>("hora-picker-grid");
        grid.Clear();

        if (reservaConfig?.tramos == null) return;

        foreach (var tramo in reservaConfig.tramos)
        {
            if (!TimeSpan.TryParse(tramo.hora_inicio, out var inicio)) continue;
            if (!TimeSpan.TryParse(tramo.hora_fin, out var fin)) continue;

            for (var t = inicio; t <= fin; t += TimeSpan.FromMinutes(15))
            {
                string hora = $"{(int)t.TotalHours:00}:{t.Minutes:00}";
                var chip = new Button { text = hora, name = $"hora-chip-{hora}" };
                chip.AddToClassList("hora-chip");
                chip.clicked += () => SeleccionarHora(hora);
                grid.Add(chip);
            }
        }

        if (!string.IsNullOrEmpty(modalHora))
        {
            foreach (var chip in grid.Children())
                if (chip is Button b && b.text == modalHora)
                {
                    chip.AddToClassList("hora-chip--active");
                    chip.style.backgroundColor = colorPpalBotones;
                }
        }
    }

    void SeleccionarHora(string hora)
    {
        modalHora = hora;
        var root = document.rootVisualElement;
        root.Q<Button>("modal-hora-btn").text = hora;
        root.Q<Button>("modal-hora-btn").RemoveFromClassList("hora-picker-btn--error");

        foreach (var chip in root.Q<VisualElement>("hora-picker-grid").Children())
        {
            chip.RemoveFromClassList("hora-chip--active");
            chip.style.backgroundColor = StyleKeyword.Null;
            if (chip is Button b && b.text == hora)
            {
                chip.AddToClassList("hora-chip--active");
                chip.style.backgroundColor = colorPpalBotones;
            }
        }

        root.Q<VisualElement>("hora-picker-overlay").style.display = DisplayStyle.None;
    }

    void SetFiltroEstado(string estado, Button activo, Button b1, Button b2)
    {
        filtroEstado = estado;
        activo.AddToClassList("chip--active");
        b1.RemoveFromClassList("chip--active");
        b2.RemoveFromClassList("chip--active");

        var colorActivo = colorPpalBotones;
        activo.style.backgroundColor = colorActivo;
        b1.style.backgroundColor = Color.white;
        b2.style.backgroundColor = Color.white;

        AplicarFiltros();
    }

    void MostrarDetalle(Reserva r)
    {
        reservaSeleccionadaId = r.id;
        reservaSeleccionada = r;
        var root = document.rootVisualElement;
        root.Q<VisualElement>("detail-modal-overlay").style.display = DisplayStyle.Flex;

        root.Q<Label>("detail-nombre").text = r.nombre;
        root.Q<Label>("detail-fecha-hora").text = $"{r.fecha}  ·  {r.hora}";
        root.Q<Label>("detail-personas").text = $"{r.personas} personas";
        root.Q<Label>("detail-telefono").text = r.telefono;
        root.Q<Label>("detail-notas").text = string.IsNullOrEmpty(r.notas) ? "Sin notas" : r.notas;

        var estadoLabel = root.Q<Label>("detail-estado");
        estadoLabel.text = r.estado == "confirmada" ? "Confirmada" : (r.estado == "cancelada" ? "Cancelada" : "Pendiente");
        estadoLabel.RemoveFromClassList("estado--confirmada");
        estadoLabel.RemoveFromClassList("estado--pendiente");
        estadoLabel.RemoveFromClassList("estado--cancelada");
        estadoLabel.AddToClassList(r.estado == "confirmada" ? "estado--confirmada" : (r.estado == "cancelada" ? "estado--cancelada" : "estado--pendiente"));

        var urgenciaLabel = root.Q<Label>("detail-urgencia");
        if (r.fecha == DateTime.Today.ToString("yyyy-MM-dd") && TimeSpan.TryParse(r.hora, out var horaTs))
        {
            var minutosRestantes = (horaTs - DateTime.Now.TimeOfDay).TotalMinutes;
            if (minutosRestantes > 0 && minutosRestantes <= 30 && r.estado != "cancelada")
            {
                urgenciaLabel.text = $"Empieza en {(int)minutosRestantes} min";
                urgenciaLabel.style.display = DisplayStyle.Flex;
            }
            else urgenciaLabel.style.display = DisplayStyle.None;
        }
        else urgenciaLabel.style.display = DisplayStyle.None;

        bool estaCancelada = r.estado == "cancelada";
        root.Q<Button>("btn-editar").style.display = estaCancelada ? DisplayStyle.None : DisplayStyle.Flex;
        root.Q<Button>("btn-confirmar").style.display = (r.estado == "confirmada" || estaCancelada) ? DisplayStyle.None : DisplayStyle.Flex;
        root.Q<Button>("btn-cancelar").style.display = estaCancelada ? DisplayStyle.None : DisplayStyle.Flex;
        root.Q<Label>("detail-cancelada-msg").style.display = estaCancelada ? DisplayStyle.Flex : DisplayStyle.None;
        RestaurarBotonCancelar();

        listView.RefreshItems();
    }

    void OcultarDetalle()
    {
        reservaSeleccionadaId = -1;
        reservaSeleccionada = null;
        if (confirmCancelCoroutine != null) StopCoroutine(confirmCancelCoroutine);
        RestaurarBotonCancelar();
        document.rootVisualElement.Q<VisualElement>("detail-modal-overlay").style.display = DisplayStyle.None;
        listView.RefreshItems();
    }

    void ShowModal()
    {
        var root = document.rootVisualElement;
        reservaEditandoId = -1;
        root.Q<Label>("modal-titulo").text = "Nueva reserva";
        root.Q<Button>("modal-guardar").text = "Guardar reserva";
        root.Q<VisualElement>("nueva-reserva-modal-box").RemoveFromClassList("nueva-reserva-modal-box--edit");
        root.Q<Label>("modal-subtitulo").style.display = DisplayStyle.None;

        root.Q<TextField>("modal-nombre").value = "";
        root.Q<TextField>("modal-telefono").value = "";
        root.Q<TextField>("modal-notas").value = "";
        personasModal = Math.Max(2, reservaConfig?.min_personas ?? 2);
        ActualizarPersonasStepperUI();

        modalFecha = fechaActual;
        ActualizarLabelFechaModal();

        modalHora = "";
        root.Q<Button>("modal-hora-btn").text = "Elegir hora";
        foreach (var chip in root.Q<VisualElement>("hora-picker-grid").Children())
            chip.RemoveFromClassList("hora-chip--active");

        root.Q<VisualElement>("modal-overlay").style.display = DisplayStyle.Flex;
        var campoNombreFocus = root.Q<TextField>("modal-nombre");
        campoNombreFocus.schedule.Execute(() => campoNombreFocus.Focus());
    }

    void AbrirEdicion(Reserva r)
    {
        OcultarDetalle();

        var root = document.rootVisualElement;
        reservaEditandoId = r.id;
        root.Q<Label>("modal-titulo").text = "Editar reserva";
        root.Q<Button>("modal-guardar").text = "Guardar cambios";
        root.Q<VisualElement>("nueva-reserva-modal-box").AddToClassList("nueva-reserva-modal-box--edit");
        var subtitulo = root.Q<Label>("modal-subtitulo");
        subtitulo.text = $"Editando la reserva de {r.nombre}";
        subtitulo.style.display = DisplayStyle.Flex;

        root.Q<TextField>("modal-nombre").value = r.nombre;
        root.Q<TextField>("modal-telefono").value = r.telefono;
        root.Q<TextField>("modal-notas").value = r.notas;
        personasModal = int.TryParse(r.personas, out var pActual) ? pActual : 2;
        ActualizarPersonasStepperUI();

        modalFecha = DateTime.TryParseExact(r.fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)
            ? f
            : fechaActual;
        ActualizarLabelFechaModal();

        modalHora = r.hora;
        root.Q<Button>("modal-hora-btn").text = string.IsNullOrEmpty(r.hora) ? "Elegir hora" : r.hora;
        foreach (var chip in root.Q<VisualElement>("hora-picker-grid").Children())
        {
            chip.RemoveFromClassList("hora-chip--active");
            if (chip is Button b && b.text == r.hora) chip.AddToClassList("hora-chip--active");
        }

        root.Q<VisualElement>("modal-overlay").style.display = DisplayStyle.Flex;
        var campoNombreFocus2 = root.Q<TextField>("modal-nombre");
        campoNombreFocus2.schedule.Execute(() => campoNombreFocus2.Focus());
    }

    void HideModal()
    {
        var root = document.rootVisualElement;
        root.Q<VisualElement>("modal-overlay").style.display = DisplayStyle.None;
        root.Q<TextField>("modal-nombre").RemoveFromClassList("campo-error");
        root.Q<Button>("modal-hora-btn").RemoveFromClassList("hora-picker-btn--error");
    }

    async Task CrearReserva()
    {
        var root = document.rootVisualElement;
        string nombre = root.Q<TextField>("modal-nombre").value;
        string telefono = root.Q<TextField>("modal-telefono").value;
        string personas = personasModal.ToString();
        string hora = modalHora;
        string notas = root.Q<TextField>("modal-notas").value;
        var fechaSeleccionada = modalFecha;

        var campoNombre = root.Q<TextField>("modal-nombre");
        var botonHora = root.Q<Button>("modal-hora-btn");
        campoNombre.RemoveFromClassList("campo-error");
        botonHora.RemoveFromClassList("hora-picker-btn--error");

        bool faltaNombre = string.IsNullOrWhiteSpace(nombre);
        bool faltaHora = string.IsNullOrWhiteSpace(hora);

        if (faltaNombre) campoNombre.AddToClassList("campo-error");
        if (faltaHora) botonHora.AddToClassList("hora-picker-btn--error");

        if (faltaNombre || faltaHora)
        {
            MostrarToast("Nombre y hora son obligatorios", false);
            return;
        }

        bool esEdicion = reservaEditandoId != -1;
        var btnGuardar = root.Q<Button>("modal-guardar");
        var btnCancelar = root.Q<Button>("modal-cancelar");
        string textoOriginal = btnGuardar.text;
        btnGuardar.text = esEdicion ? "Guardando cambios..." : "Guardando...";
        btnGuardar.SetEnabled(false);
        btnCancelar.SetEnabled(false);

        try
        {
            UnityWebRequest req;

            if (esEdicion)
            {
                var payload = new ReservaEdicion
                {
                    nombre = nombre,
                    telefono = telefono,
                    personas = personas,
                    fecha = fechaSeleccionada.ToString("yyyy-MM-dd"),
                    hora = hora,
                    notas = notas
                };
                string json = JsonUtility.ToJson(payload);
                req = new UnityWebRequest($"{apiBase}/reservas/{reservaEditandoId}", "PATCH");
                req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
            }
            else
            {
                var payload = new ReservaNueva
                {
                    restaurant_id = restaurantId,
                    nombre = nombre,
                    telefono = telefono,
                    personas = personas,
                    fecha = fechaSeleccionada.ToString("yyyy-MM-dd"),
                    hora = hora,
                    notas = notas,
                    origen = "app",
                    estado = "confirmada"
                };
                string json = JsonUtility.ToJson(payload);
                req = new UnityWebRequest($"{apiBase}/reservas", "POST");
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
                    string mensajeError = esEdicion ? "No se pudieron guardar los cambios" : "No se pudo crear la reserva";
                    try
                    {
                        var err = JsonUtility.FromJson<ErrorResponse>(req.downloadHandler.text);
                        if (err != null && !string.IsNullOrEmpty(err.error)) mensajeError = err.error;
                    }
                    catch { /* respuesta no era JSON, usamos el mensaje genérico */ }
                    MostrarToast(mensajeError, false);
                    return;
                }
            }

            MostrarToast(esEdicion ? "Reserva actualizada correctamente" : "Reserva creada correctamente", true);
            reservaEditandoId = -1;
            HideModal();
            ReservasBadgeManager.instance?.ForzarRefresco();

            if (vistaPendientes) await CargarPendientes();
            else
            {
                await CargarReservas(fechaActual);
                _ = ActualizarBadgePendientesSilencioso();
            }
        }
        finally
        {
            btnGuardar.text = textoOriginal;
            btnGuardar.SetEnabled(true);
            btnCancelar.SetEnabled(true);
        }
    }

    void ManejarClickCancelar()
    {
        var btn = document.rootVisualElement.Q<Button>("btn-cancelar");

        if (!confirmandoCancelacion)
        {
            confirmandoCancelacion = true;
            btn.text = "¿Seguro? Toca de nuevo";
            btn.AddToClassList("btn-cancel--confirm");
            btn.style.backgroundColor = new Color(190f / 255f, 40f / 255f, 40f / 255f);
            btn.style.color = Color.white;

            if (confirmCancelCoroutine != null) StopCoroutine(confirmCancelCoroutine);
            confirmCancelCoroutine = StartCoroutine(ResetConfirmacionCancelar());
            return;
        }

        if (confirmCancelCoroutine != null) StopCoroutine(confirmCancelCoroutine);
        RestaurarBotonCancelar();
        if (reservaSeleccionadaId != -1) _ = CambiarEstadoReserva(reservaSeleccionadaId, "cancelada");
    }

    IEnumerator ResetConfirmacionCancelar()
    {
        yield return new WaitForSeconds(3f);
        RestaurarBotonCancelar();
    }

    IEnumerator RevertirFilaCancelarTrasDelay(int reservaId)
    {
        yield return new WaitForSeconds(VentanaConfirmacionCancelarFila);
        if (confirmacionCancelarFilas.TryGetValue(reservaId, out var vence) && vence <= Time.time + 0.01f)
        {
            confirmacionCancelarFilas.Remove(reservaId);
            listView.RefreshItems();
        }
    }

    void RestaurarBotonCancelar()
    {
        confirmandoCancelacion = false;
        var btn = document.rootVisualElement.Q<Button>("btn-cancelar");
        btn.text = "Cancelar";
        btn.RemoveFromClassList("btn-cancel--confirm");
        btn.style.backgroundColor = Color.white;
        btn.style.color = new Color(190f / 255f, 40f / 255f, 40f / 255f);
    }

    async Task CambiarEstadoReserva(int id, string nuevoEstado)
    {
        string url = $"{apiBase}/reservas/{id}";
        string json = $"{{\"estado\":\"{nuevoEstado}\"}}";

        using var req = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            MostrarToast("Error al actualizar la reserva", false);
            return;
        }

        MostrarToast(nuevoEstado == "confirmada" ? "Reserva confirmada" : "Reserva cancelada", true);
        OcultarDetalle();
        ReservasBadgeManager.instance?.ForzarRefresco();

        if (vistaPendientes) await CargarPendientes();
        else
        {
            await CargarReservas(fechaActual);
            _ = ActualizarBadgePendientesSilencioso();
        }
    }

    void MostrarToast(string mensaje, bool exito)
    {
        var toast = document.rootVisualElement.Q<Label>("toast");
        toast.text = mensaje;
        toast.RemoveFromClassList("toast--success");
        toast.RemoveFromClassList("toast--error");
        toast.AddToClassList(exito ? "toast--success" : "toast--error");
        toast.style.display = DisplayStyle.Flex;

        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastCoroutine = StartCoroutine(OcultarToastTrasDelay());
    }

    IEnumerator OcultarToastTrasDelay()
    {
        yield return new WaitForSeconds(3f);
        document.rootVisualElement.Q<Label>("toast").style.display = DisplayStyle.None;
    }
}