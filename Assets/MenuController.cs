using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

/*
 * MenuController.cs
 * Puente entre la API de Gastrali y GastraliMenu.uxml / DishCardItem.uxml.
 *
 * REQUISITOS:
 * - Paquete "Newtonsoft Json" (com.unity.nuget.newtonsoft-json). Si no lo
 *   tienes: Window > Package Manager > botón "+" > Add package by name
 *   > com.unity.nuget.newtonsoft-json. Se usa aquí en vez de JsonUtility
 *   porque la API devuelve tipos mezclados (bool/int/string/null) para
 *   los mismos campos (igual que hacía el isOff() del JS original), y
 *   JsonUtility no tolera esa laxitud.
 *
 * CÓMO CABLEARLO EN EL INSPECTOR:
 * 1. Añade este script al mismo GameObject que tiene el UIDocument (o a
 *    cualquier GameObject y arrastra la referencia al UIDocument).
 * 2. Arrastra el asset "DishCardItem.uxml" al campo "Dish Card Template".
 * 3. Rellena "Restaurant Id" (y "Mesa" si vienes de un QR) — en la web
 *    original estos valores llegan por query string; aquí los pones a
 *    mano o los inyectas desde donde gestiones deep links / QR en tu app.
 * 4. Dale Play. El splash se oculta solo cuando los datos ya están listos
 *    (o al pasar minSplashSeconds, lo que tarde más).
 */
