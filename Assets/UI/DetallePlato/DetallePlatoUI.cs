using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

// NOTA: la clase OptionGroupData (titulo, tipo, obligatorio, opciones) ya la tienes
// definida en tu script antiguo DetallePlato.cs. No la dupliques aqui: si borras
// ese script por completo, mueve esa clase a un archivo aparte (p.ej. OptionGroupData.cs).

[RequireComponent(typeof(UIDocument))]
public class DetallePlatoUI : MonoBehaviour
{
    public static DetallePlatoUI Instance { get; private set; }

    private UIDocument uiDocument;

    // Referencias a elementos del UXML
    private VisualElement overlay;
    private VisualElement card;
    private VisualElement dishImageEl;
    private Label dishNameLabel;
    private Label dishPriceLabel;
    private Label dishDescriptionLabel;
    private VisualElement allergensRow;
    private VisualElement vegRow;
    private VisualElement optionsContainer;
    private Label quantityLabel;
    private Button btnMinus;
    private Button btnPlus;
    private Button btnAdd;
    private Button btnClose;
    private ScrollView contentScroll;

    private const float DragThreshold = 8f; // px que hay que mover antes de considerarlo "arrastre" y no un clic

    private bool isDraggingScroll;
    private bool isPointerDownPending;
    private Vector2 pointerDownPos;
    private int activePointerId = -1;

    public static int xPlato;
    public static float yPlato;
    public int currentQuantity = 1;

    private Color brandColor = new Color(0.8f, 0.35f, 0.43f);          // fallback: el rosa que tenías fijo
    private Color brandColorDisabled = new Color(0.86f, 0.75f, 0.76f); // fallback del boton deshabilitado

    // Nombres de alergenos EN EL MISMO ORDEN que DataBase.alergs1 .. alergs14.
    // Ajusta este orden al que uses realmente en tu base de datos.
    public string[] allergenNames = new string[]
    {
        "Gluten", "Crustáceos", "Huevos", "Pescado", "Cacahuetes", "Soja",
        "Lácteos", "Frutos secos", "Apio", "Mostaza", "Sésamo", "Sulfitos",
        "Moluscos", "Altramuces"
    };

    private readonly List<VisualElement> optionGroupElements = new List<VisualElement>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;

        overlay = root.Q<VisualElement>("overlay");
        card = root.Q<VisualElement>("card");
        dishImageEl = root.Q<VisualElement>("dish-image");
        dishNameLabel = root.Q<Label>("dish-name");
        dishPriceLabel = root.Q<Label>("dish-price");
        dishDescriptionLabel = root.Q<Label>("dish-description");
        allergensRow = root.Q<VisualElement>("allergens-row");
        vegRow = root.Q<VisualElement>("veg-row");
        optionsContainer = root.Q<VisualElement>("options-container");
        quantityLabel = root.Q<Label>("quantity-label");
        btnMinus = root.Q<Button>("btn-minus");
        btnPlus = root.Q<Button>("btn-plus");
        btnAdd = root.Q<Button>("btn-add");
        btnClose = root.Q<Button>("btn-close");
        contentScroll = root.Q<ScrollView>("content-scroll");

        btnMinus.clicked += OnMinusClicked;
        btnPlus.clicked += OnPlusClicked;
        btnAdd.clicked += OnAddClicked;
        btnClose.clicked += clickClose;
        overlay.RegisterCallback<PointerDownEvent>(OnOverlayPointerDown);
        contentScroll.contentContainer.RegisterCallback<PointerDownEvent>(OnScrollPointerDown, TrickleDown.TrickleDown);
        contentScroll.contentContainer.RegisterCallback<PointerMoveEvent>(OnScrollPointerMove);
        contentScroll.contentContainer.RegisterCallback<PointerUpEvent>(OnScrollPointerUp, TrickleDown.TrickleDown);
        contentScroll.contentContainer.RegisterCallback<PointerCaptureOutEvent>(OnScrollPointerCaptureOut);

