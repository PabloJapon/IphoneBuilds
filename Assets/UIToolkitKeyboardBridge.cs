using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Lets a hidden UGUI TMP_InputField stand in for a UI Toolkit TextField so an
/// existing UGUI-only custom keyboard works with zero changes to it.
///
/// Setup: one Canvas (Screen Space - Overlay, Sort Order above your UIDocument's
/// PanelSettings sort order) containing one TMP_InputField ("GhostInput").
/// Assign both here, then call Bind(field) for every TextField you want the
/// keyboard to work on, instead of keyboard.RegisterUIToolkitField(field).
/// </summary>
public class UIToolkitKeyboardBridge : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_InputField ghostInput;
    [SerializeField] private OnScreenKeyboardController keyboard;

    private TextField activeField;
    private RectTransform ghostRect;

    void Awake()
    {
        ghostRect = ghostInput.GetComponent<RectTransform>();
        ghostRect.anchorMin = ghostRect.anchorMax = new Vector2(0, 1);
        ghostRect.pivot = new Vector2(0, 1);
        ghostInput.gameObject.SetActive(false);
        ghostInput.onValueChanged.AddListener(OnGhostValueChanged);
        keyboard.RegisterInputField(ghostInput);
    }

    private bool keyboardWasActive;

    void Update()
    {
        bool keyboardActiveNow = keyboard.keyboardPanel.activeSelf;
        if (keyboardWasActive && !keyboardActiveNow && activeField != null)
        {
            ghostInput.gameObject.SetActive(false);
            activeField = null;
        }
        keyboardWasActive = keyboardActiveNow;
    }

    // Call once per TextField, replacing keyboard.RegisterUIToolkitField(field)
    public void Bind(TextField field)
    {
        field.RegisterCallback<FocusInEvent>(_ => ShowGhostOver(field));
    }

    void ShowGhostOver(TextField field)
    {
        activeField = field;
        field.Blur();

        Rect worldBound = field.worldBound;
        Rect rootLayout = field.panel.visualTree.layout;
        float scaleX = Screen.width / rootLayout.width;
        float scaleY = Screen.height / rootLayout.height;

        Rect screenRect = new Rect(
            worldBound.x * scaleX, worldBound.y * scaleY,
            worldBound.width * scaleX, worldBound.height * scaleY);

        float cs = canvas.scaleFactor;
        ghostRect.anchoredPosition = new Vector2(screenRect.x, -screenRect.y) / cs;
        ghostRect.sizeDelta = new Vector2(screenRect.width, screenRect.height) / cs;

        ghostInput.SetTextWithoutNotify(field.value);
        ghostInput.gameObject.SetActive(true);
        ghostInput.Select();
        ghostInput.ActivateInputField();
    }

    void OnGhostValueChanged(string newValue)
    {
        if (activeField == null) return;

        string oldValue = activeField.value;
        activeField.SetValueWithoutNotify(newValue);

        using (var evt = ChangeEvent<string>.GetPooled(oldValue, newValue))
        {
            evt.target = activeField;
            activeField.SendEvent(evt);
        }
    }
}