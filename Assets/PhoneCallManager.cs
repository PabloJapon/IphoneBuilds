using Mirror;
using UnityEngine;

public class PhoneCallManager : MonoBehaviour
{
    public static PhoneCallManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void NotifyIncomingCall(string restId, string numero)
    {
        if (string.IsNullOrEmpty(restId))
        {
            Debug.LogWarning($"[PhoneCallManager] Missing restaurant_id for incoming call {numero}, cannot route.");
            return;
        }

        if (!(NetworkManager.singleton is MyRoomManager mgr) ||
            !mgr.restaurantConnections.TryGetValue(restId, out var connections) ||
            connections.Count == 0)
        {
            Debug.LogWarning($"[PhoneCallManager] No valid connections for restaurantId {restId}.");
            return;
        }

        Debug.Log($"[PhoneCallManager] Routing call {numero} to {connections.Count} connection(s) for restaurantId {restId}");

        foreach (var conn in connections)
        {
            if (conn?.isReady != true)
            {
                Debug.LogWarning($"[PhoneCallManager] Skipping conn {conn?.connectionId}: isReady={conn?.isReady}");
                continue;
            }

            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
            {
                Debug.Log($"[PhoneCallManager] Sending TargetShowIncomingCall -> conn {conn.connectionId}");
                player.TargetShowIncomingCall(conn, numero);
            }
            else
            {
                Debug.LogWarning($"[PhoneCallManager] conn {conn.connectionId} has no MyRoomPlayer identity");
            }
        }
    }

    public void NotifyCallAnswered(string restId, string numero)
    {
        if (string.IsNullOrEmpty(restId)) return;

        if (!(NetworkManager.singleton is MyRoomManager mgr) ||
            !mgr.restaurantConnections.TryGetValue(restId, out var connections) ||
            connections.Count == 0)
            return;

        foreach (var conn in connections)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
                player.TargetCallAnswered(conn, numero);
        }
    }

    public void NotifyCallEnded(string restId, string numero)
    {
        if (string.IsNullOrEmpty(restId)) return;

        if (!(NetworkManager.singleton is MyRoomManager mgr) ||
            !mgr.restaurantConnections.TryGetValue(restId, out var connections) ||
            connections.Count == 0)
            return;

        foreach (var conn in connections)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
                player.TargetHideIncomingCall(conn, numero);
        }
    }
    
}