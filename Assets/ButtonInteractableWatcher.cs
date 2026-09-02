using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractableWatcher : MonoBehaviour
{
    private Button btn;
    private bool lastState;

    void Awake() => btn = GetComponent<Button>();
    void Start() => lastState = btn.interactable;

    void Update()
    {
        bool current = btn.interactable;
        if (current != lastState)
        {
            lastState = current;
            Debug.LogWarning($"[Watcher] interactable → {current} | frame={Time.frameCount} | time={Time.time}");
            Debug.LogWarning(StackTraceUtility.ExtractStackTrace());
        }
    }
}