        overlay.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        btnMinus.clicked -= OnMinusClicked;
        btnPlus.clicked -= OnPlusClicked;
        btnAdd.clicked -= OnAddClicked;
        btnClose.clicked -= clickClose;
        overlay.UnregisterCallback<PointerDownEvent>(OnOverlayPointerDown);
        contentScroll.contentContainer.UnregisterCallback<PointerDownEvent>(OnScrollPointerDown, TrickleDown.TrickleDown);
        contentScroll.contentContainer.UnregisterCallback<PointerMoveEvent>(OnScrollPointerMove);
        contentScroll.contentContainer.UnregisterCallback<PointerUpEvent>(OnScrollPointerUp, TrickleDown.TrickleDown);
        contentScroll.contentContainer.UnregisterCallback<PointerCaptureOutEvent>(OnScrollPointerCaptureOut);
    }

    // Cierra solo si se pulsa el fondo oscurecido, no la tarjeta ni su contenido
    private void OnOverlayPointerDown(PointerDownEvent evt)
    {
        if (evt.target == overlay)
            clickClose();
    }

    private void OnScrollPointerDown(PointerDownEvent evt)
    {
        isPointerDownPending = true;
        isDraggingScroll = false;
        pointerDownPos = evt.position;
        activePointerId = evt.pointerId;
    }

    private void OnScrollPointerMove(PointerMoveEvent evt)
    {
        if (evt.pointerId != activePointerId) return;
        if (!isPointerDownPending && !isDraggingScroll) return;

        if (!isDraggingScroll)
        {
            float distance = Vector2.Distance(evt.position, pointerDownPos);
            if (distance < DragThreshold) return; // aun no ha movido lo suficiente: sigue siendo un posible clic

            isDraggingScroll = true;
            isPointerDownPending = false;
            contentScroll.contentContainer.CapturePointer(activePointerId);
        }

        contentScroll.scrollOffset -= (Vector2)evt.deltaPosition;
    }

    private void OnScrollPointerUp(PointerUpEvent evt)
    {
        isPointerDownPending = false;
        if (isDraggingScroll)
        {
            isDraggingScroll = false;
            if (contentScroll.contentContainer.HasPointerCapture(evt.pointerId))
                contentScroll.contentContainer.ReleasePointer(evt.pointerId);
        }
    }

    private void OnScrollPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        isDraggingScroll = false;
        isPointerDownPending = false;
    }

    public void click()
    {
        overlay.style.display = DisplayStyle.Flex;
    }

    public void clickClose()
    {
        currentQuantity = 1;
        overlay.style.display = DisplayStyle.None;
    }

    private void ApplyBrandColor()
    {
        Color parsed;
        if (DataBasePersonalizacion.col_ppal_empl != null && DataBasePersonalizacion.col_ppal_empl.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out parsed))
        {
            brandColor = parsed;
            brandColorDisabled = Color.Lerp(parsed, Color.white, 0.55f);
        }

        dishPriceLabel.style.color = brandColor;
        UpdateAddButtonColor();
    }

    private void UpdateAddButtonColor()
    {
        btnAdd.style.backgroundColor = btnAdd.enabledSelf ? brandColor : brandColorDisabled;
    }

    public void seleccionPlato(int numeroPlato)
    {
        ClearOptionGroups();
        allergensRow.Clear();
        vegRow.Clear();
        currentQuantity = 1;
        quantityLabel.text = "1";
        btnAdd.SetEnabled(false);
        ApplyBrandColor();

        string[] nombres = DataBase.nombrePlatos;
        dishNameLabel.text = nombres[numeroPlato - 1];

        string[] descripcion = DataBase.descripcionPlatos;
        dishDescriptionLabel.text = descripcion[numeroPlato - 1];

        Sprite[] sprites = DataBase.spritePlatos;
        Sprite sprite = sprites[numeroPlato - 1];

        if (sprite == null)
        {
            card.AddToClassList("no-image");
        }
        else
        {
            card.RemoveFromClassList("no-image");
            dishImageEl.style.backgroundImage = new StyleBackground(sprite);
        }

        xPlato = numeroPlato;

        int[][] allAlergs = new int[][]
        {
            DataBase.alergs1, DataBase.alergs2, DataBase.alergs3, DataBase.alergs4,
            DataBase.alergs5, DataBase.alergs6, DataBase.alergs7, DataBase.alergs8,
            DataBase.alergs9, DataBase.alergs10, DataBase.alergs11, DataBase.alergs12,
            DataBase.alergs13, DataBase.alergs14
        };

        for (int i = 0; i < allAlergs.Length; i++)
        {
            if (allAlergs[i][numeroPlato - 1] == 1 && i < allergenNames.Length)
                allergensRow.Add(CreateChip(allergenNames[i]));
        }

        if (DataBase.vegs[numeroPlato - 1] == 1)
            vegRow.Add(CreateChip("Vegetariano"));
        else if (DataBase.vegs[numeroPlato - 1] == 2)
            vegRow.Add(CreateChip("Vegano"));

        string groups = DataBase.optionGroups[numeroPlato - 1];

        if (!string.IsNullOrWhiteSpace(groups))
        {
            var jsonGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OptionGroupData>>(groups);
            foreach (var group in jsonGroups)
                CreateOptionGroup(group.titulo, group.opciones, group.tipo, group.obligatorio);
        }

        ValidateAllGroupsSelected();

        MenuPedir menuPedir = FindObjectOfType<MenuPedir>();
        if (menuPedir != null)
            menuPedir.platoCount[numeroPlato] = 1;

        precioPlato();
        click();
    }

    private VisualElement CreateChip(string text)
    {
        Label chip = new Label(text);
        chip.AddToClassList("chip");
        return chip;
    }
    public void CreateOptionGroup(string headerText, List<string> options, string tipo = "radio", bool obligatorio = true)
    {
        VisualElement group = new VisualElement();
        group.AddToClassList("option-group");
        group.userData = new OptionGroupUIMeta { obligatorio = obligatorio, tipo = tipo };

        VisualElement headerRow = new VisualElement();
        headerRow.AddToClassList("option-group-header");

        Label title = new Label(headerText);
        title.AddToClassList("option-group-title");

        Label badge = new Label(obligatorio ? "Obligatorio" : "Opcional");
        badge.AddToClassList("badge");
        badge.AddToClassList(obligatorio ? "badge-obligatorio" : "badge-opcional");
        if (obligatorio)
        {
            badge.style.color = brandColor;
            badge.style.backgroundColor = Color.Lerp(brandColor, Color.white, 0.85f);
        }

        headerRow.Add(title);
        headerRow.Add(badge);
        group.Add(headerRow);

        bool isCheckbox = tipo == "checkbox";
        VisualElement optionsList = isCheckbox ? new VisualElement() : new RadioButtonGroup();
        group.Add(optionsList);

        foreach (string option in options)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("option-row");

            var parsed = ParseOption(option);

            VisualElement control;
            INotifyValueChanged<bool> controlValue;

            if (isCheckbox)
            {
                Toggle toggle = new Toggle();
                toggle.AddToClassList("checkbox-toggle");
                toggle.userData = parsed.name;
                toggle.RegisterValueChangedCallback(evt => ValidateAllGroupsSelected());
                control = toggle;
                controlValue = toggle;
            }
            else
            {
                RadioButton radio = new RadioButton();
                radio.AddToClassList("radio-toggle");
                radio.userData = parsed.name;
                radio.RegisterValueChangedCallback(evt => ValidateAllGroupsSelected());
                control = radio;
                controlValue = radio;
            }

            // El propio dibujo ya no recibe el clic: asi todo el clic pasa
            // siempre por la fila (row), sin duplicar ni competir con el scroll.
            control.pickingMode = PickingMode.Ignore;

            Label nameLabel = new Label(parsed.name);
            nameLabel.AddToClassList("option-name");

            Label extraLabel = new Label(parsed.extra);
            extraLabel.AddToClassList("option-extra");

            row.Add(control);
            row.Add(nameLabel);
            row.Add(extraLabel);

            // --- Clic en toda la fila, con umbral de arrastre propio ---
            bool rowPointerActive = false;
            bool rowMoved = false;
            Vector2 rowDownPos = Vector2.zero;

            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                rowPointerActive = true;
                rowMoved = false;
                rowDownPos = evt.position;
            });

            row.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!rowPointerActive) return;
                if (Vector2.Distance(evt.position, rowDownPos) >= DragThreshold)
                    rowMoved = true;
            });

            row.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (rowPointerActive && !rowMoved)
                    controlValue.value = !controlValue.value; // dispara el ValueChanged de arriba solo

                rowPointerActive = false;
            });

            optionsList.Add(row);
        }

        optionsContainer.Add(group);
        optionGroupElements.Add(group);
    }

    private (string name, string extra) ParseOption(string option)
    {
        Match match = Regex.Match(option, @"^(.+?),\s*(\d+[.,]\d+)$");
        if (match.Success)
        {
            string name = match.Groups[1].Value.Trim();
            string priceStr = match.Groups[2].Value.Replace('.', ',');
            return (name, $"+{priceStr}€");
        }
        return (option, "");
    }

    public void ClearOptionGroups()
    {
        optionsContainer.Clear();
        optionGroupElements.Clear();
    }

    public void ValidateAllGroupsSelected()
    {
        bool hasMandatory = false;
        bool allSatisfied = true;

        foreach (VisualElement group in optionGroupElements)
        {
            if (!(group.userData is OptionGroupUIMeta meta) || !meta.obligatorio) continue;

            hasMandatory = true;
            bool anySelected = false;
            if (meta.tipo == "checkbox")
                group.Query<Toggle>().ForEach(t => { if (t.value) anySelected = true; });
            else
                group.Query<RadioButton>().ForEach(t => { if (t.value) anySelected = true; });

            if (!anySelected)
            {
                allSatisfied = false;
                break;
            }
        }

        btnAdd.SetEnabled(!hasMandatory || allSatisfied);
        UpdateAddButtonColor();
        UpdatePrecioConOpciones();

        if (!hasMandatory || allSatisfied)
        {
            currentQuantity = 1;
            quantityLabel.text = "1";
        }
    }

    public Dictionary<string, string> GetOptionSelections()
    {
        Dictionary<string, string> selections = new Dictionary<string, string>();

        foreach (VisualElement group in optionGroupElements)
        {
            if (!(group.userData is OptionGroupUIMeta meta)) continue;
            Label header = group.Q<Label>(className: "option-group-title");
            if (header == null) continue;

            string groupName = header.text.Trim();

            if (meta.tipo == "checkbox")
            {
                List<string> chosen = new List<string>();
                group.Query<Toggle>().ForEach(t => { if (t.value) chosen.Add((string)t.userData); });
                for (int idx = 0; idx < chosen.Count; idx++)
                    selections[groupName + "_" + idx] = chosen[idx];
            }
            else
            {
                RadioButton selected = null;
                group.Query<RadioButton>().ForEach(t => { if (t.value) selected = t; });
                if (selected != null)
                    selections[groupName] = (string)selected.userData;
            }
        }

        return selections;
    }

    public void precioPlato()
    {
        float[] precios = DataBase.precioPlatos;
        float unitPrice = precios[xPlato - 1];
        float finalPrice = unitPrice * currentQuantity;
        btnAdd.text = "Añadir   " + finalPrice.ToString("0.00") + " €";
        dishPriceLabel.text = unitPrice.ToString("0.00") + " €";
        yPlato = finalPrice;
    }

    public void UpdatePrecioConOpciones()
    {
        float basePrice = DataBase.precioPlatos[xPlato - 1];
        Dictionary<string, string> selectedOptions = GetOptionSelections();

        float extraTotal = 0f;
        foreach (var pair in selectedOptions)
            extraTotal += ExtractOptionExtraPrice(pair.Value);

        float unitPrice = basePrice + extraTotal;
        float finalPrice = unitPrice * currentQuantity;

        btnAdd.text = "Añadir   " + finalPrice.ToString("0.00") + " €";
        dishPriceLabel.text = unitPrice.ToString("0.00") + " €";
        yPlato = finalPrice;
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

    private void OnMinusClicked()
    {
        if (currentQuantity <= 1) return;
        currentQuantity--;
        quantityLabel.text = currentQuantity.ToString();
        UpdatePrecioConOpciones();
    }

    private void OnPlusClicked()
    {
        currentQuantity++;
        quantityLabel.text = currentQuantity.ToString();
        UpdatePrecioConOpciones();
    }

    private void OnAddClicked()
    {
        MenuPedir menuPedir = FindObjectOfType<MenuPedir>();
        if (menuPedir == null)
        {
            Debug.LogWarning("[DetallePlatoUI] No se encontró MenuPedir en la escena.");
            return;
        }

        // SelectPlato() lee la cantidad desde menuPedir.platoCount[xPlato],
        // asi que hay que volcar ahi la cantidad elegida en el popup antes de llamarlo.
        menuPedir.platoCount[xPlato] = currentQuantity;

        menuPedir.SelectPlato();

        clickClose();
    }
}

public class OptionGroupUIMeta
{
    public bool obligatorio = true;
    public string tipo = "radio";
}
