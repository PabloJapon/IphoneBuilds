using System.Collections.Generic;
using UnityEngine;

public class IncomingCallOrderRouter : MonoBehaviour
{
    public static IncomingCallOrderRouter instance;
    public static string pendingNumero;

    private static readonly Queue<string> pendingQueue = new Queue<string>();
    private static bool flowActive = false;

    public GameObject canvasElegirTipo;

    void Awake()
    {
        instance = this;
    }

    public static void NotifyCallAnswered(string numero)
    {
        CallPopupController.NotifyCallEnded(numero);
        pendingQueue.Enqueue(numero);
        TryShowNext();
    }

    private static void TryShowNext()
    {
        if (flowActive || pendingQueue.Count == 0) return;
        if (instance == null || instance.canvasElegirTipo == null) return;

        pendingNumero = pendingQueue.Dequeue();
        flowActive = true;
        instance.canvasElegirTipo.SetActive(true);
    }

    public static void NotifyFlowFinished()
    {
        flowActive = false;
        pendingNumero = null;
        TryShowNext();
    }
}