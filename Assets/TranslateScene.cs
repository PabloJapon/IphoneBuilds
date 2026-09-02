using UnityEngine;
using TMPro;

public class TranslateScene : MonoBehaviour
{
    public TranslationManager translationManager;

    void Start()
    {
        // Translate all text elements in the scene
        TranslateAllText();
    }

    void TranslateAllText()
    {
        // Find all TextMeshPro Text elements in the scene
        TMP_Text[] textElements = FindObjectsOfType<TMP_Text>();

        // Translate each text element
        foreach (TMP_Text textElement in textElements)
        {
            // Get the original text from the text element
            string originalText = textElement.text;

            // Translate the original text
            translationManager.TranslateText(originalText, translatedText =>
            {
                // Update the text element with the translated text
                if (translatedText != null)
                {
                    textElement.text = translatedText;
                }
                else
                {
                    Debug.LogError("Failed to translate text: " + originalText);
                }
            });
        }
    }
}
