using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mirror;

public class MesaContentPreviaSync : MonoBehaviour
{
    // Cache the Content transform under ScrollMesa
    private Transform contentParent;

    private MenuPedir MP;

    private Button buttonPedir;

    // Prefabs
    public GameObject espacioCamareroPrefab;
    public GameObject prefabEspacioCamarero;
    private GameObject prefabEspacioCamareroBarra;
    public GameObject prefabEspacio;
    public GameObject prefabEspacioPrevia;
    public GameObject prefabEspacioBarra;
    public GameObject prefabOptionPedido;
    public GameObject prefabPedidosRealizados;
    public GameObject prefabPlatoPedido;
    public GameObject prefabPagarPlato;
    public GameObject prefabPagarTotal;
    public GameObject prefabAñadirPropina;
    public GameObject prefabOptionTextEspacio;

    // Instances
    private GameObject prefabTotalInstance;
    private GameObject prefabPropinaInstance;
    private GameObject prefabEspacioInstance;

    public Color colorOtroPedido;

    void Awake()
    {
        // Find "Scroll View/Viewport/Content" under this ScrollMesa
        var scrollView = transform.Find("Scroll View");
        if (scrollView == null)
        {
            Debug.LogError("[MesaContentSync] Couldn't find 'Scroll View' in " + name);
            return;
        }

        var viewport = scrollView.Find("Viewport");
        if (viewport == null)
        {
            Debug.LogError("[MesaContentSync] Couldn't find 'Viewport' in " + name);
            return;
        }

        contentParent = viewport.Find("Content");
        if (contentParent == null)
        {
            Debug.LogError("[MesaContentSync] Couldn't find 'Content' under Viewport in " + name);
            return;
        }
    }


