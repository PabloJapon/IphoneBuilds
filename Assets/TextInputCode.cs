using UnityEngine;
using TMPro;

public class TextInputCode : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text displayText;
    
    // Start is called before the first frame update
    void Start()
    {
        // Add a listener to the input field to detect text changes
 
        inputField.onValueChanged.AddListener(delegate { UpdateText(); });
    }

    // TITULO
    // Update the display text with the input field's text
    void UpdateText()
    {
        displayText.text = inputField.text;
    }

}