[RequireComponent(typeof(UIDocument))]
public class MenuController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset dishCardTemplate; // arrastra DishCardItem.uxml aquí

    [Header("Config API")]
    [SerializeField] private string apiBase = "https://gastrali.tail634a78.ts.net";
    [SerializeField] private string restaurantId = "";
    [SerializeField] private string mesa = "";
    [SerializeField] private float minSplashSeconds = 1.3f;

    private static readonly string[] AllergenLabels = {
        "Gluten", "Crustáceos", "Huevos", "Pescado", "Cacahuetes", "Soja", "Lácteos",
        "Frutos de cáscara", "Apio", "Mostaza", "Sésamo", "Sulfitos", "Altramuces", "Moluscos"
    };

    // ---- referencias cacheadas del árbol visual ----
    private VisualElement root, appRoot, brandSplash, sectionsContainer, menuTabsContainer, sectionNavContainer;
    private VisualElement sheet, sheetBackdrop, filterSheet, filterBackdrop, sheetAllergens, sheetOptions, sheetImg;
    private VisualElement heroEl, fVegToggle, fAllergensContainer, infoCardEl;

    private VisualElement fixedHeaderEl, headerSpacerEl;
    private ScrollView mainScrollView;
    private ScrollView menuTabsScroll;
    private ScrollView sectionNavScroll;

    private Label restNameLabel, sheetNameLabel, sheetPriceLabel, sheetDescLabel, sheetBadgeVeg;
    private Button filterChipButton, clearFiltersButton;
    private Label filterCountBadge;
    private VisualElement filterIconEl;
    private VisualElement sheetHandle;
    private bool isDraggingSheet;
    private float sheetDragStartY;

    // ---- estado ----
    private List<JObject> allItems = new();
    private List<JObject> menus = new();
    private JObject personalizacion;
    private string activeMenuId;
    private bool vegOnly;
    private readonly HashSet<int> excludedAllergens = new();

    private string sessionToken;
    private Color? accentColor;
    private Color? ppalButtonColor;
    private Color? navIconBaseColor;
    private Color? navIconActiveColor;

    // ---- detalle de plato: cantidad + opciones seleccionadas ----
    private VisualElement sheetFooterEl;
    private Button qtyMinusBtn, qtyPlusBtn, addBtn;
    private Label qtyValueLabel, addBtnPriceLabel;
    private JObject currentDish;
    private List<OptionGroup> currentOptionGroups = new();
    private List<HashSet<int>> currentSelections = new();
    private int currentQty = 1;

    // ---- carrito ----
    private readonly List<CartItem> cart = new();

    private class CartItem
    {
        public string Name;
        public string Options;
        public int Quantity;
        public float UnitPrice;
        public int Toggle;
        public int Orden;
        public float Total => UnitPrice * Quantity;
    }

    // ---- barra inferior ----
    private VisualElement pedidoViewEl, bottomNavEl;
    private Button navMenuBtn, navPedidoBtn, navAsistenciaBtn, navPagarBtn;
    private Label navBadgeLabel;

    // ---- vista de pedido ----
    private VisualElement pedidoEmptyEl, pedidoListEl, pedidoSummaryEl, pedidoHeaderEl;
    private Label pedidoTotalValueLabel;
    private Button pedidoSubmitBtn;
    private bool isSubmittingOrder;
    private Label pedidoOthersLabel;
    private VisualElement pedidoConfirmedEl, pedidoConfirmedListEl;
    private JObject latestMesaState;
    private Coroutine mesaStatePollRoutine;

    // ---- orden de platos (1º/2º/3º) ----
    private bool courseOrderEnabled;
    private VisualElement courseToggleEl, courseToggleKnobEl;
    private Color? tituloBgColor;

    // ---- asistencia ----
    private VisualElement asistenciaBackdrop, asistenciaSheet, navDotEl;
    private Button asistenciaCancelBtn, asistenciaConfirmBtn;
    private bool asistenciaActive;
    private float asistenciaSentAt = -100f;

    // ---- toast ----
    private Label toastLabel;
    private IVisualElementScheduledItem toastHideHandle;

    private bool isProgrammaticScroll;
    private string activeSectionName;
    private Coroutine verticalScrollRoutine;
    private Coroutine horizontalScrollRoutine;

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        if (dishCardTemplate == null)
        {
            Debug.LogError("MenuController: falta asignar 'Dish Card Template' (DishCardItem.uxml) en el Inspector.");
            return;
        }

        CacheReferences();
        BindStaticEvents();
        StartCoroutine(BootRoutine());
    }

    // ============================================================
    // Setup
    // ============================================================

    void CacheReferences()
    {
        appRoot = root.Q<VisualElement>("app-root");
        brandSplash = root.Q<VisualElement>("brand-splash");
        heroEl = root.Q<VisualElement>("hero");
        infoCardEl = root.Q<VisualElement>("info-card");
        fixedHeaderEl = root.Q<VisualElement>("fixed-header");
        headerSpacerEl = root.Q<VisualElement>("header-spacer");
        mainScrollView = root.Q<ScrollView>("main-scroll");
        restNameLabel = root.Q<Label>("rest-name-label");

        menuTabsScroll = root.Q<ScrollView>("menu-tabs-scroll");
        menuTabsContainer = root.Q<VisualElement>("menu-tabs");
        sectionNavContainer = root.Q<VisualElement>("section-nav");
        sectionNavScroll = root.Q<ScrollView>("section-nav-scroll");
        sectionsContainer = root.Q<VisualElement>("sections");

        filterChipButton = root.Q<Button>("filter-chip");
        filterCountBadge = root.Q<Label>("filter-count-badge");
        filterIconEl = root.Q<VisualElement>("filter-icon");

        sheet = root.Q<VisualElement>("sheet");
        sheetBackdrop = root.Q<VisualElement>("sheet-backdrop");
        sheetHandle = root.Q<VisualElement>("sheet-handle");
        sheetImg = root.Q<VisualElement>("sheet-img");
        sheetNameLabel = root.Q<Label>("sheet-name");
        sheetPriceLabel = root.Q<Label>("sheet-price");
        sheetDescLabel = root.Q<Label>("sheet-desc");
        sheetBadgeVeg = root.Q<Label>("sheet-badge-veg");
        sheetAllergens = root.Q<VisualElement>("allergens");
        sheetOptions = root.Q<VisualElement>("sheet-options");

        filterSheet = root.Q<VisualElement>("filter-sheet");
        filterBackdrop = root.Q<VisualElement>("filter-backdrop");
        clearFiltersButton = root.Q<Button>("f-clear");
        fVegToggle = root.Q<VisualElement>("f-veg");
        fAllergensContainer = root.Q<VisualElement>("f-allergens");

        sheetFooterEl = root.Q<VisualElement>("sheet-footer");
        qtyMinusBtn = root.Q<Button>("qty-minus");
        qtyPlusBtn = root.Q<Button>("qty-plus");
        qtyValueLabel = root.Q<Label>("qty-value");
        addBtn = root.Q<Button>("add-btn");
        addBtnPriceLabel = root.Q<Label>("add-btn-price");

        pedidoViewEl = root.Q<VisualElement>("pedido-view");
        bottomNavEl = root.Q<VisualElement>("bottom-nav");

        pedidoEmptyEl = root.Q<VisualElement>("pedido-empty");
        pedidoListEl = root.Q<VisualElement>("pedido-list");
        pedidoSummaryEl = root.Q<VisualElement>("pedido-summary");
        pedidoTotalValueLabel = root.Q<Label>("pedido-total-value");
        
        pedidoSubmitBtn = root.Q<Button>("pedido-submit-btn");
        pedidoOthersLabel = root.Q<Label>("pedido-others");
        pedidoConfirmedEl = root.Q<VisualElement>("pedido-confirmed");
        pedidoConfirmedListEl = root.Q<VisualElement>("pedido-confirmed-list");

        courseToggleEl = root.Q<VisualElement>("course-toggle");
        courseToggleKnobEl = root.Q<VisualElement>("course-toggle-knob");
        pedidoHeaderEl = root.Q<VisualElement>("pedido-header");

        asistenciaBackdrop = root.Q<VisualElement>("asistencia-backdrop");
        asistenciaSheet = root.Q<VisualElement>("asistencia-sheet");
        asistenciaCancelBtn = root.Q<Button>("asistencia-cancel");
        asistenciaConfirmBtn = root.Q<Button>("asistencia-confirm");
        navDotEl = root.Q<VisualElement>("nav-dot");

        toastLabel = root.Q<Label>("toast");

        navMenuBtn = root.Q<Button>("nav-menu");
        navPedidoBtn = root.Q<Button>("nav-pedido");
        navAsistenciaBtn = root.Q<Button>("nav-asistencia");
        navPagarBtn = root.Q<Button>("nav-pagar");
        navBadgeLabel = root.Q<Label>("nav-badge");
    }

    void BindStaticEvents()
    {
        filterChipButton.clicked += OpenFilterSheet;
        clearFiltersButton.clicked += ClearFilters;

        sheetBackdrop.RegisterCallback<ClickEvent>(_ => CloseSheet());
        filterBackdrop.RegisterCallback<ClickEvent>(_ => CloseFilterSheet());
        fVegToggle.RegisterCallback<ClickEvent>(_ => ToggleVeg());

        qtyMinusBtn.clicked += () =>
        {
            if (currentQty <= 1) return;
            currentQty--;
            qtyValueLabel.text = currentQty.ToString();
            UpdateAddBtnState();
        };
        qtyPlusBtn.clicked += () =>
        {
            currentQty++;
            qtyValueLabel.text = currentQty.ToString();
            UpdateAddBtnState();
        };
        addBtn.clicked += ConfirmAddToCart;

        pedidoSubmitBtn.clicked += () => StartCoroutine(SubmitOrderRoutine());

        courseToggleEl.RegisterCallback<ClickEvent>(_ =>
        {
            courseOrderEnabled = !courseOrderEnabled;
            UpdateCourseToggleVisual();
            RenderPedidoView();
        });

        navMenuBtn.clicked += SwitchToMenuView;
        navPedidoBtn.clicked += SwitchToPedidoView;
        navAsistenciaBtn.clicked += OnAsistenciaNavClicked;
        navPagarBtn.clicked += () => Debug.Log("Pagar: pendiente de implementar");

        asistenciaBackdrop.RegisterCallback<ClickEvent>(_ => CloseAsistenciaSheet());
        asistenciaCancelBtn.clicked += CloseAsistenciaSheet;
        asistenciaConfirmBtn.clicked += () => StartCoroutine(SendAsistenciaRoutine());

        sheetHandle.RegisterCallback<PointerDownEvent>(OnSheetHandlePointerDown);
        sheetHandle.RegisterCallback<PointerMoveEvent>(OnSheetHandlePointerMove);
        sheetHandle.RegisterCallback<PointerUpEvent>(OnSheetHandlePointerUp);
        sheetHandle.RegisterCallback<PointerCaptureOutEvent>(OnSheetHandlePointerCaptureOut);
        fixedHeaderEl.RegisterCallback<GeometryChangedEvent>(_ => UpdateStickyHeader());
        mainScrollView.verticalScroller.valueChanged += _ =>
        {
            UpdateStickyHeader();
            UpdateActiveSectionFromScroll();
        };

        // Limpia los placeholders estáticos que solo estaban para
        // previsualizar el layout en el UI Builder.
        sectionsContainer.Clear();
        menuTabsContainer.Clear();
        sectionNavContainer.Clear();
        sheetAllergens.Clear();
        sheetOptions?.Clear();
        fAllergensContainer.Clear();

        BuildAllergenFilterRows();
    }

    void UpdateStickyHeader()
    {
        float alturaHero = heroEl.resolvedStyle.height;
        float alturaBarras = fixedHeaderEl.resolvedStyle.height;

        headerSpacerEl.style.height = alturaBarras;

        float scrollY = mainScrollView.scrollOffset.y;
        float desplazamiento = Mathf.Clamp(alturaHero - scrollY, 0f, alturaHero);
        fixedHeaderEl.style.translate = new Translate(0, desplazamiento);
    }

    IEnumerator BootRoutine()
    {
        float startTime = Time.realtimeSinceStartup;

        yield return StartCoroutine(LoadMenuRoutine());

        float remaining = minSplashSeconds - (Time.realtimeSinceStartup - startTime);
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        HideSplash();
    }

    // ============================================================
    // Carga de datos (equivalente a loadMenu() del original)
    // ============================================================

    IEnumerator LoadMenuRoutine()
    {
        if (string.IsNullOrEmpty(restaurantId))
        {
            Debug.LogError("MenuController: falta 'Restaurant Id'.");
            ShowErrorState("Falta el restaurante", "Configura el Restaurant Id en el Inspector.");
            yield break;
        }

        var reqPerson = UnityWebRequest.Get($"{apiBase}/personalizacion/restaurant/{restaurantId}");
        var reqMenus = UnityWebRequest.Get($"{apiBase}/menus/{restaurantId}");
        var reqItems = UnityWebRequest.Get($"{apiBase}/menu/restaurant/{restaurantId}");

        var opPerson = reqPerson.SendWebRequest();
        var opMenus = reqMenus.SendWebRequest();
        var opItems = reqItems.SendWebRequest();

        yield return new WaitUntil(() => opPerson.isDone && opMenus.isDone && opItems.isDone);

        bool ok = reqPerson.result == UnityWebRequest.Result.Success
                  && reqMenus.result == UnityWebRequest.Result.Success
                  && reqItems.result == UnityWebRequest.Result.Success;

        if (!ok)
        {
            Debug.LogError("MenuController: error de red cargando el menú.");
            ShowErrorState("No se pudo cargar el menú", "Comprueba tu conexión e inténtalo de nuevo.");
            yield break;
        }

        try
        {
            var personList = JArray.Parse(reqPerson.downloadHandler.text);
            personalizacion = personList.Count > 0 ? (JObject)personList[0] : null;

            menus = JArray.Parse(reqMenus.downloadHandler.text).Cast<JObject>().ToList();
            allItems = JArray.Parse(reqItems.downloadHandler.text).Cast<JObject>().ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"MenuController: JSON inválido — {e.Message}");
            ShowErrorState("No se pudo cargar el menú", "Comprueba tu conexión e inténtalo de nuevo.");
            yield break;
        }

        if (allItems.Count == 0)
        {
            ShowErrorState("Menú no disponible", "Vuelve a intentarlo más tarde.");
            yield break;
        }

        ApplyTheme(personalizacion);
        BuildShell();

        StartCoroutine(EnsureSessionThenPollRoutine());
    }

    IEnumerator EnsureSessionRoutine()
    {
        var body = new JObject { ["restaurant_id"] = restaurantId, ["mesa"] = mesa };
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(body.ToString(Newtonsoft.Json.Formatting.None));

        using var req = new UnityWebRequest($"{apiBase}/session/start", "POST");
        req.uploadHandler = new UploadHandlerRaw(payload);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JObject.Parse(req.downloadHandler.text);
            sessionToken = data["token"]?.ToString();
        }
        else
        {
            Debug.LogWarning("session/start falló; el menú sigue siendo visible igualmente.");
        }
    }

    IEnumerator EnsureSessionThenPollRoutine()
    {
        yield return StartCoroutine(EnsureSessionRoutine());

        if (!string.IsNullOrEmpty(sessionToken) && !string.IsNullOrEmpty(mesa))
            mesaStatePollRoutine = StartCoroutine(MesaStatePollRoutine());
    }

    IEnumerator MesaStatePollRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(FetchMesaStateRoutine());
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator FetchMesaStateRoutine()
    {
        if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(restaurantId) || string.IsNullOrEmpty(mesa))
            yield break;

        using var req = UnityWebRequest.Get($"{apiBase}/mesa_state/{restaurantId}/{mesa}");
        req.SetRequestHeader("Authorization", $"Bearer {sessionToken}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) yield break;

        try
        {
            latestMesaState = JObject.Parse(req.downloadHandler.text);
            bool serverActive = latestMesaState["asistencia_active"]?.ToObject<bool>() ?? false;

            bool withinGracePeriod = Time.time - asistenciaSentAt < 5f;
            if (!(withinGracePeriod && asistenciaActive && !serverActive))
                UpdateAsistenciaIndicator(serverActive);

            if (pedidoViewEl.style.display == DisplayStyle.Flex) RenderPedidoView();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"mesa_state: JSON inválido — {e.Message}");
        }
    }

    static string OrderStateLabel(int state) => state switch
    {
        3 => "Servido",
        2 => "En camino",
        _ => "Pendiente"
    };

    // ============================================================
    // Shell (equivalente a buildShell())
    // ============================================================

    void BuildShell()
    {
        restNameLabel.text = personalizacion?["nombre_rest"]?.ToString() ?? "Menú";

        string headerImg = personalizacion?["img_url_cabecero"]?.ToString();
        if (!string.IsNullOrEmpty(headerImg))
            StartCoroutine(LoadImageInto(headerImg, heroEl));
        else
            heroEl.AddToClassList("no-image");

        List<(string id, string name)> menuList = menus.Count > 0
            ? menus.Select(m => (m["id"]?.ToString(), m["menu_name"]?.ToString())).ToList()
            : allItems.Select(i => i["menuNumber"]?.ToString()).Distinct().Select(n => (n, n)).ToList();

        if (menuList.Count == 0) return;
        activeMenuId = menuList[0].id;

        menuTabsContainer.Clear();

        // Igual que en la web (menuList.length > 1 ? renderTabs : ""):
        // si solo hay un menú, no tiene sentido mostrar una fila de tabs
        // con un único botón (y a veces ese único valor es un id "en
        // bruto" tipo el nombre de la hoja, no un nombre pensado para
        // mostrarse en pantalla).
        if (menuTabsScroll != null)
            menuTabsScroll.style.display = menuList.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;

        if (menuList.Count > 1)
        {
            foreach (var (id, name) in menuList)
            {
                var capturedId = id; // evita el problema de closures sobre la variable del foreach en for tradicionales
                var btn = new Button(() =>
                {
                    activeMenuId = capturedId;
                    RefreshMenuTabsActive();
                    RenderSections();
                })
                { text = name, userData = id };
                btn.AddToClassList("menu-tab");
                if (id == activeMenuId) btn.AddToClassList("active");
                menuTabsContainer.Add(btn);
            }
            RefreshMenuTabsActive(); 
        }

        RenderSections();
        UpdateFilterChipLabel();
    }

    void RefreshMenuTabsActive()
    {
        foreach (var child in menuTabsContainer.Children())
        {
            bool isActive = Equals(child.userData, activeMenuId);
            child.EnableInClassList("active", isActive);
            if (accentColor.HasValue)
                child.style.backgroundColor = isActive ? new StyleColor(accentColor.Value) : StyleKeyword.Null;
        }
    }

    // ============================================================
    // Secciones + platos (equivalente a renderSections() / dishCardHTML())
    // ============================================================

    void RenderSections()
    {
        IEnumerable<JObject> items = allItems.Where(i => i["menuNumber"]?.ToString() == activeMenuId);

        if (vegOnly) items = items.Where(IsVeg);
        if (excludedAllergens.Count > 0)
            items = items.Where(i => excludedAllergens.All(idx => IsOff(i[$"alerg{idx}"])));

        var itemList = items.ToList();

        var menuMeta = menus.FirstOrDefault(m => m["id"]?.ToString() == activeMenuId);
        var sectionOrder = new List<string>();
        if (menuMeta?["secciones_orden"] != null)
            sectionOrder.AddRange(menuMeta["secciones_orden"].ToString().Split(';').Where(s => !string.IsNullOrEmpty(s)));

        var seen = new HashSet<string>(sectionOrder);
        foreach (var i in itemList)
        {
            var s = i["seccion"]?.ToString();
            if (!string.IsNullOrEmpty(s) && seen.Add(s)) sectionOrder.Add(s);
        }

        var populated = sectionOrder.Where(s => itemList.Any(i => i["seccion"]?.ToString() == s)).ToList();

        sectionsContainer.Clear();

        if (populated.Count == 0)
        {
            var noResults = new Label("No se encontraron platos.");
            noResults.AddToClassList("no-results");
            sectionsContainer.Add(noResults);
        }
        else
        {
            foreach (var sectionName in populated)
                sectionsContainer.Add(BuildSectionElement(sectionName, itemList.Where(i => i["seccion"]?.ToString() == sectionName)));
        }

        BuildSectionNav(populated);
    }

    VisualElement BuildSectionElement(string sectionName, IEnumerable<JObject> dishes)
    {
        var section = new VisualElement { name = $"sec-{Slug(sectionName)}" };
        section.AddToClassList("section");

        var title = new Label(sectionName);
        title.AddToClassList("section-title");
        title.AddToClassList("font-titulo");
        section.Add(title);

        var list = new VisualElement();
        list.AddToClassList("dish-list");

        foreach (var d in dishes.OrderBy(d => SafeFloat(d["orden"])))
            list.Add(BuildDishCard(d));

        section.Add(list);
        return section;
    }

    VisualElement BuildDishCard(JObject d)
    {
        // CloneTree() devuelve un TemplateContainer que ENVUELVE al root
        // definido en DishCardItem.uxml — por eso buscamos dentro de él
        // en vez de asumir que "card" es directamente el VisualElement
        // con clase "dish-card".
        var card = dishCardTemplate.CloneTree();

        var dishCardRoot = card.Q<VisualElement>("dish-card");
        var dishName = card.Q<Label>("dish-name");
        var dishPrice = card.Q<Label>("dish-price");
        var dishDesc = card.Q<Label>("dish-desc");
        var dishThumb = card.Q<VisualElement>("dish-thumb");
        var badgeVeg = card.Q<Label>("badge-veg");

        dishName.text = d["name"]?.ToString() ?? "";
        dishPrice.text = FormatPrice(d["price"]);
        if (accentColor.HasValue) dishPrice.style.color = accentColor.Value;

        string desc = d["description"]?.ToString();
        if (string.IsNullOrEmpty(desc)) dishDesc.style.display = DisplayStyle.None;
        else dishDesc.text = desc;

        string imgUrl = d["imageUrl"]?.ToString();
        if (!string.IsNullOrEmpty(imgUrl))
        {
            dishThumb.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadImageInto(imgUrl, dishThumb));
        }
        else
        {
            dishThumb.style.display = DisplayStyle.None; // quita el hueco por completo
            dishCardRoot.AddToClassList("no-img");
        }

        badgeVeg.style.display = IsVeg(d) ? DisplayStyle.Flex : DisplayStyle.None;

        card.RegisterCallback<ClickEvent>(_ => OpenDishSheet(d));
        return card;
    }

    void BuildSectionNav(List<string> sections)
    {
        sectionNavContainer.Clear();
        activeSectionName = null;
        if (sections.Count == 0) return;

        for (int idx = 0; idx < sections.Count; idx++)
        {
            string sectionElementName = $"sec-{Slug(sections[idx])}";
            var btn = new Button(() => ScrollToSection(sectionElementName)) { text = sections[idx], userData = sectionElementName };
            btn.AddToClassList("section-nav-item");
            sectionNavContainer.Add(btn);
            if (idx == 0) activeSectionName = sectionElementName;
            SetSectionNavActive(btn, idx == 0);
        }
    }

    void ScrollToSection(string sectionElementName)
    {
        var target = sectionsContainer.Q<VisualElement>(sectionElementName);
        if (target == null) return;

        float headerHeight = fixedHeaderEl.resolvedStyle.height;
        float targetTop = target.worldBound.yMin - mainScrollView.contentContainer.worldBound.yMin;
        float targetY = Mathf.Max(0f, targetTop - headerHeight);

        if (verticalScrollRoutine != null) StopCoroutine(verticalScrollRoutine);
        verticalScrollRoutine = StartCoroutine(AnimateVerticalScrollTo(targetY, 0.35f));

        activeSectionName = sectionElementName;
        foreach (var child in sectionNavContainer.Children())
            SetSectionNavActive(child as Button, Equals(child.userData, sectionElementName));

        var activeBtn = sectionNavContainer.Children().FirstOrDefault(c => Equals(c.userData, sectionElementName)) as Button;
        if (activeBtn != null) ScrollNavToButton(activeBtn);
    }

    IEnumerator AnimateVerticalScrollTo(float targetY, float duration)
    {
        isProgrammaticScroll = true;
        float startY = mainScrollView.scrollOffset.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cúbico
            mainScrollView.scrollOffset = new Vector2(mainScrollView.scrollOffset.x, Mathf.Lerp(startY, targetY, eased));
            yield return null;
        }

        mainScrollView.scrollOffset = new Vector2(mainScrollView.scrollOffset.x, targetY);
        isProgrammaticScroll = false;
        verticalScrollRoutine = null;
    }

    void UpdateActiveSectionFromScroll()
    {
        if (isProgrammaticScroll) return;

        float referenceLine = fixedHeaderEl.worldBound.yMax;
        VisualElement current = null;

        foreach (var child in sectionsContainer.Children())
        {
            if (child.worldBound.yMin <= referenceLine) current = child;
            else break;
        }

        if (current == null || current.name == activeSectionName) return;

        activeSectionName = current.name;
        Button activeBtn = null;

        foreach (var child in sectionNavContainer.Children())
        {
            bool isActive = Equals(child.userData, activeSectionName);
            SetSectionNavActive(child as Button, isActive);
            if (isActive) activeBtn = child as Button;
        }

        if (activeBtn != null) ScrollNavToButton(activeBtn);
    }

    void ScrollNavToButton(Button btn)
    {
        if (sectionNavScroll == null) return;

        float targetX = btn.worldBound.xMin - sectionNavContainer.worldBound.xMin;
        targetX = Mathf.Max(0f, targetX);

        if (horizontalScrollRoutine != null) StopCoroutine(horizontalScrollRoutine);
        horizontalScrollRoutine = StartCoroutine(AnimateHorizontalScrollTo(targetX, 0.25f));
    }

    void SetSectionNavActive(Button btn, bool isActive)
    {
        if (btn == null) return;
        btn.EnableInClassList("active", isActive);
        if (!accentColor.HasValue) return;

        var color = isActive ? new StyleColor(accentColor.Value) : StyleKeyword.Null;
        btn.style.backgroundColor = color;
        btn.style.borderTopColor = color;
        btn.style.borderBottomColor = color;
        btn.style.borderLeftColor = color;
        btn.style.borderRightColor = color;
    }

    IEnumerator AnimateHorizontalScrollTo(float targetX, float duration)
    {
        float startX = sectionNavScroll.scrollOffset.x;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            sectionNavScroll.scrollOffset = new Vector2(Mathf.Lerp(startX, targetX, eased), 0f);
            yield return null;
        }

        sectionNavScroll.scrollOffset = new Vector2(targetX, 0f);
        horizontalScrollRoutine = null;
    }

    // ============================================================
    // Bottom sheet: detalle de plato
    // ============================================================

    void OpenDishSheet(JObject d)
    {
        currentDish = d;
        currentQty = 1;

        sheetNameLabel.text = d["name"]?.ToString() ?? "";
        sheetPriceLabel.text = FormatPrice(d["price"]);
        if (accentColor.HasValue) sheetPriceLabel.style.color = accentColor.Value;

        string desc = d["description"]?.ToString();
        sheetDescLabel.style.display = string.IsNullOrEmpty(desc) ? DisplayStyle.None : DisplayStyle.Flex;
        sheetDescLabel.text = desc ?? "";

        sheetBadgeVeg.style.display = IsVeg(d) ? DisplayStyle.Flex : DisplayStyle.None;

        string imgUrl = d["imageUrl"]?.ToString();
        if (!string.IsNullOrEmpty(imgUrl))
        {
            sheetImg.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadImageInto(imgUrl, sheetImg));
        }
        else sheetImg.style.display = DisplayStyle.None;

        sheetAllergens.Clear();
        for (int i = 1; i <= AllergenLabels.Length; i++)
        {
            if (IsOff(d[$"alerg{i}"])) continue;
            var chip = new Label(AllergenLabels[i - 1]);
            chip.AddToClassList("allergen-chip");
            sheetAllergens.Add(chip);
        }

        currentOptionGroups = ParseOptionGroups(d["optionGroups"]?.ToString());
        currentSelections = currentOptionGroups.Select(_ => new HashSet<int>()).ToList();

        if (sheetOptions != null)
        {
            sheetOptions.Clear();
            for (int gi = 0; gi < currentOptionGroups.Count; gi++)
                sheetOptions.Add(BuildOptionGroupElement(currentOptionGroups[gi], gi));
        }

        qtyValueLabel.text = "1";
        UpdateAddBtnState();

        sheetBackdrop.AddToClassList("open");
        sheet.AddToClassList("open");
    }

    void CloseSheet()
    {
        sheetBackdrop.RemoveFromClassList("open");
        sheet.RemoveFromClassList("open");
    }

    void OnSheetHandlePointerDown(PointerDownEvent evt)
    {
        isDraggingSheet = true;
        sheetDragStartY = evt.position.y;
        sheetHandle.CapturePointer(evt.pointerId);
        sheet.AddToClassList("dragging");
    }

    void OnSheetHandlePointerMove(PointerMoveEvent evt)
    {
        if (!isDraggingSheet) return;

        float delta = evt.position.y - sheetDragStartY;
        delta = Mathf.Clamp(delta, 0f, sheet.resolvedStyle.height);
        sheet.style.translate = new Translate(0, delta);
    }

    void OnSheetHandlePointerUp(PointerUpEvent evt)
    {
        if (!isDraggingSheet) return;
        sheetHandle.ReleasePointer(evt.pointerId);
        FinishSheetDrag(evt.position.y - sheetDragStartY);
    }

    void OnSheetHandlePointerCaptureOut(PointerCaptureOutEvent evt)
    {
        if (!isDraggingSheet) return;
        FinishSheetDrag(0f);
    }

    void FinishSheetDrag(float delta)
    {
        isDraggingSheet = false;
        sheet.RemoveFromClassList("dragging");
        sheet.style.translate = StyleKeyword.Null;

        float threshold = Mathf.Max(80f, sheet.resolvedStyle.height * 0.3f);
        if (delta > threshold) CloseSheet();
    }

    private class OptionGroup
    {
        public string Titulo;
        public bool Obligatorio;
        public string Tipo; // "radio" | "checkbox"
        public readonly List<(string name, float? price)> Opciones = new();
    }

    List<OptionGroup> ParseOptionGroups(string raw)
    {
        var result = new List<OptionGroup>();
        if (string.IsNullOrWhiteSpace(raw)) return result;

        try
        {
            foreach (var g in JArray.Parse(raw).Cast<JObject>())
            {
                var group = new OptionGroup
                {
                    Titulo = g["titulo"]?.ToString() ?? "Opciones",
                    Obligatorio = g["obligatorio"]?.ToObject<bool>() ?? false,
                    Tipo = g["tipo"]?.ToString() == "checkbox" ? "checkbox" : "radio"
                };

                foreach (var opt in g["opciones"] ?? new JArray())
                {
                    var parts = opt.ToString().Split(',');
                    if (parts.Length > 1 && TryParsePrice(parts[parts.Length - 1], out float price))
                        group.Opciones.Add((string.Join(",", parts.Take(parts.Length - 1)).Trim(), price));
                    else
                        group.Opciones.Add((opt.ToString().Trim(), null));
                }

                result.Add(group);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"optionGroups con formato inesperado, se omite: {e.Message}");
        }

        return result;
    }

    VisualElement BuildOptionGroupElement(OptionGroup group, int groupIndex)
    {
        var container = new VisualElement();
        container.AddToClassList("option-group");

        var head = new VisualElement();
        head.AddToClassList("og-head");
        var title = new Label(group.Titulo);
        title.AddToClassList("og-title");
        var req = new Label(group.Obligatorio ? "Obligatorio" : "Opcional");
        req.AddToClassList("og-req");
        req.AddToClassList(group.Obligatorio ? "required" : "optional");
        head.Add(title);
        head.Add(req);
        container.Add(head);

        for (int oi = 0; oi < group.Opciones.Count; oi++)
        {
            var (name, price) = group.Opciones[oi];
            var row = new VisualElement();
            row.AddToClassList("og-option");

            var icon = new VisualElement();
            icon.AddToClassList("og-icon");
            if (group.Tipo == "checkbox") icon.AddToClassList("checkbox");
            row.Add(icon);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("og-option-name");
            row.Add(nameLabel);

            if (price.HasValue)
            {
                var priceLabel = new Label($"+{FormatPrice(price.Value)}");
                priceLabel.AddToClassList("og-option-price");
                row.Add(priceLabel);
            }

            int capturedGi = groupIndex, capturedOi = oi;
            row.RegisterCallback<ClickEvent>(_ => ToggleOption(capturedGi, capturedOi));

            container.Add(row);
        }

        return container;
    }

    void ToggleOption(int gi, int oi)
    {
        var group = currentOptionGroups[gi];
        var selected = currentSelections[gi];

        if (group.Tipo == "radio")
        {
            selected.Clear();
            selected.Add(oi);
        }
        else
        {
            if (!selected.Add(oi)) selected.Remove(oi);
        }

        var groupContainer = sheetOptions.ElementAt(gi);
        var rows = groupContainer.Query<VisualElement>(className: "og-option").ToList();
        for (int i = 0; i < rows.Count; i++)
        {
            var icon = rows[i].Q<VisualElement>(className: "og-icon");
            if (icon == null) continue;
            bool isChecked = selected.Contains(i);
            icon.EnableInClassList("checked", isChecked);
            if (accentColor.HasValue)
                icon.style.backgroundColor = isChecked ? new StyleColor(accentColor.Value) : StyleKeyword.Null;
        }

        UpdateAddBtnState();
    }

    void UpdateAddBtnState()
    {
        if (currentDish == null) return;

        TryParsePrice(currentDish["price"]?.ToString(), out float basePrice);
        float extra = 0f;
        for (int gi = 0; gi < currentOptionGroups.Count; gi++)
            foreach (var oi in currentSelections[gi])
                extra += currentOptionGroups[gi].Opciones[oi].price ?? 0f;

        float total = (basePrice + extra) * currentQty;
        addBtnPriceLabel.text = FormatPrice(total);

        bool missingRequired = false;
        for (int gi = 0; gi < currentOptionGroups.Count; gi++)
            if (currentOptionGroups[gi].Obligatorio && currentSelections[gi].Count == 0) missingRequired = true;

        addBtn.SetEnabled(!missingRequired);
    }

    void ConfirmAddToCart()
    {
        if (currentDish == null) return;

        TryParsePrice(currentDish["price"]?.ToString(), out float basePrice);
        float extra = 0f;
        var optionNames = new List<string>();
        for (int gi = 0; gi < currentOptionGroups.Count; gi++)
            foreach (var oi in currentSelections[gi])
            {
                extra += currentOptionGroups[gi].Opciones[oi].price ?? 0f;
                optionNames.Add(currentOptionGroups[gi].Opciones[oi].name);
            }

        cart.Add(new CartItem
        {
            Name = currentDish["name"]?.ToString() ?? "",
            Options = string.Join(", ", optionNames),
            Quantity = currentQty,
            UnitPrice = basePrice + extra,
            Toggle = 1,
            Orden = 0
        });

        CloseSheet();
        UpdateNavBadge();
    }

    void UpdateNavBadge()
    {
        if (navBadgeLabel == null) return;
        int count = cart.Count;
        navBadgeLabel.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        navBadgeLabel.text = count.ToString();
    }

    void UpdateCourseToggleVisual()
    {
        courseToggleEl.EnableInClassList("on", courseOrderEnabled);
        courseToggleKnobEl.EnableInClassList("on", courseOrderEnabled);

        var onColor = tituloBgColor.HasValue ? new StyleColor(tituloBgColor.Value) : StyleKeyword.Null;
        courseToggleEl.style.backgroundColor = courseOrderEnabled ? onColor : StyleKeyword.Null;
    }

    void RenderPedidoView()
    {
        pedidoListEl.Clear();
        pedidoConfirmedListEl.Clear();

        var previa = latestMesaState?["previa"] as JArray;
        var confirmed = latestMesaState?["confirmed"] as JArray;

        if (previa != null && previa.Count > 0)
        {
            var names = previa.Select(d => d["name"]?.ToString() ?? "").Where(n => !string.IsNullOrEmpty(n));
            pedidoOthersLabel.text = $"En tu mesa se está añadiendo: {string.Join(", ", names)}";
            pedidoOthersLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            pedidoOthersLabel.style.display = DisplayStyle.None;
        }

        bool hasItems = cart.Count > 0;
        pedidoEmptyEl.style.display = hasItems ? DisplayStyle.None : DisplayStyle.Flex;
        pedidoSummaryEl.style.display = hasItems ? DisplayStyle.Flex : DisplayStyle.None;

        if (hasItems)
        {
            for (int i = 0; i < cart.Count; i++)
                pedidoListEl.Add(BuildCartItemElement(i));

            float total = cart.Sum(c => c.Total);
            pedidoTotalValueLabel.text = FormatPrice(total);
        }

        if (confirmed != null && confirmed.Count > 0)
        {
            pedidoConfirmedEl.style.display = DisplayStyle.Flex;
            foreach (var d in confirmed.Cast<JObject>())
                pedidoConfirmedListEl.Add(BuildConfirmedItemElement(d));
        }
        else
        {
            pedidoConfirmedEl.style.display = DisplayStyle.None;
        }
    }

    void OnAsistenciaNavClicked()
    {
        if (asistenciaActive)
        {
            ShowToast("Ya hemos avisado, un camarero vendrá pronto 🛎️");
            return;
        }
        OpenAsistenciaSheet();
    }

    void OpenAsistenciaSheet()
    {
        asistenciaBackdrop.AddToClassList("open");
        asistenciaSheet.AddToClassList("open");
    }

    void CloseAsistenciaSheet()
    {
        asistenciaBackdrop.RemoveFromClassList("open");
        asistenciaSheet.RemoveFromClassList("open");
    }

    IEnumerator SendAsistenciaRoutine()
    {
        CloseAsistenciaSheet();

        if (string.IsNullOrEmpty(sessionToken))
        {
            ShowToast("No se pudo verificar la mesa.");
            yield break;
        }

        using var req = new UnityWebRequest($"{apiBase}/orders/web/asistencia", "POST");
        req.uploadHandler = new UploadHandlerRaw(new byte[0]);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {sessionToken}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"asistencia POST OK — status {req.responseCode}, body: {req.downloadHandler.text}");
            asistenciaSentAt = Time.time;
            UpdateAsistenciaIndicator(true);
            ShowToast("Camarero avisado 🛎️");
        }
        else
        {
            Debug.LogWarning($"asistencia POST falló — status {req.responseCode}, error: {req.error}, body: {req.downloadHandler.text}");
            ShowToast("No se pudo avisar, inténtalo de nuevo.");
        }
    }

    void UpdateAsistenciaIndicator(bool active)
    {
        asistenciaActive = active;
        if (navDotEl != null)
            navDotEl.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void ShowToast(string message)
    {
        if (toastLabel == null) return;

        toastLabel.text = message;
        toastLabel.style.display = DisplayStyle.Flex;

        toastHideHandle?.Pause();
        toastHideHandle = toastLabel.schedule.Execute(() =>
        {
            toastLabel.style.display = DisplayStyle.None;
        });
        toastHideHandle.ExecuteLater(2600);
    }

    VisualElement BuildConfirmedItemElement(JObject d)
    {
        var row = new VisualElement();
        row.AddToClassList("pedido-confirmed-item");

        var info = new VisualElement();
        var nameLabel = new Label($"{d["quantity"]}x {d["name"]}");
        nameLabel.AddToClassList("pedido-item-name");
        info.Add(nameLabel);

        string options = d["options"]?.ToString();
        if (!string.IsNullOrEmpty(options))
        {
            var optsLabel = new Label(options);
            optsLabel.AddToClassList("pedido-item-opts");
            info.Add(optsLabel);
        }

        int state = d["state"]?.ToObject<int>() ?? 1;
        var statusLabel = new Label(OrderStateLabel(state));
        statusLabel.AddToClassList("pedido-confirmed-status");

        row.Add(info);
        row.Add(statusLabel);
        return row;
    }

    VisualElement BuildCartItemElement(int index)
    {
        var item = cart[index];

        var row = new VisualElement();
        row.AddToClassList("pedido-item");

        var info = new VisualElement();
        info.AddToClassList("pedido-item-info");

        var nameLabel = new Label($"{item.Quantity}x {item.Name}");
        nameLabel.AddToClassList("pedido-item-name");
        info.Add(nameLabel);

        if (!string.IsNullOrEmpty(item.Options))
        {
            var optsLabel = new Label(item.Options);
            optsLabel.AddToClassList("pedido-item-opts");
            info.Add(optsLabel);
        }

        if (courseOrderEnabled)
            info.Add(BuildCourseButtonsElement(index, item.Orden));

        var controls = new VisualElement();
        controls.AddToClassList("pedido-item-controls");

        var qtyControl = new VisualElement();
        qtyControl.AddToClassList("qty-control");
        qtyControl.AddToClassList("small");

        var minusBtn = new Button(() => ChangeCartQty(index, -1)) { text = "−" };
        minusBtn.AddToClassList("qty-btn");
        var qtyLabel = new Label(item.Quantity.ToString());
        qtyLabel.AddToClassList("qty-value");
        var plusBtn = new Button(() => ChangeCartQty(index, 1)) { text = "+" };
        plusBtn.AddToClassList("qty-btn");

        qtyControl.Add(minusBtn);
        qtyControl.Add(qtyLabel);
        qtyControl.Add(plusBtn);

        var removeBtn = new Button(() => RemoveCartItem(index));
        removeBtn.AddToClassList("pedido-item-remove");
        var removeIcon = new VisualElement();
        removeIcon.AddToClassList("pedido-item-remove-icon");
        removeBtn.Add(removeIcon);

        controls.Add(qtyControl);
        controls.Add(removeBtn);
        info.Add(controls);

        var priceLabel = new Label(FormatPrice(item.Total));
        priceLabel.AddToClassList("pedido-item-price");

        row.Add(info);
        row.Add(priceLabel);

        return row;
    }

    VisualElement BuildCourseButtonsElement(int cartIndex, int currentOrden)
    {
        var row = new VisualElement();
        row.AddToClassList("course-btns");

        for (int n = 1; n <= 3; n++)
        {
            int capturedN = n;
            var btn = new Button(() =>
            {
                var it = cart[cartIndex];
                it.Orden = it.Orden == capturedN ? 0 : capturedN;
                RenderPedidoView();
            })
            { text = $"{n}º" };
            btn.AddToClassList("course-btn");

            bool isActive = currentOrden == n;
            btn.EnableInClassList("active", isActive);
            if (isActive && accentColor.HasValue)
            {
                var c = new StyleColor(accentColor.Value);
                btn.style.backgroundColor = c;
                btn.style.borderTopColor = c;
                btn.style.borderBottomColor = c;
                btn.style.borderLeftColor = c;
                btn.style.borderRightColor = c;
                btn.style.color = new StyleColor(Color.white);
            }

            row.Add(btn);
        }

        return row;
    }

    void ChangeCartQty(int index, int delta)
    {
        if (index < 0 || index >= cart.Count) return;

        int newQty = cart[index].Quantity + delta;
        if (newQty < 1)
        {
            cart.RemoveAt(index);
        }
        else
        {
            cart[index].Quantity = newQty;
        }

        UpdateNavBadge();
        RenderPedidoView();
    }

    void RemoveCartItem(int index)
    {
        if (index < 0 || index >= cart.Count) return;
        cart.RemoveAt(index);
        UpdateNavBadge();
        RenderPedidoView();
    }

    IEnumerator SubmitOrderRoutine()
    {
        if (isSubmittingOrder) yield break;

        if (string.IsNullOrEmpty(sessionToken))
        {
            Debug.LogWarning("No hay sessionToken; no se pudo verificar la mesa.");
            yield break;
        }

        isSubmittingOrder = true;
        pedidoSubmitBtn.SetEnabled(false);

        var dishesArray = new JArray();
        foreach (var item in cart)
        {
            dishesArray.Add(new JObject
            {
                ["name"] = item.Name,
                ["options"] = item.Options,
                ["quantity"] = item.Quantity.ToString(),
                ["unitPrice"] = item.UnitPrice,
                ["price"] = FormatPrice(item.Total),
                ["toggle"] = item.Toggle,
                ["orden"] = item.Orden
            });
        }
        var body = new JObject { ["dishes"] = dishesArray };
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(body.ToString(Newtonsoft.Json.Formatting.None));

        using var req = new UnityWebRequest($"{apiBase}/orders/web/add", "POST");
        req.uploadHandler = new UploadHandlerRaw(payload);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {sessionToken}");

        yield return req.SendWebRequest();

        isSubmittingOrder = false;
        pedidoSubmitBtn.SetEnabled(true);

        if (req.result == UnityWebRequest.Result.Success)
        {
            cart.Clear();
            UpdateNavBadge();
            yield return StartCoroutine(FetchMesaStateRoutine());
            RenderPedidoView();
            Debug.Log("Pedido enviado a cocina.");
        }
        else
        {
            Debug.LogWarning($"No se pudo enviar el pedido: {req.error}");
        }
    }

    // ============================================================
    // Barra inferior
    // ============================================================

    void SwitchToMenuView()
    {
        appRoot.style.display = DisplayStyle.Flex;
        pedidoViewEl.style.display = DisplayStyle.None;
        SetNavActive(navMenuBtn);
    }

    void SwitchToPedidoView()
    {
        appRoot.style.display = DisplayStyle.None;
        pedidoViewEl.style.display = DisplayStyle.Flex;
        SetNavActive(navPedidoBtn);
        RenderPedidoView();
    }

    void SetNavActive(Button active)
    {
        foreach (var b in new[] { navMenuBtn, navPedidoBtn, navAsistenciaBtn, navPagarBtn })
        {
            if (b == null) continue;

            bool isActive = b == active;
            b.EnableInClassList("active", isActive);

            var label = b.Q<Label>(className: "nav-item-label");
            var icon = b.Q<VisualElement>(className: "nav-icon");
            var targetColor = isActive ? navIconActiveColor : navIconBaseColor;

            if (targetColor.HasValue)
            {
                var c = new StyleColor(targetColor.Value);
                if (label != null) label.style.color = c;
                if (icon != null) icon.style.unityBackgroundImageTintColor = c;
            }
        }
    }

    // ============================================================
    // Bottom sheet: filtros
    // ============================================================

    void OpenFilterSheet()
    {
        filterBackdrop.AddToClassList("open");
        filterSheet.AddToClassList("open");
    }

    void CloseFilterSheet()
    {
        filterBackdrop.RemoveFromClassList("open");
        filterSheet.RemoveFromClassList("open");
    }

    void ToggleVeg()
    {
        vegOnly = !vegOnly;
        RefreshFilterUI();
        RenderSections();
        UpdateFilterChipLabel();
    }

    void BuildAllergenFilterRows()
    {
        for (int i = 1; i <= AllergenLabels.Length; i++)
        {
            int idx = i; // copia local: necesaria para que el callback capture el valor correcto
            var row = new VisualElement();
            row.AddToClassList("og-option");

            var icon = new VisualElement { name = $"allergen-icon-{idx}" };
            icon.AddToClassList("og-icon");
            icon.AddToClassList("checkbox");
            row.Add(icon);

            var label = new Label(AllergenLabels[idx - 1]);
            label.AddToClassList("og-option-name");
            row.Add(label);

            row.RegisterCallback<ClickEvent>(_ =>
            {
                if (!excludedAllergens.Add(idx)) excludedAllergens.Remove(idx);
                RefreshFilterUI();
                RenderSections();
                UpdateFilterChipLabel();
            });

            fAllergensContainer.Add(row);
        }
    }

    void RefreshFilterUI()
    {
        fVegToggle.Q<VisualElement>(className: "og-icon")?.EnableInClassList("checked", vegOnly);

        for (int i = 1; i <= AllergenLabels.Length; i++)
            fAllergensContainer.Q<VisualElement>($"allergen-icon-{i}")?.EnableInClassList("checked", excludedAllergens.Contains(i));
    }

    void ClearFilters()
    {
        vegOnly = false;
        excludedAllergens.Clear();
        RefreshFilterUI();
        RenderSections();
        UpdateFilterChipLabel();
    }

    void UpdateFilterChipLabel()
    {
        int count = (vegOnly ? 1 : 0) + excludedAllergens.Count;
        filterChipButton.EnableInClassList("active", count > 0);

        if (count > 0 && accentColor.HasValue)
        {
            if (filterIconEl != null) filterIconEl.style.unityBackgroundImageTintColor = accentColor.Value;
            filterChipButton.style.borderTopColor = accentColor.Value;
            filterChipButton.style.borderBottomColor = accentColor.Value;
            filterChipButton.style.borderLeftColor = accentColor.Value;
            filterChipButton.style.borderRightColor = accentColor.Value;
        }
        else
        {
            if (filterIconEl != null) filterIconEl.style.unityBackgroundImageTintColor = StyleKeyword.Null;
            filterChipButton.style.borderTopColor = StyleKeyword.Null;
            filterChipButton.style.borderBottomColor = StyleKeyword.Null;
            filterChipButton.style.borderLeftColor = StyleKeyword.Null;
            filterChipButton.style.borderRightColor = StyleKeyword.Null;
        }

        if (filterCountBadge != null)
        {
            filterCountBadge.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            filterCountBadge.text = count.ToString();
            filterCountBadge.style.backgroundColor = accentColor.HasValue ? new StyleColor(accentColor.Value) : StyleKeyword.Null;
        }
    }

    // ============================================================
    // Estados de error / vacío
    // ============================================================

    void ShowErrorState(string title, string body)
    {
        sectionsContainer.Clear();
        var state = new VisualElement();
        state.AddToClassList("state");
        var t = new Label(title);
        t.AddToClassList("state-title");
        var b = new Label(body);
        state.Add(t);
        state.Add(b);
        sectionsContainer.Add(state);
        HideSplash();
    }

    // ============================================================
    // Tema (colores del restaurante — equivalente a applyTheme())
    // ============================================================

    void ApplyTheme(JObject p)
    {
        if (p == null) return;

        // UI Toolkit no permite sobreescribir las custom properties USS
        // (--accent, --card-bg, etc.) desde C# en tiempo de ejecución;
        // solo se pueden LEER valores ya definidos en un .uss (vía
        // CustomStyleProperty<T>), no fijarlos dinámicamente. Por eso
        // aquí el color se aplica como estilo inline a los elementos
        // concretos que en la web usaban var(--accent). Si añades más
        // elementos "de acento" al diseño, súmalos aquí.

        if (TryParseColor(p["col_botones"]?.ToString(), out var accent))
            accentColor = accent.value;

        if (TryParseColor(p["col_ppal_botones"]?.ToString(), out var ppalBtn))
        {
            ppalButtonColor = ppalBtn.value;
            addBtn.style.backgroundColor = ppalBtn;
            if (pedidoSubmitBtn != null) pedidoSubmitBtn.style.backgroundColor = ppalBtn;
        }

        if (TryParseColor(p["col_icono_base"]?.ToString(), out var iconBase))
            navIconBaseColor = iconBase.value;

        if (TryParseColor(p["col_icono_pulsado"]?.ToString(), out var iconActive))
            navIconActiveColor = iconActive.value;

        if (TryParseColor(p["col_fondo_icono"]?.ToString(), out var navBg))
            bottomNavEl.style.backgroundColor = navBg;

        SetNavActive(navMenuBtn); // aplica colores iniciales (Menú activo por defecto)
        UpdateCourseToggleVisual();

        if (TryParseColor(p["col_fondo_gral"]?.ToString(), out var cardBg))
        {
            root.Query<VisualElement>(className: "dish-card").ForEach(c => c.style.backgroundColor = cardBg);
        }

        // Recuadro flotante del título (info-card): equivalente a
        // --bg-titulo / --color-titulo en la web (col_fondo_titulo /
        // col_letra_titulo). Antes no se aplicaban — de ahí que el
        // cuadro no cambiara de color.
        if (TryParseColor(p["col_fondo_titulo"]?.ToString(), out var bgTitulo))
        {
            infoCardEl.style.backgroundColor = bgTitulo;
            tituloBgColor = bgTitulo.value;
            if (pedidoHeaderEl != null) pedidoHeaderEl.style.backgroundColor = bgTitulo;
        }

        if (TryParseColor(p["col_letra_titulo"]?.ToString(), out var colorTitulo))
            restNameLabel.style.color = colorTitulo;

        Debug.Log($"fondo_titulo={p["col_fondo_titulo"]} letra_titulo={p["col_letra_titulo"]}");

        // Tipografías por restaurante (letra_titulo / letra_gral): en la
        // web se cargan bajo demanda desde Google Fonts. Unity no tiene
        // equivalente en runtime — las fuentes deben importarse de
        // antemano como Font Assets y asignarse por código con
        // label.style.unityFontDefinition si quieres soportarlo por
        // restaurante. Queda fuera del alcance de este ejemplo.
    }

    static bool TryParseColor(string hex, out StyleColor color)
    {
        color = default;
        if (string.IsNullOrEmpty(hex)) return false;
        if (!ColorUtility.TryParseHtmlString(hex, out var c)) return false;
        color = new StyleColor(c);
        return true;
    }

    // ============================================================
    // Imágenes
    // ============================================================

    IEnumerator LoadImageInto(string url, VisualElement target)
    {
        string directUrl = ToDirectImageUrl(url);
        using var req = UnityWebRequestTexture.GetTexture(directUrl);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var tex = DownloadHandlerTexture.GetContent(req);
            target.style.backgroundImage = new StyleBackground(tex);
        }
        else
        {
            Debug.LogWarning($"No se pudo cargar la imagen: {directUrl}");
        }
    }

    static string ToDirectImageUrl(string url)
    {
        if (string.IsNullOrEmpty(url) || !url.Contains("drive.google.com")) return url;

        var m = Regex.Match(url, @"[?&]id=([a-zA-Z0-9_-]+)");
        if (!m.Success) m = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");

        return m.Success ? $"https://lh3.googleusercontent.com/d/{m.Groups[1].Value}=w800" : url;
    }

    // ============================================================
    // Helpers
    // ============================================================

    static bool IsOff(JToken v)
    {
        if (v == null || v.Type == JTokenType.Null) return true;
        string s = v.ToString().Trim().ToLowerInvariant();
        return s == "" || s == "0" || s == "false";
    }

    static bool IsVeg(JObject d)
    {
        var v = d["veg"];
        if (v == null) return false;
        string s = v.ToString().Trim().ToLowerInvariant();
        return s == "1" || s == "true";
    }

    static float SafeFloat(JToken t, float fallback = 0f)
    {
        if (t == null) return fallback;
        return float.TryParse(t.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float v) ? v : fallback;
    }

    static string FormatPrice(JToken priceToken)
    {
        string raw = priceToken?.ToString();
        return TryParsePrice(raw, out float v) ? FormatPrice(v) : (raw ?? "");
    }

    // Acepta "11.65" y "11,65": si aparece una coma decimal, la
    // normalizamos a punto ANTES de parsear (antes se interpretaba
    // como separador de miles y "11,65" se convertía en 1165).
    static bool TryParsePrice(string raw, out float value)
    {
        value = 0f;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        raw = raw.Trim().Replace(",", ".");
        return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    static string FormatPrice(float v) => v.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',') + " €";

    static string Slug(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string norm = Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return norm;
    }

    void HideSplash()
    {
        brandSplash.AddToClassList("splash-hidden");
        appRoot.AddToClassList("app-visible");
        // La duración debe coincidir con transition-duration de
        // .brand-splash en el USS (0.6s) para no cortar el fade.
        brandSplash.schedule.Execute(() => brandSplash.RemoveFromHierarchy()).ExecuteLater(650);
    }
}
