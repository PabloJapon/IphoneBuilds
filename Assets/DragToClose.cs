using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragToClose : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform scrollContent;

    public float dragThreshold = -1000f;   // Y position to trigger close
    public float slideDistance = 500f;     // Extra slide distance
    public float slideDuration = 0.3f;     // Slide animation duration

    private bool isSliding = false;

    void Start()
    {
        scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollContent = scrollRect.content;
        }
    }

    void Update()
    {
        if (isSliding || scrollContent == null)
            return;

        float posY = scrollContent.anchoredPosition.y;

        if (posY < dragThreshold)
        {
            StartCoroutine(SlideContentAndClose());
        }
    }

    private IEnumerator SlideContentAndClose()
    {
        isSliding = true;

        Vector2 startPos = scrollContent.anchoredPosition;
        Vector2 endPos = startPos - new Vector2(0, slideDistance);

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            scrollContent.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        scrollContent.anchoredPosition = endPos;
        gameObject.SetActive(false);
        DetallePlato.Instance.ClearOptionGroups();
    }

    private void OnEnable()
    {
        if (scrollContent != null)
        {
            scrollContent.anchoredPosition = Vector2.zero;
        }

        isSliding = false;
    }
}
