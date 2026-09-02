using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This script should be attached to each Mesa UI button prefab.
/// It visually updates the mesa's color based on MesaColorType.
/// </summary>
public class MesaColorSync : MonoBehaviour
{
    [HideInInspector] public int mesaNumber;
    private MesaColorType currentColorType = MesaColorType.Default;

    private Image buttonImage;
    private TMP_Text buttonText;

    [Header("Color Settings (set in Inspector)")]
    public Color defaultColor = Color.white;
    public Color defaultTextColor = Color.black;

    public Color yellowColor = Color.yellow;
    public Color yellowTextColor = Color.white;

    public Color blueColor = Color.blue;
    public Color blueTextColor = Color.white;

    public Color redColor = Color.red;
    public Color redTextColor = Color.white;

    public Color greyColor = Color.grey;
    public Color greyTextColor = Color.black;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
            Debug.LogError("MesaColorSync: No Image component found!");

        buttonText = GetComponentInChildren<TMP_Text>();
        if (buttonText == null)
            Debug.LogWarning("MesaColorSync: No TMP_Text found in children.");
    }

    public void SetColor(MesaColorType colorType, bool notify = true)
    {
        if (buttonImage == null) return;

        bool changed = colorType != currentColorType;
        currentColorType = colorType;

        switch (colorType)
        {
            case MesaColorType.Yellow:
                buttonImage.color = yellowColor;
                if (buttonText != null) buttonText.color = yellowTextColor;
                break;
            case MesaColorType.Blue:
                buttonImage.color = blueColor;
                if (buttonText != null) buttonText.color = blueTextColor;
                break;
            case MesaColorType.Red:
                buttonImage.color = redColor;
                if (buttonText != null) buttonText.color = redTextColor;
                break;
            case MesaColorType.Grey:
                buttonImage.color = greyColor;
                if (buttonText != null) buttonText.color = greyTextColor;
                break;
            case MesaColorType.Default:
            default:
                buttonImage.color = defaultColor;
                if (buttonText != null) buttonText.color = defaultTextColor;
                break;
        }

        if (notify && changed && SceneManager.GetActiveScene().name == "TPVScene" && TPVNotificaciones.instance != null)
            TPVNotificaciones.instance.NotifyMesaChanged(mesaNumber);
    }
}
