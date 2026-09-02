#if UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable] public class AsistenciaRequest { public int id; public string restaurant_id; public int mesa; }
[System.Serializable] public class AsistenciaRequestsResponse { public List<AsistenciaRequest> requests; }
[System.Serializable] public class IncomingCall { public int id; public string numero; public string restaurant_id; }
[System.Serializable] public class CallsResponse { public List<IncomingCall> calls; }

public class WebOrderBridge : MonoBehaviour
{
    private static bool pollerActive = false;

    public string apiBase = "https://gastrali.tail634a78.ts.net";
    public string bridgeApiKey = "wuerjhakrguh7346873qkjrgbh985467uswfhiiargoiihy23r8yhrfnhrgq3lkm";
    public float pollIntervalSeconds = 2f;

    void Start()
    {
        if (pollerActive) { Debug.Log("[WebOrderBridge] Duplicate poller skipped."); return; }
        pollerActive = true;
        StartCoroutine(PollLoop());
    }

    IEnumerator PollLoop()
    {
        Debug.Log("[WebOrderBridge] Poll loop started");
        while (true)
        {
            yield return StartCoroutine(FetchAndProcess());
            yield return StartCoroutine(FetchAndProcessAsistencia());
            yield return StartCoroutine(FetchAndProcessCalls());
            yield return StartCoroutine(FetchAndProcessCallEndings());
            yield return StartCoroutine(FetchAndProcessCallAnswered());
            yield return new WaitForSeconds(pollIntervalSeconds);
        }
    }

    IEnumerator FetchAndProcessCalls()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{apiBase}/calls/pending"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            var response = JsonUtility.FromJson<CallsResponse>(req.downloadHandler.text);
            if (response?.calls == null) yield break;

            foreach (var call in response.calls)
            {
                Debug.Log($"[WebOrderBridge] Llamada entrante: {call.numero} (Restaurant: {call.restaurant_id})");
                PhoneCallManager.instance?.NotifyIncomingCall(call.restaurant_id, call.numero);
                yield return StartCoroutine(ConfirmCallSynced(call.id));
            }
        }
    }

    IEnumerator ConfirmCallSynced(int callId)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm($"{apiBase}/calls/{callId}/synced", ""))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
        }
    }

    IEnumerator FetchAndProcessCallEndings()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{apiBase}/calls/ended"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            var response = JsonUtility.FromJson<CallsResponse>(req.downloadHandler.text);
            if (response?.calls == null) yield break;

            foreach (var call in response.calls)
            {
                Debug.Log($"[WebOrderBridge] Llamada finalizada: {call.numero} (Restaurant: {call.restaurant_id})");
                PhoneCallManager.instance?.NotifyCallEnded(call.restaurant_id, call.numero);
                yield return StartCoroutine(ConfirmCallClosed(call.id));
            }
        }
    }

    IEnumerator ConfirmCallClosed(int callId)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm($"{apiBase}/calls/{callId}/closed", ""))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[ConfirmCallClosed] Failed to close call {callId}: {req.error}");
        }
    }

    IEnumerator FetchAndProcessCallAnswered()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{apiBase}/calls/answered/pending"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            var response = JsonUtility.FromJson<CallsResponse>(req.downloadHandler.text);
            if (response?.calls == null) yield break;

            foreach (var call in response.calls)
            {
                Debug.Log($"[WebOrderBridge] Llamada contestada: {call.numero} (Restaurant: {call.restaurant_id})");
                PhoneCallManager.instance?.NotifyCallAnswered(call.restaurant_id, call.numero);
                yield return StartCoroutine(ConfirmAnsweredSynced(call.id));
            }
        }
    }

    IEnumerator ConfirmAnsweredSynced(int callId)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm($"{apiBase}/calls/{callId}/answered_synced", ""))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[ConfirmAnsweredSynced] Failed to sync call {callId}: {req.error}");
        }
    }

    IEnumerator FetchAndProcess()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{apiBase}/orders/web/pending"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5; // seconds � abort instead of hanging forever
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[WebOrderBridge] Fetch failed: {req.error} ({req.responseCode})");
                yield break;
            }

            Debug.Log($"[WebOrderBridge] Fetch OK: {req.downloadHandler.text}");
            var response = JsonUtility.FromJson<WebOrdersResponse>(req.downloadHandler.text);
            if (response?.orders == null) yield break;

            foreach (var order in response.orders)
            {
                ProcessOrder(order);
                yield return StartCoroutine(ConfirmSynced(order.id));
            }
        }
    }

    void ProcessOrder(WebOrder order)
    {
        if (order.mesa <= 0)
        {
            Debug.LogWarning($"[WebOrderBridge] Skipping order {order.id}: invalid mesa");
            return;
        }
        int n = order.dishes.Count;
        string[] nombre = new string[n], opciones = new string[n], cantidad = new string[n], precio = new string[n], nota = new string[n];
        int[] toggle = new int[n], orden = new int[n];

        for (int i = 0; i < n; i++)
        {
            var d = order.dishes[i];
            nombre[i] = d.name; opciones[i] = d.options; cantidad[i] = d.quantity;
            precio[i] = d.price; toggle[i] = d.toggle; nota[i] = d.nota; orden[i] = d.orden;
        }

        MesaStateManager.instance.ProcessIncomingPedido(order.restaurant_id, order.mesa, n, nombre, opciones, cantidad, precio, toggle, nota, orden);
    }

    IEnumerator ConfirmSynced(int orderId)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm($"{apiBase}/orders/web/{orderId}/synced", ""))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
        }
    }

    IEnumerator FetchAndProcessAsistencia()
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{apiBase}/orders/web/asistencia/pending"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            var response = JsonUtility.FromJson<AsistenciaRequestsResponse>(req.downloadHandler.text);
            if (response?.requests == null) yield break;

            foreach (var r in response.requests)
            {
                MesaStateManager.instance.RequestAsistencia(r.restaurant_id, r.mesa);
                yield return StartCoroutine(ConfirmAsistenciaSynced(r.id));
            }
        }
    }

    IEnumerator ConfirmAsistenciaSynced(int id)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm($"{apiBase}/orders/web/asistencia/{id}/synced", ""))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
        }
    }
}
#endif