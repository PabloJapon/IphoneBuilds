using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SwitchToggleEmpleados : MonoBehaviour
{
    [SerializeField] RectTransform uiHandleRectTransform;
    [SerializeField] Color backgroundActiveColor;
    [SerializeField] Color handleActiveColor;

    Image backgroundImage, handleImage;
    Color backgroundDefaultColor, handleDefaultColor;

    Toggle toggle;
    Vector2 handlePosition;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        handlePosition = uiHandleRectTransform.anchoredPosition;

        backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
        handleImage = uiHandleRectTransform.GetComponent<Image>();

        backgroundDefaultColor = backgroundImage.color;
        handleDefaultColor = handleImage.color;

        toggle.onValueChanged.AddListener(OnSwitch);
    }

    void OnSwitch(bool on)
    {
        // Animate handle position
        uiHandleRectTransform.DOAnchorPos(on ? handlePosition * -1 : handlePosition, .4f).SetEase(Ease.InOutBack);

        // Animate background color
        backgroundImage.DOColor(on ? backgroundActiveColor : backgroundDefaultColor, .6f);

        // Animate handle color
        handleImage.DOColor(on ? handleActiveColor : handleDefaultColor, .4f);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}