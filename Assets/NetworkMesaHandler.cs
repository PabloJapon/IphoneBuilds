using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class NetworkMesaHandler : NetworkBehaviour
{
    [Command]
    public void CmdJuntarMesas(List<float> mesaIds)
    {
        string restId = GetComponent<MyRoomPlayer>().RestaurantID;
        Debug.Log($"[CmdJuntarMesas] Received mesas to join for RestaurantID: {restId}");

        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            foreach (NetworkConnectionToClient conn in conns)
            {
                Debug.Log($"[CmdJuntarMesas] Sending to conn {conn.connectionId}");
                TargetJuntarMesas(conn, mesaIds.ToArray());
            }
        }
    }

    [TargetRpc]
    public void TargetJuntarMesas(NetworkConnection conn, float[] mesaIds)
    {
        Debug.Log($"[TargetJuntarMesas] Received mesas to join: {string.Join(", ", mesaIds)}");

        // Call your existing logic here locally (perhaps via LongPressDebug or direct)
        LongPressDebug.HandleJuntarMesasFromNetwork(mesaIds);
    }

    [Command]
    public void CmdSepararMesas(List<float> mesaIds)
    {
        string restId = GetComponent<MyRoomPlayer>().RestaurantID;
        Debug.Log($"[CmdSepararMesas] Received mesas to separate for RestaurantID: {restId}");

        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            foreach (NetworkConnectionToClient conn in conns)
            {
                Debug.Log($"[CmdSepararMesas] Sending to conn {conn.connectionId}");
                TargetSepararMesas(conn, mesaIds.ToArray());
            }
        }
    }

    [TargetRpc]
    public void TargetSepararMesas(NetworkConnection conn, float[] mesaIds)
    {
        Debug.Log($"[TargetSepararMesas] Received mesas to separate: {string.Join(", ", mesaIds)}");
        LongPressDebug.HandleSepararMesasFromNetwork(mesaIds);
    }

    [Command]
    public void CmdCambiarMesa(float oldId, float newId)
    {
        string restId = GetComponent<MyRoomPlayer>().RestaurantID;
        Debug.Log($"[CmdCambiarMesa] Request to change mesa {oldId} ➡ {newId} for RestaurantID: {restId}");

        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            foreach (NetworkConnectionToClient conn in conns)
            {
                TargetCambiarMesa(conn, oldId, newId);
            }
        }
    }

    [TargetRpc]
    public void TargetCambiarMesa(NetworkConnection conn, float oldId, float newId)
    {
        Debug.Log($"[TargetCambiarMesa] Change mesa {oldId} ➡ {newId}");

        LongPressDebug.HandleCambiarMesaFromNetwork(oldId, newId);
    }


}
