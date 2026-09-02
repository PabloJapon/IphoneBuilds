using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EnviarDatosQrs : MonoBehaviour
{
    public string url;  // URL base del servidor

    // campos de entrada para la informacion que hay que llevar a la base de datos
    public TMP_InputField mensajeQr;
    public TMP_Dropdown letraQr;
    public TMP_Dropdown sizeLetraQr;
    public Image colorLetraQr; 
    public Image colorFondoQr;
    public Toggle myToggle;

    // Lista de opciones (tipografías)
    public FontImageList fontImageList; // Referencia al ScriptableObject

    public Button enviarButton2;  // El botón que enviará los datos de Personalizar QRs
    
    private void Start()
    {
        // Asignar la función OnButtonClick al evento onClick del botón
        enviarButton2.onClick.AddListener(OnButtonClick2);
    }

        
    // Función que se llama cuando el botón1 es pulsado
    public void OnButtonClick2()
    {
        // 1. Textos
        string id = LoginManagerResponsable.restaurantID; 
        string mensaje_qr = mensajeQr.text;
 
        // 2. Colores
        Color color_letra_qr = colorLetraQr.color;
        string col_letra_qr = ColorToHex(color_letra_qr);

        Color color_fondo_qr = colorFondoQr.color;
        string col_fondo_qr = ColorToHex(color_fondo_qr);

        // Dropdowns
        int size_letra_qr=-1;
        if (sizeLetraQr.value == 0)
        {
            size_letra_qr = 100;
        }
        else if (sizeLetraQr.value == 1)
        {
            size_letra_qr = 130;
        }
        else if (sizeLetraQr.value == 2)
        {
            size_letra_qr = 160;
        }

        string letra_qr="aa";
        letra_qr = fontImageList.fontNames[letraQr.value];

        // Toggle mensaje
        int if_mensaje_qr = -1;
        if (myToggle.isOn == true)
        {
            if_mensaje_qr = 1;
        } 
        else
        {
            if_mensaje_qr = 0;
        }


        // Crear un JSON con los campos actualizados
        string jsonData = $"{{\"mensaje_qr\":\"{mensaje_qr}\",\"if_mensaje_qr\":\"{if_mensaje_qr}\",\"col_letra_qr\":\"{col_letra_qr}\",\"col_fondo_qr\":\"{col_fondo_qr}\",\"size_letra_qr\":\"{size_letra_qr}\",\"letra_qr\":\"{letra_qr}\",\"id\":\"{id}\"}}";

        // Iniciar la coroutine para enviar los datos
        StartCoroutine(EnviarDatosRequest("/update",jsonData));

        
    }


    private string ColorToHex(Color color)
    {
        return "#" +  ColorUtility.ToHtmlStringRGB(color);
    }

    private IEnumerator EnviarDatosRequest(string endpoint, string jsonData)
    {
        // Crear la solicitud POST
        UnityWebRequest request = new UnityWebRequest(url + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Enviar la solicitud y esperar la respuesta
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError("Error al enviar datos: " + request.error);
        }
        else
        {
            Debug.Log("Respuesta: " + request.downloadHandler.text);
        }
    }
}
 