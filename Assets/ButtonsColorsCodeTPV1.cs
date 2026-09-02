// este codigo sirve para cambiar solo los colores de los iconos y textos de la barra de abajo (imagen en DataBasePersonalizacion)

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ButtonsColorsCodeTPV1 : MonoBehaviour
{
    public Button[] buttons;
    public Color selectedBackgroundColor;
    public Color selectedTextColor;
    public Color defaultBackgroundColor;
    public Color defaultTextColor;

    private Button selectedButton;

    public DataBase DB; // Reference to the first DataBase component
    public DataBasePersonalizacion DB2; // Reference to the second DataBase component

    private bool isDBLoaded = false;
    private bool isDB2Loaded = false;

    public bool TPV = false;
    public Image[] imageButtonBorders;

    void OnEnable()
    {
        // Nos suscribimos siempre que el objeto se active
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;

        // Si ya estaban cargadas antes, marcamos los flags y chequeamos
        if (DB.IsLoaded) OnDBLoaded();
        if (DB2.IsLoaded) OnDB2Loaded();
    }

    void Start()
    {
        // Esto sigue sirviendo para el caso en que el objeto esté activo desde el inicio
        DB.OnDataLoaded += OnDBLoaded;
        DB2.OnDataLoaded += OnDB2Loaded;
    }

    private void OnDestroy()
    {
        // Nos desuscribimos para evitar fugas de memoria
        DB.OnDataLoaded -= OnDBLoaded;
        DB2.OnDataLoaded -= OnDB2Loaded;
    }

    private void OnDBLoaded()
    {
        isDBLoaded = true;
        CheckIfBothDatabasesAreLoaded();
    }

    private void OnDB2Loaded()
    {
        isDB2Loaded = true;
        CheckIfBothDatabasesAreLoaded();
    }

    private void CheckIfBothDatabasesAreLoaded()
    {
        if (isDBLoaded && isDB2Loaded)
        {
            ChangeImageColor();
            InitializeButtons();
        }
    }

    private void InitializeButtons()
    {
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            ChangeButtonColor(button, defaultBackgroundColor, defaultTextColor);

            button.onClick.AddListener(() => SelectButton(button));
        }

        if (buttons.Length > 0)
        {
            SelectButton(buttons[0]);
        }

        if (TPV)
        {
            foreach (Image image in imageButtonBorders)
            {
                image.color = selectedBackgroundColor;
            }
        }
    }

    public void ChangeImageColor()
    {
        Color newColorIconoPulsado;
        Color newColorIconoNoPulsado;

        if (!TPV)
        {
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_icono_pulsado[0], out newColorIconoPulsado))
            {
                selectedBackgroundColor = newColorIconoPulsado;
                selectedTextColor = newColorIconoPulsado;
            }
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_icono_base[0], out newColorIconoNoPulsado))
            {
                defaultBackgroundColor = newColorIconoNoPulsado;
                defaultTextColor = newColorIconoNoPulsado;
            }
        }
        else if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_botones[0], out newColorIconoPulsado))
        {
            selectedBackgroundColor = newColorIconoPulsado;
            selectedTextColor = Color.white;

            defaultBackgroundColor = Color.white;
            defaultTextColor = newColorIconoPulsado;
        }
    }

    public void SelectButton(Button button)
    {
        if (selectedButton != null)
        {
            ChangeButtonColor(selectedButton, defaultBackgroundColor, defaultTextColor);
        }

        selectedButton = button;
        ChangeButtonColor(selectedButton, selectedBackgroundColor, selectedTextColor);
    }

    void ChangeButtonColor(Button button, Color backgroundColor, Color textColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.selectedColor = backgroundColor;
        colors.highlightedColor = backgroundColor;
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
