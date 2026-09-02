using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Mirror;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class PaymentConfirmation
{
    public float amount;
    public string id;  // The restaurant ID
    public int table_number;
    public string method;
    public string id_payment; // Unique identifier for each payment confirmation
}

public class PaymentConfirmationReceiver : NetworkBehaviour
{
    private const string url = "https://gastrali.tail634a78.ts.net/get_confirmations";
    public Color colorPagado;

    public GameObject prefabPagadoCamarero;
    public GameObject prefabPagadoTPV;
    public GameObject prefabPagoParcialCamarero;
    private GameObject prefabEspacioInstance;
    private Dictionary<int, GameObject> confirmados = new Dictionary<int, GameObject>();

    private GameObject objectToColorize;
    private GameObject mesa;

    // Set to store unique payment IDs of already processed confirmations
    private HashSet<string> processedPaymentIds = new HashSet<string>();

    private DataBaseMovimientosCaja GetMovimientosCaja()
    {
        return FindObjectOfType<DataBaseMovimientosCaja>();
    }

    private TMP_Text GetTurnoText()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("TextIdTurno");
        if (obj != null)
            return obj.GetComponent<TMP_Text>();

        Debug.LogError("TextIdTurno not found!");
        return null;
    }

    void Start()
    {
        // Start polling for payment confirmations
        StartCoroutine(PollPaymentConfirmations(3f)); // Poll every 3 seconds
    }

    // Coroutine that keeps checking for payment confirmations at regular intervals
    private IEnumerator PollPaymentConfirmations(float interval)
    {
        while (true)
        {
            yield return StartCoroutine(GetPaymentConfirmations());

            // Wait for the specified interval before polling again
            yield return new WaitForSeconds(interval);
        }
    }

    // Coroutine to get payment confirmations
    private IEnumerator GetPaymentConfirmations()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Wait for the web request to complete
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Handle the response if successful
                HandleResponse(webRequest.downloadHandler.text);
            }
        }
    }

    // Handle the received JSON response from the server
    private void HandleResponse(string jsonResponse)
    {
        PaymentConfirmation[] confirmations = JsonHelper.FromJson<PaymentConfirmation>(jsonResponse);

        if (confirmations == null || confirmations.Length == 0)
        {
            //Debug.Log("No confirmations received or failed to parse JSON.");
            return;
        }

        if (CrearCamarero.mesasDictionary.Count == 0) // test if you are inside the restaurant
        {
            return;
        }
        else
        {
            foreach (var confirmation in confirmations)
            {
                if (confirmation.id == Navigation.idRestaurante)
                {
                    if (!processedPaymentIds.Contains(confirmation.id_payment))
                    {
                        // Mark as processed
                        processedPaymentIds.Add(confirmation.id_payment);

                        // Send colorization command
                        SendColorizeButtonPagado(confirmation.method, confirmation.amount, confirmation.table_number);

                        // 🔥 REGISTER PAYMENT (CONFIRMED ONLY)
                        var movimientosCaja = GetMovimientosCaja();
                        var turnoText = GetTurnoText();

                        if (movimientosCaja != null && turnoText != null && !string.IsNullOrEmpty(turnoText.text))
                        {
                            string tipo = "IngresoTarjeta";

                            StartCoroutine(movimientosCaja.AddMovimientoCaja(
                                tipo,
                                "Pago cliente por Gastrali mesa " + confirmation.table_number,
                                confirmation.amount
                            ));
                        }
                    }
                }
            }
        }
    }


    // Send a command to the server to colorize the button
    [Command]
    public void SendColorizeButtonPagado(string method, float amount, int table_number)
    {
        RpcColorizeButtonPagado(method, amount, table_number);
    }

    // Client-side method to apply the colorization of the button
    [ClientRpc]
    void RpcColorizeButtonPagado(string method, float amount, int table_number)
    {
        ColorizeButton(method, amount, table_number);
    }

    // Method to colorize the button and instantiate the confirmation UI
    private void ColorizeButton(string method, float amount, int table_number)
    {
        if (SceneManager.GetActiveScene().name == "MobileScene" || SceneManager.GetActiveScene().name == "TPVScene")
        {
            CrearCamarero.buttonMesaDictionary.TryGetValue(table_number, out objectToColorize);
            CrearCamarero.mesasDictionary.TryGetValue(table_number, out mesa);

            if (objectToColorize != null)
            {
                var image = objectToColorize.GetComponent<Image>();
                var text = objectToColorize.GetComponentInChildren<TMP_Text>();

                if (image != null) image.color = colorPagado;
                if (text != null) text.color = Color.white;
            }

            if (mesa != null)
            {
                if (SceneManager.GetActiveScene().name == "MobileScene")
                {
                    if (method == "Equitativo")
                    {
                        prefabEspacioInstance = Instantiate(prefabPagoParcialCamarero, transform.position, Quaternion.identity);
                        prefabEspacioInstance.GetComponentInChildren<TMP_Text>().text = "Pago parcial equitativo: " + amount + " €";
                    }
                    else if (method == "Elegir")
                    {
                        prefabEspacioInstance = Instantiate(prefabPagoParcialCamarero, transform.position, Quaternion.identity);
                        prefabEspacioInstance.GetComponentInChildren<TMP_Text>().text = "Pago parcial a elegir: " + amount + " €";
                    }
                    else // "Todo"
                    {
                        prefabEspacioInstance = Instantiate(prefabPagadoCamarero, transform.position, Quaternion.identity);
                    }

                    // Fuente 
                    TMP_Text[] texts = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
                    string rutafuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
                    TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral);
                    if (fuenteGeneral == null)
                        fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral + " SDF");
                    texts[0].font = fuenteGeneral;

                    // Cambiar Botones ese scrollMesa
                    mesa.transform.GetChild(2).gameObject.SetActive(false); // Tomar nota
                    mesa.transform.GetChild(3).gameObject.SetActive(false); // Cerrar mesa
                    mesa.transform.GetChild(4).gameObject.SetActive(true); // Resetear Mesa
                }
                else
                {
                    prefabEspacioInstance = Instantiate(prefabPagadoTPV, transform.position, Quaternion.identity);

                    // Fuente 
                    TMP_Text[] texts = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
                    string rutafuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
                    TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral);
                    if (fuenteGeneral == null)
                        fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral + " SDF");
                    texts[0].font = fuenteGeneral;
                }
                prefabEspacioInstance.transform.SetParent(mesa.transform.GetChild(0).GetChild(0).GetChild(0), false);
                prefabEspacioInstance.transform.SetSiblingIndex(0);
                confirmados[table_number] = prefabEspacioInstance;

            }
        }
    }
}

// Helper class to parse JSON arrays
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string modifiedJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(modifiedJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
