using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;

[Serializable] public class RestauranteInfo { public string nombre_rest; }
[Serializable] public class RestaurantesWrapper { public RestauranteInfo[] items; }
[Serializable] public class VerifyResponse { public bool ok; public string nombre; public bool aviso_aceptado; }
[Serializable] public class UltimoFichajeResponse { public bool ok; public string ultimo_tipo; }
[Serializable] public class FichajeAddResponse { public string empleado; public string fecha_hora; }
[Serializable] public class VerifyRequest { public string id; public string codigo; }
[Serializable] public class IncidenciaCheckResponse { public bool ok; public bool pendiente; public string fecha_entrada; }
[Serializable] public class ResolverIncidenciaRequest { public string id; public string codigo; public string hora_propuesta; }
[Serializable] public class FichajeRequest { public string id; public string codigo; public string tipo; }

[RequireComponent(typeof(UIDocument))]
public class FichajeController : MonoBehaviour
{
    [SerializeField] private string apiBase = "https://gastrali.tail634a78.ts.net";
    private const int MAX_PIN_LENGTH = 6;
    private const float AUTO_RETURN_SECONDS = 5f;

    private UIDocument doc;
    private VisualElement root, screenPin, screenConfirm, screenIncidencia, screenAviso, pinDots;
    private Label restaurantNameLbl, pinErrorLbl, clockLbl;
    private Label incidenciaTextLbl, incidenciaTimeLbl;
    private Button incidenciaConfirmBtn;
    private int incHour, incMin;
    private string incFechaEntrada;
    private Label stampIconLbl, stampTypeLbl, stampEmployeeLbl, stampTimeLbl;
    private Button enterBtn;

    private string pin = "";
    private string empleadoNombre;
    private string restId;
    private Coroutine clockCoroutine;
    private Coroutine returnCoroutine;
    private bool modoDirecto = false;

    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        screenPin = root.Q<VisualElement>("screen-pin");
        screenConfirm = root.Q<VisualElement>("screen-confirm");
        screenIncidencia = root.Q<VisualElement>("screen-incidencia");
        screenAviso = root.Q<VisualElement>("screen-aviso");
        root.Q<Button>("aviso-aceptar").clicked += () => _ = AceptarAvisoYContinuar();

        incidenciaTextLbl = root.Q<Label>("incidencia-text");
        incidenciaTimeLbl = root.Q<Label>("incidencia-time");
        incidenciaConfirmBtn = root.Q<Button>("incidencia-confirm");
        root.Q<Button>("incidencia-hour-up").clicked += () => IncHourStep(1);
        root.Q<Button>("incidencia-hour-down").clicked += () => IncHourStep(-1);
        root.Q<Button>("incidencia-min-up").clicked += () => IncMinStep(15);
        root.Q<Button>("incidencia-min-down").clicked += () => IncMinStep(-15);
        incidenciaConfirmBtn.clicked += () => _ = ConfirmarIncidencia();
        root.Q<Button>("incidencia-skip").clicked += SaltarIncidencia;
        pinDots = root.Q<VisualElement>("pin-dots");

        restaurantNameLbl = root.Q<Label>("restaurant-name");
        pinErrorLbl = root.Q<Label>("pin-error");
        clockLbl = root.Q<Label>("live-clock");
        stampIconLbl = root.Q<Label>("stamp-icon");
        stampTypeLbl = root.Q<Label>("stamp-type");
        stampEmployeeLbl = root.Q<Label>("stamp-employee");
        stampTimeLbl = root.Q<Label>("stamp-time");

        enterBtn = root.Q<Button>("key-enter");

        root.Q<Button>("fichaje-close").clicked += CerrarPanel;
        root.Q<Button>("key-1").clicked += () => AddDigit("1");
        root.Q<Button>("key-2").clicked += () => AddDigit("2");
        root.Q<Button>("key-3").clicked += () => AddDigit("3");
        root.Q<Button>("key-4").clicked += () => AddDigit("4");
        root.Q<Button>("key-5").clicked += () => AddDigit("5");
        root.Q<Button>("key-6").clicked += () => AddDigit("6");
        root.Q<Button>("key-7").clicked += () => AddDigit("7");
        root.Q<Button>("key-8").clicked += () => AddDigit("8");
        root.Q<Button>("key-9").clicked += () => AddDigit("9");
        root.Q<Button>("key-0").clicked += () => AddDigit("0");
        root.Q<Button>("key-del").clicked += DeleteDigit;
        enterBtn.clicked += () => _ = SubmitPin();

