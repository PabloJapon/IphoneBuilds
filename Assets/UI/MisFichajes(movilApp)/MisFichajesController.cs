using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MisFichajesController : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private string apiBaseUrl = "https://gastrali.tail634a78.ts.net";

    [Header("Sesión (opcional, si no se usa PlayerPrefs)")]
    [SerializeField] private string restaurantIdOverride = "";
    [SerializeField] private int employeeIdOverride = 0;

    public event Action OnBackButtonPressed;

    private VisualElement _root;
    private Button _backButton;
    private Button _prevWeekButton;
    private Button _nextWeekButton;
    private Label _weekRangeLabel;
    private ScrollView _dayList;
    private Label _statusLabel;

    private int _weekOffset = 0;
    private string _restaurantId;
    private int _employeeId;

    private static readonly string[] DiasAbrev = { "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB", "DOM" };
    private static readonly string[] MesesAbrev =
    {
        "ene", "feb", "mar", "abr", "may", "jun",
        "jul", "ago", "sep", "oct", "nov", "dic"
    };

    private static readonly Dictionary<string, string> TipoLabel = new Dictionary<string, string>
    {
        { "ENTRADA", "Entrada" },
        { "SALIDA", "Salida" },
        { "INICIO_PAUSA", "Inicio pausa" },
        { "FIN_PAUSA", "Fin pausa" }
    };

    private static readonly Dictionary<string, string> TipoClass = new Dictionary<string, string>
    {
        { "ENTRADA", "tipo-tag--entrada" },
        { "SALIDA", "tipo-tag--salida" },
        { "INICIO_PAUSA", "tipo-tag--pausa" },
        { "FIN_PAUSA", "tipo-tag--pausa" }
    };

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _backButton = _root.Q<Button>("back-button");
        _prevWeekButton = _root.Q<Button>("prev-week-button");
        _nextWeekButton = _root.Q<Button>("next-week-button");
        _weekRangeLabel = _root.Q<Label>("week-range-label");
        _dayList = _root.Q<ScrollView>("day-list");
        _statusLabel = _root.Q<Label>("status-label");

        _backButton.clicked += () =>
        {
            gameObject.SetActive(false);
            OnBackButtonPressed?.Invoke();
        };
        _prevWeekButton.clicked += () => ChangeWeek(-1);
        _nextWeekButton.clicked += () => ChangeWeek(1);

        _restaurantId = string.IsNullOrEmpty(restaurantIdOverride)
            ? SesionEmpleado.RestaurantId
            : restaurantIdOverride;

        _employeeId = employeeIdOverride != 0
            ? employeeIdOverride
            : SesionEmpleado.IdEmpleado;

        _weekOffset = 0;
        StartCoroutine(LoadWeek());
    }

    private void ChangeWeek(int delta)
    {
        _weekOffset += delta;
        StartCoroutine(LoadWeek());
    }

    private DateTime GetMondayOfCurrentWeek()
    {
        DateTime today = DateTime.Now.Date;
        int diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-diff).AddDays(_weekOffset * 7);
    }

    private IEnumerator LoadWeek()
    {
        if (string.IsNullOrEmpty(_restaurantId))
        {
            ShowStatus("Falta restaurant_id de sesión.");
            yield break;
        }
        if (_employeeId == 0)
        {
            ShowStatus("Falta id_empleado de sesión.");
            yield break;
        }

        SetNavInteractable(false);

        DateTime monday = GetMondayOfCurrentWeek();
        DateTime sunday = monday.AddDays(6);

        UpdateWeekRangeLabel(monday, sunday);

        string fechaInicio = monday.ToString("yyyy-MM-dd");
        string fechaFin = sunday.ToString("yyyy-MM-dd");

        string url = string.Format(
            "{0}/fichajes/empleado?restaurant_id={1}&id_empleado={2}&fecha_inicio={3}&fecha_fin={4}",
            apiBaseUrl.TrimEnd('/'),
            UnityWebRequest.EscapeURL(_restaurantId),
            _employeeId,
            fechaInicio,
            fechaFin);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool failed = request.result != UnityWebRequest.Result.Success;
#else
            bool failed = request.isNetworkError || request.isHttpError;
#endif
            if (failed)
            {
                Debug.LogError($"[MisFichajes] Error al pedir fichajes: {request.error}");
                ShowStatus("No se pudieron cargar los fichajes. Reintenta.");
                SetNavInteractable(true);
                yield break;
            }

            FichajesResponse response = JsonUtility.FromJson<FichajesResponse>(request.downloadHandler.text);
            Dictionary<string, List<FichajeDto>> porFecha = AgruparPorFecha(response);

            HideStatus();
            BuildDayList(monday, porFecha);
        }

        SetNavInteractable(true);
    }

    private Dictionary<string, List<FichajeDto>> AgruparPorFecha(FichajesResponse response)
    {
        var porFecha = new Dictionary<string, List<FichajeDto>>();
        if (response?.fichajes == null)
        {
            return porFecha;
        }

        foreach (FichajeDto fichaje in response.fichajes)
        {
            string clave = fichaje.fecha_hora.Length >= 10 ? fichaje.fecha_hora.Substring(0, 10) : fichaje.fecha_hora;

            if (!porFecha.TryGetValue(clave, out List<FichajeDto> lista))
            {
                lista = new List<FichajeDto>();
                porFecha[clave] = lista;
            }

            lista.Add(fichaje);
        }

        return porFecha;
    }

    private void BuildDayList(DateTime monday, Dictionary<string, List<FichajeDto>> porFecha)
    {
        _dayList.Clear();

        for (int i = 0; i < 7; i++)
        {
            DateTime dia = monday.AddDays(i);
            string clave = dia.ToString("yyyy-MM-dd");
            bool esHoy = dia.Date == DateTime.Now.Date;

            porFecha.TryGetValue(clave, out List<FichajeDto> fichajesDelDia);

            _dayList.Add(CreateDayRow(i, dia, esHoy, fichajesDelDia));
        }
    }

    private VisualElement CreateDayRow(int weekdayIndex, DateTime dia, bool esHoy, List<FichajeDto> fichajes)
    {
        bool sinFichajes = fichajes == null || fichajes.Count == 0;

        var row = new VisualElement();
        row.AddToClassList("day-row");
        if (esHoy) row.AddToClassList("day-row--today");
        if (sinFichajes) row.AddToClassList("day-row--free");

        var dayInfo = new VisualElement();
        dayInfo.AddToClassList("day-info");

        var weekdayLabel = new Label(esHoy ? "HOY" : DiasAbrev[weekdayIndex]);
        weekdayLabel.AddToClassList("day-weekday");

        var dateLabel = new Label(dia.Day.ToString());
        dateLabel.AddToClassList("day-date");

        dayInfo.Add(weekdayLabel);
        dayInfo.Add(dateLabel);

        var shiftsContainer = new VisualElement();
        shiftsContainer.AddToClassList("day-shifts");

        if (sinFichajes)
        {
            var freeLabel = new Label("Sin fichajes");
            freeLabel.AddToClassList("shift-free");
            shiftsContainer.Add(freeLabel);
        }
        else
        {
            foreach (FichajeDto fichaje in fichajes)
            {
                var shiftRow = new VisualElement();
                shiftRow.AddToClassList("shift-row");

                var timeLabel = new Label(FormatHora(fichaje.fecha_hora));
                timeLabel.AddToClassList("shift-time");
                shiftRow.Add(timeLabel);

                var tipoTag = new Label(TipoLabel.TryGetValue(fichaje.tipo, out string label) ? label : fichaje.tipo);
                tipoTag.AddToClassList("tipo-tag");
                if (TipoClass.TryGetValue(fichaje.tipo, out string tipoClass))
                {
                    tipoTag.AddToClassList(tipoClass);
                }
                shiftRow.Add(tipoTag);

                shiftsContainer.Add(shiftRow);
            }
        }

        row.Add(dayInfo);
        row.Add(shiftsContainer);

        return row;
    }

    private string FormatHora(string fechaHora)
    {
        // fechaHora viene como "yyyy-MM-dd HH:MM:SS"
        if (string.IsNullOrEmpty(fechaHora) || fechaHora.Length < 16) return "--:--";
        return fechaHora.Substring(11, 5);
    }

    private void UpdateWeekRangeLabel(DateTime monday, DateTime sunday)
    {
        string inicio = $"{monday.Day} {MesesAbrev[monday.Month - 1]}";
        string fin = $"{sunday.Day} {MesesAbrev[sunday.Month - 1]}";
        _weekRangeLabel.text = $"{inicio} — {fin}";
    }

    private void SetNavInteractable(bool interactable)
    {
        _prevWeekButton.SetEnabled(interactable);
        _nextWeekButton.SetEnabled(interactable);
    }

    private void ShowStatus(string mensaje)
    {
        _statusLabel.text = mensaje;
        _statusLabel.style.display = DisplayStyle.Flex;
    }

    private void HideStatus()
    {
        _statusLabel.style.display = DisplayStyle.None;
    }

    [Serializable]
    private class FichajeDto
    {
        public int id_fichaje;
        public int id_empleado;
        public string tipo;
        public string fecha_hora;
        public string observaciones;
    }

    [Serializable]
    private class FichajesResponse
    {
        public FichajeDto[] fichajes;
    }
}