using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

public class EditarMenu : MonoBehaviour
{
    public string url;
    private TMP_Text textTitulo;
    private TMP_Text textDescripcion;
    private TMP_Text textPrecio;
    private TMP_Text textNumero;
    private TMP_Text textSeccion;
    private TMP_Text textUrl;
    private Image imagePlato;
    private TMP_Text textAlerg1;
    private TMP_Text textAlerg2;
    private TMP_Text textAlerg3;
    private TMP_Text textAlerg4;
    private TMP_Text textAlerg5;
    private TMP_Text textAlerg6;
    private TMP_Text textAlerg7;
    private TMP_Text textAlerg8;
    private TMP_Text textAlerg9;
    private TMP_Text textAlerg10;
    private TMP_Text textAlerg11;
    private TMP_Text textAlerg12;
    private TMP_Text textAlerg13;
    private TMP_Text textAlerg14;
    private TMP_Text textVeg;
    private TMP_Text textoptionGroups;
    private TMP_Text textDestino;
    public Image imageRellenarPlato;
    public GameObject buttonSubirImagen;
    public GameObject buttonBorrarImagen;
    public TMP_Text textUrlImagenRellenarPlato;

    public GameObject prefabPlato;
    public GameObject[] prefabsPlato;
    public GameObject masPlatoPrefab;
    public GameObject masSeccionPrefab;
    public GameObject canvasRellenarPlato;
    public GameObject canvasRellenarSeccion;
    public GameObject Error1;
    public GameObject Error2;

    public GameObject textSeccionPrefab;
    private bool newSection = false;

    public DataBase DB;

    public bool creatingData = false;

    // Menus
    public GameObject contentMenusParent;
    public GameObject canvasMenus;
    public GameObject prefabCanvasMenu;
    public GameObject menusRoot; // canvas menus parent
    public GameObject crearMenuButtonPrefab;
    private GameObject masMenuPrefabInstance;

    // DataBase data
    private int[] numeroMenus;
    private string[] nombres;
    private string[] descripcion;
    private float[] precios;
    private Sprite[] sprites; 
    private string[] secciones;
    private int[] toggles;
    private string[] imageUrls;
    private int[] alergs1;
    private int[] alergs2;
    private int[] alergs3;
    private int[] alergs4;
    private int[] alergs5;
    private int[] alergs6;
    private int[] alergs7;
    private int[] alergs8;
    private int[] alergs9;
    private int[] alergs10;
    private int[] alergs11;
    private int[] alergs12;
    private int[] alergs13;
    private int[] alergs14;
    private int[] vegs;
    private string[] optionGroups;

    // Cuadro opciones
    public GameObject prefabCuadroOpciones;
    public Transform contentDetallePlato;
    public GameObject buttonAñadirOpcion;

    private Sprite newSprite;

    public GameObject imageLoadingMenu;
    public static bool menuReady = false;

    void Start()
    {
        imageLoadingMenu.SetActive(true);
        DB.OnDataLoaded += OnDatabaseLoaded;
    }

    void OnDestroy()
    {
        DB.OnDataLoaded -= OnDatabaseLoaded;
    }

    private void OnDatabaseLoaded()
    {
        CreatePrefabs();
    }

    void CreatePrefabs()
    {
        if (DataBase.nombrePlatos == null) // New user
        {
           // Debug.Log("New User - Editar Menu");
        }
        else
        {
            numeroMenus = DataBase.numeroMenu;
            nombres = DataBase.nombrePlatos;
            descripcion = DataBase.descripcionPlatos;
            precios = DataBase.precioPlatos;
            sprites = DataBase.spritePlatos;
            secciones = DataBase.seccion;
            toggles = DataBase.toggle;
            imageUrls = DataBase.imageUrls;
            alergs1 = DataBase.alergs1;
            alergs2 = DataBase.alergs2;
            alergs3 = DataBase.alergs3;
            alergs4 = DataBase.alergs4;
            alergs5 = DataBase.alergs5;
            alergs6 = DataBase.alergs6;
            alergs7 = DataBase.alergs7;
            alergs8 = DataBase.alergs8;
            alergs9 = DataBase.alergs9;
            alergs10 = DataBase.alergs10;
            alergs11 = DataBase.alergs11;
            alergs12 = DataBase.alergs12;
            alergs13 = DataBase.alergs13;
            alergs14 = DataBase.alergs14;
            vegs = DataBase.vegs;
            optionGroups = DataBase.optionGroups;
            prefabsPlato = new GameObject[nombres.Length];

            // Crear cada menu, despues incluir los platos correspondientes
            List<int> uniqueMenus = numeroMenus
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (int numMenu in uniqueMenus)
            {
                // Crear el botón del menú
                var prefabMenuInstance = Instantiate(prefabPlato, transform.position, Quaternion.identity);
                prefabMenuInstance.transform.SetParent(contentMenusParent.transform, false);
                prefabMenuInstance.name = $"Menu_{numMenu}";

                TMP_Text[] texts = prefabMenuInstance.GetComponentsInChildren<TMP_Text>();
                texts[0].text = $"Menú {numMenu}";
                int cantidadPlatos = numeroMenus.Count(n => n == numMenu);
                texts[1].text = $"{cantidadPlatos} platos";

                // Crear el Canvas del menú
                var prefabCanvasMenuInstance = Instantiate(prefabCanvasMenu, transform.position, Quaternion.identity);
                prefabCanvasMenuInstance.transform.SetParent(menusRoot.transform, false);
                prefabCanvasMenuInstance.name = $"CanvasMenu{numMenu}";
                prefabCanvasMenuInstance.SetActive(false); // Oculto por defecto

                TMP_Text[] textsCanvas = prefabCanvasMenuInstance.GetComponentsInChildren<TMP_Text>();
                textsCanvas[0].text = $"Menú {numMenu}";

                Transform contentMenu = prefabCanvasMenuInstance.transform.Find("Scroll View/Viewport/ContentMenu");
                contentMenuPorNumeroMenu[numMenu] = contentMenu;

                seccionContainersPorMenu[numMenu] = new Dictionary<string, GameObject>();

                CreateSeccionText(numMenu);

                // Guardar la referencia para el botón
                var botonMenu = prefabMenuInstance.GetComponent<Button>();
                botonMenu.onClick.AddListener(() =>
                {
                    canvasMenus.SetActive(false);
                    prefabCanvasMenuInstance.SetActive(true);
                });

                // Add the "Mas Seccion" button at the end of everything
                CreateMasSeccionButton(numMenu, contentMenu);
            }

            // Add create new menu button
            CreateMasMenuButton();



            //CreateSeccionText();

            // Loop through platos and instantiate each under their respective section container
            for (int i = 0; i < nombres.Length; i++)
            {
                CreatePrefab(i);
            }

            // Now create the "Mas Plato" button for each section
            foreach (var menuKVP in seccionContainersPorMenu)
            {
                int numMenu = menuKVP.Key;

                foreach (var seccion in menuKVP.Value.Keys)
                {
                    CreateMasPlatoButton(numMenu, seccion);
                }
            }

            // Add the "Mas Seccion" button at the end of everything
            //CreateMasSeccionButton();
        }

        imageLoadingMenu.SetActive(false);
        menuReady = true;
    }

