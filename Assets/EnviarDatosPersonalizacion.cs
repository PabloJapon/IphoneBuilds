using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EnviarDatosPersonalizacion : MonoBehaviour
{
    public string url;  // URL base del servidor

    // campos de entrada para la informacion que hay que llevar a la base de datos
    public TMP_InputField inputFieldNombreRest;  // Campo de entrada para el nombre del restaurante
    public TMP_InputField inputFieldNumMesas;  // Campo de entrada para el número de mesas
    public TMP_Text imagenCabeceroUrl;
    public Image colorFondoTitulo; 
    public Image colorLetraTitulo; 
    public Image colorFondoBarsec;
    public Image colorBotonBarsec;
    public TMP_Dropdown sizeLetraTitulo;
    public TMP_Dropdown letraTitulo;
    public TMP_Dropdown letraGral;
    public TMP_Dropdown letraTitulos;
    public TMP_Dropdown sizeLetraGral;
    public TMP_Dropdown sizeLetraTitulos;
    public Image colorFondoGral;
    public Image colorFondoTitulos;
    public Image colorFondoIcono;
    public Image colorPpalBotones;
    public Image colorSecBotones;
    public TMP_Dropdown Icono;
    public Image colorIconoBase;
    public Image colorIconoPulsado;
    public TMP_Dropdown Redondez;

    // Empleados
    public TMP_Dropdown letraEmpl;
    public Image colPpalEmpl;
    public Image colSecEmpl;
    public GameObject panelCocinas; // para el tema de varias cocinas

    // Lista de opciones (tipografías)
    public FontImageList fontImageList; // Referencia al ScriptableObject

    public Button enviarButton;  // El botón que enviará los datos de Personalizar Apps

       private void Start()
    {
        // Asignar la función OnButtonClick al evento onClick del botón
        enviarButton.onClick.AddListener(OnButtonClick1);
    }

    // Función que se llama cuando el botón1 es pulsado
    public void OnButtonClick1()
    {
        // 1. Textos
        string nombre_rest = inputFieldNombreRest.text;  // Obtener el valor del input field
        string num_mesas = inputFieldNumMesas.text;  // Obtener el valor del input field
        string id = LoginManagerResponsable.restaurantID; 
        string img_url_cabecero = imagenCabeceroUrl.text; 
 
        // 2. Colores
        Color color_fondo_titulo = colorFondoTitulo.color;
        string col_fondo_titulo = ColorToHex(color_fondo_titulo);

        Color color_letra_titulo = colorLetraTitulo.color;
        string col_letra_titulo = ColorToHex(color_letra_titulo);

        Color color_barra_sec = colorFondoBarsec.color;
        string col_fondo = ColorToHex(color_barra_sec);

        Color color_boton_sec = colorBotonBarsec.color;
        string col_botones = ColorToHex(color_boton_sec);

        Color color_fondo_gral = colorFondoGral.color;
        string col_fondo_gral = ColorToHex(color_fondo_gral);

        Color color_fondo_titulos = colorFondoTitulos.color;
        string col_fondo_titulos = ColorToHex(color_fondo_titulos);

        Color color_fondo_icono = colorFondoIcono.color;
        string col_fondo_icono = ColorToHex(color_fondo_icono);

        Color color_ppal_botones = colorPpalBotones.color;
        string col_ppal_botones = ColorToHex(color_ppal_botones);

        Color color_sec_botones = colorSecBotones.color;
        string col_sec_botones = ColorToHex(color_sec_botones);

        Color color_icono_base = colorIconoBase.color;
        string col_icono_base = ColorToHex(color_icono_base);

        Color color_icono_pulsado = colorIconoPulsado.color;
        string col_icono_pulsado = ColorToHex(color_icono_pulsado);

        Color color_ppal_empl = colPpalEmpl.color;
        string col_ppal_empl = ColorToHex(color_ppal_empl);

        Color color_sec_empl = colSecEmpl.color;
        string col_sec_empl = ColorToHex(color_sec_empl);

        // 3. Dropdowns
        // 3.1. Tamaño letras
        int size_letra_titulo=-1;
        if (sizeLetraTitulo.value == 0)
        {
            size_letra_titulo = 100;
        }
        else if (sizeLetraTitulo.value == 1)
        {
            size_letra_titulo = 130;
        }
        else if (sizeLetraTitulo.value == 2)
        {
            size_letra_titulo = 160;
        }

        int size_letra_gral=-1;
        if (sizeLetraGral.value == 0)
        {
            size_letra_gral = 100;
        }
        else if (sizeLetraGral.value == 1)
        {
            size_letra_gral = 130;
        }
        else if (sizeLetraGral.value == 2)
        {
            size_letra_gral = 160;
        }

        int size_letra_titulos=-1;
        if (sizeLetraTitulos.value == 0)
        {
            size_letra_titulos = 100;
        }
        else if (sizeLetraTitulos.value == 1)
        {
            size_letra_titulos = 130;
        }
        else if (sizeLetraTitulos.value == 2)
        {
            size_letra_titulos = 160;
        }

        // 3.2. Tipo letras
        
        string letra_titulo="aa"; 

        letra_titulo = fontImageList.fontNames[letraTitulo.value];
        // if (letraTitulo.value == 0)
        // {
        //     letra_titulo = "LiberationSans SDF";
        // }
        // else if (letraTitulo.value == 1)
        // {
        //     letra_titulo = "ANTQUAB SDF 1";
        // }
        // else if (letraTitulo.value == 2)
        // {
        //     letra_titulo = "BAHNSCHRIFT 1 SDF";
        // }

        string letra_gral="aa";

        letra_gral = fontImageList.fontNames[letraGral.value];
        // if (letraGral.value == 0)
        // {
        //     letra_gral = "LiberationSans SDF";
        // }
        // else if (letraGral.value == 1)
        // {
        //     letra_gral = "ANTQUAB SDF 1";
        // }
        // else if (letraGral.value == 2)
        // {
        //     letra_gral = "BAHNSCHRIFT 1 SDF";
        // }

        string letra_titulos="aa";

        letra_titulos = fontImageList.fontNames[letraTitulos.value];
        // if (letraTitulos.value == 0)
        // {
        //     letra_titulos = "LiberationSans SDF";
        // }
        // else if (letraTitulos.value == 1)
        // {
        //     letra_titulos = "ANTQUAB SDF 1";
        // }
        // else if (letraTitulos.value == 2)
        // {
        //     letra_titulos = "BAHNSCHRIFT 1 SDF";
        // }

        string letra_empl="aa";

        letra_empl = fontImageList.fontNames[letraEmpl.value];
        // if (letraEmpl.value == 0)
        // {
        //     letra_empl = "LiberationSans SDF";
        // }
        // else if (letraEmpl.value == 1)
        // {
        //     letra_empl = "ANTQUAB SDF 1";
        // }
        // else if (letraEmpl.value == 2)
        // {
        //     letra_empl = "BAHNSCHRIFT 1 SDF";
        // }

        // 3.3. Iconos
        int icono=Icono.value;

        // 3.4 Redondez esquinas
        //int redondez_gral=-1;
        //if (Redondez.value == 0)
        //{
        //redondez_gral = 0;
        //}
        //else if (Redondez.value == 1)
        //{
        //redondez_gral = 65;
        //}
        //else if (Redondez.value == 2)
        //{
        //redondez_gral = 130;
        //}

        // 4. Varias cocinas
        TMP_InputField[] arrayCocinas = panelCocinas.GetComponentsInChildren<TMP_InputField>();
        string cocinas = string.Join(";", arrayCocinas.Select(c => c.text).ToArray());
        

        // Crear un JSON con los campos actualizados
        string jsonData = $"{{\"nombre_rest\":\"{nombre_rest}\",\"num_mesas\":\"{num_mesas}\",\"img_url_cabecero\":\"{img_url_cabecero}\",\"col_fondo_titulo\":\"{col_fondo_titulo}\",\"col_letra_titulo\":\"{col_letra_titulo}\",\"col_fondo\":\"{col_fondo}\",\"col_botones\":\"{col_botones}\",\"size_letra_titulo\":\"{size_letra_titulo}\",\"letra_titulo\":\"{letra_titulo}\",\"letra_gral\":\"{letra_gral}\",\"letra_titulos\":\"{letra_titulos}\",\"size_letra_gral\":\"{size_letra_gral}\",\"size_letra_titulos\":\"{size_letra_titulos}\",\"col_fondo_gral\":\"{col_fondo_gral}\",\"col_fondo_titulos\":\"{col_fondo_titulos}\",\"col_fondo_icono\":\"{col_fondo_icono}\",\"col_ppal_botones\":\"{col_ppal_botones}\",\"col_sec_botones\":\"{col_sec_botones}\",\"icono\":\"{icono}\",\"col_icono_base\":\"{col_icono_base}\",\"col_icono_pulsado\":\"{col_icono_pulsado}\",\"letra_empl\":\"{letra_empl}\",\"col_ppal_empl\":\"{col_ppal_empl}\",\"col_sec_empl\":\"{col_sec_empl}\",\"cocinas\":\"{cocinas}\",\"id\":\"{id}\"}}";

        StartCoroutine(EnviarDatosRequest("/update",jsonData));

        // Además actualizamos las variables en las que hemos guardado los datos de la DB importada
        // Porque al darle a No Guardar tiramos de esos datos para rellenar los campos, y no tienen porqué estar actualizados
        // Iniciar la coroutine para enviar los datos
        DataBasePersonalizacionRespScene.nombre_rest[0]=nombre_rest;
        DataBasePersonalizacionRespScene.num_mesas[0]=int.Parse(num_mesas);
        DataBasePersonalizacionRespScene.img_url_cabecero[0]=img_url_cabecero;
        DataBasePersonalizacionRespScene.col_fondo_titulo[0]=col_fondo_titulo;
        DataBasePersonalizacionRespScene.col_letra_titulo[0]=col_letra_titulo;
        DataBasePersonalizacionRespScene.col_fondo[0]=col_fondo;
        DataBasePersonalizacionRespScene.col_botones[0]=col_botones;
        DataBasePersonalizacionRespScene.size_letra_titulo[0]=size_letra_titulo;
        DataBasePersonalizacionRespScene.letra_titulo[0]=letra_titulo;
        DataBasePersonalizacionRespScene.letra_gral[0]=letra_gral;
        DataBasePersonalizacionRespScene.letra_titulos[0]=letra_titulos;
        DataBasePersonalizacionRespScene.size_letra_gral[0]=size_letra_gral;
        DataBasePersonalizacionRespScene.size_letra_titulos[0]=size_letra_titulos;
        DataBasePersonalizacionRespScene.col_fondo_gral[0]=col_fondo_gral;
        DataBasePersonalizacionRespScene.col_fondo_titulos[0]=col_fondo_titulos;
        DataBasePersonalizacionRespScene.col_fondo_icono[0]=col_fondo_icono;
        DataBasePersonalizacionRespScene.col_ppal_botones[0]=col_ppal_botones;
        DataBasePersonalizacionRespScene.col_sec_botones[0]=col_sec_botones;
        DataBasePersonalizacionRespScene.icono[0]=icono;
        DataBasePersonalizacionRespScene.col_icono_base[0]=col_icono_base;
        DataBasePersonalizacionRespScene.col_icono_pulsado[0]=col_icono_pulsado;
        DataBasePersonalizacionRespScene.letra_empl[0]=letra_empl;
        DataBasePersonalizacionRespScene.col_ppal_empl[0]=col_ppal_empl;
        DataBasePersonalizacionRespScene.col_sec_empl[0]=col_sec_empl;
        DataBasePersonalizacionRespScene.cocinas[0]=cocinas;

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