        ResetPin();
        if (modoDirecto)
        {
            screenPin.style.display = DisplayStyle.None;
            screenConfirm.style.display = DisplayStyle.None;
            screenIncidencia.style.display = DisplayStyle.None;
        }
        else
        {
            MostrarScreen(screenPin);
        }
        clockCoroutine = StartCoroutine(TickClock());
        _ = InitRestaurant();
    }

    void OnDisable()
    {
        if (clockCoroutine != null) StopCoroutine(clockCoroutine);
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
    }

    void CerrarPanel()
    {
        empleadoNombre = null;
        modoDirecto = false;
        ResetPin();
        MostrarScreen(screenPin);
        gameObject.SetActive(false);
    }

    void MostrarScreen(VisualElement screen)
    {
        screenPin.style.display = DisplayStyle.None;
        screenConfirm.style.display = DisplayStyle.None;
        screenIncidencia.style.display = DisplayStyle.None;
        screenAviso.style.display = DisplayStyle.None;
        screen.style.display = DisplayStyle.Flex;
    }

    IEnumerator TickClock()
    {
        while (true)
        {
            clockLbl.text = DateTime.Now.ToString("HH:mm:ss");
            yield return new WaitForSeconds(1f);
        }
    }

    string ObtenerRestaurantId()
    {
        GameObject go = GameObject.FindGameObjectWithTag("textID");
        return go != null ? go.GetComponent<TMPro.TMP_Text>()?.text : null;
    }

    async Task InitRestaurant()
    {
        restId = ObtenerRestaurantId();
        if (string.IsNullOrEmpty(restId))
        {
            restaurantNameLbl.text = "Restaurante no identificado";
            return;
        }

        using var req = UnityWebRequest.Get($"{apiBase}/personalizacion/restaurant/{restId}");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            restaurantNameLbl.text = "Reloj de fichaje";
            return;
        }

        var wrapped = "{\"items\":" + req.downloadHandler.text + "}";
        var data = JsonUtility.FromJson<RestaurantesWrapper>(wrapped);
        restaurantNameLbl.text = (data.items != null && data.items.Length > 0 && !string.IsNullOrEmpty(data.items[0].nombre_rest))
            ? data.items[0].nombre_rest
            : "Reloj de fichaje";
    }

    // ---------- PIN ----------

    void RenderPinDots()
    {
        pinDots.Clear();
        int len = Mathf.Max(pin.Length, 4);
        for (int i = 0; i < len; i++)
        {
            var dot = new VisualElement();
            dot.AddToClassList("pin-dot");
            if (i < pin.Length) dot.AddToClassList("pin-dot--filled");
            pinDots.Add(dot);
        }
        enterBtn.SetEnabled(pin.Length >= 3);
    }

    void ResetPin()
    {
        pin = "";
        pinErrorLbl.text = "";
        RenderPinDots();
    }

    void AddDigit(string d)
    {
        if (pin.Length >= MAX_PIN_LENGTH) return;
        pin += d;
        pinErrorLbl.text = "";
        RenderPinDots();
    }

    void DeleteDigit()
    {
        if (pin.Length > 0) pin = pin.Substring(0, pin.Length - 1);
        RenderPinDots();
    }

    async Task SubmitPin()
    {
        if (string.IsNullOrEmpty(restId)) restId = ObtenerRestaurantId();

        enterBtn.SetEnabled(false);

        string json = JsonUtility.ToJson(new VerifyRequest { id = restId, codigo = pin });
        using var req = new UnityWebRequest($"{apiBase}/personal/verify", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();
        
        VerifyResponse data = null;
        if (req.result == UnityWebRequest.Result.Success)
            data = JsonUtility.FromJson<VerifyResponse>(req.downloadHandler.text);

        Debug.Log($"[FICHAR DEBUG] verify raw response: {req.downloadHandler.text}");

        if (data != null && data.ok)
        {
            empleadoNombre = data.nombre;
            if (!data.aviso_aceptado)
            {
                MostrarScreen(screenAviso);
            }
            else
            {
                bool tienePendiente = await CheckIncidencia();
                if (tienePendiente)
                    MostrarScreen(screenIncidencia);
                else
                    await RegistrarFichajeAutomatico();
            }
        }
        else if (modoDirecto)
        {
            modoDirecto = false;
            ResetPin();
            gameObject.SetActive(false);
            FichajeEvents.RaiseFichajeCodigoInvalido();
        }
        else
        {
            pinErrorLbl.text = "Código no reconocido. Inténtalo de nuevo.";
            ResetPin();
            MostrarScreen(screenPin);
        }

        RenderPinDots();
    }

    async Task AceptarAvisoYContinuar()
    {
        string json = JsonUtility.ToJson(new VerifyRequest { id = restId, codigo = pin });
        using var req = new UnityWebRequest($"{apiBase}/personal/aceptar_aviso", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        bool tienePendiente = await CheckIncidencia();
        if (tienePendiente)
            MostrarScreen(screenIncidencia);
        else
            await RegistrarFichajeAutomatico();
    }

    // ---------- Action ----------

    // Entrada directa desde IniciarSesionTPVPersonal: reutiliza el código ya tecleado,
    // se salta el teclado numérico y va directo a verify -> pantalla de acción.
    public async void FicharConCodigo(string restIdIn, string codigoIn)
    {
        modoDirecto = true;
        gameObject.SetActive(true);
        restId = restIdIn;
        pin = codigoIn;
        await SubmitPin();
    }

    async Task RegistrarFichajeAutomatico()
    {
        string tipo = "ENTRADA";

        string lastJson = JsonUtility.ToJson(new VerifyRequest { id = restId, codigo = pin });
        using var lastReq = new UnityWebRequest($"{apiBase}/fichajes/last", "POST");
        lastReq.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(lastJson));
        lastReq.downloadHandler = new DownloadHandlerBuffer();
        lastReq.SetRequestHeader("Content-Type", "application/json");
        var lastOp = lastReq.SendWebRequest();
        while (!lastOp.isDone) await Task.Yield();

        if (lastReq.result == UnityWebRequest.Result.Success)
        {
            var lastData = JsonUtility.FromJson<UltimoFichajeResponse>(lastReq.downloadHandler.text);
            if (lastData.ok && lastData.ultimo_tipo == "ENTRADA") tipo = "SALIDA";
        }

        await RegistrarFichaje(tipo);
    }

    async Task<bool> CheckIncidencia()
    {
        string json = JsonUtility.ToJson(new VerifyRequest { id = restId, codigo = pin });
        using var req = new UnityWebRequest($"{apiBase}/fichajes/check_incidencia", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success) return false;

        var data = JsonUtility.FromJson<IncidenciaCheckResponse>(req.downloadHandler.text);
        if (data != null && data.ok && data.pendiente)
        {
            incFechaEntrada = data.fecha_entrada;
            incHour = 20; incMin = 0;
            incidenciaTextLbl.text = $"No registraste tu salida el {FormatFechaSimple(incFechaEntrada)}.\n¿A qué hora terminaste tu turno?";
            UpdateIncidenciaTimeLabel();
            return true;
        }
        return false;
    }

    string FormatFechaSimple(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        var f = raw.Split(' ')[0].Split('-');
        return f.Length == 3 ? $"{f[2]}/{f[1]}/{f[0]}" : raw;
    }

    void UpdateIncidenciaTimeLabel() => incidenciaTimeLbl.text = $"{incHour:00}:{incMin:00}";

    void IncHourStep(int delta)
    {
        incHour = (incHour + delta + 24) % 24;
        UpdateIncidenciaTimeLabel();
    }

    void IncMinStep(int delta)
    {
        incMin += delta;
        if (incMin >= 60) { incMin = 0; IncHourStep(1); }
        else if (incMin < 0) { incMin = 45; IncHourStep(-1); }
        UpdateIncidenciaTimeLabel();
    }

    async Task ConfirmarIncidencia()
    {
        incidenciaConfirmBtn.SetEnabled(false);
        string horaStr = $"{incHour:00}:{incMin:00}";
        string json = JsonUtility.ToJson(new ResolverIncidenciaRequest { id = restId, codigo = pin, hora_propuesta = horaStr });
        using var req = new UnityWebRequest($"{apiBase}/fichajes/resolver_incidencia", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        incidenciaConfirmBtn.SetEnabled(true);
        await RegistrarFichajeAutomatico();
    }

    async void SaltarIncidencia()
    {
        await RegistrarFichajeAutomatico();
    }

    async Task RegistrarFichaje(string tipo)
    {

        string json = JsonUtility.ToJson(new FichajeRequest { id = restId, codigo = pin, tipo = tipo });
        using var req = new UnityWebRequest($"{apiBase}/fichajes/add", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            pinErrorLbl.text = "Error de conexión. Inténtalo de nuevo.";
            ResetPin();
            MostrarScreen(screenPin);
            return;
        }

        var data = JsonUtility.FromJson<FichajeAddResponse>(req.downloadHandler.text);
        MostrarConfirmacion(tipo, data);
    }

    // ---------- Confirm ----------

    void MostrarConfirmacion(string tipo, FichajeAddResponse data)
    {
        bool esEntrada = tipo == "ENTRADA";

        stampIconLbl.RemoveFromClassList("stamp--salida");
        if (!esEntrada) stampIconLbl.AddToClassList("stamp--salida");

        stampIconLbl.text = esEntrada ? "→" : "⇥";
        stampTypeLbl.text = esEntrada ? "Entrada" : "Salida";
        stampEmployeeLbl.text = !string.IsNullOrEmpty(data.empleado) ? data.empleado : empleadoNombre;
        stampTimeLbl.text = FormatFechaHora(data.fecha_hora);

        MostrarScreen(screenConfirm);
        FichajeEvents.RaiseFichajeRegistrado();

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnCountdown());
    }

    string FormatFechaHora(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return DateTime.Now.ToString("dd/MM/yyyy · HH:mm:ss");
        var parts = raw.Split(' ');
        if (parts.Length != 2) return raw;
        var f = parts[0].Split('-');
        if (f.Length != 3) return raw;
        return $"{f[2]}/{f[1]}/{f[0]} · {parts[1]}";
    }

    IEnumerator ReturnCountdown()
    {
        yield return new WaitForSeconds(AUTO_RETURN_SECONDS);
        CerrarPanel();
    }
}