    private void CreateMasMenuButton()
    {
        // Instanciar el botón 'Más Empleado'
        masMenuPrefabInstance = Instantiate(crearMenuButtonPrefab, transform.position, Quaternion.identity);

        // Asignarlo como hijo del contenedor general
        masMenuPrefabInstance.transform.SetParent(contentMenusParent.transform, false);
        masMenuPrefabInstance.GetComponent<RectTransform>().localScale = Vector3.one;

        // Asignar el listener al botón
        var buttonMas = masMenuPrefabInstance.GetComponentInChildren<Button>();
        if (buttonMas != null)
        {
            buttonMas.onClick.AddListener(() => OnClickCrearMenu(-1, true)); // Usa -1 para indicar "nuevo"
        }

        // Asegurar que esté al final
        masMenuPrefabInstance.transform.SetAsLastSibling();
    }

    private void OnClickCrearMenu(int index, bool isNew)
    {
        StartCoroutine(CreateNewMenu());
    }

    private IEnumerator CreateNewMenu()
    {
        int newMenuNumber = 1;

        if (numeroMenus != null && numeroMenus.Length > 0)
        {
            newMenuNumber = numeroMenus.Max() + 1;
        }

        string restaurantID = LoginManagerResponsable.restaurantID;

        string jsonData =
            $"{{\"restaurant_id\":\"{restaurantID}\",\"menu_number\":\"{newMenuNumber}\"}}";

        UnityWebRequest request =
            new UnityWebRequest(url + "/menus/add", "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("Menu created");

            CreateSingleMenuUI(newMenuNumber);
        }
    }

    private void CreateSingleMenuUI(int numMenu)
    {
        var prefabMenuInstance =
            Instantiate(prefabPlato, transform.position, Quaternion.identity);

        prefabMenuInstance.transform.SetParent(contentMenusParent.transform, false);

        prefabMenuInstance.name = $"Menu_{numMenu}";

        TMP_Text[] texts =
            prefabMenuInstance.GetComponentsInChildren<TMP_Text>();

        texts[0].text = $"Menú {numMenu}";
        texts[1].text = "0 platos";

        var prefabCanvasMenuInstance =
            Instantiate(prefabCanvasMenu, transform.position, Quaternion.identity);

        prefabCanvasMenuInstance.transform.SetParent(menusRoot.transform, false);

        prefabCanvasMenuInstance.name = $"CanvasMenu{numMenu}";

        prefabCanvasMenuInstance.SetActive(false);

        TMP_Text[] textsCanvas =
            prefabCanvasMenuInstance.GetComponentsInChildren<TMP_Text>();

        textsCanvas[0].text = $"Menú {numMenu}";

        Transform contentMenu =
            prefabCanvasMenuInstance.transform.Find("Scroll View/Viewport/ContentMenu");

        contentMenuPorNumeroMenu[numMenu] = contentMenu;

        seccionContainersPorMenu[numMenu] =
            new Dictionary<string, GameObject>();

        var botonMenu = prefabMenuInstance.GetComponent<Button>();

        botonMenu.onClick.AddListener(() =>
        {
            canvasMenus.SetActive(false);
            prefabCanvasMenuInstance.SetActive(true);
        });

        CreateMasSeccionButton(numMenu, contentMenu);

        masMenuPrefabInstance.transform.SetAsLastSibling();
    }

    Dictionary<int, Dictionary<string, GameObject>> seccionContainersPorMenu = new Dictionary<int, Dictionary<string, GameObject>>();
    Dictionary<int, Transform> contentMenuPorNumeroMenu = new Dictionary<int, Transform>();


    void CreateSeccionText(int numMenu)
    {
        Transform contentMenu = contentMenuPorNumeroMenu[numMenu];
        var seccionContainers = seccionContainersPorMenu[numMenu];

        HashSet<string> uniqueSecciones = new HashSet<string>(secciones); // o filtra las secciones que correspondan a este menu

        foreach (string seccion in uniqueSecciones)
        {
            if (!seccionContainers.ContainsKey(seccion))
            {
                // Instanciar título y contenedor, parent = contentMenu
                var seccionTitleInstance = Instantiate(textSeccionPrefab, transform.position, Quaternion.identity);
                seccionTitleInstance.transform.SetParent(contentMenu, false);
                seccionTitleInstance.GetComponent<TMP_Text>().text = seccion;

                GameObject seccionContainer = new GameObject(seccion + "Container");
                seccionContainer.transform.SetParent(contentMenu, false);

                var layout = seccionContainer.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = false;
                layout.childControlHeight = false;
                layout.childControlWidth = false;
                layout.spacing = 0;
                layout.padding = new RectOffset(10, 10, 10, 10);

                var fitter = seccionContainer.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                seccionContainers[seccion] = seccionContainer;

                if (newSection)
                {
                    if (contentMenu.childCount >= 2)
                    {
                        seccionContainer.transform.SetSiblingIndex(contentMenu.childCount - 3);
                        seccionTitleInstance.transform.SetSiblingIndex(contentMenu.childCount - 3);
                    }
                    else
                    {
                        seccionContainer.transform.SetAsFirstSibling();
                        seccionTitleInstance.transform.SetAsFirstSibling();
                    }

                    CreateMasPlatoButton(numMenu, seccion);
                    newSection = false;
                }
            }
        }
    }


    private void CreatePrefab(int index)
    {
        int numMenu = numeroMenus[index];
        string platoSeccion = secciones[index];

        if (!seccionContainersPorMenu.ContainsKey(numMenu))
        {
            Debug.LogError("Menú no encontrado: " + numMenu);
            return;
        }

        var seccionContainers = seccionContainersPorMenu[numMenu];

        if (!seccionContainers.ContainsKey(platoSeccion))
        {
            Debug.LogError("Sección no encontrada: " + platoSeccion);
            return;
        }

        // Instantiate the plato under the correct seccion container
        var prefabPlatoInstance = Instantiate(prefabPlato, transform.position, Quaternion.identity);
        prefabPlatoInstance.transform.SetParent(seccionContainers[platoSeccion].transform, false);

        prefabsPlato[index] = prefabPlatoInstance;

        var prefabPlatoRect = prefabPlatoInstance.GetComponent<RectTransform>();
        prefabPlatoRect.localScale = new Vector3(1, 1, 1);  // Maintain the original scale

        // Set the prefab details (names, description, etc.)
        SetPrefabDetails(prefabPlatoInstance, index, numeroMenus, nombres, descripcion, precios, sprites, secciones, toggles, imageUrls, alergs1, alergs2, alergs3, alergs4, alergs5, alergs6, alergs7, alergs8, alergs9, alergs10, alergs11, alergs12, alergs13, alergs14, vegs, optionGroups);

        var button = prefabPlatoInstance.GetComponentsInChildren<Button>();
        if (button[0] != null)
        {
            button[0].onClick.AddListener(() => OnClickButtonPlato(index, false, ""));
        }

        if (button[1] != null)
        {
            button[1].onClick.AddListener(() => DeleteMenuOnClick(index));
        }
    }

