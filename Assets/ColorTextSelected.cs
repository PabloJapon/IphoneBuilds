using UnityEngine;
using TMPro;

public class ColorTextSelected : MonoBehaviour
{
    public Color targetColor = Color.white; // Color to change the text to

    public void ChangeColorText()
    {
        // Find the TMP_Text component in the child of the current GameObject
        TMP_Text textChild = GetComponentInChildren<TMP_Text>();
        textChild.color = targetColor;
    }
}
