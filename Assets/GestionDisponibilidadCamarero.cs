using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GestionDisponibilidadCamarero : MonoBehaviour
{
    public DataBase DB;
    public GameObject parent;           // el Content donde se instancian secciones + platos
    public GameObject prefabSeccion;    // el mismo prefab de cabecera que usa CrearMenu
    public GameObject prefabPlatoGestion; // PLATO1Disponible
    public DataBasePersonalizacion DB2;      // para los colores (col_botones, etc.)
    public GameObject zonaEtiquetas;         // contenedor de las etiquetas arriba
    public GameObject prefabEtiqueta;        // prefab de la etiqueta de sección
    public GameObject zonaEtiquetas2;
    public ScrollRect scrollRect;            // el ScrollRect del "parent" (contenido con los platos)
    public ScrollRect etiquetasScrollRect;   // el ScrollRect horizontal de las etiquetas
    public ScrollRect etiquetasScrollRect2;
    public float smoothTime = 0.3f;
    public float sectionHeaderHeight = 150f; // altura del titulo de seccion

    private bool isDBLoaded = false;
    private Dictionary<int, GameObject> platosInstanciados = new Dictionary<int, GameObject>();
    private bool isDB2Loaded = false;
    private Dictionary<string, RectTransform> sectionRectTransforms = new Dictionary<string, RectTransform>();
    private string currentActiveSection = null;
    private Vector2 lastScrollPosition;
    private Coroutine currentScrollCoroutine;
    private Coroutine currentEtiquetasScrollCoroutine;
    private Dictionary<string, (Button btn1, Button btn2)> sectionButtons = new Dictionary<string, (Button, Button)>();
    private Coroutine currentEtiquetasScrollCoroutine2;

    void Start()
    {
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;
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
        DataBase.OnDisponibleChanged += SincronizarToggleExterno;
    }

    void OnDisable()
    {
        DataBase.OnDisponibleChanged -= SincronizarToggleExterno;
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

    // Si el plato se desactiva/activa desde OTRA pantalla (TPV, u otro móvil de camarero),
    // reflejamos el cambio en el toggle de este panel sin volver a llamar a la API.
    private void SincronizarToggleExterno(int platoIndex, bool nuevoValor)
    {
        if (!platosInstanciados.ContainsKey(platoIndex)) return;

        GameObject plato = platosInstanciados[platoIndex];

        Toggle toggle = plato.GetComponentInChildren<Toggle>(true);
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(nuevoValor);

        SwitchToggleTPV switchVisual = plato.GetComponentInChildren<SwitchToggleTPV>(true);
        if (switchVisual != null)
        {
            Toggle switchToggle = switchVisual.GetComponent<Toggle>();
            if (switchToggle != null)
                switchToggle.SetIsOnWithoutNotify(nuevoValor);

            switchVisual.RefreshVisual();
        }

        Transform overlay = plato.transform.Find("OverlayAgotado");
        if (overlay != null)
            overlay.gameObject.SetActive(!nuevoValor);
    }

    private void CreateMenuItems()
    {
        string[] nombres = DataBase.nombrePlatos;
        string[] secciones = DataBase.seccion;
        int[] menuNumber = DataBase.numeroMenu;
        int[] disponibles = DataBase.disponible;

        Dictionary<string, List<int>> platosBySeccion = new Dictionary<string, List<int>>();

        for (int i = 0; i < secciones.Length; i++)
        {
            if (menuNumber[i] != 1)
                continue;

            if (!platosBySeccion.ContainsKey(secciones[i]))
                platosBySeccion[secciones[i]] = new List<int>();

            platosBySeccion[secciones[i]].Add(i);
        }

        List<string> ordenSecciones = new List<string>();
        string seccionesOrdenRaw = DataBase.seccionesOrden;
        if (!string.IsNullOrEmpty(seccionesOrdenRaw))
        {
            foreach (string s in seccionesOrdenRaw.Split(';'))
            {
                string trimmed = s.Trim();
                if (!string.IsNullOrEmpty(trimmed) && platosBySeccion.ContainsKey(trimmed))
                    ordenSecciones.Add(trimmed);
            }
        }
        foreach (var key in platosBySeccion.Keys)
        {
            if (!ordenSecciones.Contains(key))
                ordenSecciones.Add(key);
        }

        foreach (string seccionKey in ordenSecciones)
        {
            GameObject seccionInstance = Instantiate(prefabSeccion, transform.position, Quaternion.identity);
            seccionInstance.transform.SetParent(parent.transform, false);

            TMP_Text seccionTitle = seccionInstance.GetComponentInChildren<TMP_Text>();
            seccionTitle.text = seccionKey;

            Image fondoSeccion = seccionInstance.GetComponentInChildren<Image>();
            if (fondoSeccion != null)
                UpdateTextColor(fondoSeccion, seccionTitle);

            // Guardar el RectTransform de la sección para poder hacer scroll hasta ella
            RectTransform sectionRectTransform = seccionInstance.GetComponent<RectTransform>();
            sectionRectTransforms[seccionKey] = sectionRectTransform;

            // --- Etiqueta 1 ---
            GameObject etiquetaInstance = Instantiate(prefabEtiqueta, transform.position, Quaternion.identity);
            etiquetaInstance.transform.SetParent(zonaEtiquetas.transform, false);

            TMP_Text etiquetaTitle = etiquetaInstance.GetComponentInChildren<TMP_Text>();
            etiquetaTitle.text = seccionKey;

            Canvas.ForceUpdateCanvases();
            float padding = 200f;
            RectTransform etiquetaRect = etiquetaInstance.GetComponent<RectTransform>();
            etiquetaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, etiquetaTitle.preferredWidth + padding);

            Button etiquetaButton = etiquetaInstance.GetComponent<Button>();
            if (etiquetaButton == null)
                etiquetaButton = etiquetaInstance.AddComponent<Button>();

            // --- Etiqueta 2 ---
            GameObject etiqueta2Instance = Instantiate(prefabEtiqueta, transform.position, Quaternion.identity);
            etiqueta2Instance.transform.SetParent(zonaEtiquetas2.transform, false);

            TMP_Text etiqueta2Title = etiqueta2Instance.GetComponentInChildren<TMP_Text>();
            etiqueta2Title.text = seccionKey;

            Canvas.ForceUpdateCanvases();
            RectTransform etiqueta2Rect = etiqueta2Instance.GetComponent<RectTransform>();
            etiqueta2Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, etiqueta2Title.preferredWidth + padding);

            Button etiqueta2Button = etiqueta2Instance.GetComponent<Button>();
            if (etiqueta2Button == null)
                etiqueta2Button = etiqueta2Instance.AddComponent<Button>();

            // --- Listeners y guardado ---
            string seccionKeyCapturada = seccionKey; // evitar problema de closure en el foreach
            etiquetaButton.onClick.AddListener(() => OnEtiquetaClicked(seccionKeyCapturada));
            etiqueta2Button.onClick.AddListener(() => OnEtiquetaClicked(seccionKeyCapturada));

            sectionButtons[seccionKey] = (etiquetaButton, etiqueta2Button);

            // Inicializamos el color de fondo con el color base (si no, se queda con el color del prefab)
            ResetButtonColor(etiquetaButton);
            ResetButtonColor(etiqueta2Button);

            foreach (int i in platosBySeccion[seccionKey])
            {
                GameObject plato = Instantiate(prefabPlatoGestion, transform.position, Quaternion.identity);
                plato.transform.SetParent(parent.transform, false);

                platosInstanciados[i] = plato;

                TMP_Text[] textComponents = plato.GetComponentsInChildren<TMP_Text>();
                textComponents[0].text = nombres[i]; // asume que el primer TMP_Text es el nombre del plato

                Image fondoImage = plato.GetComponentInChildren<Image>();
                UpdateTextColor(fondoImage, textComponents[0]);

                AspectFill aspectFill = plato.GetComponentInChildren<AspectFill>();
                    if (aspectFill != null)
                    {
                        Image imagePlato = aspectFill.GetComponent<Image>();
                        imagePlato.sprite = DataBase.spritePlatos[i];
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

                bool estaDisponible = disponibles[i] == 1;

                PlatoToggleDisponible toggleScript = plato.GetComponent<PlatoToggleDisponible>();
                if (toggleScript != null)
                {
                    toggleScript.DB = DB;
                    toggleScript.Setup(i, estaDisponible);
                }

                Transform overlay = plato.transform.Find("OverlayAgotado");
                if (overlay != null)
                    overlay.gameObject.SetActive(!estaDisponible);
            }
        }
    }

    private void OnEtiquetaClicked(string seccionName)
    {
        if (sectionRectTransforms.ContainsKey(seccionName))
            ScrollToSection(sectionRectTransforms[seccionName]);

        ChangeColorEtiqueta(seccionName);
        currentActiveSection = seccionName;
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
            ScrollEtiquetasToButton(btn1, btn2);
        }
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn == null) return;
        Color newColorFondo;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo))
        {
            Image img = btn.GetComponentsInChildren<Image>()[0];
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            img.color = newColorFondo;
            UpdateTextColor(img, txt);
        }
    }

    private void ActivateButtonColor(Button btn)
    {
        if (btn == null) return;
        Color newColorFondo;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorFondo))
        {
            Image img = btn.GetComponentsInChildren<Image>()[0];
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            txt.color = newColorFondo;
            UpdateImageColor(txt, img);
        }
    }

    public void ScrollToSection(RectTransform targetSection)
    {
        if (currentScrollCoroutine != null)
            StopCoroutine(currentScrollCoroutine);
        currentScrollCoroutine = StartCoroutine(SmoothScroll(targetSection));
    }

    private IEnumerator SmoothScroll(RectTransform targetSection)
    {
        float sectionY = targetSection.anchoredPosition.y;
        float offset = 900f;
        Vector2 targetPos = new Vector2(scrollRect.content.anchoredPosition.x, -sectionY - offset);
        Vector2 velocity = Vector2.zero;

        yield return null;

        while (Vector2.Distance(scrollRect.content.anchoredPosition, targetPos) > 0.1f)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                currentScrollCoroutine = null;
                yield break;
            }

            scrollRect.content.anchoredPosition = Vector2.SmoothDamp(
                scrollRect.content.anchoredPosition, targetPos, ref velocity, smoothTime);
            yield return null;
        }

        scrollRect.content.anchoredPosition = targetPos;
        currentScrollCoroutine = null;
    }

    private void ScrollEtiquetasToButton(Button btn1, Button btn2)
    {
        if (currentEtiquetasScrollCoroutine != null)
            StopCoroutine(currentEtiquetasScrollCoroutine);
        if (currentEtiquetasScrollCoroutine2 != null)
            StopCoroutine(currentEtiquetasScrollCoroutine2);

        currentEtiquetasScrollCoroutine = StartCoroutine(SmoothScrollEtiquetasToBtn(btn1, etiquetasScrollRect));
        currentEtiquetasScrollCoroutine2 = StartCoroutine(SmoothScrollEtiquetasToBtn(btn2, etiquetasScrollRect2));
    }

    private IEnumerator SmoothScrollEtiquetasToBtn(Button btn, ScrollRect sr)
    {
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        RectTransform content = sr.content;
        RectTransform viewport = sr.viewport;

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
        float targetX = Mathf.Clamp(btnX - halfWidth - 40f, 0f, maxX);

        Vector2 velocity = Vector2.zero;
        Vector2 targetPos = new Vector2(-targetX, sr.content.anchoredPosition.y);

        while (Vector2.Distance(sr.content.anchoredPosition, targetPos) > 0.5f)
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                yield break;

            sr.content.anchoredPosition = Vector2.SmoothDamp(
                sr.content.anchoredPosition, targetPos, ref velocity, smoothTime);
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

    private void UpdateActiveEtiquetaByScroll()
    {
        // Si hay un scroll animado en curso (por click), no interferimos
        if (currentScrollCoroutine != null) return;

        string currentSection = null;
        float currentScrollY = scrollRect.content.anchoredPosition.y;

        foreach (var kvp in sectionRectTransforms)
        {
            float sectionY = -kvp.Value.anchoredPosition.y;

            if (sectionY <= currentScrollY + sectionHeaderHeight)
            {
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
}