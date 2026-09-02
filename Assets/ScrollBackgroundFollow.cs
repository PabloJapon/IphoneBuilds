using UnityEngine;
using UnityEngine.UI;

public class ScrollBackgroundFollow : MonoBehaviour
{
    [Tooltip("Assign the ScrollRect component")]
    public ScrollRect scrollRect;

    [Tooltip("Match this to your ScrollView's top offset (e.g. 1200)")]
    public float topOffset = 1200f;

    private RectTransform bgRect;
    private float originalY;

    void Start()
    {
        bgRect = GetComponent<RectTransform>();
    }

    public void ResetOrigin()
    {
        if (bgRect == null) bgRect = GetComponent<RectTransform>();
        originalY = bgRect.anchoredPosition.y;
    }

    void LateUpdate()
    {
        if (scrollRect == null) return;

        float contentOffsetY = scrollRect.content.anchoredPosition.y;

        bgRect.anchoredPosition = new Vector2(
            bgRect.anchoredPosition.x,
            originalY + topOffset + contentOffsetY
        );
    }
}