using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum KeyType { Character, Backspace, Shift, Space, Enter, SwitchLayout }

// Se pone en el prefab de cada botón del teclado (Button + TMP_Text hijo)
public class KeyButton : MonoBehaviour
{
    public TMP_Text label;
    public Button button;

    private string normalChar;
    private string shiftChar;
    public KeyType keyType = KeyType.Character;

    private OnScreenKeyboardController controller;

    public int targetLayoutIndex;

    public void SetSwitchTarget(int index, string newLabel)
    {
        targetLayoutIndex = index;
        if (label != null) label.text = newLabel;
}

    public void Init(string normal, string shift, OnScreenKeyboardController ctrl, KeyType type = KeyType.Character)
    {
        normalChar = normal;
        shiftChar = shift;
        controller = ctrl;
        keyType = type;

        if (label != null)
            label.text = normal;

        button.onClick.AddListener(OnClick);
    }

    public void UpdateShiftState(bool shiftActive)
    {
        if (keyType == KeyType.Character && label != null)
            label.text = shiftActive ? shiftChar : normalChar;
    }

    private void OnClick()
    {
        switch (keyType)
        {
            case KeyType.Character:
                controller.InsertText(controller.ShiftActive ? shiftChar : normalChar);
                break;
            case KeyType.Backspace:
                controller.Backspace();
                break;
            case KeyType.Shift:
                controller.ToggleShift();
                break;
            case KeyType.Space:
                controller.InsertText(" ");
                break;
            case KeyType.Enter:
                controller.Enter();
                break;
            case KeyType.SwitchLayout:
                controller.SwitchLayout(targetLayoutIndex);
                break;
        }
    }
}