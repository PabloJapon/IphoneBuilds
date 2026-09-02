using UnityEngine;
using UnityEngine.UI;

public class ToggleColorChange : MonoBehaviour
{
    public Toggle toggle;
    public Color offColor = Color.white;
    public Color onColor = Color.green;

    private Image checkmarkImage;

    void Start()
    {
        if (toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }

        if (toggle != null)
        {
            // Remove the default graphic switching behavior
            toggle.graphic = null;

            // Create a new Image component for the checkmark
            GameObject checkmarkObject = new GameObject("Checkmark");
            checkmarkObject.transform.SetParent(toggle.transform, false);
            checkmarkImage = checkmarkObject.AddComponent<Image>();

            // Set the initial color based on the toggle state
            checkmarkImage.color = toggle.isOn ? onColor : offColor;

            // Subscribe to the onValueChanged event
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        else
        {
            Debug.LogWarning("No Toggle component found.");
        }
    }

    void OnToggleValueChanged(bool isOn)
    {
        // Change the color of the checkmark based on the toggle state
        if (checkmarkImage != null)
        {
            checkmarkImage.color = isOn ? onColor : offColor;
        }
    }
}