using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class OnScreenDebugger : MonoBehaviour
{
    private static List<string> logs = new List<string>();
    private Vector2 scrollPos;

    void OnEnable() => Application.logMessageReceived += HandleLog;
    void OnDisable() => Application.logMessageReceived -= HandleLog;

    void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            logs.Add($"[{type}] {message}\n{stackTrace}");
            if (logs.Count > 20) logs.RemoveAt(0);
        }
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Box(new Rect(10, 10, Screen.width - 20, 300), "");
        GUILayout.BeginArea(new Rect(15, 15, Screen.width - 30, 290));
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (var log in logs)
            GUILayout.Label(log);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUI.color = Color.white;
    }
}