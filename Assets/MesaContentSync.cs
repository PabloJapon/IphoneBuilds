using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mirror;

public class MesaContentSync : MonoBehaviour
{
    private Transform contentParent;

    private MenuPedir MP;
    private GameObject contentPedido;

    private GameObject mesa;

    // Prefabs
    public GameObject espacioCamareroPrefab;
    public GameObject prefabEspacioCamarero;
    private GameObject prefabEspacioCamareroBarra;
    public GameObject prefabEspacio;
    public GameObject prefabEspacioBarra;
    public GameObject prefabOptionPedido;
    public GameObject prefabPedidosRealizados;
    public GameObject prefabPlatoPedido;
    public GameObject prefabPagarPlato;
    public GameObject prefabPagarTotal;
    public GameObject prefabAñadirPropina;
    public GameObject prefabCamareroOrdenHeader;

    // Instances
    private GameObject prefabTotalInstance;
    private GameObject prefabPropinaInstance;

    public Color colorNotaCamarero;

    void Awake()
    {
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

    public void SetContentCamarero(MesaData data, int mesaNumber)
    {
        int count = data.nEspacios;

        // Fuentes
        string rutaFuenteCamarero = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
        TMP_FontAsset fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFuenteCamarero);
        if (fuenteCamarero == null)
            fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFuenteCamarero + " SDF");

        // Sorting
        bool hasOrden = data.ordenPlato != null && data.ordenPlato.Any(o => o > 0);
        string[] ordenLabels = { "-------   Sin orden   -------", "-----------   1º   -----------", "-----------   2º   -----------", "-----------   3º   -----------" };
        List<int> sortedIndices = hasOrden
            ? Enumerable.Range(0, count)
                .OrderBy(i => data.ordenPlato[i] == 0 ? 999 : data.ordenPlato[i])
                .ThenBy(i => i)
                .ToList()
            : Enumerable.Range(0, count).ToList();

        int lastOrden = -1;

        foreach (int i in sortedIndices)
        {
            // Section header
            Transform contentT = null;
            Transform targetHeader = null;

            if (hasOrden)
            {
                int orden = i < data.ordenPlato.Length ? data.ordenPlato[i] : 0;
                CrearCamarero.mesasDictionary.TryGetValue(mesaNumber, out mesa);
                contentT = mesa.transform.GetChild(0).GetChild(0).GetChild(0);

                string expectedText = (orden > 0 && orden < ordenLabels.Length) ? ordenLabels[orden] : "Sin orden";
                string headerCloneName = prefabCamareroOrdenHeader.name + "(Clone)";

                // Look for an existing header matching this orden
                foreach (Transform child in contentT)
                {
                    if (!child.name.StartsWith(headerCloneName)) continue;

                    TMP_Text existingHeaderText = child.GetComponentInChildren<TMP_Text>();
                    if (existingHeaderText != null && existingHeaderText.text == expectedText)
                    {
                        targetHeader = child;
                        break;
                    }
                }

                if (targetHeader == null)
                {
                    GameObject header = Instantiate(prefabCamareroOrdenHeader, contentT, false);
                    TMP_Text headerText = header.GetComponentInChildren<TMP_Text>();
                    headerText.text = expectedText;
                    headerText.font = fuenteCamarero;
                    targetHeader = header.transform;
                }

                lastOrden = orden;
            }

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                prefabEspacioCamareroBarra = Instantiate(prefabEspacioCamarero, transform.position, Quaternion.identity);

                RectTransform prefabEspacioRect = prefabEspacioCamareroBarra.GetComponent<RectTransform>();
                prefabEspacioRect.localScale = new Vector3(1, 1, 1);
                prefabEspacioRect.offsetMin = new Vector2(0, 0);
                prefabEspacioRect.offsetMax = new Vector2(0, 300);
                prefabEspacioRect.localPosition = new Vector3(0, 600 - i * 300, 0);
            }
            else
            {
                prefabEspacioCamareroBarra = Instantiate(prefabEspacioBarra, transform.position, Quaternion.identity);
            }

            CrearCamarero.mesasDictionary.TryGetValue(mesaNumber, out mesa);
            if (contentT == null)
                contentT = mesa.transform.GetChild(0).GetChild(0).GetChild(0);

            prefabEspacioCamareroBarra.transform.SetParent(contentT, false);

