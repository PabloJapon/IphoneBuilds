using UnityEngine;
using UnityEngine.UI;

public class ScrollSync : MonoBehaviour
{
    public ScrollRect scrollRect1;  // First scroll view
    public ScrollRect scrollRect2;  // Second scroll view

    private bool isSyncing = false;

    void Start()
    {
        // Add listeners to detect when either scroll rect is scrolled
        scrollRect1.onValueChanged.AddListener((Vector2 pos) => SyncScrolls(scrollRect1, scrollRect2));
        scrollRect2.onValueChanged.AddListener((Vector2 pos) => SyncScrolls(scrollRect2, scrollRect1));
    }

    void SyncScrolls(ScrollRect source, ScrollRect target)
    {
        if (isSyncing) return;

        isSyncing = true;
        target.horizontalNormalizedPosition = source.horizontalNormalizedPosition;
        isSyncing = false;
    }
}