    private void CreateMasPlatoButton(int numMenu, string seccion)
    {
        if (!seccionContainersPorMenu.ContainsKey(numMenu))
        {
            Debug.LogError("Menu not found: " + numMenu);
            return;
        }

        var seccionContainers = seccionContainersPorMenu[numMenu];

        if (!seccionContainers.ContainsKey(seccion))
        {
            Debug.LogError("Seccion not found: " + seccion);
            return;
        }

        var masPlatoPrefabInstance = Instantiate(masPlatoPrefab, transform.position, Quaternion.identity);
        masPlatoPrefabInstance.transform.SetParent(seccionContainers[seccion].transform, false);

        var masPlatoPrefabRect = masPlatoPrefabInstance.GetComponent<RectTransform>();
        masPlatoPrefabRect.localScale = new Vector3(1, 1, 1);

        var buttonMas = masPlatoPrefabInstance.GetComponentInChildren<Button>();
        if (buttonMas != null)
        {
            buttonMas.onClick.AddListener(() => OnClickButtonPlato(-1, true, seccion));
        }

        masPlatoPrefabInstance.transform.SetAsLastSibling();
    }


    private void CreateMasSeccionButton(int numMenu, Transform contentMenu)
    {
        if (!seccionContainersPorMenu.ContainsKey(numMenu))
        {
            Debug.LogError("Menú no encontrado: " + numMenu);
            return;
        }

        var masSeccionPrefabInstance = Instantiate(masSeccionPrefab, transform.position, Quaternion.identity);
        masSeccionPrefabInstance.transform.SetParent(contentMenu, false);

        var masPlatoPrefabRect = masSeccionPrefabInstance.GetComponent<RectTransform>();
        masPlatoPrefabRect.localScale = new Vector3(1, 1, 1);

        var buttonMas = masSeccionPrefabInstance.GetComponentInChildren<Button>();
        if (buttonMas != null)
        {
            buttonMas.onClick.AddListener(() => OnClickButtonSeccion());
        }
    }

