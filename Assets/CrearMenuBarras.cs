using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CrearMenuBarras : MonoBehaviour
{
    public GameObject contentMenuPrefab;
    public GameObject contentMenuPrefabDisponible;
    public GameObject zonaEtiquetasPrefab;
    public GameObject contentMenuParent;
    public GameObject zonaEtiquetasParent;
    private Dictionary<int, GameObject> contentMenuByID = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> zonaEtiquetasByID = new Dictionary<int, GameObject>();

    public GameObject prefabEtiqueta;
    public GameObject prefabGridLayout;
    public GameObject prefabGridLayoutDisponible;
    public DataBase DB;
    public DataBasePersonalizacion DB2;
    public GameObject prefabPlatoTPV;

    // Scroll horizontal etiquetas
    public ScrollRect etiquetasScrollRect;
    public ScrollRect scrollRectGestion; // Solo se usa cuando modoGestion = true
    public float smoothTime = 0.3f;

    private TMP_Text textTitulo;
    private TMP_Text textDescripcion;
    private TMP_Text textPrecio;
    private TMP_Text textNumero;
    private TMP_Text textSeccion;
    private TMP_Text textToggle;
    private Image imagePlato;

    private Button botonSeleccionado = null;

    // Para desactivar platos
    public bool modoGestion = false;
    public GameObject prefabPlatoGestion; // aquí irá PLATOTPVdisponible

    private bool isDBLoaded = false;
    private bool isDB2Loaded = false;

    private Dictionary<string, GameObject> sectionGrids = new Dictionary<string, GameObject>();
    // Para guardar platos desactivados
    private Dictionary<int, GameObject> platosInstanciados = new Dictionary<int, GameObject>();

    void Start()
    {
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;
    }

    private void OnDestroy()
    {
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

    private void CheckIfBothDatabasesAreLoaded()
    {
        if (isDBLoaded && isDB2Loaded)
            CreateMenuItems();
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

        Toggle toggle = plato.GetComponentInChildren<Toggle>(true);
        if (toggle != null)
        {
            toggle.SetIsOnWithoutNotify(nuevoValor);

            SwitchToggleTPV switchVisual = toggle.GetComponent<SwitchToggleTPV>();
            if (switchVisual != null)
                switchVisual.RefreshVisual();
        }
    }

    private void CreateMenuItems()
    {
        string[] secciones = DataBase.seccion;
        int[] menuNumbers = DataBase.numeroMenu;

        // Get unique menu IDs in order
        List<int> menuIDs = new List<int>();
        foreach (int m in menuNumbers)
            if (!menuIDs.Contains(m)) menuIDs.Add(m);

        // Instantiate one contentMenu and zonaEtiquetas per menu
        foreach (int menuID in menuIDs)
        {
            GameObject prefabContentAUsar = modoGestion ? contentMenuPrefabDisponible : contentMenuPrefab;
            var content = Instantiate(prefabContentAUsar, contentMenuParent.transform);
            content.name = "ContentMenu" + menuID;
            contentMenuByID[menuID] = content;
            if (!modoGestion)
                NavigationCamarero.Instance.contentMenus[menuID] = content;

            // Pa que se instancie por debajo del panel de Varios (que si no lo tapa)
            Transform simpleCalculator = contentMenuParent.transform.Find("SimpleCalculator");
            if (simpleCalculator != null)
                content.transform.SetSiblingIndex(simpleCalculator.GetSiblingIndex());

            // Assign first menu as ScrollRect content
            // Assign first menu as ScrollRect content
            if (menuID == menuIDs[0])
            {
                if (!modoGestion)
                    NavigationCamarero.Instance.scrollRect.content = content.GetComponent<RectTransform>();
                else if (scrollRectGestion != null)
                    scrollRectGestion.content = content.GetComponent<RectTransform>();
            }

            var etiquetas = Instantiate(zonaEtiquetasPrefab, zonaEtiquetasParent.transform);
            etiquetas.name = "ZonaEtiquetas" + menuID;
            zonaEtiquetasByID[menuID] = etiquetas;
            if (!modoGestion)
                NavigationCamarero.Instance.zonaHorizontales[menuID] = etiquetas;
            if (menuID == menuIDs[0])
                etiquetasScrollRect.content = etiquetas.GetComponent<RectTransform>();
        }

        // Group platos by menu → section
        Dictionary<int, Dictionary<string, List<int>>> platosByMenu = new Dictionary<int, Dictionary<string, List<int>>>();
        for (int i = 0; i < secciones.Length; i++)
        {
            int menu = menuNumbers[i];
            string sec = secciones[i];
            if (!platosByMenu.ContainsKey(menu)) platosByMenu[menu] = new Dictionary<string, List<int>>();
            if (!platosByMenu[menu].ContainsKey(sec)) platosByMenu[menu][sec] = new List<int>();
            platosByMenu[menu][sec].Add(i);
        }

        foreach (var menuPair in platosByMenu)
        {
            int menuID = menuPair.Key;
            GameObject menuContent = contentMenuByID[menuID];
            GameObject zonaEtiquetasMenu = zonaEtiquetasByID[menuID];

            // Get section order from DB
            List<string> seccionesOrdenadas = new List<string>();
            if (DataBase.seccionesOrdenById.TryGetValue(menuID, out string orden) && !string.IsNullOrEmpty(orden))
                seccionesOrdenadas = new List<string>(orden.Split(';'));

            // Add any sections not in saved order (safety fallback)
            foreach (var sec in menuPair.Value.Keys)
                if (!seccionesOrdenadas.Contains(sec)) seccionesOrdenadas.Add(sec);

            foreach (string seccionName in seccionesOrdenadas)
            {
                if (!menuPair.Value.ContainsKey(seccionName)) continue;

                GameObject sectionButton = Instantiate(prefabEtiqueta, zonaEtiquetasMenu.transform);
                TMP_Text etiquetaTitle = sectionButton.GetComponentInChildren<TMP_Text>();
                etiquetaTitle.text = seccionName;

                RectTransform parentRect = sectionButton.GetComponent<RectTransform>();
                float preferredWidth = etiquetaTitle.preferredWidth;
                float padding = 30f;
                parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + padding);
                sectionButton.transform.GetChild(0).GetComponent<RectTransform>()
                    .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + padding - 10);

                Button button = sectionButton.GetComponent<Button>();
                string seccionKey = seccionName;
                GameObject capturedMenuContent = menuContent;
                button.onClick.AddListener(() => OnSectionClicked(seccionKey, capturedMenuContent, button));

                GameObject prefabGridAUsar = modoGestion ? prefabGridLayoutDisponible : prefabGridLayout;
                GameObject gridLayout = Instantiate(prefabGridAUsar, menuContent.transform);
                string gridKey = menuID + "_" + seccionName;
                sectionGrids[gridKey] = gridLayout;
                gridLayout.SetActive(false);
                gridLayout.name = seccionName;

                CreatePlatosForSection(gridLayout, menuPair.Value[seccionName]);
            }
        }
        if (modoGestion)
            SeleccionarPrimeraSeccion();
    }

    private void CreatePlatosForSection(GameObject gridLayout, List<int> platoIndexes)
    {
        string[] nombres = DataBase.nombrePlatos;
        string[] descripcion = DataBase.descripcionPlatos;
        float[] precios = DataBase.precioPlatos;
        Sprite[] sprites = DataBase.spritePlatos;
        string[] secciones = DataBase.seccion;
        int[] toggles = DataBase.toggle;
        int[] disponibles = DataBase.disponible;

        foreach (int i in platoIndexes)
        {
            GameObject prefabAUsar = modoGestion ? prefabPlatoGestion : prefabPlatoTPV;
            GameObject plato = Instantiate(prefabAUsar, transform.position, Quaternion.identity);

                        plato.transform.SetParent(gridLayout.transform, false);

            bool estaDisponible = disponibles[i] == 1;
            platosInstanciados[i] = plato;

            if (modoGestion)
            {
                PlatoToggleDisponible toggleScript = plato.GetComponent<PlatoToggleDisponible>();
                if (toggleScript != null)
                {
                    toggleScript.DB = DB;
                    toggleScript.Setup(i, estaDisponible);
                }
            }

            Button botonPlato = plato.GetComponent<Button>();
            if (botonPlato != null)
                botonPlato.interactable = estaDisponible;

            Transform overlay = plato.transform.Find("OverlayAgotado");
            if (overlay != null)
                overlay.gameObject.SetActive(!estaDisponible);

            TMP_Text[] textComponents = plato.GetComponentsInChildren<TMP_Text>();
            textTitulo = textComponents[0];
            textDescripcion = textComponents[1];
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

            Image[] imageComponents = plato.GetComponentsInChildren<Image>();
            if (imageComponents.Length > 0)
            {
                imagePlato = imageComponents[2];
                imagePlato.sprite = sprites[i];
                imagePlato.preserveAspect = true;

                if (imagePlato.sprite == null)
                {
                    imagePlato.gameObject.SetActive(false);
                    imagePlato.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    AspectFill aspectFill = imagePlato.GetComponent<AspectFill>();
                    if (aspectFill != null)
                        aspectFill.AdjustToCover();
                }
            }
        }
    }

    private void OnSectionClicked(string sectionName, GameObject menuContent, Button clickedButton)
    {
        Color newColorFondo3;

        // Restaurar botón anterior
        if (botonSeleccionado != null)
        {
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo3))
            {
                Image[] imagesAnt = botonSeleccionado.GetComponentsInChildren<Image>();
                Image imgAnterior = imagesAnt[1];
                TMP_Text txtAnterior = botonSeleccionado.GetComponentInChildren<TMP_Text>();
                imgAnterior.color = newColorFondo3;
                UpdateTextColor(imgAnterior, txtAnterior);
            }
        }

        // Cambiar colores del botón pulsado
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo3))
        {
            Image[] images = clickedButton.GetComponentsInChildren<Image>();
            Image img = images[1];
            TMP_Text txt = clickedButton.GetComponentInChildren<TMP_Text>();
            txt.color = newColorFondo3;
            UpdateImageColor(txt, img);
        }

        botonSeleccionado = clickedButton;

        // Activar/desactivar grids
        string menuID = menuContent.name.Replace("ContentMenu", "");

        foreach (var kvp in sectionGrids)
        {
            if (kvp.Key.StartsWith(menuID + "_"))
                kvp.Value.SetActive(false);
        }

        string key = menuID + "_" + sectionName;
        if (sectionGrids.ContainsKey(key))
            sectionGrids[key].SetActive(true);

        // Scroll horizontal de etiquetas
        StartCoroutine(ScrollEtiquetasToButtonCoroutine(clickedButton));
    }

    private IEnumerator ScrollEtiquetasToButtonCoroutine(Button btn)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;

        StartCoroutine(SmoothScrollEtiquetasToBtn(btn, etiquetasScrollRect));
    }

    private IEnumerator SmoothScrollEtiquetasToBtn(Button btn, ScrollRect sr)
    {
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        RectTransform content = sr.content;
        RectTransform viewport = sr.viewport != null ? sr.viewport : sr.GetComponent<RectTransform>();

        // Esperar hasta que el layout esté realmente listo
        float waited = 0f;
        while (content.rect.width < 1f || btnRect.rect.width < 1f)
        {
            waited += Time.deltaTime;
            if (waited > 1f) yield break;
            Canvas.ForceUpdateCanvases();
            yield return null;
        }

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;

        if (contentWidth <= viewportWidth) yield break;

        float btnX = btnRect.anchoredPosition.x;
        float halfWidth = btnRect.rect.width / 2f;
        float maxX = contentWidth - viewportWidth;
        float targetX = Mathf.Clamp(btnX - halfWidth - 3f, 0f, maxX);

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

    void UpdateTextColor(Image boton, TMP_Text text)
    {
        Color backgroundColor = boton.color;
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        text.color = luminance > 0.5f ? Color.black : Color.white;
    }

    void UpdateImageColor(TMP_Text texto, Image imageCambiaColor)
    {
        Color textColor = texto.color;
        float luminance = 0.299f * textColor.r + 0.587f * textColor.g + 0.114f * textColor.b;
        imageCambiaColor.color = luminance > 0.5f ? Color.black : Color.white;
    }

    private void SeleccionarPrimeraSeccion()
    {
        if (contentMenuByID.Count == 0) return;

        int firstMenuID = contentMenuByID.Keys.Min();

        foreach (var kv in contentMenuByID)
            kv.Value.SetActive(kv.Key == firstMenuID);

        foreach (var kv in zonaEtiquetasByID)
            kv.Value.SetActive(kv.Key == firstMenuID);

        Button primerBoton = zonaEtiquetasByID[firstMenuID].GetComponentInChildren<Button>();
        if (primerBoton != null)
            primerBoton.onClick.Invoke();
    }
}