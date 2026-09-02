using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CrearMenu : MonoBehaviour
{
    public static CrearMenu instance;
    private string firstSeccionName;

    private TMP_Text textTitulo;
    private TMP_Text textDescripcion;
    private TMP_Text textPrecio;
    private TMP_Text textNumero;
    private TMP_Text textSeccion;
    private TMP_Text textToggle;
    private Image fondoImage;
    private Image fondoImage2;
    private Image fondoImage3;
    private Image fondoImage4;

    public GameObject zonaEtiquetas;
    public GameObject zonaEtiquetas2;
    public GameObject prefabEtiqueta;
    public GameObject prefabEtiquetaBarra;
    private GameObject prefabEtiquetaInstance;
    public GameObject prefabSeccion;
    public GameObject prefabPlato;
    public GameObject prefabPlatoTPV;
    private GameObject prefabPlatoInstance;
    public GameObject parent;

    public float sectionHeaderHeight = 150f; // altura del titulo de seccion

    public DataBase DB; // Reference to the first DataBase component
    public DataBasePersonalizacion DB2; // Reference to the second DataBase component

    private bool isDBLoaded = false;
    private bool isDB2Loaded = false;

    // Add a reference to the ScrollRect that holds the sections
    public ScrollRect scrollRect;
    public float smoothTime = 0.3f; // How long it takes to reach the target
    private Coroutine currentScrollCoroutine;

    // Scroll horizontal etiquetas
    public ScrollRect etiquetasScrollRect;
    private Coroutine currentEtiquetasScrollCoroutine;
    public ScrollRect etiquetasScrollRect2;
    private Coroutine currentEtiquetasScrollCoroutine2;

    // Dictionary to store the RectTransform of each section
    private Dictionary<string, RectTransform> sectionRectTransforms = new Dictionary<string, RectTransform>();

    // Para que detecte la seccion actual, este diccionario guarda los dos botones
    private Dictionary<string, (Button btn1, Button btn2)> sectionButtons = new Dictionary<string, (Button, Button)>();
    private string currentActiveSection = null;

    private Vector2 lastScrollPosition;
    private Dictionary<int, GameObject> platosInstanciados = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;
        //scrollRect.onValueChanged.AddListener(OnScrollValueChanged); // Para detectar seccion actual
    }

    public void ResetToFirstSeccion()
    {
        if (string.IsNullOrEmpty(firstSeccionName)) return;

        if (sectionRectTransforms.TryGetValue(firstSeccionName, out RectTransform target))
        {
            scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0);
        }

        ChangeColorEtiqueta(firstSeccionName);
        currentActiveSection = firstSeccionName;
    }

    void Update()
    {
        if (scrollRect.content.anchoredPosition != lastScrollPosition)
        {
            lastScrollPosition = scrollRect.content.anchoredPosition;
            UpdateActiveEtiquetaByScroll();
        }
    }

    void OnEnable()
    {
        DataBase.OnDisponibleChanged += ActualizarDisponibilidadVisual;
    }

    void OnDisable()
    {
        DataBase.OnDisponibleChanged -= ActualizarDisponibilidadVisual;
    }

    private void ActualizarDisponibilidadVisual(int platoIndex, bool nuevoValor)
    {
        if (!platosInstanciados.ContainsKey(platoIndex)) return;

        GameObject plato = platosInstanciados[platoIndex];

        Button botonPlato = plato.GetComponent<Button>();
        if (botonPlato != null)
            botonPlato.interactable = nuevoValor;

        Transform overlay = plato.transform.Find("OverlayAgotado");
        if (overlay != null)
            overlay.gameObject.SetActive(!nuevoValor);
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
        isDB2Loaded = true;
        CheckIfBothDatabasesAreLoaded();
    }

    // Function that checks if both databases are loaded
    private void CheckIfBothDatabasesAreLoaded()
    {
        if (isDBLoaded && isDB2Loaded)
        {
            CreateMenuItems(); // Now we are sure both databases are loaded
        }
    }
    
    private void CreateMenuItems()
    {
        // Accessing the DataBase script
        string[] nombres = DataBase.nombrePlatos;
        string[] descripcion = DataBase.descripcionPlatos;
        float[] precios = DataBase.precioPlatos;
        Sprite[] sprites = DataBase.spritePlatos;
        string[] secciones = DataBase.seccion;
        int[] toggles = DataBase.toggle;
        int[] menuNumber = DataBase.numeroMenu;

        // Create a dictionary to store platos by their section
        Dictionary<string, List<int>> platosBySeccion = new Dictionary<string, List<int>>();

        // Group platos by section
        for (int i = 0; i < secciones.Length; i++)
        {
            if (menuNumber[i] != 1)
                continue; // skip platos not in menu 1

            if (!platosBySeccion.ContainsKey(secciones[i]))
            {
                platosBySeccion[secciones[i]] = new List<int>();
            }
            platosBySeccion[secciones[i]].Add(i); // Store the index of the plato
        }

        // Determine section order from saved secciones_orden (";"-separated string)
        // DataBasePersonalizacion.seccionesOrden should be populated when fetching /menus/<restaurant_id>
        List<string> ordenSecciones = new List<string>();
        string seccionesOrdenRaw = DataBase.seccionesOrden; // e.g. "Entrantes;Pizzas;Postres"
        if (!string.IsNullOrEmpty(seccionesOrdenRaw))
        {
            foreach (string s in seccionesOrdenRaw.Split(';'))
            {
                string trimmed = s.Trim();
                if (!string.IsNullOrEmpty(trimmed) && platosBySeccion.ContainsKey(trimmed))
                    ordenSecciones.Add(trimmed);
            }
        }
        // Append any sections not present in the saved order (new/unsynced sections) at the end
        foreach (var key in platosBySeccion.Keys)
        {
            if (!ordenSecciones.Contains(key))
                ordenSecciones.Add(key);
        }

        firstSeccionName = ordenSecciones.Count > 0 ? ordenSecciones[0] : null;

        // Instantiate prefabs for each section and its platos, in saved order
        foreach (string seccionKey in ordenSecciones)
        {
            var seccion = new KeyValuePair<string, List<int>>(seccionKey, platosBySeccion[seccionKey]);
            // Instantiate the section header (as before)
            GameObject prefabSeccionInstance = Instantiate(prefabSeccion, transform.position, Quaternion.identity);
            prefabSeccionInstance.transform.SetParent(parent.transform, false);

            TMP_Text seccionTitle = prefabSeccionInstance.GetComponentInChildren<TMP_Text>();
            seccionTitle.text = seccion.Key;

            // Store the RectTransform of the section for scrolling later
            RectTransform sectionRectTransform = prefabSeccionInstance.GetComponent<RectTransform>();
            sectionRectTransforms[seccion.Key] = sectionRectTransform;

            // Instantiate etiquetas (first instance)
            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                prefabEtiquetaInstance = Instantiate(prefabEtiqueta, transform.position, Quaternion.identity);
            }
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {
                prefabEtiquetaInstance = Instantiate(prefabEtiquetaBarra, transform.position, Quaternion.identity);
            }

            prefabEtiquetaInstance.transform.SetParent(zonaEtiquetas.transform, false);

            TMP_Text etiquetaTitle = prefabEtiquetaInstance.GetComponentInChildren<TMP_Text>();
            etiquetaTitle.text = seccion.Key;

            // Adjust parent width
            RectTransform parentRect1 = prefabEtiquetaInstance.GetComponent<RectTransform>();

            // Force TMP to update preferred size
            Canvas.ForceUpdateCanvases();

            // Get preferred width and apply it to the parent
            float preferredWidth1 = etiquetaTitle.preferredWidth;
            float padding = 200f; // Adjust padding if needed
            parentRect1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth1 + padding);

            // Instantiate etiquetas2 (second instance)
            GameObject prefabEtiqueta2Instance = Instantiate(prefabEtiqueta, transform.position, Quaternion.identity);
            prefabEtiqueta2Instance.transform.SetParent(zonaEtiquetas2.transform, false);

            TMP_Text etiqueta2Title = prefabEtiqueta2Instance.GetComponentInChildren<TMP_Text>();
            etiqueta2Title.text = seccion.Key;

            // Adjust parent width
            RectTransform parentRect2 = prefabEtiqueta2Instance.GetComponent<RectTransform>();

            // Force TMP to update preferred size
            Canvas.ForceUpdateCanvases();

            // Get preferred width and apply it to the second parent
            float preferredWidth2 = etiqueta2Title.preferredWidth;
            parentRect2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth2 + padding);

            // Add Button component and hook up the event to the OnEtiquetaClicked function
            Button etiquetaButton = prefabEtiquetaInstance.GetComponent<Button>();
            if (etiquetaButton == null)
            {
                etiquetaButton = prefabEtiquetaInstance.AddComponent<Button>();  // Add Button if it doesn't exist
            }
            etiquetaButton.onClick.AddListener(() => OnEtiquetaClicked(seccion.Key, etiquetaButton));  // Assign the event
            

            // Add Button component and hook up the event to the OnEtiquetaClicked function
            Button etiqueta2Button = prefabEtiqueta2Instance.GetComponent<Button>();
            if (etiqueta2Button == null)
            {
                etiqueta2Button = prefabEtiqueta2Instance.AddComponent<Button>();  // Add Button if it doesn't exist
            }
            etiqueta2Button.onClick.AddListener(() => OnEtiquetaClicked(seccion.Key, etiqueta2Button));  // Assign the event

            sectionButtons[seccion.Key] = (etiquetaButton, etiqueta2Button);

            // Now instantiate the platos for this section
            foreach (int i in seccion.Value)
            {
                if (SceneManager.GetActiveScene().name == "MobileScene")
                {
                    prefabPlatoInstance = Instantiate(prefabPlato, transform.position, Quaternion.identity);
                }
                else
                {
                    prefabPlatoInstance = Instantiate(prefabPlatoTPV, transform.position, Quaternion.identity);
                }
                prefabPlatoInstance.transform.SetParent(parent.transform, false);

                platosInstanciados[i] = prefabPlatoInstance;

                // Set the correct scale and position for the prefab
                RectTransform prefabPlatoRect = prefabPlatoInstance.GetComponent<RectTransform>();

                prefabPlatoRect.localScale = new Vector3(1, 1, 1);
                if (SceneManager.GetActiveScene().name == "MobileScene")
                {
                    prefabPlatoRect.offsetMin = new Vector2(0, 0);
                    prefabPlatoRect.offsetMax = new Vector2(2400, 850);
                }
                else
                {
                    prefabPlatoRect.offsetMin = new Vector2(0, 0);
                    prefabPlatoRect.offsetMax = new Vector2(240, 85);
                }

                // Find TMP_Text components in children and assign the dish info
                TMP_Text[] textComponents = prefabPlatoInstance.GetComponentsInChildren<TMP_Text>();

                textTitulo = textComponents[0];
                textDescripcion = textComponents[1];
                // hacemos que el texto descripcion solo muestre las dos primeras líneas seguidas de puntos suspensivos
                textDescripcion.enableWordWrapping = true;
                textDescripcion.overflowMode = TextOverflowModes.Ellipsis;
                textDescripcion.maxVisibleLines = 2;

                textPrecio = textComponents[2];
                textNumero = textComponents[3];
                textSeccion = textComponents[4];
                textToggle = textComponents[5];

                textTitulo.text = nombres[i];
                textDescripcion.text = descripcion[i];
                textPrecio.text = precios[i].ToString("0.00").Replace(".", ",") + "€";
                textNumero.text = i.ToString();
                textSeccion.text = secciones[i];
                textToggle.text = toggles[i].ToString();

                bool estaDisponible = DataBase.disponible[i] == 1;

                Button botonPlatoActual = prefabPlatoInstance.GetComponent<Button>();
                if (botonPlatoActual != null)
                    botonPlatoActual.interactable = estaDisponible;

                Transform overlayAgotado = prefabPlatoInstance.transform.Find("OverlayAgotado");
                if (overlayAgotado != null)
                    overlayAgotado.gameObject.SetActive(!estaDisponible);

                // Add image
                AspectFill aspectFill = prefabPlatoInstance.GetComponentInChildren<AspectFill>();

                if (aspectFill != null)
                {
                    Image imagePlato = aspectFill.GetComponent<Image>();
                    imagePlato.sprite = sprites[i];
                    imagePlato.preserveAspect = true;

                    if (imagePlato.sprite == null)
                    {
                        imagePlato.gameObject.SetActive(false);
                        imagePlato.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        aspectFill.AdjustToCover();
                    }
                }

                if (SceneManager.GetActiveScene().name == "MobileScene")
                {
                    // Update text color based on background
                    fondoImage = prefabPlatoInstance.GetComponentInChildren<Image>(); 
                    fondoImage2 = prefabSeccionInstance.GetComponentInChildren<Image>(); 
                    fondoImage3 = prefabEtiquetaInstance.GetComponentInChildren<Image>(); 
                    fondoImage4 = prefabEtiqueta2Instance.GetComponentInChildren<Image>(); 
                    UpdateTextColor(fondoImage, textTitulo);
                    UpdateTextColor(fondoImage, textDescripcion);
                    UpdateTextColor(fondoImage, textPrecio);
                    UpdateTextColor(fondoImage2,seccionTitle);
                    UpdateTextColor(fondoImage3,etiquetaTitle);
                    UpdateTextColor(fondoImage4,etiqueta2Title);
                }

                // Update fuente
                string rutaFuenteTit = "Fonts/" + DataBasePersonalizacion.letra_titulos[0].Replace(" ", "");
                TMP_FontAsset fuenteTit = Resources.Load<TMP_FontAsset>(rutaFuenteTit);
                if (fuenteTit == null)
                    fuenteTit = Resources.Load<TMP_FontAsset>(rutaFuenteTit + " SDF");
                seccionTitle.font = fuenteTit;
                
                string rutaFuenteGral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
                TMP_FontAsset fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral);
                if (fuenteGral == null)
                    fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral + " SDF");
                textTitulo.font = fuenteGral;
                textDescripcion.font = fuenteGral;
                textPrecio.font = fuenteGral;
                etiquetaTitle.font = fuenteGral;
                etiqueta2Title.font = fuenteGral;
            }
        }
    }

    // New function to scroll to the correct section when an etiqueta is clicked
    private void OnEtiquetaClicked(string seccionName, Button clickedButton)
    {
        // Check if the section exists in the dictionary
        if (sectionRectTransforms.ContainsKey(seccionName))
        {
            RectTransform targetSection = sectionRectTransforms[seccionName];
            ScrollToSection(targetSection);
        }

        ChangeColorEtiqueta(seccionName);
        currentActiveSection = seccionName; // actualizo en que seccion estamos
    }

    private void ChangeColorEtiqueta(string seccionName)
    {
        if (currentActiveSection != null && sectionButtons.ContainsKey(currentActiveSection))
        {
            var (prevBtn1, prevBtn2) = sectionButtons[currentActiveSection];
            ResetButtonColor(prevBtn1);
            ResetButtonColor(prevBtn2);
        }

        if (sectionButtons.ContainsKey(seccionName))
        {
            var (btn1, btn2) = sectionButtons[seccionName];
            ActivateButtonColor(btn1);
            ActivateButtonColor(btn2);
            ScrollEtiquetasToButton(btn1, btn2); // <-- now passes both
        }
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn == null) return;
        Color newColorFondo3;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo3))
        {
            Image img = btn.GetComponentsInChildren<Image>()[0];
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            img.color = newColorFondo3;
            UpdateTextColor(img, txt);
        }
    }

    private void ActivateButtonColor(Button btn)
    {
        if (btn == null) return;
        Color newColorFondo3;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo3))
        {
            Image img = btn.GetComponentsInChildren<Image>()[0];
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            txt.color = newColorFondo3;
            UpdateImageColor(txt, img);
        }
    }

    public void ScrollToSection(RectTransform targetSection)
    {
        //Debug.Log("ScrollToSection: Starting scroll to " + targetSection.name);
        // Cancel any ongoing scroll coroutine
        if (currentScrollCoroutine != null)
        {
            //Debug.Log("ScrollToSection: Stopping previous scroll coroutine");
            StopCoroutine(currentScrollCoroutine);
        }
        currentScrollCoroutine = StartCoroutine(SmoothScroll(targetSection));
    }

    private IEnumerator SmoothScroll(RectTransform targetSection)
    {
        //Debug.Log("SmoothScroll: Starting scroll coroutine for " + targetSection.name);
        float sectionY = targetSection.anchoredPosition.y;
        float offset = 900f;  // Adjust this offset as needed
        Vector2 targetPos = new Vector2(scrollRect.content.anchoredPosition.x, -sectionY - offset);
        Vector2 velocity = Vector2.zero;

        // Optionally wait one frame to let button events process if the scroll was triggered by a button click
        yield return null;

        while (Vector2.Distance(scrollRect.content.anchoredPosition, targetPos) > 0.1f)
        {
            // Check for any touch or click; cancel the scroll if detected
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                currentScrollCoroutine = null; // <- añade esto
                yield break;
            }

            scrollRect.content.anchoredPosition = Vector2.SmoothDamp(
                scrollRect.content.anchoredPosition,
                targetPos,
                ref velocity,
                smoothTime
            );

            //Debug.Log("SmoothScroll: Current position: " + scrollRect.content.anchoredPosition + " Target: " + targetPos);
            yield return null;  // Wait for the next frame
        }

        scrollRect.content.anchoredPosition = targetPos;
        //Debug.Log("SmoothScroll: Finished scrolling. Final position: " + scrollRect.content.anchoredPosition);
        currentScrollCoroutine = null;
    }

    // Funciones para detectar en qué sección estamos al hacer scroll y que se actualice el color de las etiquetas de arriba
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        
        Debug.Log("SCROLL DETECTADO: " + scrollPosition);
        UpdateActiveEtiquetaByScroll();
    }

    // Detecta qué sección es la más visible
    private void UpdateActiveEtiquetaByScroll()
    {
        // Si hay un scroll animado en curso, no actualizamos las etiquetas
        if (currentScrollCoroutine != null) return;

        string currentSection = null;
        float currentScrollY = scrollRect.content.anchoredPosition.y;

        foreach (var kvp in sectionRectTransforms)
        {
            float sectionY = -kvp.Value.anchoredPosition.y;

            // La sección ha superado el borde superior si su Y es menor que el scroll actual
            if (sectionY <= currentScrollY + sectionHeaderHeight)
            {
                // Nos quedamos con la más reciente (la de mayor Y que haya pasado)
                if (currentSection == null || sectionY > -sectionRectTransforms[currentSection].anchoredPosition.y)
                {
                    currentSection = kvp.Key;
                }
            }
        }

        if (currentSection != null && currentSection != currentActiveSection)
        {
            ChangeColorEtiqueta(currentSection);
            currentActiveSection = currentSection;
        }
    }

    // Scroll horizontal etiquetas
    // Pass BOTH buttons: btn1 for etiquetasScrollRect, btn2 for etiquetasScrollRect2
    private void ScrollEtiquetasToButton(Button btn1, Button btn2)
    {
        // Yield one frame to ensure layout is ready (fixes first-click bug)
        StartCoroutine(ScrollEtiquetasToButtonCoroutine(btn1, btn2));
    }

    private IEnumerator ScrollEtiquetasToButtonCoroutine(Button btn1, Button btn2)
    {
        // Wait one frame so Unity has finished layout after ForceUpdateCanvases
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;

        if (currentEtiquetasScrollCoroutine != null)
            StopCoroutine(currentEtiquetasScrollCoroutine);
        if (currentEtiquetasScrollCoroutine2 != null)
            StopCoroutine(currentEtiquetasScrollCoroutine2);

        currentEtiquetasScrollCoroutine = StartCoroutine(SmoothScrollEtiquetasToBtn(btn1, etiquetasScrollRect));
        currentEtiquetasScrollCoroutine2 = StartCoroutine(SmoothScrollEtiquetasToBtn(btn2, etiquetasScrollRect2));
    }

    // Each scroll rect uses its own button's position for the calculation
    private IEnumerator SmoothScrollEtiquetasToBtn(Button btn, ScrollRect sr)
    {
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        RectTransform content = sr.content;
        RectTransform viewport = sr.viewport;

        // Wait until layout is actually ready (fixes first-click on zonaEtiquetas)
        float waited = 0f;
        while (content.rect.width < 1f || btnRect.rect.width < 1f)
        {
            waited += Time.deltaTime;
            if (waited > 1f) yield break; // safety timeout
            Canvas.ForceUpdateCanvases();
            yield return null;
        }

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth) yield break;

        float btnX = btnRect.anchoredPosition.x;
        float halfWidth = btnRect.rect.width / 2f;
        float maxX = contentWidth - viewportWidth;
        float targetX = Mathf.Clamp(btnX - halfWidth - 40f, 0f, maxX);

        Vector2 velocity = Vector2.zero;
        Vector2 targetPos = new Vector2(-targetX, sr.content.anchoredPosition.y);

        while (Vector2.Distance(sr.content.anchoredPosition, targetPos) > 0.5f)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                yield break;

            sr.content.anchoredPosition = Vector2.SmoothDamp(
                sr.content.anchoredPosition,
                targetPos,
                ref velocity,
                smoothTime
            );
            yield return null;
        }

        sr.content.anchoredPosition = targetPos;
    }

    private IEnumerator SmoothScrollEtiquetas(float targetX, ScrollRect sr)
    {
        Vector2 velocity = Vector2.zero;
        Vector2 targetPos = new Vector2(targetX, sr.content.anchoredPosition.y);

        while (Vector2.Distance(sr.content.anchoredPosition, targetPos) > 0.5f)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                yield break;

            sr.content.anchoredPosition = Vector2.SmoothDamp(
                sr.content.anchoredPosition,
                targetPos,
                ref velocity,
                smoothTime
            );
            yield return null;
        }

        sr.content.anchoredPosition = targetPos;
    }

    // Función para que se cmabie el color de texto a blanco o negro en función del color de fondo
    void UpdateTextColor(Image boton, TMP_Text text)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        // Cambiar el color del texto basado en la luminancia
        if (luminance > 0.5f)
        {
            // Fondo claro, texto negro
            text.color = Color.black;
        }
        else
        {
            // Fondo oscuro, texto blanco
            text.color = Color.white;
        }
    }

    void UpdateImageColor(TMP_Text texto, Image imageCambiaColor)
    {
        // Obtener el color del texto
        Color textColor = texto.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * textColor.r + 0.587f * textColor.g + 0.114f * textColor.b;

        // Cambiar el color de la imagen basado en la luminancia
        if (luminance > 0.5f)
        {
            // Texto claro → imagen negra
            imageCambiaColor.color = Color.black;
        }
        else
        {
            // Texto oscuro → imagen blanca
            imageCambiaColor.color = Color.white;
        }
    }

}