    private void SetPrefabDetails(GameObject prefab, int index, int[] menuNum, string[] names, string[] desc, float[] prices, Sprite[] sprites, string[] sections, int[] toggles, string[] imageUrls, int[] alergs1, int[] alergs2, int[] alergs3, int[] alergs4, int[] alergs5, int[] alergs6, int[] alergs7, int[] alergs8, int[] alergs9, int[] alergs10, int[] alergs11, int[] alergs12, int[] alergs13, int[] alergs14, int[] vegs, string[] optionGroups)
    {
        var textComponents = prefab.GetComponentsInChildren<TMP_Text>();

        textTitulo = textComponents[0];
        textDescripcion = textComponents[1];
        textPrecio = textComponents[2];
        textNumero = textComponents[3];
        textSeccion = textComponents[4];
        textUrl = textComponents[5];
        textAlerg1 = textComponents[6];
        textAlerg2 = textComponents[7];
        textAlerg3 = textComponents[8];
        textAlerg4 = textComponents[9];
        textAlerg5 = textComponents[10];
        textAlerg6 = textComponents[11];
        textAlerg7 = textComponents[12];
        textAlerg8 = textComponents[13];
        textAlerg9 = textComponents[14];
        textAlerg10 = textComponents[15];
        textAlerg11 = textComponents[16];
        textAlerg12 = textComponents[17];
        textAlerg13 = textComponents[18];
        textAlerg14 = textComponents[19];
        textVeg = textComponents[20];
        textoptionGroups = textComponents[21];
        textDestino = textComponents[22];

        textTitulo.text = names[index];
        textDescripcion.text = desc[index];
        textPrecio.text = prices[index].ToString("0.00").Replace(".", ",") + "€";
        textNumero.text = index.ToString();
        textSeccion.text = sections[index];
        textUrl.text = imageUrls[index];
        textAlerg1.text = alergs1[index].ToString(); // hasta aquí vamos bien
        textAlerg2.text = alergs2[index].ToString(); 
        textAlerg3.text = alergs3[index].ToString(); 
        textAlerg4.text = alergs4[index].ToString(); 
        textAlerg5.text = alergs5[index].ToString(); 
        textAlerg6.text = alergs6[index].ToString(); 
        textAlerg7.text = alergs7[index].ToString(); 
        textAlerg8.text = alergs8[index].ToString(); 
        textAlerg9.text = alergs9[index].ToString(); 
        textAlerg10.text = alergs10[index].ToString(); 
        textAlerg11.text = alergs11[index].ToString(); 
        textAlerg12.text = alergs12[index].ToString(); 
        textAlerg13.text = alergs13[index].ToString(); 
        textAlerg14.text = alergs14[index].ToString(); 
        textVeg.text = vegs[index].ToString();
        textoptionGroups.text = optionGroups[index].ToString();
        textDestino.text = toggles[index].ToString();

        // toggle
        // var togglesComponent = prefab.GetComponentInChildren<Toggle>();

        // if (toggles[index] == 1)
        // {
        //     togglesComponent.isOn = true;
        // }
        // else
        // {
        //     // Navigate to the last child of the grandson
        //     Transform toggleTransform = togglesComponent.transform;

        //     if (toggleTransform.childCount > 0)
        //     {
        //         Transform firstChild = toggleTransform.GetChild(0); // First child (son)

        //         if (firstChild.childCount > 0)
        //         {
        //             Transform lastGrandChild = firstChild.GetChild(firstChild.childCount - 1); // Last grandson

        //             lastGrandChild.gameObject.SetActive(false); // Disable the GameObject
        //         }
        //     }
        // }


        var imageComponents = prefab.GetComponentsInChildren<Image>();
        if (imageComponents.Length > 1)
        {
            imagePlato = imageComponents[2];
            imagePlato.sprite = sprites[index];
        }

       // Debug.Log("text setprefabdetails" + textAlerg1.text);
    }
    private void AddNewPrefab(int index, string name, string description, float price, string imageUrl, string seccion, int alerg1, int alerg2, int alerg3, int alerg4, int alerg5, int alerg6, int alerg7, int alerg8, int alerg9, int alerg10, int alerg11, int alerg12, int alerg13, int alerg14, int veg, string optionGroups, int toggle)
    {
        GameObject realParent = GameObject.Find(seccion + "Container");
        var prefabInstance = Instantiate(prefabPlato, transform.position, Quaternion.identity, realParent.transform);

        // Force the layout system to update after adding the new prefab
        LayoutRebuilder.ForceRebuildLayoutImmediate(realParent.GetComponent<RectTransform>());

        // Set prefab details
        var textComponents = prefabInstance.GetComponentsInChildren<TMP_Text>();
        textTitulo = textComponents[0];
        textDescripcion = textComponents[1];
        textPrecio = textComponents[2];
        textNumero = textComponents[3];
        textSeccion = textComponents[4];
        textUrl = textComponents[5];
        textAlerg1 = textComponents[6];
        textAlerg2 = textComponents[7];
        textAlerg3 = textComponents[8];
        textAlerg4 = textComponents[9];
        textAlerg5 = textComponents[10];
        textAlerg6 = textComponents[11];
        textAlerg7 = textComponents[12];
        textAlerg8 = textComponents[13];
        textAlerg9 = textComponents[14];
        textAlerg10 = textComponents[15];
        textAlerg11 = textComponents[16];
        textAlerg12 = textComponents[17];
        textAlerg13 = textComponents[18];
        textAlerg14 = textComponents[19];
        textVeg = textComponents[20];
        textoptionGroups = textComponents[21];
        textDestino = textComponents[22];

        textTitulo.text = name;
        textDescripcion.text = description;
        textPrecio.text = price.ToString("0.00").Replace(".", ",") + "€";
        textNumero.text = index.ToString();
        textSeccion.text = seccion;
        textUrl.text = imageUrl;
        textAlerg1.text = alerg1.ToString();
        textAlerg2.text = alerg2.ToString();
        textAlerg3.text = alerg3.ToString(); 
        textAlerg4.text = alerg4.ToString(); 
        textAlerg5.text = alerg5.ToString(); 
        textAlerg6.text = alerg6.ToString();
        textAlerg7.text = alerg7.ToString(); 
        textAlerg8.text = alerg8.ToString();
        textAlerg9.text = alerg9.ToString(); 
        textAlerg10.text = alerg10.ToString();
        textAlerg11.text = alerg11.ToString();
        textAlerg12.text = alerg12.ToString();
        textAlerg13.text = alerg13.ToString(); 
        textAlerg14.text = alerg14.ToString();
        textVeg.text = veg.ToString();
        textoptionGroups.text = optionGroups.ToString();
        textDestino.text = toggle.ToString();

        var button = prefabInstance.GetComponentsInChildren<Button>();
        if (button[0] != null)
        {
            button[0].onClick.AddListener(() => OnClickButtonPlato(index, false, "")); // así??
        }

        if (button[1] != null)
        {
            button[1].onClick.AddListener(() => DeleteMenuOnClick(index));
        }

        var imageComponents = prefabInstance.GetComponentsInChildren<Image>();
        if (imageComponents.Length > 1)
        {
            imagePlato = imageComponents[1];
            // Start the coroutine to download the image and set the sprite when ready
            if (imageUrl != "")
            {
                StartCoroutine(DownloadAndSetImage(imageUrl, imagePlato));
            }
            else
            {
                imagePlato.sprite = null;
            }
        }

        // toggle
        // var togglesComponent = prefabInstance.GetComponentInChildren<Toggle>();
        // // Navigate to the last child of the grandson
        // Transform toggleTransform = togglesComponent.transform;

        // if (toggleTransform.childCount > 0)
        // {
        //     Transform firstChild = toggleTransform.GetChild(0); // First child (son)

        //     if (firstChild.childCount > 0)
        //     {
        //         Transform lastGrandChild = firstChild.GetChild(firstChild.childCount - 1); // Last grandson

        //         lastGrandChild.gameObject.SetActive(false); // Disable the GameObject
        //     }
        // }


        Array.Resize(ref prefabsPlato, prefabsPlato.Length + 1);
        prefabsPlato[prefabsPlato.Length - 1] = prefabInstance; // Use Length - 1
        creatingData = false;

        // Call delayed sibling index adjustment to place the new prefab right before the last one
        StartCoroutine(DelayedSiblingIndex(prefabInstance, realParent));
    }

    private IEnumerator DownloadAndSetImage(string url, Image targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to download image from URL: " + url);
            Debug.LogError("Error message: " + request.error);
            yield break;
        }

        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

