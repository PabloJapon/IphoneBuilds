using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorsButtons : MonoBehaviour
{
    public Button[] buttons;
    public Color selectedBackgroundColor;
    public Color selectedTextColor;
    public Color defaultBackgroundColor;
    public Color defaultTextColor;

    private Button selectedButton;

    void Start()
    {
        // Set transition to instant color change for all buttons
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            ChangeButtonColor(button, defaultBackgroundColor, defaultTextColor);

            // Assign onClick listeners to each button
            button.onClick.AddListener(() => SelectButton(button));
        }
        // Select the default button
        if (buttons.Length > 0)
        {
            SelectButton(buttons[0]);
        }
    }

    void SelectButton(Button button)
    {
        Debug.Log("Button selected: " + button.name);

        // Reset color of previously selected button
        if (selectedButton != null)
        {
            Debug.Log("Resetting color of previously selected button: " + selectedButton.name);
            ChangeButtonColor(selectedButton, defaultBackgroundColor, defaultTextColor);
        }

        // Change color of newly selected button
        selectedButton = button;
        ChangeButtonColor(selectedButton, selectedBackgroundColor, selectedTextColor);
    }

    void ChangeButtonColor(Button button, Color backgroundColor, Color textColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor; // Change background color
        colors.selectedColor = backgroundColor; // Change background color
        colors.highlightedColor = backgroundColor; // Change background color
        button.colors = colors;

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = textColor; // Change text color
        }
        else
        {
            Debug.LogWarning("No TextMeshProUGUI component found on button: " + button.name);
        }
    }
}

