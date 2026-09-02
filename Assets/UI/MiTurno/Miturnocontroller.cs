using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

/// <summary>
/// Controlador de la pantalla "Mi turno" (UI Toolkit).
/// Requiere un UIDocument en el mismo GameObject apuntando a
/// MiTurnoScreen.uxml (con MiTurnoScreen.uss enlazado en el UXML
/// o asignado en el propio UIDocument/Panel Settings).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MiTurnoController : MonoBehaviour
{
    [Header("API")]
    [Tooltip("Ej: https://gastrali.tail634a78.ts.net")]
    [SerializeField] private string apiBaseUrl = "https://gastrali.tail634a78.ts.net";

    [Header("Sesión (opcional, si no se usa PlayerPrefs)")]
    [Tooltip("Si se deja vacío, se lee de PlayerPrefs 'restaurant_id'.")]
    [SerializeField] private string restaurantIdOverride = "";
    [Tooltip("Si se deja en 0, se lee de PlayerPrefs 'id_empleado'.")]
    [SerializeField] private int employeeIdOverride = 0;

    public event Action OnBackButtonPressed;

    // ---- Referencias UI ----
    private VisualElement _root;
    private Button _backButton;
    private Button _prevWeekButton;
    private Button _nextWeekButton;
    private Label _weekRangeLabel;
    private ScrollView _dayList;
    private Label _statusLabel;

    // ---- Estado ----
    private int _weekOffset = 0; // 0 = semana actual, -1 = semana anterior, +1 = siguiente
    private string _restaurantId;
    private int _employeeId;

    private static readonly string[] DiasAbrev = { "LUN", "MAR", "MIÉ", "JUE", "VIE", "SÁB", "DOM" };
    private static readonly string[] MesesAbrev =
    {
        "ene", "feb", "mar", "abr", "may", "jun",
        "jul", "ago", "sep", "oct", "nov", "dic"
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
            "{0}/turnos/empleado?restaurant_id={1}&id_empleado={2}&fecha_inicio={3}&fecha_fin={4}",
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
                Debug.LogError($"[MiTurno] Error al pedir turnos: {request.error}");
                ShowStatus("No se pudieron cargar los turnos. Reintenta.");
                SetNavInteractable(true);
                yield break;
            }

            TurnosResponse response = JsonUtility.FromJson<TurnosResponse>(request.downloadHandler.text);
            Dictionary<string, List<TurnoDto>> porFecha = AgruparPorFecha(response);

            HideStatus();
            BuildDayList(monday, porFecha);
        }

        SetNavInteractable(true);
    }

    private Dictionary<string, List<TurnoDto>> AgruparPorFecha(TurnosResponse response)
    {
        var porFecha = new Dictionary<string, List<TurnoDto>>();
        if (response?.turnos == null)
        {
            return porFecha;
        }

        foreach (TurnoDto turno in response.turnos)
        {
            if (!porFecha.TryGetValue(turno.fecha, out List<TurnoDto> lista))
            {
                lista = new List<TurnoDto>();
                porFecha[turno.fecha] = lista;
            }

            lista.Add(turno);
        }

        return porFecha;
    }

    private void BuildDayList(DateTime monday, Dictionary<string, List<TurnoDto>> porFecha)
    {
        _dayList.Clear();

        for (int i = 0; i < 7; i++)
        {
            DateTime dia = monday.AddDays(i);
            string clave = dia.ToString("yyyy-MM-dd");
            bool esHoy = dia.Date == DateTime.Now.Date;

            porFecha.TryGetValue(clave, out List<TurnoDto> turnosDelDia);

            _dayList.Add(CreateDayRow(i, dia, esHoy, turnosDelDia));
        }
    }

    private VisualElement CreateDayRow(int weekdayIndex, DateTime dia, bool esHoy, List<TurnoDto> turnos)
    {
        bool sinTurnos = turnos == null || turnos.Count == 0;

        var row = new VisualElement();
        row.AddToClassList("day-row");
        if (esHoy) row.AddToClassList("day-row--today");
        if (sinTurnos) row.AddToClassList("day-row--free");

        // Columna izquierda: día de la semana + número
        var dayInfo = new VisualElement();
        dayInfo.AddToClassList("day-info");

        var weekdayLabel = new Label(esHoy ? "HOY" : DiasAbrev[weekdayIndex]);
        weekdayLabel.AddToClassList("day-weekday");

        var dateLabel = new Label(dia.Day.ToString());
        dateLabel.AddToClassList("day-date");

        dayInfo.Add(weekdayLabel);
        dayInfo.Add(dateLabel);

        // Columna derecha: turnos del día (o "Libre")
        var shiftsContainer = new VisualElement();
        shiftsContainer.AddToClassList("day-shifts");

        if (sinTurnos)
        {
            var freeLabel = new Label("Libre");
            freeLabel.AddToClassList("shift-free");
            shiftsContainer.Add(freeLabel);
        }
        else
        {
            foreach (TurnoDto turno in turnos)
            {
                var shiftRow = new VisualElement();
                shiftRow.AddToClassList("shift-row");

                var shiftLabel = new Label($"{FormatHora(turno.hora_inicio)} - {FormatHora(turno.hora_fin)}");
                shiftLabel.AddToClassList("shift-time");
                shiftRow.Add(shiftLabel);

                if (!string.IsNullOrEmpty(turno.puesto))
                {
                    var puestoTag = new Label(turno.puesto);
                    puestoTag.AddToClassList("puesto-tag");
                    shiftRow.Add(puestoTag);
                }

                shiftsContainer.Add(shiftRow);
            }
        }

        row.Add(dayInfo);
        row.Add(shiftsContainer);

        return row;
    }

    private string FormatHora(string horaCruda)
    {
        // La API puede devolver "HH:MM" o "HH:MM:SS"; nos quedamos con HH:MM.
        if (string.IsNullOrEmpty(horaCruda)) return "--:--";
        return horaCruda.Length >= 5 ? horaCruda.Substring(0, 5) : horaCruda;
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

    // ---- DTOs que reflejan la respuesta de GET /turnos ----

    [Serializable]
    private class TurnoDto
    {
        public int id;
        public int id_empleado;
        public string nombre_empleado;
        public string fecha;
        public string hora_inicio;
        public string hora_fin;
        public string puesto;
        public string notas;
    }

    [Serializable]
    private class TurnosResponse
    {
        public TurnoDto[] turnos;
    }
}