        // Set the downloaded sprite to the target image component
        targetImage.sprite = sprite;
    }

    IEnumerator DelayedSiblingIndex(GameObject prefabInstance, GameObject realParent)
    {
        yield return null;  // Wait for the next frame to let Unity handle layout processing

        // Get the current child count again (after layout update)
        int childCount = realParent.transform.childCount;

        // Set the sibling index to right before the last child
        prefabInstance.transform.SetSiblingIndex(childCount - 2); // childCount - 2 puts it just before the last element
    }

    public void DeleteImage()
    {
        imageRellenarPlato.GetComponent<Image>().sprite = null;
        buttonSubirImagen.SetActive(true);
        buttonBorrarImagen.SetActive(false);
        textUrlImagenRellenarPlato.text = "";
    }

    void UpdatePrefabs()
    {
        numeroMenus = DataBase.numeroMenu;
        nombres = DataBase.nombrePlatos;
        descripcion = DataBase.descripcionPlatos;
        precios = DataBase.precioPlatos;
        sprites = DataBase.spritePlatos;
        secciones = DataBase.seccion;
        toggles = DataBase.toggle;
        imageUrls = DataBase.imageUrls;
        alergs1 = DataBase.alergs1;
        alergs2 = DataBase.alergs2;
        alergs3 = DataBase.alergs3;
        alergs4 = DataBase.alergs4;
        alergs5 = DataBase.alergs5;
        alergs6 = DataBase.alergs6;
        alergs7 = DataBase.alergs7;
        alergs8 = DataBase.alergs8;
        alergs9 = DataBase.alergs9;
        alergs10 = DataBase.alergs10;
        alergs11 = DataBase.alergs11;
        alergs12 = DataBase.alergs12;
        alergs13 = DataBase.alergs13;
        alergs14 = DataBase.alergs14;
        vegs = DataBase.vegs;

        if (prefabsPlato == null)
        {
            prefabsPlato = new GameObject[0]; // Prevent further errors
        }

        if (nombres == null)
        {
            nombres = new string[0]; // Prevent further errors
        }

        if (prefabsPlato.Length < nombres.Length)
        {
            creatingData = false;
        }
        else if (prefabsPlato.Length > nombres.Length)
        {
            List<GameObject> prefabsList = new List<GameObject>(prefabsPlato);

            for (int i = prefabsList.Count - 1; i >= 0; i--)
            {
                TMP_Text textComponent = prefabsList[i].transform.GetChild(0).GetComponent<TMP_Text>();

                if (textComponent != null)
                {
                    string prefabName = textComponent.text;

                    if (!nombres.Contains(prefabName))
                    {
                        Debug.Log($"Destroying prefab: {prefabName}");

                        Destroy(prefabsList[i]);
                        prefabsList.RemoveAt(i);
                        break; 
                    }
                }
            }

            prefabsPlato = prefabsList.ToArray();
        }
        else
        {
            for (int i = 0; i < nombres.Length; i++)
            {
                SetPrefabDetails(prefabsPlato[i], i, numeroMenus, nombres, descripcion, precios, sprites, secciones, toggles, imageUrls, alergs1, alergs2, alergs3, alergs4, alergs5, alergs6, alergs7, alergs8, alergs9, alergs10, alergs11, alergs12, alergs13, alergs14, vegs, optionGroups);
            }
        }
    }

    void OnClickButtonPlato(int index, bool isNew, string seccion)//
    {
        canvasRellenarPlato.SetActive(true);

        var inputFields = canvasRellenarPlato.GetComponentsInChildren<TMP_InputField>();
        var imageFields = canvasRellenarPlato.GetComponentsInChildren<Image>();
        var textFields = canvasRellenarPlato.GetComponentsInChildren<TMP_Text>();
        var toggleFields = canvasRellenarPlato.GetComponentsInChildren<Toggle>();

        if (isNew)
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i].text = string.Empty;
            }

            inputFields[4].text = seccion;

            imageFields[4].sprite = null;
            buttonSubirImagen.SetActive(true);
            buttonBorrarImagen.SetActive(false);
            creatingData = true;

            // Reseteamos toggles si es nuevo plato
            for (int i=0; i<toggleFields.Length; i++)
            {
                toggleFields[i].isOn = false;

            }
        }
        else
        {
            var clickedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            var textComponents = clickedObject.GetComponentsInChildren<TMP_Text>();
            var imageComponents = clickedObject.GetComponentsInChildren<Image>();

            inputFields[0].text = textComponents[0].text;
            inputFields[1].text = textComponents[1].text;
            inputFields[2].text = textComponents[2].text.Replace("€", "").Replace(".",",");
            inputFields[3].text = textComponents[0].text;
            inputFields[4].text = textComponents[4].text;
            textFields[0].text = textComponents[5].text;
            textFields[1].text = textComponents[21].text.Trim(); // option groups

            imageFields[4].sprite = imageComponents[2].sprite;
            imageFields[4].color = imageComponents[2].color;

            if (imageComponents[2].sprite != null)
            {
                buttonSubirImagen.SetActive(false);
                buttonBorrarImagen.SetActive(true);
            }
            else
            {
                buttonSubirImagen.SetActive(true);
                buttonBorrarImagen.SetActive(false);
            }

            // Alergenos
            for (int i=0; i< 16 - 1; i++)  //aqui antes era  for (int i=0; i< toggleFields.Length - 1; i++) ... -1 because not optiongroups toggle (antes de añadir los toggles de cocinas)
            {
                // Debug.Log(textComponents.Length);
                // Debug.Log(i + 6);

                if (textComponents[i+6].text == "1")
                {
                    toggleFields[i].isOn = true;
                }
                else
                {
                    toggleFields[i].isOn = false;
                }
            }

            // Veg
            if (textComponents[20].text == "1")
            {
                toggleFields[14].isOn = false; // Ninguno
                toggleFields[15].isOn = true; // Vegetariano
                toggleFields[16].isOn = false; // Vegano
            }
            else if (textComponents[20].text == "2")
            {
                toggleFields[14].isOn = false;
                toggleFields[15].isOn = false;
                toggleFields[16].isOn = true;
            }
            else
            {
                toggleFields[14].isOn = true;
                toggleFields[15].isOn = false;
                toggleFields[16].isOn = false;
            }

            // Options
            string rawOptions = textComponents[21].text.Trim();

            SpawnCuadroOpcionesFromText(rawOptions);

            // Cocinas: los toggles cocinas son a partir del 17
            int nElegido = int.Parse(textComponents[22].text);
            toggleFields[17 + nElegido].isOn = true;
        }
    }

    public void SpawnCuadroOpciones()
    {
        GameObject newObject = Instantiate(prefabCuadroOpciones, contentDetallePlato);
        int lastIndex = buttonAñadirOpcion.transform.GetSiblingIndex();
        newObject.transform.SetSiblingIndex(lastIndex);
    }

    public void SpawnCuadroOpcionesFromText(string rawOptions)
    {
        rawOptions = rawOptions.Trim();

        if (string.IsNullOrWhiteSpace(rawOptions))
        {
            return;
        }

        string[] groups = rawOptions.Split(';');
        List<string> validGroups = new List<string>();

        foreach (string group in groups)
        {
            string trimmed = group.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                validGroups.Add(trimmed);
            }
        }

        int groupCount = validGroups.Count;

        for (int i = 0; i < groupCount; i++)
        {
            GameObject cuadroOpciones = Instantiate(prefabCuadroOpciones, contentDetallePlato);

            // Extract group data
            string groupText = validGroups[i]; // e.g., "Salsas: Ketchup, Mayonesa"
            string title = "";
            string[] options = new string[0];

            // Split title and options
            if (groupText.Contains(":"))
            {
                string[] parts = groupText.Split(':');
                title = parts[0].Trim(); // e.g., "Salsas"
                options = parts[1].Split(','); // e.g., [ "Ketchup", " Mayonesa" ]
            }

            // Get all TMP_InputFields inside the prefab
            TMP_InputField[] inputFields = cuadroOpciones.GetComponentsInChildren<TMP_InputField>();

            // Assign title to the first input
            if (inputFields.Length > 0) inputFields[0].text = title;

            // Assign options if available
            if (inputFields.Length > 1 && options.Length > 0)
                inputFields[1].text = options[0].Trim(); // e.g., "Ketchup"

            if (inputFields.Length > 2 && options.Length > 1)
                inputFields[2].text = options[1].Trim(); // e.g., "Mayonesa"

            int lastIndex = buttonAñadirOpcion.transform.GetSiblingIndex();
            cuadroOpciones.transform.SetSiblingIndex(lastIndex);
        }
    }

    public void Quit()
    {
        int childCount = contentDetallePlato.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = contentDetallePlato.GetChild(i);

            // Skip first and last
            if (i == 0 || i == childCount - 1)
                continue;

            Destroy(child.gameObject);
        }

        var toggleFields = canvasRellenarPlato.GetComponentsInChildren<Toggle>();
        // Reseteamos toggles
        for (int i = 0; i < toggleFields.Length; i++)
        {
            toggleFields[i].isOn = false;

        }

        creatingData = false;
        canvasRellenarPlato.SetActive(false);
    }

    public void QuitSeccion()
    {
        creatingData = false;
        canvasRellenarSeccion.SetActive(false);
    }

    void OnClickButtonSeccion()
    {
        canvasRellenarSeccion.SetActive(true);
        var inputField = canvasRellenarSeccion.GetComponentInChildren<TMP_InputField>();

        // Clear previous input
        inputField.text = string.Empty;

        var buttons = canvasRellenarSeccion.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(AddNewSection);
        buttons[1].onClick.AddListener(QuitSeccion);
    }

    public void AddNewSection()
    {
        var inputField = canvasRellenarSeccion.GetComponentInChildren<TMP_InputField>();

        // Get the new section name from the input field
        string newSectionName = inputField.text; // Assuming the first input field is for the section name

        if (string.IsNullOrWhiteSpace(newSectionName))
        {
            Debug.LogError("Section name cannot be empty.");
            return;
        }

        if (DataBase.seccion == null)
        {
            DataBase.seccion = new string[0]; // Initialize with an empty array
        }
        Array.Resize(ref DataBase.seccion, DataBase.seccion.Length + 1);
        DataBase.seccion[DataBase.seccion.Length - 1] = newSectionName;

        // Update secciones to reflect the latest DataBase
        secciones = DataBase.seccion; // Ensure this is updating your reference

        // Create the section UI in the menu
        newSection = true;
        //CreateSeccionText();

        // Optionally, you can close the section creation UI
        canvasRellenarSeccion.SetActive(false);
    }

    public void UpdateMenu()
    {
        // Numero menu
        Debug.Log(menusRoot.GetComponentInChildren<TMP_Text>().text);
        int menuNumber = int.Parse(System.Text.RegularExpressions.Regex.Match(menusRoot.GetComponentInChildren<TMP_Text>().text, @"\d+").Value);

        var inputFields = canvasRellenarPlato.GetComponentsInChildren<TMP_InputField>();
        var texts = canvasRellenarPlato.GetComponentsInChildren<TMP_Text>();
        var imagePlato = canvasRellenarPlato.GetComponentsInChildren<Image>();
        var toggles = canvasRellenarPlato.GetComponentsInChildren<Toggle>();
        int[] alergenos = new int[14];
        int veg = 0;
        string optiongroups = "";
        int destino = 0;

        for (int i = 0; i < 14; i++)
        {
            alergenos[i] = toggles[i].isOn ? 1 : 0;
        }

        // Veg
        if (toggles[14].isOn) // Ninguno
        {
            veg = 0;
        }
        else if (toggles[15].isOn) // Vegetariano
        {
            veg = 1;
        }
        else // Vegano
        {
            veg = 2;
        }

        // Option Groups
        string result = "";

        foreach (Transform child in canvasRellenarPlato.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "CuadroOpciones(Clone)")
            {
                var inputs = child.GetComponentsInChildren<TMP_InputField>();
                if (inputs.Length == 0) continue;

                string title = inputs[0].text;
                string[] options = new string[inputs.Length - 1];
                for (int i = 1; i < inputs.Length; i++)
                    options[i - 1] = inputs[i].text;

                result += $"{title}: {string.Join(", ", options)}; ";
            }
        }

        if (result.EndsWith("; "))
            result = result.Substring(0, result.Length - 2);

        Debug.Log(result);
        optiongroups = result;




        if (inputFields[0].text == "") // Clear url text if there is no image plato
        {
            Error1.SetActive(true);

            Debug.LogError("Nombre incorrecto.");
            return;
        }

        if (imagePlato[2].sprite == null) // Clear url text if there is no image plato
        {
            texts[0].text = "";
        }

        // Destino (cocina, camarero etc)
        for (int i = 17; i < 21; i++)
        {
            if (toggles[i].isOn)
            {
                destino=i-17;
                Debug.Log("toggle cocina encendido número: "+i);
            }
        }

        // para el precio: que sirva igual . y , para los decimales y que quite el símbolo € en caso de introducirlo
        string priceText = inputFields[2].text.Replace("€", "").Trim(); // Quita el símbolo de €
        
        // Normaliza los formatos decimales (permite usar tanto 2,30 como 2.30)
        priceText = priceText.Replace(".", ","); // Cambia puntos a comas para el formato español
        
        if (!float.TryParse(priceText, NumberStyles.Any, new CultureInfo("es-ES"), out float price))
        {
            Error2.SetActive(true);
            Debug.LogError("Error al convertir el precio: " + priceText);
            return;
        }
    

        if (creatingData)
        {
            Debug.Log("creatingData");
            // If creating new data, initiate the creation process with the provided information and default image URL.
            StartCoroutine(CreateMenuData(inputFields[0].text, menuNumber, inputFields[1].text, price, texts[0].text, inputFields[4].text, alergenos[0], alergenos[1], alergenos[2], alergenos[3], alergenos[4], alergenos[5], alergenos[6], alergenos[7], alergenos[8], alergenos[9], alergenos[10], alergenos[11], alergenos[12], alergenos[13], veg, optiongroups, destino));
        }
        else
        {
            // For existing items, use the original name to update the item with new details and default image URL.
            string originalName = inputFields[3].text;
            string seccion = inputFields[4].text;

            if (inputFields.Length < 2 || texts.Length < 1 || alergenos.Length < 14)
            {
                Debug.LogError("Uno de los arrays no tiene la cantidad necesaria de elementos.");
            }

            Debug.Log("UPDATE: " + originalName + inputFields[0].text + menuNumber + inputFields[1].text + price + texts[0].text + seccion);
            StartCoroutine(UpdateMenuData(originalName, inputFields[0].text, menuNumber, inputFields[1].text, price, texts[0].text, seccion,  alergenos[0], alergenos[1], alergenos[2], alergenos[3], alergenos[4], alergenos[5], alergenos[6], alergenos[7], alergenos[8], alergenos[9], alergenos[10], alergenos[11], alergenos[12], alergenos[13], veg, optiongroups, destino));
        }
    }

    public IEnumerator UpdateMenuData(string originalName, string newName, int menuNumber, string description, float price, string imageUrl, string seccion, int alerg1, int alerg2, int alerg3, int alerg4, int alerg5, int alerg6, int alerg7, int alerg8, int alerg9, int alerg10, int alerg11, int alerg12, int alerg13, int alerg14, int veg, string optiongroups, int toggle)
    {
        var id = LoginManagerResponsable.restaurantID;
        string jsonData = $"{{\"id\":\"{id}\",\"name\":\"{originalName}\",\"new_name\":\"{newName}\",\"menuNumber\":\"{menuNumber}\",\"description\":\"{description}\",\"price\":{price.ToString(CultureInfo.InvariantCulture)},\"imageUrl\":\"{imageUrl}\",\"seccion\":\"{seccion}\",\"alerg1\":\"{alerg1}\",\"alerg2\":\"{alerg2}\",\"alerg3\":\"{alerg3}\",\"alerg4\":\"{alerg4}\",\"alerg5\":\"{alerg5}\",\"alerg6\":\"{alerg6}\",\"alerg7\":\"{alerg7}\",\"alerg8\":\"{alerg8}\",\"alerg9\":\"{alerg9}\",\"alerg10\":\"{alerg10}\",\"alerg11\":\"{alerg11}\",\"alerg12\":\"{alerg12}\",\"alerg13\":\"{alerg13}\",\"alerg14\":\"{alerg14}\",\"veg\":\"{veg}\",\"optionGroups\":\"{optiongroups}\",\"toggle\":\"{toggle}\"}}";
        yield return SendRequest("/update", jsonData);
        UpdatePrefab(originalName, newName, description, price, imageUrl, alerg1, alerg2, alerg3, alerg4, alerg5, alerg6, alerg7, alerg8, alerg9, alerg10, alerg11, alerg12, alerg13, alerg14, veg, optiongroups, toggle);
        canvasRellenarPlato.SetActive(false);
        Debug.Log("info enviada: " + jsonData);
    }

    private void UpdatePrefab(string originalName, string newName, string description, float price, string imageUrl, int alerg1, int alerg2, int alerg3, int alerg4, int alerg5, int alerg6, int alerg7, int alerg8, int alerg9, int alerg10, int alerg11, int alerg12, int alerg13, int alerg14, int veg, string optiongroups, int toggle)
    {
        int index = Array.IndexOf(DataBase.nombrePlatos, originalName);
        if (index >= 0 && index < prefabsPlato.Length)
        {
            //DataBase.nombrePlatos[index] = newName;
            //DataBase.descripcionPlatos[index] = description;
            //DataBase.precioPlatos[index] = price;
            //DataBase.imageUrls[index] = imageUrl;
            //DataBase.alergs1[index] = alerg1;
            //DataBase.alergs2[index] = alerg2;
            //DataBase.alergs3[index] = alerg3;
            //DataBase.alergs4[index] = alerg4;
            //DataBase.alergs5[index] = alerg5;
            //DataBase.alergs6[index] = alerg6;
            //DataBase.alergs7[index] = alerg7;
            //DataBase.alergs8[index] = alerg8;
            //DataBase.alergs9[index] = alerg9;
            //DataBase.alergs10[index] = alerg10;
            //DataBase.alergs11[index] = alerg11;
            //DataBase.alergs12[index] = alerg12;
            //DataBase.alergs13[index] = alerg13;
            //DataBase.alergs14[index] = alerg14;
            //DataBase.vegs[index] = veg;

            var prefab = prefabsPlato[index];
            var texts = prefab.GetComponentsInChildren<TMP_Text>();
            texts[0].text = newName;
            texts[1].text = description;
            texts[2].text = price.ToString("0.00").Replace(".", ",") + "€";
            texts[5].text = imageUrl;
            texts[6].text = alerg1.ToString();
            texts[7].text = alerg2.ToString();
            texts[8].text = alerg3.ToString();
            texts[9].text = alerg4.ToString();
            texts[10].text = alerg5.ToString();
            texts[11].text = alerg6.ToString();
            texts[12].text = alerg7.ToString();
            texts[13].text = alerg8.ToString();
            texts[14].text = alerg9.ToString();
            texts[15].text = alerg10.ToString();
            texts[16].text = alerg11.ToString();
            texts[17].text = alerg12.ToString();
            texts[18].text = alerg13.ToString();
            texts[19].text = alerg14.ToString();
            texts[20].text = veg.ToString();
            texts[21].text = optiongroups.ToString();
            texts[22].text = toggle.ToString();

            var imageComponents = prefab.GetComponentsInChildren<Image>();
            imageComponents[2].sprite = DataBase.spritePlatos[index];
            StartCoroutine(LoadSpriteFromUrl(imageUrl, index));
            // Start the coroutine to download the image and set the sprite when ready
            if (imageUrl != "")
            {
                StartCoroutine(DownloadAndSetImage(imageUrl, imageComponents[2]));
            }
            else
            {
                imageComponents[2].sprite = null;
            }

            // var toggles = prefab.GetComponentsInChildren<Toggle>();
            // if (alerg1 == 1)
            // {
            //     toggles[0].isOn = true;
            // }
            // else
            // {
            //     toggles[0].isOn = false;
            // }
        }
    }

    private IEnumerator CreateMenuData(string name, int menuNumber, string description, float price, string imageUrl, string seccion, int alerg1, int alerg2, int alerg3, int alerg4, int alerg5, int alerg6, int alerg7, int alerg8, int alerg9, int alerg10, int alerg11, int alerg12, int alerg13, int alerg14, int veg, string optiongroups, int toggle)
    {
        // int toggle=0; //toggle default
        var id = LoginManagerResponsable.restaurantID;
        string jsonData = $"{{\"id\":\"{id}\",\"name\":\"{name}\",\"menuNumber\":\"{menuNumber}\",\"description\":\"{description}\",\"price\":{price.ToString(CultureInfo.InvariantCulture)},\"imageUrl\":\"{imageUrl}\",\"seccion\":\"{seccion}\",\"toggle\":\"{toggle}\",\"alerg1\":\"{alerg1}\",\"alerg2\":\"{alerg2}\",\"alerg3\":\"{alerg3}\",\"alerg4\":\"{alerg4}\",\"alerg5\":\"{alerg5}\",\"alerg6\":\"{alerg6}\",\"alerg7\":\"{alerg7}\",\"alerg8\":\"{alerg8}\",\"alerg9\":\"{alerg9}\",\"alerg10\":\"{alerg10}\",\"alerg11\":\"{alerg11}\",\"alerg12\":\"{alerg12}\",\"alerg13\":\"{alerg13}\",\"alerg14\":\"{alerg14}\",\"veg\":\"{veg}\",\"optiongroups\":\"{optiongroups}\",\"toggle\":\"{toggle}\"}}";
        yield return SendRequest("/add", jsonData);

        if (DataBase.nombrePlatos == null)
        {
            DataBase.nombrePlatos = new string[0]; // Prevent further errors
        }
        int newIndex = DataBase.nombrePlatos.Length;

        Array.Resize(ref DataBase.nombrePlatos, newIndex + 1);
        DataBase.nombrePlatos[newIndex] = name;

        Array.Resize(ref DataBase.descripcionPlatos, newIndex + 1);
        DataBase.descripcionPlatos[newIndex] = description;

        Array.Resize(ref DataBase.precioPlatos, newIndex + 1);
        DataBase.precioPlatos[newIndex] = price;

        StartCoroutine(LoadSpriteFromUrl(imageUrl, newIndex));
        Array.Resize(ref DataBase.spritePlatos, newIndex + 1);

        Array.Resize(ref DataBase.seccion, newIndex + 1);
        DataBase.seccion[newIndex] = seccion;

        Array.Resize(ref DataBase.imageUrls, newIndex + 1);
        DataBase.imageUrls[newIndex] = imageUrl;

        // Array.Resize(ref DataBase.toggle, newIndex + 1);
        // DataBase.toggle[newIndex] = toggle;

        Array.Resize(ref DataBase.alergs1, newIndex + 1);
        DataBase.alergs1[newIndex] = alerg1;

        Array.Resize(ref DataBase.alergs2, newIndex + 1);
        DataBase.alergs2[newIndex] = alerg2;

        Array.Resize(ref DataBase.alergs3, newIndex + 1);
        DataBase.alergs3[newIndex] = alerg3;

        Array.Resize(ref DataBase.alergs4, newIndex + 1);
        DataBase.alergs4[newIndex] = alerg4;

        Array.Resize(ref DataBase.alergs5, newIndex + 1);
        DataBase.alergs5[newIndex] = alerg5;

        Array.Resize(ref DataBase.alergs6, newIndex + 1);
        DataBase.alergs6[newIndex] = alerg6;

        Array.Resize(ref DataBase.alergs7, newIndex + 1);
        DataBase.alergs7[newIndex] = alerg7;

        Array.Resize(ref DataBase.alergs8, newIndex + 1);
        DataBase.alergs8[newIndex] = alerg8;

        Array.Resize(ref DataBase.alergs9, newIndex + 1);
        DataBase.alergs9[newIndex] = alerg9;

        Array.Resize(ref DataBase.alergs10, newIndex + 1);
        DataBase.alergs10[newIndex] = alerg10;

        Array.Resize(ref DataBase.alergs11, newIndex + 1);
        DataBase.alergs11[newIndex] = alerg11;

        Array.Resize(ref DataBase.alergs12, newIndex + 1);
        DataBase.alergs12[newIndex] = alerg12;

        Array.Resize(ref DataBase.alergs13, newIndex + 1);
        DataBase.alergs13[newIndex] = alerg13;

        Array.Resize(ref DataBase.alergs14, newIndex + 1);
        DataBase.alergs14[newIndex] = alerg14;

        Array.Resize(ref DataBase.vegs, newIndex + 1);
        DataBase.vegs[newIndex] = veg;

        Array.Resize(ref DataBase.optionGroups, newIndex + 1);
        DataBase.optionGroups[newIndex] = optiongroups;

        Array.Resize(ref DataBase.toggle, newIndex + 1);
        DataBase.toggle[newIndex] = toggle;

        AddNewPrefab(newIndex, name, description, price, imageUrl, seccion, alerg1, alerg2, alerg3, alerg4, alerg5, alerg6, alerg7, alerg8, alerg9, alerg10, alerg11, alerg12, alerg13, alerg14, veg, optiongroups, toggle);
        canvasRellenarPlato.SetActive(false);
    }

    private IEnumerator SendRequest(string endpoint, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url + "/menu" + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            //Debug.Log("Response: " + request.downloadHandler.text);
            UpdatePrefabs();
        }
    }
    private void DeleteMenuOnClick(int index)
    {
        // Call the coroutine to delete the data from the backend and the array
        string jsonData = $"{{\"name\":\"{DataBase.nombrePlatos[index]}\"}}";
        StartCoroutine(DeleteMenuData(jsonData, index));
    }
    private IEnumerator DeleteMenuData(string jsonData, int index)
    {
        UnityWebRequest request = new UnityWebRequest(url + "/menu" + "/delete", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);

            // Remove item from database arrays
            RemoveItemFromArray(ref DataBase.nombrePlatos, index);
            RemoveItemFromArray(ref DataBase.descripcionPlatos, index);
            RemoveItemFromArray(ref DataBase.precioPlatos, index);
            RemoveItemFromArray(ref DataBase.spritePlatos, index);
            RemoveItemFromArray(ref DataBase.seccion, index); 

            // Destroy the corresponding prefab and remove it from the prefabsPlato array
            Destroy(prefabsPlato[index]);
            RemoveItemFromArray(ref prefabsPlato, index);

            // Update the prefab indices and their text components to reflect the new ordering
            UpdatePrefabIndices();
        }
    }

    private void RemoveItemFromArray<T>(ref T[] array, int index)
    {
        for (int i = index; i < array.Length - 1; i++)
        {
            array[i] = array[i + 1];
        }
        Array.Resize(ref array, array.Length - 1);
    }

    private void UpdatePrefabIndices()
    {
        for (int i = 0; i < prefabsPlato.Length; i++)
        {
            var textComponents = prefabsPlato[i].GetComponentsInChildren<TMP_Text>();

            textComponents[3].text = i.ToString(); // Update the index number in the UI

            // Optionally, update any other UI elements that depend on the index (like button callbacks)
            var buttons = prefabsPlato[i].GetComponentsInChildren<Button>();

            if (buttons[0] != null)
            {
                int updatedIndex = i;  // Capture the correct index in a local variable
                buttons[0].onClick.RemoveAllListeners();  // Clear previous listeners
                buttons[0].onClick.AddListener(() => OnClickButtonPlato(updatedIndex, false, ""));
            }

            if (buttons[1] != null)
            {
                int updatedIndex = i;  // Capture the correct index in a local variable
                buttons[1].onClick.RemoveAllListeners();  // Clear previous listeners
                buttons[1].onClick.AddListener(() => DeleteMenuOnClick(updatedIndex));
            }
        }
    }

    private IEnumerator LoadSpriteFromUrl(string url, int newIndex)
    {
        if (url != "")
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Failed to download image from URL: " + url);
                Debug.LogError("Error message: " + request.error);
                yield break;
            }

            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            DataBase.spritePlatos[newIndex] = newSprite;
        }
        else
        {
            newSprite = null;
        }
    }

    // Desactivar canvas de error
    public void DesactivarError1()
    {
        Error1.SetActive(false);
    }
    public void DesactivarError2()
    {
        Error2.SetActive(false);
    }
}
