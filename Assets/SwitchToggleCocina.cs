using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; // Necesario para TextMeshPro

public class SwitchToggleCocina : MonoBehaviour
{
    [SerializeField] RectTransform uiHandleRectTransform;
    [SerializeField] Color backgroundActiveColor;
    [SerializeField] Color handleActiveColor;

    Image backgroundImage, handleImage;
    Color backgroundDefaultColor, handleDefaultColor;

    Toggle toggle;
    Vector2 handlePosition;

    TextMeshProUGUI labelText; // ← Aquí usamos TMP

    void OnEnable()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (uiHandleRectTransform != null && backgroundImage == null)
        {
            handlePosition = uiHandleRectTransform.anchoredPosition;

            backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
            handleImage = uiHandleRectTransform.GetComponent<Image>();

            backgroundDefaultColor = backgroundImage.color;
            handleDefaultColor = handleImage.color;

            toggle.onValueChanged.AddListener(OnSwitch);
        }

        // 👇 Asignar colores desde la personalización
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionCocinaScene.col_ppal_empl[0], out Color colorSec))
        {
            backgroundActiveColor = colorSec;
            handleActiveColor = GetContrastingShade(colorSec);
        }

        // Busca el TMP entre los hijos del Toggle
        labelText = GetComponentInChildren<TextMeshProUGUI>();

        OnSwitch(toggle.isOn); // Aplica visual inicial
    }

    Color GetContrastingShade(Color baseColor)
    {
        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;
        float factor = 0.65f; // cuánto oscurecer/aclarar (0-1)

        if (luminance > 0.25f)
        {
            // Suficientemente claro: oscurecemos
            return new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
        }
        else
        {
            // Demasiado oscuro: aclaramos en su lugar
            return new Color(
                Mathf.Lerp(baseColor.r, 1f, factor),
                Mathf.Lerp(baseColor.g, 1f, factor),
                Mathf.Lerp(baseColor.b, 1f, factor),
                baseColor.a
            );
        }
    }

    void OnSwitch(bool on)
    {
        // Animate handle position
        uiHandleRectTransform.DOAnchorPos(on ? handlePosition * -1 : handlePosition, .4f).SetEase(Ease.InOutBack);

        // Animate background color
        backgroundImage.DOColor(on ? backgroundActiveColor : backgroundDefaultColor, .6f);

        // Animate handle color
        handleImage.DOColor(on ? handleActiveColor : handleDefaultColor, .4f);

        // Cambia el texto del Toggle
        if (labelText != null)
        {
            labelText.text = on ? "Esta cocina ya está lista para empezar" : "Esta cocina aún no está lista";
        }
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}