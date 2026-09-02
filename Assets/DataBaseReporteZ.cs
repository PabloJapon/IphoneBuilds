using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

public class DataBaseReporteZ : MonoBehaviour
{
    [Header("Server")]
    public string url;

    [Header("UI Texts")]
    public TMP_Text textId;        // Restaurant ID
    public TMP_Text textIdTurno;   // Shift ID
    public TMP_InputField dineroAperturaCaja;
    public TMP_InputField dineroCierreCaja;
    public TMP_Text empleado;

    [Header("Buttons")]
    public Button buttonAperturaCaja; // Open Shift
    public Button buttonCierreCaja;   // Close Shift
    public Button buttonReporteX;   
    public Button buttonReporteZ;
    public Button buttonAñadirMovimiento;

    [Header("Canvas")]
    public GameObject canvasAperturaCaja;
    public GameObject canvasCierreCaja;

    [Header("UI Containers")]
    public GameObject containerReporteX;
    public GameObject containerReporteZ;

    private CultureInfo spanishCulture;
    private ReporteZList reporteZList;

    // =========================
    // SERIALIZABLE REQUESTS / RESPONSES
    // =========================
    [System.Serializable]
    public class OpenShiftRequest { public string id_restaurante; public string AbiertaPor; public float FondoCajaInicial; }
    [System.Serializable]
    public class CloseShiftRequest { public string id_turno; public string CerradaPor; public float FondoCajaFinal_Real; public float IngresosEfectivo; public float IngresosTarjeta; public float RetirosEfectivo; public float DepositosEfectivo; public float FondoCajaFinal_Teorico; public float Descuadre; }
    [System.Serializable]
    public class OpenShiftResponse { public string message; public string id_turno; }

    void Awake()
    {
        spanishCulture = new CultureInfo("es-ES");
        spanishCulture.NumberFormat.CurrencySymbol = "€";

        StartCoroutine(WaitForRestaurantID());
    }

    // =========================
    // WAIT FOR RESTAURANT ID
    // =========================
    private IEnumerator WaitForRestaurantID()
    {
        while (string.IsNullOrEmpty(textId.text))
            yield return null;

        StartCoroutine(LoadReporte());
    }

    // =========================
    // LOAD SHIFT DATA
    // =========================
    public void ReloadData()
    {
        if (!string.IsNullOrEmpty(textId.text))
            StartCoroutine(LoadReporte());
    }

    private IEnumerator LoadReporte()
    {
        string fullUrl = url + "/restaurant/" + textId.text;
        /* Debug.Log("Loading shifts from: " + fullUrl); */

        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Fetch error: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        string wrappedJson = "{\"reportez\":" + json + "}";
        reporteZList = JsonUtility.FromJson<ReporteZList>(wrappedJson);

        if (reporteZList == null || reporteZList.reportez == null)
        {
            reporteZList = new ReporteZList { reportez = new List<ReporteZEntry>() };
            Debug.LogWarning("No shifts found for this restaurant.");
        }

        UpdateButtons();
    }

    // =========================
    // UPDATE BUTTONS BASED ON SHIFT STATE
    // =========================
    private void UpdateButtons()
    {
        // Find open shift
        var openShift = reporteZList.reportez
            .FirstOrDefault(x => string.IsNullOrEmpty(x.FechaCierre));

        bool shiftOpen = openShift != null;

        // Button states
        buttonAperturaCaja.interactable = !shiftOpen;
        buttonCierreCaja.interactable = shiftOpen;
        buttonReporteX.interactable = shiftOpen;
        buttonReporteZ.interactable = !shiftOpen;
        buttonAñadirMovimiento.interactable = shiftOpen;

        ReporteZEntry shiftToDisplay = null;

        if (shiftOpen)
        {
            shiftToDisplay = openShift;
            textIdTurno.text = openShift.id_turno;
        }
        else
        {
            // 🔥 Get LAST closed shift
            shiftToDisplay = reporteZList.reportez
                .Where(x => !string.IsNullOrEmpty(x.FechaCierre))
                .OrderByDescending(x => DateTime.Parse(x.FechaCierre, spanishCulture))
                .FirstOrDefault();

            if (shiftToDisplay != null)
                textIdTurno.text = shiftToDisplay.id_turno;
        }

        // Fill UI if we found something
        if (shiftToDisplay != null)
        {
            FillReporteX(shiftToDisplay);
            FillReporteZ(shiftToDisplay);
        }
    }

    // =========================
    // FILL REPORT UI
    // =========================
    private void FillReporteX(ReporteZEntry item)
    {
        TMP_Text[] t = containerReporteX.GetComponentsInChildren<TMP_Text>();
        t[0].text = item.FechaApertura;
        t[1].text = item.AbiertaPor;
        t[2].text = item.FondoCajaInicial.ToString("C", spanishCulture);
        t[3].text = item.IngresosEfectivo.ToString("C", spanishCulture);
        t[4].text = item.IngresosTarjeta.ToString("C", spanishCulture);
        t[5].text = item.DepositosEfectivo.ToString("C", spanishCulture);
        t[6].text = item.RetirosEfectivo.ToString("C", spanishCulture);
        t[7].text = item.FondoCajaFinal_Teorico.ToString("C", spanishCulture);
    }

