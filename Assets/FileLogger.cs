using UnityEngine;
using System.IO;

public class FileLogger : MonoBehaviour
{
    string logPath;

    void Awake()
    {
        logPath = Path.Combine(Application.persistentDataPath, "debug_log.txt");
        File.WriteAllText(logPath, $"Log started.\n");
    }

    void OnEnable() => Application.logMessageReceived += HandleLog;
    void OnDisable() => Application.logMessageReceived -= HandleLog;

    void HandleLog(string message, string stackTrace, LogType type)
    {
        try { File.AppendAllText(logPath, $"[{type}] {message}\n{stackTrace}\n\n"); }
        catch { }
    }
}