    public void SetContentClientePrevia(MesaDataPrevia data, int mesaNumber)
    {
        // Clear existing previa rows before rebuilding
        GameObject contentPedido = GameObject.FindGameObjectWithTag("contentPedido");
        if (contentPedido == null)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.CompareTag("contentPedido")) { contentPedido = obj; break; }
            }
        }

        if (contentPedido != null)
        {
            foreach (Transform child in contentPedido.transform.Cast<Transform>().ToList())
            {
                if (child.name == "EspacioPrevia(Clone)")
                    DestroyImmediate(child.gameObject); // ← immediate, not deferred
            }
        }

        // Fuentes
        string rutafuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral + " SDF");

        // // Create platos (based on nEspacios)  // alomejor no va
        // int count = data.nEspacios;
        // Create platos (based on espacios)
        int count = data.nombrePlatoString.Length;
        for (int i = 0; i < count; i++)
        {
            int myConnId = NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().myConnectionId;
            if (data.ownerConnectionId[i] == myConnId) continue;

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                prefabEspacioInstance = Instantiate(prefabEspacioPrevia, transform.position, Quaternion.identity);
            }
            else
            {
                prefabEspacioInstance = Instantiate(prefabEspacioBarra, transform.position, Quaternion.identity);
            }
            // Look for canvasPedido as parent
            if (contentPedido == null)
            {
                GameObject[] allContentPedido = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allContentPedido)
                {
                    if (obj.CompareTag("contentPedido"))
                    {
                        contentPedido = obj;
                        break;
                    }
                }
            }
            prefabEspacioInstance.transform.SetParent(contentPedido.transform, false);

            // Move the instantiated object to the desired position in the hierarchy - right to the bottom
            //prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount);
            //prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount - 1);
            prefabEspacioInstance.transform.SetSiblingIndex(0);

            // Dar formato al prefab
            RectTransform prefabEspacioRect = prefabEspacioInstance.GetComponent<RectTransform>();
            prefabEspacioRect.localScale = new Vector3(1, 1, 1);
            prefabEspacioRect.offsetMin = new Vector2(0, 0);

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                prefabEspacioRect.offsetMax = new Vector2(550, 550);
            }
            else
            {
                prefabEspacioRect.offsetMax = new Vector2(720, 120);
            }

            TMP_Text[] textsEspacio = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            textsEspacio[0].text = data.nombrePlatoString[i];

            string c = data.cantidadPlatoString[i];
            string p = data.precioPlatoString[i].Replace("€", "").Replace(",", ".");

            if (float.TryParse(p, NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float precio))
                textsEspacio[1].text = precio.ToString("0.00").Replace(".", ",") + "€";
            else
                textsEspacio[1].text = "Error de datos";

            textsEspacio[3].text = data.cantidadPlatoString[i];

            textsEspacio[5].text = data.togglePlato[i].ToString();

            // Fuentes
            textsEspacio[0].font = fuenteGeneral;
            textsEspacio[1].font = fuenteGeneral;
            textsEspacio[2].font = fuenteGeneral;
            textsEspacio[3].font = fuenteGeneral;
            textsEspacio[4].font = fuenteGeneral;
            textsEspacio[5].font = fuenteGeneral;
            textsEspacio[6].font = fuenteGeneral;


            //// Desactivar imagen cantidad
            //prefabEspacioInstance.GetComponentInChildren<Image>().color = colorOtroPedido;

            //// Desactivar botones
            //prefabEspacioInstance.transform.Find("FixedContainer/Cantidad/Plus1").gameObject.SetActive(false);
            //prefabEspacioInstance.transform.Find("FixedContainer/Cantidad/Numero1/Minus1").gameObject.SetActive(false);
            //prefabEspacioInstance.transform.Find("FixedContainer/Cantidad/Numero1/ImageBasura").gameObject.SetActive(false);

            // AGREGAR LAS OPCIONES DEL PEDIDO
            RectTransform fixedContainer = prefabEspacioInstance.transform.GetChild(0) as RectTransform;
            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                // Reduce height of fixed container
                Vector2 size = fixedContainer.sizeDelta;
                size.y = 250; // Set height
                fixedContainer.sizeDelta = size;
            }

            GameObject lastOptionInstance = null; // Track the last one
            // Prepare to collect all pair.Value strings
            int j = 1;
            string[] opciones = data.opcionesPlato[i].Split(',');
            RectTransform rt = textsEspacio[4].GetComponent<RectTransform>(); // texto "pedido por otra persona"
            if (opciones[0] != "") // Comprobar que hay opciones
            {
                foreach (string opcion in opciones)
                {
                    // add prefab option groups
                    GameObject prefabOptionTextEspacioInstance = Instantiate(prefabOptionTextEspacio, transform.position, Quaternion.identity);
                    prefabOptionTextEspacioInstance.transform.SetParent(prefabEspacioInstance.transform.GetChild(1), false);

                    // put texts
                    prefabOptionTextEspacioInstance.GetComponent<TMP_Text>().text = StripPrice(opcion.Trim());

                    // manually increase height of Espacio (clone)
                    if (SceneManager.GetActiveScene().name == "MobileScene")
                    {
                        float sum = 550 + 200 * j;
                        prefabEspacioRect.offsetMax = new Vector2(sum, sum);
                    }
                    else
                    {
                        float sum = 120 + 50 * j;
                        prefabEspacioRect.offsetMax = new Vector2(sum, sum);
                    }

                    // Bajar texto otra persona
                    Vector2 pos = rt.anchoredPosition;
                    pos.y = -300 - 200 * j;
                    rt.anchoredPosition = pos;

                    // Save this as the last created option instance
                    lastOptionInstance = prefabOptionTextEspacioInstance;

                    j++;
                }

                //// Now set Cantidad as a child of the last instantiated option
                //RectTransform cantidad = fixedContainer.transform.GetChild(3) as RectTransform;

                //// Re-parent to the last created option instance
                //cantidad.SetParent(lastOptionInstance.transform, false);

                //// Set anchored position to x = -150, y = 0 (adjust y if needed)
                //if (SceneManager.GetActiveScene().name == "MobileScene")
                //{
                //    cantidad.anchoredPosition = new Vector2(-150, 0);
                //}
                //else
                //{
                //    cantidad.anchoredPosition = new Vector2(220, 60);
                //}
            }
            else
            {
                //RectTransform cantidad = fixedContainer.transform.GetChild(3) as RectTransform;

                //// Set anchored position to x = -150, y = 0 (adjust y if needed)
                //if (SceneManager.GetActiveScene().name == "MobileScene")
                //{
                //    cantidad.anchoredPosition = new Vector2(-150, 0);
                //}
                //else
                //{
                //    cantidad.anchoredPosition = new Vector2(220, 60);
                //}
            }
        }

        // Hacer la suma total
        MP = GameObject.Find("MenuPedir")?.GetComponent<MenuPedir>();

        // Capture previous total before recalculating
        string previousTotal = MP.textPrecioTotal.text;

        MP.HacerSumaPedidos();

        // Sync precioTotal2 visibility if a prior order has already been made
        if (MP.primerPedidoHecho2)
        {
            MP.precioTotal2.SetActive(true);
            MP.precioTotal2.GetComponentInChildren<TMP_Text>().text = previousTotal;
            MP.precioTotal2.GetComponentInChildren<TMP_Text>().font =
                Resources.Load<TMP_FontAsset>("Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", ""));
        }
        else if (MP.primerPedidoHecho)
        {
            MP.precioTotal2.SetActive(true);
            MP.precioTotal2.GetComponentInChildren<TMP_Text>().text = previousTotal;
            MP.primerPedidoHecho2 = true;
            MP.primerPedidoHecho = false;
        }

        // Habilitar boton pedir
        buttonPedir = GameObject.FindGameObjectWithTag("buttonPedir")?.GetComponent<Button>();
        if (buttonPedir == null)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allGameObjects)
            {
                if (obj.CompareTag("buttonPedir"))
                {
                    buttonPedir = obj.GetComponent<Button>();
                    break;
                }
            }
        }

        buttonPedir.interactable = true;
    }

    private string StripPrice(string option)
    {
        // Removes patterns like "+0.00€", "+2.10€", "-1.50€" etc.
        int plusIndex = option.LastIndexOf('+');
        int minusIndex = option.LastIndexOf('-');

        int cutIndex = -1;
        if (plusIndex > 0) cutIndex = plusIndex;
        if (minusIndex > cutIndex) cutIndex = minusIndex;

        if (cutIndex > 0)
            return option.Substring(0, cutIndex).Trim();

        return option;
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