    private void FillReporteZ(ReporteZEntry item)
    {
        TMP_Text[] t = containerReporteZ.GetComponentsInChildren<TMP_Text>();
        t[0].text = item.FechaApertura;
        t[1].text = item.AbiertaPor;
        t[2].text = item.FechaCierre ?? "";
        t[3].text = item.CerradaPor ?? "";
        t[4].text = item.FondoCajaInicial.ToString("C", spanishCulture);
        t[5].text = item.IngresosEfectivo.ToString("C", spanishCulture);
        t[6].text = item.IngresosTarjeta.ToString("C", spanishCulture);
        t[7].text = item.DepositosEfectivo.ToString("C", spanishCulture);
        t[8].text = item.RetirosEfectivo.ToString("C", spanishCulture);
        t[9].text = (item.IngresosEfectivo + item.IngresosTarjeta + item.DepositosEfectivo - item.RetirosEfectivo).ToString("C", spanishCulture); // SALDO TOTAL FINAL
        t[10].text = item.FondoCajaFinal_Teorico.ToString("C", spanishCulture); 
        t[11].text = item.FondoCajaFinal_Real.ToString("C", spanishCulture); 
        t[12].text = item.Descuadre.ToString("C", spanishCulture); // Fondo caja
    }

    // =========================
    // OPEN SHIFT
    // =========================
    public void CrearReporteNuevo()
    {
        string empleadoName = empleado.text.Replace("Hola, ", "").Trim();
        string dineroText = Regex.Replace(dineroAperturaCaja.text, @"[^\d\.,-]", "").Replace(",", ".");

        if (!float.TryParse(dineroText, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out float dinero))
        {
            Debug.LogError("Invalid money format for opening shift.");
            return;
        }

        StartCoroutine(OpenShift(textId.text, empleadoName, dinero));
    }

    private IEnumerator OpenShift(string idRestaurante, string abiertaPor, float FondoCajaInicial)
    {
        string urlOpen = url + "/open";
        OpenShiftRequest payload = new OpenShiftRequest { id_restaurante = idRestaurante, AbiertaPor = abiertaPor, FondoCajaInicial = FondoCajaInicial };

        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("Opening shift with: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(urlOpen, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("OpenShift ERROR: " + request.error);
            Debug.LogError("BODY: " + request.downloadHandler.text);
        }
        else
        {
            OpenShiftResponse res = JsonUtility.FromJson<OpenShiftResponse>(request.downloadHandler.text);
            textIdTurno.text = res.id_turno;
            Debug.Log("Shift created: " + res.id_turno);
            StartCoroutine(LoadReporte());

            FindObjectOfType<DataBaseMovimientosCaja>().ReloadMovimientos();
        }
    }

    public void CancelCrearReporte()
    {
        UpdateButtons();
        dineroAperturaCaja.text = "";
        canvasAperturaCaja.SetActive(false);
        Debug.Log("Cancelled creating new shift.");
    }

    // =========================
    // CLOSE SHIFT
    // =========================
    public void CerrarReporte()
    {
        if (string.IsNullOrEmpty(textIdTurno.text))
        {
            Debug.LogError("No open shift to close.");
            return;
        }

        string dineroText = Regex.Replace(dineroCierreCaja.text, @"[^\d\.,-]", "").Replace(",", ".");

        if (!float.TryParse(dineroText, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out float dinero))
        {
            Debug.LogError("Invalid money format for opening shift.");
            return;
        }

        StartCoroutine(CloseShift(dinero));
    }

    private IEnumerator CloseShift(float dineroCierre)
    {
        string urlClose = url + "/close";
        var openShift = reporteZList.reportez.FirstOrDefault(x => string.IsNullOrEmpty(x.FechaCierre));

        if (openShift == null)
        {
            Debug.LogError("No open shift found in list to close.");
            yield break;
        }

        CloseShiftRequest payload = new CloseShiftRequest
        {
            id_turno = openShift.id_turno,
            CerradaPor = empleado.text.Replace("Hola, ", "").Trim(),
            FondoCajaFinal_Real = dineroCierre
        };

        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("Closing shift with: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(urlClose, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("CloseShift ERROR: " + request.error);
            Debug.LogError("BODY: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Shift closed: " + openShift.id_turno);
            StartCoroutine(LoadReporte());

            FindObjectOfType<DataBaseMovimientosCaja>().ReloadMovimientos();
        }
    }


    public void CancelCerrarReporte()
    {
        UpdateButtons();
        dineroCierreCaja.text = "";
        canvasCierreCaja.SetActive(false);
        Debug.Log("Cancelled closing shift.");
    }
}

// =========================
// DATA CLASSES
// =========================
[System.Serializable]
public class ReporteZEntry
{
    public string id_restaurante;
    public string id_turno;
    public string FechaApertura;
    public string FechaCierre;
    public string AbiertaPor;
    public string CerradaPor;
    public float FondoCajaInicial;
    public float FondoCajaFinal_Real;
    public float IngresosEfectivo;
    public float IngresosTarjeta;
    public float RetirosEfectivo;
    public float DepositosEfectivo;
    public float FondoCajaFinal_Teorico;
    public float Descuadre;
}

[System.Serializable]
public class ReporteZList
{
    public List<ReporteZEntry> reportez;
}