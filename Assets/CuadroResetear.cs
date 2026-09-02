using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Globalization;

public class CuadroResetear : MonoBehaviour
{
    private String numeroMesa;
    private string id;
    private const string deleteUrl = "https://gastrali.tail634a78.ts.net/confirmacion/delete";

    private GameObject mesa;

    // Arrays for registros pedidos
    private String nombrePlatos;
    private String nPlatos;
    private float precio;

    // Called when the user clicks "No" – simply hide this confirmation UI.
    public void OnNoResetButtonClicked()
    {
        gameObject.SetActive(false);
    }

    // evita doble envío si se pulsa "Sí" dos veces seguidas
    private bool _yaProcesado = false;

    private void OnEnable()
    {
        _yaProcesado = false;
        nombrePlatos = "";
        nPlatos = "";
        precio = 0f;
    }

    // Called when the user clicks "Yes" – process both pagado and order items removal. - Enviar a base de datos el registro del pedido
    public void OnYesResetButtonClicked()
    {
        if (_yaProcesado) return;
        _yaProcesado = true;

        nombrePlatos = "";
        nPlatos = "";
        precio = 0f;

        // Get numero mesa
        numeroMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;

        // Get the ID
        id = GameObject.FindGameObjectWithTag("textID").GetComponent<TMP_Text>().text;

        bool huboPlatos = false;
        bool pagadoBorrado = false;

        // Delete Pagado item en base de datos - reteneter datos platos para enviar a registros
        if (CrearCamarero.mesasDictionary.TryGetValue(int.Parse(numeroMesa), out mesa))
        {
            Transform Content = mesa.transform.GetChild(0).GetChild(0).GetChild(0);
            int i = 0;
            foreach (Transform son in Content)
            {
                if (son.name.Contains("PagadoCamarero(Clone)") || son.name.Contains("PagadoTPV(Clone)"))
                {
                    if (!pagadoBorrado)
                    {
                        DeletePagado(id, int.Parse(numeroMesa));
                        pagadoBorrado = true;
                    }
                }

                else if (son.name.Contains("EspacioCamarero(Clone)") || son.name.Contains("EspacioBarraCamarero(Clone)"))
                {
                    TMP_Text[] textsEspacio = son.GetComponentsInChildren<TMP_Text>();
                    nombrePlatos = nombrePlatos + textsEspacio[0].text + ";";
                    nPlatos = nPlatos + textsEspacio[1].text + ";";
                    float precioPlato = ExtractFloat(textsEspacio[2].text);
                    precio = precio + precioPlato;
                    huboPlatos = true;
                }

                i++;
            }
        }

        Debug.Log($"[CuadroResetear] mesa={numeroMesa} id={id} huboPlatos={huboPlatos} precio={precio} platos={nombrePlatos} n={nPlatos}");

        // Send data to database registros pedidos — solo si de verdad hubo platos.
        if (huboPlatos)
        {
            RegistrosPedidosToServer registrosPedidosToServer = GetComponent<RegistrosPedidosToServer>();
            int mesaNum = int.Parse(numeroMesa);
            string empresaId = mesaNum >= 1000 ? DataBaseEmpresasDeliveryTPV.idEmpresa : null;
            registrosPedidosToServer.SendDataToServer(id, precio, numeroMesa, nombrePlatos, nPlatos, empresaId);
        }


        // --- Call the ResetMesaHandler on the local player ---
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer != null)
        {
            var resetHandler = localPlayer.GetComponent<ResetMesaHandler>();
            if (resetHandler != null)
            {
                resetHandler.CmdResetearMesa(id, int.Parse(numeroMesa));
            }
            else
            {
                Debug.LogError("ResetMesaHandler not found on local player.");
            }
        }
        else
        {
            Debug.LogError("Local player not found.");
        }

        // Cerrar Detalle Mesa
        GameObject.FindWithTag("camareroMesas").GetComponent<CrearCamarero>().clickClose();

        gameObject.SetActive(false); // ahora sí, todo el trabajo ya está lanzado
    }

    public void DeletePagado(string id, int tableNumber)
    {
        // Runs on a persistent object so this coroutine is NOT killed
        // when this dialog's GameObject gets SetActive(false) right after.
        NetworkCoroutineRunner.Instance.StartCoroutine(DeletePagadoCoroutine(id, tableNumber));
    }

    private IEnumerator DeletePagadoCoroutine(string id, int tableNumber)
    {
        DeletePagadoData data = new DeletePagadoData
        {
            id = id,
            table_number = tableNumber
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[DeletePagado] Request STARTING, payload: " + json);

        using (UnityWebRequest webRequest = new UnityWebRequest(deleteUrl, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            Debug.Log("[DeletePagado] Request FINISHED, code: " + webRequest.responseCode);

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error deleting item: " + webRequest.error);
                Debug.LogError("Server Response: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log("Item deleted successfully!");
                Debug.Log("Server Response: " + webRequest.downloadHandler.text);
            }
        }
    }


    private float ExtractFloat(string input)
    {
        // Set to a specific culture that uses a comma as a decimal separator (for example, Spanish)
        CultureInfo culture = new CultureInfo("es-ES");
        string decimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator;

        // Use regex to remove all characters except digits and the decimal separator
        string sanitizedInput = Regex.Replace(input, @"[^\d" + Regex.Escape(decimalSeparator) + "]", "");

        if (float.TryParse(sanitizedInput, NumberStyles.Float, culture, out float result))
        {
            return result;
        }
        Debug.LogError("Failed to extract float from input: " + input);
        return 0;
    }
}

[System.Serializable]
public class DeletePagadoData
{
    public string id;
    public int table_number;
}