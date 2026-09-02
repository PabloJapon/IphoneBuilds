using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonsColorsCodeResponsable : MonoBehaviour
{
    public Button[] buttons;
    public Color selectedBackgroundColor;
    public Color selectedTextColor;
    public Color defaultBackgroundColor;
    public Color defaultTextColor;
    public Color highlightedBackgroundColor; // New highlighted color

    private Button selectedButton;

    void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            ChangeButtonColor(button, defaultBackgroundColor, defaultTextColor, highlightedBackgroundColor);

            button.onClick.AddListener(() => SelectButton(button));
        }

        if (buttons.Length > 0)
        {
            SelectButton(buttons[0]);
        }
    }

    void SelectButton(Button button)
    {
        if (selectedButton != null)
        {
            ChangeButtonColor(selectedButton, defaultBackgroundColor, defaultTextColor, highlightedBackgroundColor);
        }

        selectedButton = button;
        ChangeButtonColor(selectedButton, selectedBackgroundColor, selectedTextColor, selectedBackgroundColor);
    }

    void ChangeButtonColor(Button button, Color backgroundColor, Color textColor, Color highlightColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.selectedColor = backgroundColor;
        colors.highlightedColor = highlightColor; // Different highlight color
        button.colors = colors;

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = textColor;
        }
        else
        {
            Debug.LogWarning("No TextMeshProUGUI component found on button: " + button.name);
        }
    }
}