            if (targetHeader != null)
            {
                // Find the end of this header's section: right before the next header (or end of list)
                string headerCloneName = prefabCamareroOrdenHeader.name + "(Clone)";
                int headerIndex = targetHeader.GetSiblingIndex();
                int insertIndex = headerIndex + 1;

                for (int s = headerIndex + 1; s < contentT.childCount; s++)
                {
                    Transform sibling = contentT.GetChild(s);
                    if (sibling == prefabEspacioCamareroBarra.transform) continue;
                    if (sibling.name.StartsWith(headerCloneName)) break;
                    insertIndex = s + 1;
                }

                prefabEspacioCamareroBarra.transform.SetSiblingIndex(insertIndex);
            }

            TMP_Text[] texts = prefabEspacioCamareroBarra.GetComponentsInChildren<TMP_Text>();
            texts[0].text = data.nombrePlatoString[i];
            texts[1].text = data.cantidadPlatoString[i];
            texts[2].text = data.precioPlatoString[i];

            int capturedIndex = i;
            string capturedNombre = data.nombrePlatoString[i];
            int capturedCantidad = int.Parse(data.cantidadPlatoString[i]);
            int capturedBatchId = data.batchIdPlato != null && capturedIndex < data.batchIdPlato.Length ? data.batchIdPlato[capturedIndex] : 0;
            int localIndexInBatch = GetLocalIndexInBatch(data.batchIdPlato, capturedIndex, capturedBatchId);

            DishTag dishTag = prefabEspacioCamareroBarra.AddComponent<DishTag>();
            dishTag.batchId = capturedBatchId;
            dishTag.localIndex = localIndexInBatch;

            Toggle toggle = prefabEspacioCamareroBarra.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                bool alreadyDelivered = data.estadoPlato != null &&
                                        data.estadoPlato.Length > i &&
                                        data.estadoPlato[i] == 3;

                toggle.SetIsOnWithoutNotify(alreadyDelivered);

