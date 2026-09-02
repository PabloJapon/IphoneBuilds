using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using UnityEngine.SceneManagement;
using Mirror;

public class MenuPedir : MonoBehaviour
{
    public TMP_Text inputMesa;
    public Button buttonPedir;
    public GameObject prefabEspacio;
    public GameObject prefabEspacioBarra;
    public GameObject prefabPlato;
    public GameObject prefabSeccion;
    public GameObject prefabEtiqueta;
    private GameObject prefabEspacioInstance;
    public GameObject prefabOptionTextEspacio;
    private Image fondoImage;
    private Image fondoImageSec;
    private Image[] fondoImageEti;
    private Image fondoImageEtiMovil;
    public int maxPlatoCount = 5; // Maximum number of times a plato can be chosen
    public int[] platoCount; // Array to store the count for each plato

    public TMP_Text[] numerosArray;
    public TMP_Text[] preciosArray;
    public TMP_Text[] nPlatosArray;
    public float totalSum = 0f; // Variable to hold the total sum
    public TMP_Text textPrecioTotal;
    public TMP_Text textTotal;
    public TMP_Text textTotal2;

    public GameObject imageNotificacion;
    public GameObject imageNotificacionCamarero; // circulo notificacion pestaña Pedido para camareros en movil

    // For pedidos after pedidos
    public GameObject precioTotal2;
    public GameObject precioTotal;

    // Database
    private string[] nombrePlatos;
    private float[] precioPlatos;
    private int[] togglePlatos;

    private int nEspacios;
    public int i;
    public int nPedidosEspacios;
    public Button[] buttonAdd;
    public Button[] buttonQuitar;

    public int[] numeroPlato;
    public TMP_Text[] nombrePlato;
    public string optionsPlato;
    public string[] nombrePlatoString;
    public string[] precioPlato;
    public string[] cantidadPlato;
    public int[] cantidadPlatoInt;
    public int[] toggleCamarero;
    public GameObject[] prefabsEspacio;

    public Toggle ordenActivoToggle;


    public DataBase DB; // Reference to the first DataBase component
    public DataBasePersonalizacion DB2; // Reference to the second DataBase component

    private bool isDBLoaded = false;
    private bool isDB2Loaded = false;

    // Para subir el boton pedir para próximos pedidos
    public bool primerPedidoHecho = false;
    public bool primerPedidoHecho2 = false;
    public GameObject canvasPedidoParent;

    public GameObject contentPedido;
    private GameObject pedidosRealizados;
    public Button buttonPagar;

    private int desiredIndex;
    private bool countPlatosPedidos = true;
    private int j;

    public NavigationCamarero NC;

    public Color SelectedButtonOrdenColor = new Color(0.4f, 0.6f, 1f);


    void Start()
    {
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;
        buttonPedir.interactable = false;
        buttonPagar.interactable = false;
        precioTotal2.SetActive(false);

        // Find "PedidosRealizados" even if it's disabled
        pedidosRealizados = null;
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.CompareTag("PedidosRealizados"))
            {
                pedidosRealizados = obj;
                break;
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        DB.OnDataLoaded -= OnDBLoaded;
        DB2.OnDataLoaded -= OnDB2Loaded;
    }

    private void OnDBLoaded()
    {
        isDBLoaded = true;
        CheckIfBothDatabasesAreLoaded();
    }

    private void OnDB2Loaded()
    {
        //infoText.text = "DB2";
        isDB2Loaded = true;
        CheckIfBothDatabasesAreLoaded();
    }

    // Function that checks if both databases are loaded
    private void CheckIfBothDatabasesAreLoaded()
    {
        if (isDBLoaded && isDB2Loaded)
        {
            OnDatabaseLoaded(); // Now we are sure both databases are loaded
        }
    }

