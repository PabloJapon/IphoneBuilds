using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SyncQR : MonoBehaviour
{   // References
    public Image imageColorSourceImage;
    public Image textColorSourceImage;
    public TMP_InputField textMensaje;
    public TMP_Dropdown dropdownFuente;
    public TMP_Dropdown dropdownTamaño;
    public Toggle myToggle;

    // To sync
    public TextMeshProUGUI firstTextMeshProComponent;
    public TextMeshProUGUI secondTextMeshProComponent;
    public Image imageToChange;

    // Internal parameters
    private Color lastTextColor;
    private Color lastImageColor;
    private String lastTextMensaje;
    private String lastTextFuente;
    private int lastTextTamaño;
    private int lastToggle;

    // para cambiar fuente
    public FontImageList fontImageList; // Referencia al ScriptableObject

    void Start()
    {
        // Initialize last values to current values to avoid unnecessary updates at the beginning
        lastTextColor = textColorSourceImage.color;
        lastImageColor = imageColorSourceImage.color;
        lastTextMensaje = textMensaje.text;
        lastTextFuente = fontImageList.fontNames[dropdownFuente.value];
        lastTextTamaño = dropdownTamaño.value;
        if (myToggle.isOn == true)
        {
            lastToggle = 1;
        }
        else
        {
            lastToggle = 0;
        }

        // Apply the initial values to make sure everything is in sync
        UpdateTextColors(lastTextColor);
        UpdateImageColor(lastImageColor);
        UpdateText(lastTextMensaje,lastToggle);
        UpdateFuente(lastTextFuente);
        UpdateTamaño(lastTextTamaño);
    }


    void Update()
    {
        if (textColorSourceImage.color != lastTextColor)
        {
            lastTextColor = textColorSourceImage.color;
            UpdateTextColors(lastTextColor);
        }

        if (imageColorSourceImage.color != lastImageColor)
        {
            lastImageColor = imageColorSourceImage.color;
            UpdateImageColor(lastImageColor);
        }

        if (textMensaje.text != lastTextMensaje)
        {
            lastTextMensaje = textMensaje.text;
            UpdateText(lastTextMensaje,lastToggle);
        }

        if (fontImageList.fontNames[dropdownFuente.value] != lastTextFuente)
        {
            lastTextFuente = fontImageList.fontNames[dropdownFuente.value];
            UpdateFuente(lastTextFuente);
        }

        if (dropdownTamaño.value != lastTextTamaño)
        {
            lastTextTamaño = dropdownTamaño.value;
            UpdateTamaño(lastTextTamaño);
        }

        int myToggleValue=-1;
        if (myToggle.isOn == true)
        {
            myToggleValue=1;
        }
        else
        { 
            myToggleValue=0;
        }

        if (myToggleValue != lastToggle)
        {
            lastToggle = myToggleValue;
            UpdateText(lastTextMensaje,lastToggle);
        }
    }

    void UpdateTextColors(Color newColor)
    {
        firstTextMeshProComponent.color = newColor;
        secondTextMeshProComponent.color = newColor;
    }

    void UpdateImageColor(Color newColor)
    {
        imageToChange.color = newColor;
    }

    void UpdateText(String newText, int value)
    {
        if (value ==1)
        {
            secondTextMeshProComponent.text = newText;
        }
        else
        {
            secondTextMeshProComponent.text = "";
        }
    }

    void UpdateFuente(String newFont)
    {
        TMP_FontAsset newFont2 = Resources.Load<TMP_FontAsset>("Fonts/" + newFont);
        firstTextMeshProComponent.font = newFont2;
        secondTextMeshProComponent.font = newFont2;
    }

    void UpdateTamaño(int newSize)
    {
        if (newSize == 0) // Pequeño
        {
            firstTextMeshProComponent.fontSize = 35;
            secondTextMeshProComponent.fontSize = 20;
        }
        if (newSize == 1) // Mediano
        {
            firstTextMeshProComponent.fontSize = 40;
            secondTextMeshProComponent.fontSize = 25;
        }
        if (newSize == 2) // Grande
        {
            firstTextMeshProComponent.fontSize = 45;
            secondTextMeshProComponent.fontSize = 30;
        }
    }

}