                if (alreadyDelivered)
                {
                    toggle.interactable = false;
                }
                else
                {
                    toggle.onValueChanged.AddListener((bool isOn) =>
                    {
                        if (!isOn)
                        {
                            toggle.SetIsOnWithoutNotify(true);
                            return;
                        }

                        var player = NetworkClient.connection.identity.GetComponent<MyRoomPlayer>();
                        if (player == null) { Debug.LogError("[Toggle] player is null!"); return; }
                        player.CmdUpdateDishState(player.RestaurantID, mesaNumber, capturedBatchId, localIndexInBatch, capturedNombre, capturedCantidad, data.opcionesPlato[capturedIndex], 3);
                        // Mesa turning grey when fully delivered is now decided authoritatively
                        // on the server (MesaStateManager.UpdateDishState), from the real
                        // estadoPlato array — not by one client guessing from row colors.
                    });
                }
            }

            texts[0].font = fuenteCamarero;
            texts[1].font = fuenteCamarero;

            // Restore yellow highlight for direct-to-waiter dishes on late join
            if (data.estadoPlato != null && data.estadoPlato.Length > i && data.estadoPlato[i] == 2)
            {
                Image rowImage = prefabEspacioCamareroBarra.GetComponent<Image>();
                if (rowImage != null && ColorUtility.TryParseHtmlString("#FFC368", out Color amarillo))
                {
                    rowImage.color = amarillo;
                }
            }

            // Options
            string[] opciones = data.opcionesPlato[i].Split(',');

            int j = 1;
            foreach (string opcion in opciones)
            {
                if (!string.IsNullOrWhiteSpace(opcion))
                {
                    if (SceneManager.GetActiveScene().name == "MobileScene")
                    {
                        GameObject optionPedido = Instantiate(prefabOptionPedido, prefabEspacioCamareroBarra.transform.GetChild(2));
                        optionPedido.GetComponent<TMP_Text>().text = StripPrice(opcion.Trim());
                        float sum = 280 + 140 * j;
                        prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta = new Vector2(sum, sum);
                    }
                    else
                    {
                        GameObject optionPedido = Instantiate(prefabOptionPedido, prefabEspacioCamareroBarra.transform.GetChild(2));
                        optionPedido.GetComponent<TMP_Text>().text = StripPrice(opcion.Trim());
                        optionPedido.GetComponent<TMP_Text>().font = fuenteCamarero;
                        optionPedido.GetComponent<TMP_Text>().fontSize = 20;

                        RectTransform rt = optionPedido.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 50);

                        float sum = 90 + 30 * j;
                        prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta = new Vector2(sum, sum);
                    }

                    j++;
                }
            }

            if (data.notaPlato != null && i < data.notaPlato.Length && !string.IsNullOrWhiteSpace(data.notaPlato[i]))
            {
                Transform notaDisplay = prefabEspacioCamareroBarra.transform.Find("NotaDisplay");
                if (notaDisplay != null)
                {
                    notaDisplay.gameObject.SetActive(true);
                    TMP_Text notaTxt = notaDisplay.GetComponent<TMP_Text>();
                    notaTxt.text = "Nota: " + data.notaPlato[i];
                    notaTxt.font = fuenteCamarero;
                    notaTxt.color = colorNotaCamarero;

                    if (SceneManager.GetActiveScene().name == "MobileScene")
                    {
                        float sum = 280 + 140 * j;
                        prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta = new Vector2(sum, sum);
                    }
                    else if (SceneManager.GetActiveScene().name == "TPVScene")
                    {
                        RectTransform notaRt = notaDisplay.GetComponent<RectTransform>();
                        notaRt.sizeDelta = new Vector2(notaRt.sizeDelta.x, 40);

                        float currentHeight = prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta.y;
                        prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta = new Vector2(
                            prefabEspacioCamareroBarra.GetComponent<RectTransform>().sizeDelta.x,
                            currentHeight + 20);
                    }
                }
            }
        }

        if (count > 0)
        {
            CrearCamarero crearCamarero = FindObjectOfType<CrearCamarero>();
            crearCamarero.SetMesaButtonsInteractable((int)mesaNumber, true);
        }

        var localPlayer = NetworkClient.connection?.identity?.GetComponent<MyRoomPlayer>();
        if (localPlayer != null)
            MesaStateManager.instance.SetLocalMesaContent(localPlayer.RestaurantID, mesaNumber, data);
    }


    public void SetContentCliente(MesaData data)
    {
        string rutafuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "").Replace(" ", "");
        TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral + " SDF");

        MP = GameObject.Find("MenuPedir")?.GetComponent<MenuPedir>();
        contentPedido = GameObject.FindGameObjectWithTag("contentPedido");
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

        foreach (Transform espacio in contentPedido.transform.Cast<Transform>().ToList())
        {
            if (espacio.name == "Espacio(Clone)" || espacio.name == "EspacioPrevia(Clone)")
            {
                Destroy(espacio.gameObject);
            }
        }

        GameObject pedidosRealizadosInstance = null;
        if (MP.primerPedidoHecho2 == false)
        {
            pedidosRealizadosInstance = Instantiate(prefabPedidosRealizados, transform.position, Quaternion.identity);
            pedidosRealizadosInstance.transform.SetParent(contentPedido.transform, false);
            RectTransform pedidosRealizadosRect = pedidosRealizadosInstance.GetComponent<RectTransform>();
            pedidosRealizadosRect.localScale = new Vector3(1, 1, 1);
            pedidosRealizadosInstance.transform.SetSiblingIndex(0);

            foreach (TMP_Text texto in pedidosRealizadosInstance.GetComponentsInChildren<TMP_Text>(true))
            {
                texto.font = fuenteGeneral;
            }
        }

        int desiredIndex = contentPedido.transform.childCount - 2;
        for (int i = 0; i < data.nombrePlatoString.Length; i++)
        {
            GameObject platoPedidoInstance = Instantiate(prefabPlatoPedido, transform.position, Quaternion.identity);
            platoPedidoInstance.transform.SetParent(contentPedido.transform, false);
            RectTransform platoPedidoRect = platoPedidoInstance.GetComponent<RectTransform>();
            platoPedidoRect.localScale = new Vector3(1, 1, 1);
            platoPedidoInstance.transform.SetSiblingIndex(desiredIndex + i);

                int batchIdForTag = data.batchIdPlato != null && i < data.batchIdPlato.Length ? data.batchIdPlato[i] : 0;
                DishTag dishTag = platoPedidoInstance.AddComponent<DishTag>();
                dishTag.batchId = batchIdForTag;
                dishTag.localIndex = GetLocalIndexInBatch(data.batchIdPlato, i, batchIdForTag);

            TMP_Text[] texts = platoPedidoInstance.GetComponentsInChildren<TMP_Text>();

            TMP_Text textoAdaptado = texts[0];
            textoAdaptado.enableWordWrapping = true;
            textoAdaptado.overflowMode = TextOverflowModes.Ellipsis;
            textoAdaptado.maxVisibleLines = 1;

            texts[0].text = data.nombrePlatoString[i];
            texts[1].text = data.precioPlatoString[i];
            texts[2].text = data.cantidadPlatoString[i];

            texts[0].font = fuenteGeneral;
            texts[1].font = fuenteGeneral;
            texts[2].font = fuenteGeneral;
            texts[3].font = fuenteGeneral;

            string[] opciones = data.opcionesPlato[i].Split(',');

            int j = 1;
            foreach (string opcion in opciones)
            {
                if (!string.IsNullOrWhiteSpace(opcion) && SceneManager.GetActiveScene().name == "MobileScene")
                {
                    GameObject optionPedido = Instantiate(prefabOptionPedido, platoPedidoInstance.transform.GetChild(4));
                    optionPedido.GetComponent<TMP_Text>().text = StripPrice(opcion.Trim());
                    float sum = 280 + 140 * j;
                    platoPedidoInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(sum, sum);

                    j++;
                }
            }
        }

        MP.PedidoSameMesa();

        GameObject contentPagar = GameObject.FindGameObjectWithTag("contentPagar");
        if (contentPagar == null)
        {
            contentPagar = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.CompareTag("contentPagar"));
        }
        TMP_Text amountText = GameObject.FindGameObjectWithTag("amountText").GetComponent<TMP_Text>();
        PaymentHandler PH = GameObject.Find("Payment").GetComponent<PaymentHandler>();

        float totalSum = 0;
        for (int i = 0; i < data.nEspacios; i++)
        {
            GameObject prefabEspacioInstance = Instantiate(prefabPagarPlato, transform.position, Quaternion.identity);

            prefabEspacioInstance.transform.SetParent(contentPagar.transform, false);

            TMP_Text[] texts = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            texts[0].text = data.nombrePlatoString[i];
            texts[1].text = data.cantidadPlatoString[i];
            texts[2].text = data.precioPlatoString[i].Replace("€", " €");

            texts[0].font = fuenteGeneral;
            texts[1].font = fuenteGeneral;
            texts[2].font = fuenteGeneral;

            totalSum += ExtractFloat(data.precioPlatoString[i]);
        }

        if (GameObject.FindGameObjectWithTag("pagarTotal") == null)
        {
            prefabTotalInstance = Instantiate(prefabPagarTotal, transform.position, Quaternion.identity);
            prefabTotalInstance.transform.SetParent(contentPagar.transform, false);

            TMP_Text[] textsTotal = prefabTotalInstance.GetComponentsInChildren<TMP_Text>();
            textsTotal[1].text = totalSum.ToString("0.00").Replace(".", ",") + " €";

            textsTotal[0].font = fuenteGeneral;
            textsTotal[1].font = fuenteGeneral;

            amountText.text = totalSum.ToString("0.00").Replace(".", ",") + " €";

            prefabPropinaInstance = Instantiate(prefabAñadirPropina, transform.position, Quaternion.identity);
            prefabPropinaInstance.transform.SetParent(contentPagar.transform, false);

            TMP_Text[] textPropina = prefabPropinaInstance.GetComponentsInChildren<TMP_Text>();
            textPropina[0].font = fuenteGeneral;
        }
        else
        {
            if (prefabTotalInstance == null)
            {
                prefabTotalInstance = GameObject.FindGameObjectWithTag("pagarTotal");
            }
            if (prefabPropinaInstance == null)
            {
                prefabPropinaInstance = GameObject.FindGameObjectWithTag("pagarTotal");
            }

            TMP_Text[] textsTotal = prefabTotalInstance.GetComponentsInChildren<TMP_Text>();
            float sum_added = float.Parse(textsTotal[1].text.Replace(",", ".").Replace(" €", "")) / 100 + totalSum;
            textsTotal[1].text = sum_added.ToString("0.00").Replace(".", ",") + " €";

            textsTotal[1].font = fuenteGeneral;
            amountText.text = sum_added.ToString("0.00").Replace(".", ",") + " €";

            prefabTotalInstance.transform.SetAsLastSibling();
            prefabPropinaInstance.transform.SetAsLastSibling();
        }
    }

    private string StripPrice(string option)
    {
        int plusIndex = option.LastIndexOf('+');
        int minusIndex = option.LastIndexOf('-');

        int cutIndex = -1;
        if (plusIndex > 0) cutIndex = plusIndex;
        if (minusIndex > cutIndex) cutIndex = minusIndex;

        if (cutIndex > 0)
            return option.Substring(0, cutIndex).Trim();

        return option;
    }

    private int GetLocalIndexInBatch(int[] batchIdPlato, int globalIndex, int batchId)
    {
        int count = 0;
        for (int k = 0; k < globalIndex; k++)
            if (batchIdPlato[k] == batchId) count++;
        return count;
    }

    private float ExtractFloat(string input)
    {
        CultureInfo culture = new CultureInfo("es-ES");
        string decimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator;
        string sanitizedInput = Regex.Replace(input, @"[^\d" + Regex.Escape(decimalSeparator) + "]", "");

        if (float.TryParse(sanitizedInput, NumberStyles.Float, culture, out float result))
        {
            return result;
        }
        Debug.LogError("Failed to extract float from input: " + input);
        return 0;
    }

    public void ClearContent()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
    }
}