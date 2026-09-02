using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ColorPickerCode : MonoBehaviour
{
    private Image buttonImage; // Usa el componente Image para el botón
    public List<GameObject> targetGameObjects; // Lista de GameObjects cuyos colores quieres cambiar

    public int numberColor;

    // Claves para PlayerPrefs para almacenar el color seleccionado
    public const string SelectedColor1Key = "SelectedColor1";
    public const string SelectedColor2Key = "SelectedColor2";

    void Start()
    {
        buttonImage = GetComponent<Image>(); // Obtiene el componente Image

        if (numberColor == 1)
        {
            var savedColor = PlayerPrefs.GetString(SelectedColor1Key);
            Color loadedColor1;

            if (ColorUtility.TryParseHtmlString("#" + savedColor, out loadedColor1))
            {
                // Carga el color exitosamente desde PlayerPrefs
                buttonImage.color = loadedColor1;
            }
            else
            {
                // Falló al cargar el color desde PlayerPrefs
                Debug.LogWarning("Failed to parse the saved color string.");
            }
        }
        else
        {
            var savedColor = PlayerPrefs.GetString(SelectedColor2Key);
            Color loadedColor2;

            if (ColorUtility.TryParseHtmlString("#" + savedColor, out loadedColor2))
            {
                // Carga el color exitosamente desde PlayerPrefs
                buttonImage.color = loadedColor2;
            }
            else
            {
                // Falló al cargar el color desde PlayerPrefs
                Debug.LogWarning("Failed to parse the saved color string.");
            }
        }
    }

    public void ChooseColorButtonClick()
    {
        ColorPicker.Create(buttonImage.color, "Choose the button's color!", SetColor, ColorFinished, true);
    }

    private void SetColor(Color currentColor)
    {
        buttonImage.color = currentColor; // Cambia el color del botón
    }

    private void ColorFinished(Color finishedColor)
    {
        string colorKey = numberColor == 1 ? SelectedColor1Key : SelectedColor2Key;

        // Guarda el color seleccionado en PlayerPrefs
        PlayerPrefs.SetString(colorKey, ColorUtility.ToHtmlStringRGBA(finishedColor));
        PlayerPrefs.Save();

        // Cambia el color de todos los GameObjects en la lista
        foreach (GameObject targetGameObject in targetGameObjects)
        {
            ChangeGameObjectColor(targetGameObject, finishedColor);
        }
    }

    private void ChangeGameObjectColor(GameObject target, Color color)
    {
        if (target != null)
        {
            // Intenta cambiar el color del Renderer
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
                return;
            }

            // Intenta cambiar el color del Image (para UI elements)
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
                return;
            }

            // Intenta cambiar el color del SpriteRenderer (para sprites en 2D)
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                return;
            }

            // Intenta cambiar el color del Text (para UI Text)
            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                text.color = color;
                return;
            }

            // Intenta cambiar el color del TextMeshProUGUI (para TextMeshPro UI Text)
            TextMeshProUGUI textMeshProUGUI = target.GetComponent<TextMeshProUGUI>();
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.color = color;
                return;
            }

            // Si no se encontró ningún componente compatible, muestra una advertencia
            Debug.LogWarning("El GameObject no tiene un componente Renderer, Image, SpriteRenderer, Text, o TextMeshProUGUI.");
        }
        else
        {
            Debug.LogWarning("No se ha asignado ningún GameObject.");
        }
    }
}