using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DataBaseMovimientosCaja : MonoBehaviour
{
    public string url;
    public TMP_Text textId;
    public TMP_Text textIdTurno;

    private List<string> id_restaurante = new List<string>();
    private List<string> id_turno = new List<string>();
    private List<string> Hora = new List<string>();
    private List<string> Tipo = new List<string>();
    private List<string> Concepto = new List<string>();
    private List<float> Cantidad = new List<float>();
    private List<string> Empleado = new List<string>();

    private MovimientosCajaList movimientosCajaList;

    public GameObject contentRegistrosMovimientosCaja;
    public GameObject prefabRegistrosMovimientosCaja;

    public GameObject canvasMovimientosCaja;
    public TMP_Text empleadoName;


    private ReporteZList reporteZList;
    public GameObject advertenciaTurnoNoEmpezado;


    void Awake()
    {
        if ((SceneManager.GetActiveScene().name == "TPVScene"))
        {
            StartCoroutine(WaitForIDs());
        }

        // Get turno id only in mobile
        else if ((SceneManager.GetActiveScene().name == "MobileScene"))
        {
            StartCoroutine(WaitForRestaurantID());
        }
    }

    private IEnumerator WaitForRestaurantID()
    {
        while (string.IsNullOrEmpty(textId.text))
            yield return null;

        StartCoroutine(LoadReporte());
    }
    private IEnumerator LoadReporte()
    {
        string fullUrl = url + "/reporteZ/" + "/restaurant/" + textId.text;

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

        // Find open shift to update turno id
        var openShift = reporteZList.reportez.FirstOrDefault(x => string.IsNullOrEmpty(x.FechaCierre));

        bool shiftOpen = openShift != null;

        if (shiftOpen)
        {
            textIdTurno.text = openShift.id_turno;
        }
    }

    [System.Serializable]
    public class ReporteZList
    {
        public List<ReporteZEntry> reportez;
    }

    private IEnumerator WaitForIDs()
    {
        // Wait for restaurant ID
        while (string.IsNullOrEmpty(textId.text))
            yield return null;

        // Wait for turno ID
        while (string.IsNullOrEmpty(textIdTurno.text))
            yield return null;

        StartCoroutine(LoadMovimientosCajaData());
    }

    public IEnumerator LoadMovimientosCajaData()
    {
        if ((SceneManager.GetActiveScene().name == "TPVScene"))
        {
            UnityWebRequest request = UnityWebRequest.Get(url + "/movimientosCaja/" + "/restaurant/" + textId.text);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Failed to fetch MovimientosCaja: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            //Debug.Log("Received JSON data movimientoscaja: " + json);

            // Arreglamos el JSON para que sea compatible con JsonUtility
            string wrappedJson = "{\"movimientoscaja\":" + json + "}";

            // Deserializamos
            movimientosCajaList = JsonUtility.FromJson<MovimientosCajaList>(wrappedJson);

            foreach (var item in movimientosCajaList.movimientoscaja)
            {
                // 🔥 FILTER by id_turno - only show this turno
                if (item.id_turno != textIdTurno.text)
                    continue;

                id_restaurante.Add(item.id_restaurante);
                id_turno.Add(item.id_turno);
                Hora.Add(item.Hora);
                Tipo.Add(item.Tipo);
                Concepto.Add(item.Concepto);
                Cantidad.Add(item.Cantidad);
                Empleado.Add(item.Empleado);
            }

            CreatePrefabButtons();
        }
    }

    void CreatePrefabButtons()
    {
        for (int i = 0; i < id_restaurante.Count; i++)
        {
            CreatePrefab(i);
        }
    }
    private void CreatePrefab(int index)
    {
        var prefabInstance = Instantiate(prefabRegistrosMovimientosCaja, transform.position, Quaternion.identity);
        prefabInstance.transform.SetParent(contentRegistrosMovimientosCaja.transform, false);
        prefabInstance.transform.SetSiblingIndex(0);

        TMP_Text[] texts = prefabInstance.GetComponentsInChildren<TMP_Text>();

        texts[0].text = Hora[index];
        texts[1].text = Tipo[index];
        texts[2].text = Concepto[index];
        texts[3].text = Cantidad[index].ToString("N2", new CultureInfo("es-ES")) + " €";
        texts[4].text = Empleado[index];
    }

    public void ReloadMovimientos()
    {
        Debug.Log("Reloading movimientos for turno: " + textIdTurno.text);

        ClearData();
        StartCoroutine(LoadMovimientosCajaData());
    }

    public void AñadirMovimiento()
    {
        if (string.IsNullOrWhiteSpace(textIdTurno.text))
        {
            advertenciaTurnoNoEmpezado.SetActive(true);
        }

        else
        {
            TMP_Dropdown dropdownTipo = canvasMovimientosCaja.GetComponentInChildren<TMP_Dropdown>();
            TMP_InputField[] inputs = canvasMovimientosCaja.GetComponentsInChildren<TMP_InputField>();

            if (dropdownTipo == null || inputs.Length < 2)
            {
                Debug.LogError("UI elements not found in canvasMovimientosCaja");
                return;
            }

            string tipo = dropdownTipo.options[dropdownTipo.value].text;
            string concepto = inputs[0].text;

            string cantidadText = inputs[1].text.Replace(",", ".");
            float cantidad;

            if (!float.TryParse(cantidadText, NumberStyles.Any, CultureInfo.InvariantCulture, out cantidad))
            {
                Debug.LogError("Cantidad is not a valid number");
                return;
            }

            StartCoroutine(AddMovimientoCaja(tipo, concepto, cantidad));
        }
    }

    public IEnumerator AddMovimientoCaja(
    string tipo,
    string concepto,
    float cantidad)
    {
        string endpoint = url + "/movimientosCaja" + "/add";

        string empleado = "";

        if (empleadoName != null)
        {
            empleado = empleadoName.text.Replace("Hola, ", "").Trim();
        }

        MovimientosCajaPost newItem = new MovimientosCajaPost
        {
            id_restaurante = textId.text,
            id_turno = textIdTurno.text,
            Tipo = tipo,
            Concepto = concepto,
            Cantidad = cantidad,
            Empleado = empleado
        };

        string jsonData = JsonUtility.ToJson(newItem);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Error adding movimiento: " + request.error);
        }
        else
        {
            Debug.Log("Movimiento added successfully: " + request.downloadHandler.text);


            if ((SceneManager.GetActiveScene().name == "TPVScene"))
            {
                ClearData();
                StartCoroutine(LoadMovimientosCajaData());

                CancelarMovimiento();
                FindObjectOfType<DataBaseReporteZ>().ReloadData();
            }
        }
    }
    void ClearData()
    {
        id_restaurante.Clear();
        id_turno.Clear();
        Hora.Clear();
        Tipo.Clear();
        Concepto.Clear();
        Cantidad.Clear();
        Empleado.Clear();

        foreach (Transform child in contentRegistrosMovimientosCaja.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void CancelarMovimiento()
    {
        ClearUIFields();
        canvasMovimientosCaja.SetActive(false);
    }
    void ClearUIFields()
    {
        TMP_Dropdown dropdownTipo = canvasMovimientosCaja.GetComponentInChildren<TMP_Dropdown>();
        TMP_InputField[] inputs = canvasMovimientosCaja.GetComponentsInChildren<TMP_InputField>();

        if (dropdownTipo != null)
            dropdownTipo.value = 0;

        foreach (TMP_InputField input in inputs)
            input.text = "";
    }

}

[System.Serializable]
public class MovimientosCajaEntry
{
    public string id_restaurante;
    public string id_turno;
    public string Hora;
    public string Tipo;
    public string Concepto;
    public float Cantidad;
    public string Empleado;
}

[System.Serializable]
public class MovimientosCajaList
{
    public List<MovimientosCajaEntry> movimientoscaja;
}

[System.Serializable]
public class MovimientosCajaPost
{
    public string id_restaurante;
    public string id_turno;
    public string Tipo;
    public string Concepto;
    public float Cantidad;
    public string Empleado;
}