    private void OnDatabaseLoaded()
    {
        // Database
        nombrePlatos = DataBase.nombrePlatos;
        precioPlatos = DataBase.precioPlatos;
        togglePlatos = DataBase.toggle;

        nEspacios = 0;
        i = 0;

        int index = DataBase.nombrePlatos.Length + 1;

        buttonAdd = new Button[index];
        buttonQuitar = new Button[index];
        numeroPlato = new int[index];
        nombrePlato = new TMP_Text[index];
        nombrePlatoString = new string[index];
        //optionsPlato = new string[index];
        precioPlato = new string[index];
        cantidadPlato = new string[index];
        cantidadPlatoInt = new int[index];
        toggleCamarero = new int[index];
        prefabsEspacio = new GameObject[index];
        platoCount = new int[index];

        // You can add more initialization logic here if needed

        // Cambiar características con la DataBase Personalización
        // 1. Colores
        fondoImage = prefabPlato.GetComponentInChildren<Image>(); // fondo de cada plato
        fondoImageSec = prefabSeccion.GetComponentInChildren<Image>(); // fondo del título de cada seccion
        //fondoImageEti = prefabEtiqueta.GetComponentsInChildren<Image>(); // fondo del título de cada seccion
        ChangeImageColor();

        if (ordenActivoToggle != null)
        {
            ordenActivoToggle.onValueChanged.AddListener((bool isOn) =>
            {
                foreach (Transform espacio in contentPedido.transform)
                {
                    if (espacio.name != "Espacio(Clone)" && espacio.name != "EspacioBarraPedido(Clone)") continue;

                    // Search entire espacio hierarchy by name — works regardless of reparenting
                    foreach (Transform t in espacio.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == "OrdenButtons")
                        {
                            t.gameObject.SetActive(isOn);
                            break;
                        }
                    }
                }
            });
        }
    }


    public void SelectVarios(string result)
    {
        buttonPedir.interactable = true;

        string rutaFuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "").Replace(" ", "");
        TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral + " SDF");

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            prefabEspacioInstance = Instantiate(prefabEspacio, transform.position, Quaternion.identity);
        }
        else
        {
            prefabEspacioInstance = Instantiate(prefabEspacioBarra, transform.position, Quaternion.identity);
        }

        prefabsEspacio[i] = prefabEspacioInstance;
        prefabEspacioInstance.transform.SetParent(contentPedido.transform, false);

        desiredIndex = 0;
        foreach (Transform espacio in contentPedido.transform)
        {
            if (espacio.name == "Espacio(Clone)")
            {
                if (espacio.GetComponent<Image>().color != Color.white)
                {
                    desiredIndex++;
                }
            }
        }

        // Move the instantiated object to the desired position in the hierarchy
        if (primerPedidoHecho2) // ya hay un pedido hecho
        {
            prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount - desiredIndex - 5);
        }
        else
        {
            prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount - desiredIndex - 3);
        }

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
            prefabEspacioRect.offsetMax = new Vector2(720, 140);
        }

        TMP_Text[] textsEspacio = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
        textsEspacio[0].text = "Varios";
        textsEspacio[0].font = fuenteGeneral;
        nombrePlato[i] = textsEspacio[0];
        nombrePlatoString[i] = textsEspacio[0].text;

        textsEspacio[1].text = result + " €";
        textsEspacio[1].font = fuenteGeneral;
        precioPlato[i] = textsEspacio[1].text;

        // mark Varios with a sentinel toggle so it's excluded everywhere
        if (textsEspacio.Length > 7)
            textsEspacio[7].text = "-1";

        //textsEspacio[4].text = platoCount[platoNumber].ToString();
        //textsEspacio[4].font = fuenteGeneral;
        cantidadPlato[i] = "1";
        cantidadPlatoInt[i] = int.Parse(textsEspacio[4].text);

        Button[] buttonsEspacio = prefabEspacioInstance.GetComponentsInChildren<Button>();
        buttonAdd[i] = buttonsEspacio[1];
        buttonQuitar[i] = buttonsEspacio[2];

        int currentEspacios = i;
        //buttonAdd[currentEspacios].onClick.AddListener(() => AddCantidadPedido());
        //buttonQuitar[currentEspacios].onClick.AddListener(() => QuitarCantidadPedido(currentEspacios, platoNumber));

        // ---- Wire Nota button (igual que SelectPlato) ----
        Button notaBtn = prefabEspacioInstance.transform.Find("FixedContainer/NotaButton")?.GetComponent<Button>();
        TMP_InputField notaInput = prefabEspacioInstance.transform.Find("FixedContainer/NotaInput")?.GetComponent<TMP_InputField>();

        if (notaBtn != null && notaInput != null)
        {
            if (Navigation.camarero == false) // Clientes no ven el boton nota
                notaBtn.gameObject.SetActive(false);

            float notaExpandHeight = 500f;
            if (SceneManager.GetActiveScene().name == "TPVScene")
                notaExpandHeight = 100f;

            GameObject capturedEspacio = prefabEspacioInstance;

            notaBtn.onClick.AddListener(() =>
            {
                bool isOpen = notaInput.gameObject.activeSelf;
                RectTransform rt = capturedEspacio.GetComponent<RectTransform>();

                if (isOpen)
                {
                    notaInput.text = "";
                    notaInput.gameObject.SetActive(false);
                    notaBtn.GetComponentInChildren<TMP_Text>().text = "+ Nota";
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y - notaExpandHeight);
                }
                else
                {
                    notaInput.gameObject.SetActive(true);
                    notaBtn.GetComponentInChildren<TMP_Text>().text = "– Nota";
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + notaExpandHeight);
                    notaInput.Select();
                }
            });
        }

        // ---- Wire orden buttons (igual que SelectPlato) ----
        Transform ordenButtonsT = prefabEspacioInstance.transform.Find("FixedContainer/OrdenButtons");
        TMP_Text ordenValueTxt = prefabEspacioInstance.transform.Find("FixedContainer/OrdenPlato")?.GetComponent<TMP_Text>();

        if (ordenButtonsT != null)
        {
            ordenButtonsT.gameObject.SetActive(ordenActivoToggle != null && ordenActivoToggle.isOn);

            if (ordenValueTxt != null)
            {
                Button[] ordenBtns = ordenButtonsT.GetComponentsInChildren<Button>();
                Color normalColor = Color.white;

                for (int b = 0; b < ordenBtns.Length && b < 3; b++)
                {
                    int capturedOrden = b + 1;
                    ordenBtns[b].onClick.AddListener(() =>
                    {
                        Color newColorBotonOrden;
                        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out newColorBotonOrden))
                            SelectedButtonOrdenColor = newColorBotonOrden;
                        ordenValueTxt.text = capturedOrden.ToString();
                        for (int x = 0; x < ordenBtns.Length; x++)
                        {
                            bool isSelected = (x == capturedOrden - 1);
                            ColorBlock cb = ordenBtns[x].colors;
                            cb.normalColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                            cb.selectedColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                            cb.highlightedColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                            ordenBtns[x].colors = cb;

                            TMP_Text btnTxt = ordenBtns[x].GetComponentInChildren<TMP_Text>();
                            if (btnTxt != null)
                                btnTxt.color = isSelected ? Color.white : Color.black;
                        }
                    });
                }
            }
        }

        imageNotificacion.SetActive(true);
        if (Navigation.camarero && SceneManager.GetActiveScene().name == "MobileScene" && imageNotificacionCamarero != null)
            imageNotificacionCamarero.SetActive(true);

        // ESTARIA BIEN QUE LA FUENTE SOLO SE CAMBIARA UNA VEZ NO CADA VEZ QUE PIDES
        textPrecioTotal.font = fuenteGeneral;
        textTotal.font = fuenteGeneral;
        textTotal2.font = fuenteGeneral;

        HacerSumaPedidos();

        // Send selected plato to sync for other clients in the mesa to see it
        MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();

        playerController.CmdSendPreviaToServer(float.Parse(inputMesa.text), new string[] { "Varios" },
            new string[] { optionsPlato }, new string[] { cantidadPlato[i] }, new string[] { precioPlato[i] }, new int[] { -1 });
    }

    public void SelectPlato()
    {
        string rutaFuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "").Replace(" ", "");
        TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral + " SDF");

        if (contentPedido.activeInHierarchy == false)
        {
            contentPedido.SetActive(true);
        }

        if (precioTotal.activeInHierarchy == false)
        {
            precioTotal.SetActive(true);
        }

        if (primerPedidoHecho) // ya hay un pedido hecho
        {
            // 2 barra total
            precioTotal2.SetActive(true);
            precioTotal2.GetComponentInChildren<TMP_Text>().text = textPrecioTotal.text;
            precioTotal2.GetComponentInChildren<TMP_Text>().font = fuenteGeneral;

            //buttonPedir.transform.SetParent(contentPedido.transform);

            j += 1;
            precioTotal.transform.SetSiblingIndex(0);
            //pedidosRealizados.transform.SetSiblingIndex(precioTotal.transform.GetSiblingIndex() + 1);
            //buttonPedir.transform.SetSiblingIndex(j);

            primerPedidoHecho = false;
            primerPedidoHecho2 = true;
        }

        int platoNumber = SceneManager.GetActiveScene().name == "TPVScene"
            ? DetallePlatoUI.xPlato
            : DetallePlato.xPlato;

        if (platoCount[platoNumber] <= 0)
            platoCount[platoNumber] = 1;

        /* Debug.Log($"[SelectPlato] platoNumber={platoNumber} platoCount={platoCount[platoNumber]}"); */

        buttonPedir.interactable = true;

        // Plato ya pedido - only merge if same plato AND same options
        Dictionary<string, string> currentOptions = SceneManager.GetActiveScene().name == "TPVScene"
            ? DetallePlatoUI.Instance.GetOptionSelections()
            : DetallePlato.Instance.GetOptionSelections();
        string currentOptionsStr = string.Join(", ", currentOptions.Values);

        int existingIndex = -1;
        for (int k = 0; k < i; k++)
        {
            if (numeroPlato[k] == platoNumber && prefabsEspacio[k] != null)
            {
                string existingOptionsStr = GetOptionsFromPrefab(prefabsEspacio[k]);
                if (existingOptionsStr == currentOptionsStr)
                {
                    existingIndex = k;
                    break;
                }
            }
        }

        if (existingIndex >= 0)
        {
            GameObject prefabEspacioInstance = prefabsEspacio[existingIndex];
            TMP_Text[] textsEspacio = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            textsEspacio[0].text = nombrePlatos[platoNumber - 1];
            textsEspacio[1].text = (platoCount[platoNumber] * precioPlatos[platoNumber - 1]).ToString("0.00").Replace(".", ",") + "€";
            precioPlato[existingIndex] = textsEspacio[1].text;
            textsEspacio[4].text = platoCount[platoNumber].ToString();
            cantidadPlato[existingIndex] = textsEspacio[4].text;
            cantidadPlatoInt[existingIndex] = int.Parse(textsEspacio[4].text);
            toggleCamarero[i] = int.Parse(textsEspacio[5].text);
        }
        else
        {
            numeroPlato[i] = platoNumber;
            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                prefabEspacioInstance = Instantiate(prefabEspacio, transform.position, Quaternion.identity);
            }
            else
            {
                prefabEspacioInstance = Instantiate(prefabEspacioBarra, transform.position, Quaternion.identity);
            }

            prefabsEspacio[i] = prefabEspacioInstance;
            prefabEspacioInstance.transform.SetParent(contentPedido.transform, false);

            desiredIndex = 0;
            foreach (Transform espacio in contentPedido.transform)
            {
                if (espacio.name == "Espacio(Clone)")
                {
                    if (espacio.GetComponent<Image>().color != Color.white)
                    {
                        desiredIndex++;
                    }
                }
            }

            // Move the instantiated object to the desired position in the hierarchy
            if (primerPedidoHecho2) // ya hay un pedido hecho
            {
                prefabEspacioInstance.transform.SetSiblingIndex(0);
                //prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount - desiredIndex - 5);
            }
            else
            {
                prefabEspacioInstance.transform.SetSiblingIndex(0);
                //prefabEspacioInstance.transform.SetSiblingIndex(contentPedido.transform.childCount - desiredIndex - 3);
            }

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
                prefabEspacioRect.offsetMax = new Vector2(720, 140);
            }

            TMP_Text[] textsEspacio = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            textsEspacio[0].text = nombrePlatos[platoNumber - 1];
            textsEspacio[0].font = fuenteGeneral;
            nombrePlato[i] = textsEspacio[0];
            nombrePlatoString[i] = textsEspacio[0].text;

            textsEspacio[1].text = (platoCount[platoNumber] * precioPlatos[platoNumber - 1]).ToString("0.00").Replace(".", ",") + " €";
            textsEspacio[1].font = fuenteGeneral;
            precioPlato[i] = textsEspacio[1].text;
            textsEspacio[4].text = platoCount[platoNumber].ToString();
            textsEspacio[4].font = fuenteGeneral;
            cantidadPlato[i] = textsEspacio[4].text;
            cantidadPlatoInt[i] = int.Parse(textsEspacio[4].text);

            textsEspacio[7].text = togglePlatos[platoNumber - 1].ToString();
            toggleCamarero[i] = int.Parse(textsEspacio[7].text);

            Button[] buttonsEspacio = prefabEspacioInstance.GetComponentsInChildren<Button>();
            buttonAdd[i] = buttonsEspacio[1];
            buttonQuitar[i] = buttonsEspacio[2];

            int currentEspacios = i;
            buttonAdd[currentEspacios].onClick.AddListener(() => AddCantidadPedido());
            buttonQuitar[currentEspacios].onClick.AddListener(() => QuitarCantidadPedido(currentEspacios, platoNumber));

            // option groups
            Dictionary<string, string> selectedOptions = SceneManager.GetActiveScene().name == "TPVScene"
                ? DetallePlatoUI.Instance.GetOptionSelections()
                : DetallePlato.Instance.GetOptionSelections();

            RectTransform fixedContainer = prefabEspacioInstance.transform.GetChild(0) as RectTransform;
            GameObject lastOptionInstance = null; // declared outside so nota wiring can access it

            if (selectedOptions.Count > 0)
            {
                if (SceneManager.GetActiveScene().name == "MobileScene")
                {
                    // Reduce height of fixed container
                    Vector2 size = fixedContainer.sizeDelta;
                    size.y = 220; // Set height
                    fixedContainer.sizeDelta = size;
                }
                // Prepare to collect all pair.Value strings
                List<string> allOptionValues = new List<string>();
                int optionIndex = 1;
                foreach (var pair in selectedOptions)
                {
                    // add prefab option groups
                    GameObject prefabOptionTextEspacioInstance = Instantiate(prefabOptionTextEspacio, transform.position, Quaternion.identity);
                    prefabOptionTextEspacioInstance.transform.SetParent(prefabEspacioInstance.transform.GetChild(1), false);

                    // put texts (strip price suffix from display, price is added to plato total separately)
                    prefabOptionTextEspacioInstance.GetComponent<TMP_Text>().text = StripPrice(pair.Value);

                    // manually increase height of Espacio (clone)

                    if (SceneManager.GetActiveScene().name == "MobileScene")
                    {
                        float sum = 520 + 130 * optionIndex;
                        prefabEspacioRect.offsetMax = new Vector2(sum, sum);
                    }
                    else
                    {
                        float sum = 140 + 35 * optionIndex;
                        prefabEspacioRect.offsetMax = new Vector2(sum, sum);
                    }

                    // Save this as the last created option instance
                    lastOptionInstance = prefabOptionTextEspacioInstance;

                    // Collect option value
                    allOptionValues.Add(pair.Value);

                    optionIndex++; // Increment counter
                }
                // After loop: Join all options and assign to optionsPlato
                optionsPlato = string.Join(", ", allOptionValues);

                // --- sum up extra prices from options ---
                float extraTotal = 0f;
                foreach (var val in allOptionValues)
                    extraTotal += ExtractOptionExtraPrice(val);

                if (extraTotal > 0f)
                {
                    float basePrice = precioPlatos[platoNumber - 1];
                    float newPrice = (basePrice + extraTotal) * platoCount[platoNumber];
                    textsEspacio[1].text = newPrice.ToString("0.00").Replace(".", ",") + " €";
                    precioPlato[i] = textsEspacio[1].text;
                }

                // Now set Cantidad as a child of the last instantiated option
                if (lastOptionInstance != null)
                {
                    RectTransform cantidad = fixedContainer.transform.GetChild(3) as RectTransform;

                    // Re-parent to the last created option instance
                    cantidad.SetParent(lastOptionInstance.transform, false);
                    if (SceneManager.GetActiveScene().name == "MobileScene")
                        cantidad.anchoredPosition = new Vector2(-150, -220);
                    else
                        cantidad.anchoredPosition = new Vector2(220, -40);

                }

            }
            else
            {
                optionsPlato = "";
            }

            // Wire Nota button
            Button notaBtn = prefabEspacioInstance.transform.Find("FixedContainer/NotaButton")?.GetComponent<Button>();
            TMP_InputField notaInput = prefabEspacioInstance.transform.Find("FixedContainer/NotaInput")?.GetComponent<TMP_InputField>();

            // If this dish has options, reparent NotaButton and NotaInput into the last option instance (same as Cantidad)
            if (selectedOptions.Count > 0 && lastOptionInstance != null)
            {
                if (notaBtn != null)
                {
                    notaBtn.transform.SetParent(lastOptionInstance.transform, false);
                    if (SceneManager.GetActiveScene().name == "MobileScene")
                        notaBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-700, -220);
                    else
                        notaBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(90, -40);
                }
                if (notaInput != null)
                {
                    Vector2 anchorVector = new Vector2(-125, -700);
                    if (SceneManager.GetActiveScene().name == "TPVScene")
                        anchorVector = new Vector2(95, -175);

                    notaInput.transform.SetParent(lastOptionInstance.transform, false);
                    notaInput.GetComponent<RectTransform>().anchoredPosition = anchorVector;
                }
            }

            if (notaBtn != null && notaInput != null)
            {
                if (Navigation.camarero == false) // Clientes no ven el boton nota
                    notaBtn.gameObject.SetActive(false);
                float notaExpandHeight = 500f;
                if (SceneManager.GetActiveScene().name == "TPVScene")
                    notaExpandHeight = 100f;
                GameObject capturedEspacio = prefabEspacioInstance;

                notaBtn.onClick.AddListener(() =>
                {
                    bool isOpen = notaInput.gameObject.activeSelf;
                    RectTransform rt = capturedEspacio.GetComponent<RectTransform>();

                    if (isOpen)
                    {
                        notaInput.text = "";
                        notaInput.gameObject.SetActive(false);
                        notaBtn.GetComponentInChildren<TMP_Text>().text = "+ Nota";
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y - notaExpandHeight);
                    }
                    else
                    {
                        notaInput.gameObject.SetActive(true);
                        notaBtn.GetComponentInChildren<TMP_Text>().text = "– Nota";
                        rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + notaExpandHeight);
                        notaInput.Select();
                    }
                });
            }

            // Wire orden buttons
            Transform ordenButtonsT = prefabEspacioInstance.transform.Find("FixedContainer/OrdenButtons");
            TMP_Text ordenValueTxt = prefabEspacioInstance.transform.Find("FixedContainer/OrdenPlato")?.GetComponent<TMP_Text>();

            if (ordenButtonsT != null)
            {
                // Reparent first if options exist (same as notaBtn)
                if (selectedOptions.Count > 0 && lastOptionInstance != null)
                {
                    ordenButtonsT.SetParent(lastOptionInstance.transform, false);
                    if (SceneManager.GetActiveScene().name == "MobileScene")
                        ordenButtonsT.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, -220);
                    else
                        ordenButtonsT.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -65);
                }

                // THEN check toggle state — after reparent so SetActive works on final parent
                ordenButtonsT.gameObject.SetActive(ordenActivoToggle != null && ordenActivoToggle.isOn);

                if (ordenValueTxt != null)
                {
                    Button[] ordenBtns = ordenButtonsT.GetComponentsInChildren<Button>();
                    Color normalColor = Color.white;

                    for (int b = 0; b < ordenBtns.Length && b < 3; b++)
                    {
                        int capturedOrden = b + 1;
                        ordenBtns[b].onClick.AddListener(() =>
                        {
                            Color newColorBotonOrden;
                            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out newColorBotonOrden))
                                SelectedButtonOrdenColor = newColorBotonOrden;
                            ordenValueTxt.text = capturedOrden.ToString();
                            for (int x = 0; x < ordenBtns.Length; x++)
                            {
                                bool isSelected = (x == capturedOrden - 1);
                                ColorBlock cb = ordenBtns[x].colors;
                                cb.normalColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                                cb.selectedColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                                cb.highlightedColor = isSelected ? SelectedButtonOrdenColor : normalColor;
                                ordenBtns[x].colors = cb;

                                TMP_Text btnTxt = ordenBtns[x].GetComponentInChildren<TMP_Text>();
                                if (btnTxt != null)
                                    btnTxt.color = isSelected ? Color.white : Color.black;
                            }
                        });
                    }
                }
            }

            nEspacios++; // alomejor hay que revisar

            // Send selected plato to sync for other clients in the mesa to see it
            MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();

            playerController.CmdSendPreviaToServer(float.Parse(inputMesa.text), new string[] { nombrePlatos[platoNumber - 1] },
                new string[] { optionsPlato }, new string[] { cantidadPlato[i] }, new string[] { precioPlato[i] }, new int[] { toggleCamarero[i] });
        }

        imageNotificacion.SetActive(true);
        if (Navigation.camarero && SceneManager.GetActiveScene().name == "MobileScene" && imageNotificacionCamarero != null)
            imageNotificacionCamarero.SetActive(true);

        // ESTARIA BIEN QUE LA FUENTE SOLO SE CAMBIARA UNA VEZ NO CADA VEZ QUE PIDES
        textPrecioTotal.font = fuenteGeneral;
        textTotal.font = fuenteGeneral;
        textTotal2.font = fuenteGeneral;

        HacerSumaPedidos();

        // DetallePlato.Instance.ClearOptionGroups(); // removed - resets button incorrectly

        precioTotal.SetActive(true);
    }

    public void AgregarPedidoAnterior(List<TPV_DataManager.OrderItem> items)
    {
        string rutaFuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "").Replace(" ", "");
        TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFuenteGeneral + " SDF");

        if (contentPedido.activeInHierarchy == false)
            contentPedido.SetActive(true);
        if (precioTotal.activeInHierarchy == false)
            precioTotal.SetActive(true);

        foreach (var item in items)
        {
            GameObject prefabEspacioInstance;
            if (SceneManager.GetActiveScene().name == "MobileScene")
                prefabEspacioInstance = Instantiate(prefabEspacio, transform.position, Quaternion.identity);
            else
                prefabEspacioInstance = Instantiate(prefabEspacioBarra, transform.position, Quaternion.identity);

            prefabsEspacio[i] = prefabEspacioInstance;
            prefabEspacioInstance.transform.SetParent(contentPedido.transform, false);
            prefabEspacioInstance.transform.SetSiblingIndex(0);

            RectTransform prefabEspacioRect = prefabEspacioInstance.GetComponent<RectTransform>();
            prefabEspacioRect.localScale = Vector3.one;
            prefabEspacioRect.offsetMin = Vector2.zero;
            prefabEspacioRect.offsetMax = SceneManager.GetActiveScene().name == "MobileScene" ? new Vector2(550, 550) : new Vector2(720, 140);

            string displayName = string.IsNullOrWhiteSpace(item.opciones) ? item.nombre : item.nombre + " (" + item.opciones + ")";

            TMP_Text[] textsEspacio = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            textsEspacio[0].text = displayName;
            textsEspacio[0].font = fuenteGeneral;
            nombrePlato[i] = textsEspacio[0];
            nombrePlatoString[i] = displayName;

            textsEspacio[1].text = item.precio.Contains("€") ? item.precio : item.precio + " €";
            textsEspacio[1].font = fuenteGeneral;
            precioPlato[i] = textsEspacio[1].text;

            textsEspacio[4].text = item.cantidad;
            textsEspacio[4].font = fuenteGeneral;
            cantidadPlato[i] = item.cantidad;
            cantidadPlatoInt[i] = int.TryParse(item.cantidad, out int cInt) ? cInt : 1;

            Button[] buttonsEspacio = prefabEspacioInstance.GetComponentsInChildren<Button>();
            buttonAdd[i] = buttonsEspacio[1];
            buttonQuitar[i] = buttonsEspacio[2];

            int currentEspacios = i;
            buttonAdd[currentEspacios].onClick.AddListener(() => AddCantidadPedido());
            buttonQuitar[currentEspacios].onClick.AddListener(() => QuitarCantidadPedido(currentEspacios, -1));

            imageNotificacion.SetActive(true);
            if (Navigation.camarero && SceneManager.GetActiveScene().name == "MobileScene" && imageNotificacionCamarero != null)
                imageNotificacionCamarero.SetActive(true);

            MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();
            playerController.CmdSendPreviaToServer(float.Parse(inputMesa.text), new string[] { displayName },
                new string[] { "" }, new string[] { cantidadPlato[i] }, new string[] { precioPlato[i] }, new int[] { 0 });

            i++;
            nEspacios++;
        }

        textPrecioTotal.font = fuenteGeneral;
        textTotal.font = fuenteGeneral;
        textTotal2.font = fuenteGeneral;

        buttonPedir.interactable = true;
        HacerSumaPedidos();
    }

    public void HacerSumaPedidos()
    {
        totalSum = 0;
        bool foundEspacio = false;

        foreach (Transform espacio in contentPedido.transform)
        {
            if (espacio.name == "Espacio(Clone)" ||
                espacio.name == "EspacioPrevia(Clone)" ||
                espacio.name == "EspacioBarraPedido(Clone)")
            {
                foundEspacio = true;
                Transform textPrecio = espacio.Find("FixedContainer/Text Precio 1");

                if (textPrecio == null)
                {
                    Debug.LogWarning("TextPrecio NOT found in: " + espacio.name);
                    continue;
                }

                TMP_Text tmpText = textPrecio.GetComponent<TMP_Text>();

                if (tmpText == null)
                {
                    Debug.LogWarning("TMP_Text missing in: " + espacio.name);
                    continue;
                }

                float floatVal = ExtractFloat(tmpText.text);
                totalSum += floatVal;
            }
        }

        if (!foundEspacio)
        {
            totalSum = 0;
            // Si no hay nada no se puede pedir
            buttonPedir.interactable = false;
            precioTotal.SetActive(false);
        }

        textPrecioTotal.text = totalSum.ToString("0.00").Replace(".", ",") + " €";
        //precioTotal.transform.SetAsLastSibling();
    }

    public void PedidoLocal()
    {
        // Clear arrays
        Array.Clear(platoCount, 0, platoCount.Length);
        Array.Clear(numeroPlato, 0, numeroPlato.Length);
        Array.Clear(nombrePlato, 0, nombrePlato.Length);
        Array.Clear(nombrePlatoString, 0, nombrePlatoString.Length);
        Array.Clear(precioPlato, 0, precioPlato.Length);
        Array.Clear(cantidadPlato, 0, cantidadPlato.Length);
        Array.Clear(cantidadPlatoInt, 0, cantidadPlatoInt.Length);
        Array.Clear(toggleCamarero, 0, toggleCamarero.Length);
        Array.Clear(prefabsEspacio, 0, prefabsEspacio.Length);
        nPedidosEspacios = nPedidosEspacios + i;
        nEspacios = 0;
        i = 0;

        if (SceneManager.GetActiveScene().name == "TPVScene")
        {
            NC.Mesas();
        }
    }

    public void PedidoSameMesa()
    {
        if (Navigation.camarero == true || SceneManager.GetActiveScene().name == "TPVScene") // total inactive en camarero y tpvtambién
        {
            precioTotal.SetActive(false);
            primerPedidoHecho = false;
            primerPedidoHecho2 = false;
        }

        else
        {
            primerPedidoHecho = true;
            primerPedidoHecho2 = true;
            if (precioTotal2.activeInHierarchy)
            {
                float sumaTotal = ExtractFloat(textPrecioTotal.text) + ExtractFloat(precioTotal2.GetComponentInChildren<TMP_Text>().text);
                textPrecioTotal.text = sumaTotal.ToString("0.00").Replace(".", ",") + " €";
                precioTotal2.SetActive(false);
            }

            precioTotal.transform.SetAsLastSibling();

            buttonPedir.interactable = false;
            buttonPagar.interactable = true;
            countPlatosPedidos = true;
        }
    }


    public void BajarBotonPedido()
    {
        buttonPedir.transform.SetParent(canvasPedidoParent.transform);
        buttonPedir.transform.SetSiblingIndex(2);

        RectTransform rectTransform = buttonPedir.GetComponent<RectTransform>();

        // Set anchor to center (ensures correct positioning)
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Set position (Pos X = 0, Pos Y = -1800, Pos Z = 0)
        rectTransform.anchoredPosition = new Vector3(0, 600, 0);


        // Set size (Width = 2200, Height = 260)
        //rectTransform.sizeDelta = new Vector2(2200, 260);
    }

    void AddCantidadPedido()
    {
        GameObject button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        Transform parentEspacio = button.transform.parent.parent.parent;
        TMP_Text[] childtexts = parentEspacio.GetComponentsInChildren<TMP_Text>();

        Transform parentEspacioOpciones = button.transform.parent.parent.parent.parent;
        TMP_Text[] childtextsOpciones = parentEspacioOpciones.GetComponentsInChildren<TMP_Text>();

        Transform parentOpciones = button.transform.parent.parent.parent;
        Transform lastChild = parentOpciones.GetChild(parentOpciones.childCount - 1);
        TMP_Text[] lastChildTexts = lastChild.GetComponentsInChildren<TMP_Text>();

        int cantidad = 0;
        string precio = "";
        string dishName = "";
        string dishOpciones = "";

        bool isTPVScene = SceneManager.GetActiveScene().name == "TPVScene";
        bool hasOpciones = (isTPVScene && parentEspacio.name != "EspacioBarraPedido(Clone)") ||
                           (!isTPVScene && parentEspacio.name != "Espacio(Clone)");

        if (hasOpciones)
        {
            cantidad = int.Parse(lastChildTexts[2].text);
            precio = childtextsOpciones[1].text;
            dishName = childtextsOpciones[0].text;
            dishOpciones = GetOptionsFromPrefab(parentEspacioOpciones.gameObject);
        }
        else
        {
            cantidad = int.Parse(childtexts[4].text);
            precio = childtexts[1].text;
            dishName = childtexts[0].text;
            dishOpciones = "";
        }

        cantidad++;
        string newCantidad = cantidad.ToString();
        float unitPrice = ExtractFloat(precio) / (cantidad - 1);
        string newPrecio = (unitPrice * cantidad).ToString("F2");

        MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();
        playerController.CmdUpdateCantidad(int.Parse(inputMesa.text), dishName, dishOpciones, newCantidad, newPrecio);

        if (hasOpciones)
        {
            lastChildTexts[2].text = newCantidad;
            childtextsOpciones[1].text = newPrecio.Replace(".", ",") + " €";
        }
        else
        {
            childtexts[4].text = newCantidad;
            childtexts[1].text = newPrecio.Replace(".", ",") + " €";
        }

        HacerSumaPedidos();
    }

    void QuitarCantidadPedido(int currentEspacios, int platoNumber)
    {
        GameObject button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        Transform parentEspacio = button.transform.parent.parent.parent.parent;
        TMP_Text[] childtexts = parentEspacio.GetComponentsInChildren<TMP_Text>();
        GameObject espacioClone = parentEspacio.gameObject;

        Transform parentEspacioOpciones = button.transform.parent.parent.parent.parent.parent;
        TMP_Text[] childtextsOpciones = parentEspacioOpciones.GetComponentsInChildren<TMP_Text>();

        Transform parentOpciones = button.transform.parent.parent.parent.parent;
        Transform lastChild = parentOpciones.GetChild(parentOpciones.childCount - 1);
        TMP_Text[] lastChildTexts = lastChild.GetComponentsInChildren<TMP_Text>();

        int cantidad = 0;
        string precio = "";
        string dishName = "";
        string dishOpciones = "";

        bool isTPVScene = SceneManager.GetActiveScene().name == "TPVScene";
        bool hasOpciones = (isTPVScene && parentEspacio.name != "EspacioBarraPedido(Clone)") ||
                           (!isTPVScene && parentEspacio.name != "Espacio(Clone)");

        if (hasOpciones)
        {
            cantidad = int.Parse(lastChildTexts[2].text);
            precio = childtextsOpciones[1].text;
            dishName = childtextsOpciones[0].text;
            dishOpciones = GetOptionsFromPrefab(parentEspacioOpciones.gameObject);
            espacioClone = parentEspacio.parent.gameObject;
        }
        else
        {
            cantidad = int.Parse(childtexts[4].text);
            precio = childtexts[1].text;
            dishName = childtexts[0].text;
            dishOpciones = "";
        }

        if (cantidad == 1)
        {
            MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();
            playerController.CmdDeletePlato(int.Parse(inputMesa.text), dishName, dishOpciones);

            Destroy(espacioClone);
            StartCoroutine(RecalcularDespues());
        }
        else
        {
            cantidad--;
            string newCantidad = cantidad.ToString();
            float unitPrice = ExtractFloat(precio) / (cantidad + 1);
            string newPrecio = (unitPrice * cantidad).ToString("F2");

            MyPlayerController playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();
            playerController.CmdUpdateCantidad(int.Parse(inputMesa.text), dishName, dishOpciones, newCantidad, newPrecio);

            if (hasOpciones)
            {
                lastChildTexts[2].text = newCantidad;
                childtextsOpciones[1].text = newPrecio.Replace(".", ",") + " €";
            }
            else
            {
                childtexts[4].text = newCantidad;
                childtexts[1].text = newPrecio.Replace(".", ",") + " €";
            }

            HacerSumaPedidos();
        }
    }

    IEnumerator RecalcularDespues()
    {
        yield return null; // wait 1 frame till it is destroyed
        HacerSumaPedidos();
    }

    public void UpdatePlatoCountPedido(int index, int platoQuantity, int platoNumber)
    {
        // Update numerosArray with the updated count for the specified espacio
        numerosArray[index].text = platoQuantity.ToString();
        preciosArray[index].text = (platoQuantity * precioPlatos[platoNumber - 1]).ToString("0.00").Replace(".", ",") + " €";

        // Suma Total
        totalSum = 0;
        for (int i = 0; i < preciosArray.Length; i++)
        {
            float floatVal = ExtractFloat(preciosArray[i].text);
            if (!float.IsNaN(floatVal))
            {
                totalSum += floatVal; // Add the extracted float to the total sum only if it's not NaN
            }
        }

        textPrecioTotal.text = totalSum.ToString("0.00").Replace(".", ",") + " €";
    }


    float ExtractFloat(string input)
    {
        // Using regular expressions to find the flfondoimageetioat value
        Match match = Regex.Match(input, @"(\d+,\d+)");
        if (match.Success)
        {
            // Convert comma to dot for parsing the float value
            string floatValueString = match.Groups[0].Value.Replace(',', '.');
            return float.Parse(floatValueString, CultureInfo.InvariantCulture);
        }
        else
        {
            // Debug.LogWarning("No float value found in the input string.");
            return float.NaN; // Return NaN (Not a Number) to indicate failure
        }
    }

    public float ExtractOptionExtraPrice(string optionValue)
    {
        float total = 0f;
        foreach (Match match in Regex.Matches(optionValue, @"\+(\d+[.,]\d+)"))
        {
            string val = match.Groups[1].Value.Replace(',', '.');
            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float extra))
                total += extra;
        }
        return total;
    }

    public void ChangeImageColor()
    {
        Color newColorFondo;
        Color newColorFondo2;
        Color newColorFondo3;

        // Convertimos el string hex a un Color
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_gral[0], out newColorFondo)) // Cambiamos color al fondo de la barra de secciones
        {
            fondoImage = prefabPlato.GetComponentInChildren<Image>();
            fondoImage.color = newColorFondo;
        }

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_titulos[0], out newColorFondo2)) // Cambiamos color al fondo de la barra de secciones
        {
            fondoImageSec = prefabSeccion.GetComponentInChildren<Image>();
            fondoImageSec.color = newColorFondo2;
        }

        // Cambiar color de etiquetas secciones
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo3)) // Cambiamos color al fondo de la barra de secciones
        {
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {
                // REVISAR - funciona para TPV pero da error en movil -arreglao
                fondoImageEti = prefabEtiqueta.GetComponentsInChildren<Image>();
                fondoImageEti[0].color = newColorFondo3;
                fondoImageEti[1].color = newColorFondo3;
            }

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                // REVISAR - funciona para TPV pero da error en movil -arreglao
                fondoImageEtiMovil = prefabEtiqueta.GetComponentInChildren<Image>();
                fondoImageEtiMovil.color = newColorFondo3;
            }
        }
    }

    private string GetOptionsFromPrefab(GameObject prefab)
    {
        Transform optionContainer = prefab.transform.GetChild(1);
        if (optionContainer == null) return "";

        List<string> options = new List<string>();
        foreach (Transform child in optionContainer)
        {
            TMP_Text txt = child.GetComponent<TMP_Text>();
            if (txt != null)
                options.Add(txt.text);
        }
        return string.Join(", ", options);
    }

    private string StripPrice(string option)
    {
        int plusIndex = option.LastIndexOf('+');
        int cutIndex = plusIndex > 0 ? plusIndex : -1;
        if (cutIndex > 0)
            return option.Substring(0, cutIndex).Trim();
        return option;
    }
}