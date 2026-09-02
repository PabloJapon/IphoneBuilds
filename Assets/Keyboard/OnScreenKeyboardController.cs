using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class OnScreenKeyboardController : MonoBehaviour
{
    [Header("Referencias de escena")]
    public GameObject keyboardPanel;      // el panel que se muestra/oculta
    public Transform rowsContainer;       // objeto vacío con Vertical Layout Group
    public GameObject keyButtonPrefab;    // prefab: Button + TMP_Text hijo, con KeyButton.cs
    public float rowHeight = 50f;

    [Header("Fila de teclas especiales")]
    public KeyButton spaceKey;
    public KeyButton backspaceKey;
    public KeyButton shiftKey;
    public KeyButton enterKey;
    public KeyButton layoutToggleKey; // el botón "123" / "ABC"

    [Header("Teclado numérico (fijo, a la derecha)")]
    public KeyButton[] numPadKeys; // asígnalos en este orden: 7 8 9 4 5 6 1 2 3 0
    private readonly string[] numPadDigits = { "7","8","9","4","5","6","1","2","3",",","0","." };

    [Header("Layouts disponibles (idiomas)")]
    public KeyboardLayoutData[] layouts;
    private int currentLayoutIndex = 0;

    private TMP_InputField activeInputField;
    private TextField activeUIToolkitField;

    private int lastShownFrame = -1;
    private bool isBlocked = false;
    public bool WasShownThisFrame() => lastShownFrame == Time.frameCount;
    public bool ShiftActive { get; private set; } = false;

    private readonly List<KeyButton> allKeyButtons = new List<KeyButton>();

    void Start()
    {
        if (layouts == null || layouts.Length == 0)
        {
            Debug.LogError("[OnScreenKeyboardController] El array 'Layouts' está vacío. " +
                "Asigna al menos un asset KeyboardLayoutData en el Inspector.", this);
            return;
        }

        SetupSpecialKeys();
        SetupNumPad(); 
        RegisterAllInputFieldsInScene(); 
        BuildLayout(layouts[currentLayoutIndex]);
        keyboardPanel.SetActive(false);
    }

    void Update()
    {
        if (!keyboardPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Hide();
        }
    }

    // Llamar esto una vez por cada TMP_InputField que deba abrir el teclado
    public void RegisterInputField(TMP_InputField field)
    {
        field.onSelect.AddListener(_ => Show(field));
    }

    // Para que salga el teclado en cada inputfield
    public void RegisterAllInputFieldsInScene()
    {
        TMP_InputField[] todos = FindObjectsOfType<TMP_InputField>(true); // true = incluye los inactivos
        foreach (var field in todos)
        {
            RegisterInputField(field);
        }
        
    }

    public void Show(TMP_InputField field)
    {
        activeUIToolkitField = null;
        activeInputField = field;
        keyboardPanel.SetActive(true);
        lastShownFrame = Time.frameCount;
    }

    public void RegisterUIToolkitField(TextField field)
    {
        field.RegisterCallback<FocusInEvent>(evt => ShowUIToolkit(field));
    }

    public void ShowUIToolkit(TextField field)
    {
        activeInputField = null;
        activeUIToolkitField = field;
        keyboardPanel.SetActive(true);
        lastShownFrame = Time.frameCount;
    }

    public void Hide()
    {
        keyboardPanel.SetActive(false);
        activeInputField = null;
        activeUIToolkitField = null;
        EventSystem.current.SetSelectedGameObject(null);   // <-- nueva línea
    }

    void LateUpdate()
    {
        if (!keyboardPanel.activeSelf || isBlocked) return;
        if (lastShownFrame == Time.frameCount) return;

        bool pointerDown = Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        if (!pointerDown) return;

        Vector2 pointerPos = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        RectTransform keyboardRect = keyboardPanel.GetComponent<RectTransform>();
        bool tocoElTeclado = RectTransformUtility.RectangleContainsScreenPoint(keyboardRect, pointerPos, null);

        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        bool esInputField = selected != null && selected.GetComponent<TMP_InputField>() != null;

        if (!esInputField && !tocoElTeclado)
            Hide();
    }

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;

        if (blocked)
        {
            keyboardPanel.SetActive(false);
        }
        else if (activeInputField != null || activeUIToolkitField != null)
        {
            // Si había un campo esperando foco, mostramos el teclado ahora que ya no está bloqueado
            keyboardPanel.SetActive(true);
        }
    }

    public void InsertText(string text)
    {
        if (activeInputField != null)
        {
            int start = Mathf.Min(activeInputField.stringPosition, activeInputField.selectionStringAnchorPosition);
            int end = Mathf.Max(activeInputField.stringPosition, activeInputField.selectionStringAnchorPosition);

            activeInputField.text = activeInputField.text.Remove(start, end - start).Insert(start, text);
            activeInputField.stringPosition = start + text.Length;
            activeInputField.selectionStringAnchorPosition = activeInputField.stringPosition;
        }
        else if (activeUIToolkitField != null)
        {
            int start = Mathf.Min(activeUIToolkitField.cursorIndex, activeUIToolkitField.selectIndex);
            int end = Mathf.Max(activeUIToolkitField.cursorIndex, activeUIToolkitField.selectIndex);

            activeUIToolkitField.value = activeUIToolkitField.value.Remove(start, end - start).Insert(start, text);
            activeUIToolkitField.cursorIndex = start + text.Length;
            activeUIToolkitField.selectIndex = activeUIToolkitField.cursorIndex;
        }
        else return;

        if (ShiftActive) ToggleShift();
    }

    public void Backspace()
    {
        if (activeInputField != null)
        {
            int start = Mathf.Min(activeInputField.stringPosition, activeInputField.selectionStringAnchorPosition);
            int end = Mathf.Max(activeInputField.stringPosition, activeInputField.selectionStringAnchorPosition);

            if (start != end)
            {
                activeInputField.text = activeInputField.text.Remove(start, end - start);
                activeInputField.stringPosition = start;
            }
            else
            {
                if (start == 0) return;
                activeInputField.text = activeInputField.text.Remove(start - 1, 1);
                activeInputField.stringPosition = start - 1;
            }
            activeInputField.selectionStringAnchorPosition = activeInputField.stringPosition;
        }
        else if (activeUIToolkitField != null)
        {
            int start = Mathf.Min(activeUIToolkitField.cursorIndex, activeUIToolkitField.selectIndex);
            int end = Mathf.Max(activeUIToolkitField.cursorIndex, activeUIToolkitField.selectIndex);

            if (start != end)
            {
                activeUIToolkitField.value = activeUIToolkitField.value.Remove(start, end - start);
                activeUIToolkitField.cursorIndex = start;
            }
            else
            {
                if (start == 0) return;
                activeUIToolkitField.value = activeUIToolkitField.value.Remove(start - 1, 1);
                activeUIToolkitField.cursorIndex = start - 1;
            }
            activeUIToolkitField.selectIndex = activeUIToolkitField.cursorIndex;
        }
    }

    public void Enter()
    {
        Hide();
    }

    public void ToggleShift()
    {
        ShiftActive = !ShiftActive;
        foreach (var key in allKeyButtons)
            key.UpdateShiftState(ShiftActive);
    }

    // Llamar desde un botón "cambiar idioma", pasando el índice del layout en el array
    public void SwitchLayout(int index)
    {
        if (index < 0 || index >= layouts.Length) return;
        currentLayoutIndex = index;
        BuildLayout(layouts[currentLayoutIndex]);

        if (layoutToggleKey != null)
        {
            if (currentLayoutIndex == 0) layoutToggleKey.SetSwitchTarget(1, "#+=");
            else layoutToggleKey.SetSwitchTarget(0, "ABC");
        }
    }

    private void BuildLayout(KeyboardLayoutData layoutData)
    {
        // Sacamos las teclas especiales de donde estuvieran antes de borrar filas,
        // para no destruirlas por error junto con las filas viejas
        if (shiftKey != null) shiftKey.transform.SetParent(transform, false);
        if (backspaceKey != null) backspaceKey.transform.SetParent(transform, false);

        foreach (Transform child in rowsContainer)
            Destroy(child.gameObject);
        allKeyButtons.Clear();

        List<Transform> rowTransforms = new List<Transform>();

        foreach (var row in layoutData.rows)
        {
            if (row.normalKeys.Length != row.shiftKeys.Length)
            {
                Debug.LogError($"[OnScreenKeyboardController] En el layout '{layoutData.layoutName}', " +
                    $"una fila tiene {row.normalKeys.Length} normalKeys pero {row.shiftKeys.Length} shiftKeys. " +
                    "Deben tener el mismo tamaño.", this);
                continue;
            }

            GameObject rowObj = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObj.transform.SetParent(rowsContainer, false);

            HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, rowHeight);

            rowTransforms.Add(rowObj.transform);

            for (int i = 0; i < row.normalKeys.Length; i++)
            {
                GameObject keyObj = Instantiate(keyButtonPrefab, rowObj.transform);
                KeyButton kb = keyObj.GetComponent<KeyButton>();
                kb.Init(row.normalKeys[i], row.shiftKeys[i], this);
                allKeyButtons.Add(kb);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(keyboardPanel.GetComponent<RectTransform>());
        }

        // Colocamos shift y borrar en la última fila (z x c... ), uno delante y otro detrás
        int lastIndex = rowTransforms.Count - 1;
        if (lastIndex >= 0)
        {
            if (shiftKey != null)
            {
                shiftKey.transform.SetParent(rowTransforms[lastIndex], false);
                shiftKey.transform.SetAsFirstSibling();
                allKeyButtons.Add(shiftKey);

                RectTransform rt = shiftKey.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(107f, 70f);
            }
            if (backspaceKey != null)
            {
                backspaceKey.transform.SetParent(rowTransforms[lastIndex], false);
                backspaceKey.transform.SetAsLastSibling();
                allKeyButtons.Add(backspaceKey);

                RectTransform rt = backspaceKey.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(107f, 70f);
            }
        }

        // Colocamos intro y espacio en la lista para que respondan al shift si algún día hiciera falta
        // (no se mueven de sitio, se quedan donde los coloques tú en su propia fila fija)
        if (enterKey != null) allKeyButtons.Add(enterKey);
        if (spaceKey != null) allKeyButtons.Add(spaceKey);
    }

    private void SetupSpecialKeys()
    {
        // Estas cuatro teclas se inicializan una sola vez. shift y borrar se reubican
        // dentro de BuildLayout; espacio e intro se quedan fijas donde las coloques en el editor.
        if (spaceKey != null) spaceKey.Init(" ", " ", this, KeyType.Space);
        if (backspaceKey != null) backspaceKey.Init("", "", this, KeyType.Backspace);
        if (shiftKey != null) shiftKey.Init("", "", this, KeyType.Shift);
        if (enterKey != null) enterKey.Init("", "", this, KeyType.Enter);
        if (layoutToggleKey != null)
        {
            layoutToggleKey.Init("#+=", "#+=", this, KeyType.SwitchLayout);
            layoutToggleKey.SetSwitchTarget(1, "#+=");
        }
    }

    private void SetupNumPad()
    {
        for (int i = 0; i < numPadKeys.Length && i < numPadDigits.Length; i++)
        {
            if (numPadKeys[i] != null)
                numPadKeys[i].Init(numPadDigits[i], numPadDigits[i], this, KeyType.Character);
        }
    }

}