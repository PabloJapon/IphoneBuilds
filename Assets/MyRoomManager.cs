using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MyRoomManager : NetworkRoomManager
{
    public Dictionary<string, List<NetworkConnectionToClient>> restaurantConnections = new Dictionary<string, List<NetworkConnectionToClient>>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[MyRoomManager] OnStartClient | active: {NetworkClient.active} connected: {NetworkClient.isConnected}");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("[MyRoomManager] ✅ OnClientConnect SUCCESS");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log($"[MyRoomManager] ❌ OnClientDisconnect | active: {NetworkClient.active} connected: {NetworkClient.isConnected}");
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log($"[MyRoomManager] OnServerConnect - new connection: {conn.connectionId} from {conn.address}");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);
        Debug.LogError($"[MyRoomManager] 🚨 ERROR: {error} | {reason}");
    }

    public void RegisterPlayer(MyRoomPlayer player)
    {
        string restId = player.RestaurantID;
        NetworkConnectionToClient conn = player.connectionToClient;
        Debug.Log($"[MyRoomManager] Registering conn {conn.connectionId} with RestaurantID: {restId}");

        if (!restaurantConnections.ContainsKey(restId))
        {
            restaurantConnections[restId] = new List<NetworkConnectionToClient>();
        }

        if (!restaurantConnections[restId].Contains(conn))
        {
            restaurantConnections[restId].Add(conn);
            Debug.Log($"[MyRoomManager] Added conn {conn.connectionId} to restaurant {restId}");
            SendMesasToPlayer(conn, restId);
        }
        else
        {
            Debug.LogWarning($"[MyRoomManager] conn {conn.connectionId} already registered for {restId} — skipping duplicate resync");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"[MyRoomManager] Removing conn {conn.connectionId} from restaurantConnections");

        List<string> keysToRemove = new List<string>();

        foreach (var key in restaurantConnections.Keys)
        {
            if (restaurantConnections[key].Contains(conn))
            {
                restaurantConnections[key].Remove(conn);
                Debug.Log($"[MyRoomManager] Removed connection {conn.connectionId} from RestaurantID: {key}");
                if (restaurantConnections[key].Count == 0)
                    keysToRemove.Add(key);
                break;
            }
        }

        foreach (var key in keysToRemove)
        {
            restaurantConnections.Remove(key);
            Debug.Log($"[MyRoomManager] Removed empty entry for RestaurantID: {key}");
        }

        base.OnServerDisconnect(conn);
    }

    // This handles the editor Stop button case
    public override void OnApplicationQuit()
    {
        if (NetworkClient.isConnected || NetworkClient.active)
            StopClient();

        if (NetworkServer.active)
            StopServer();

        NetworkClient.Shutdown();
    }

    public void SendMesasToPlayer(NetworkConnectionToClient conn, string restaurantId)
    {
        Debug.Log($"[MyRoomManager] Sending mesas to connection {conn.connectionId} for restaurant {restaurantId}");

        if (conn.identity != null)
        {
            MyRoomPlayer player = conn.identity.GetComponent<MyRoomPlayer>();
            if (player == null)
            {
                Debug.LogWarning($"[MyRoomManager] No MyRoomPlayer on conn {conn.connectionId}");
                return;
            }

            // --- Get COLORS ---
            if (!MesaStateManager.instance.TryGetMesaColorStates(restaurantId, out var mesaColorDict))
            {
                //Debug.LogWarning($"No mesas (colors) found for restaurant {restaurantId}");
                mesaColorDict = new Dictionary<int, MesaColorType>();
            }

            // --- Get CONTENTS ---
            if (!MesaStateManager.instance.TryGetContentStates(restaurantId, out var contentDict))
            {
                //Debug.LogWarning($"No mesas (content) found for restaurant {restaurantId}");
                contentDict = new Dictionary<int, MesaData>();
            }

            // --- Get CONTENTS PREVIA ---
            if (!MesaStateManager.instance.TryGetContentPreviaStates(restaurantId, out var contentDictPrevia))
            {
                //Debug.LogWarning($"No mesas previa (content) found for restaurant {restaurantId}");
                contentDictPrevia = new Dictionary<int, MesaDataPrevia>();
            }

            // --- Merge mesas (colors + content only) ---
            HashSet<int> mesaSet = new HashSet<int>(mesaColorDict.Keys);
            mesaSet.UnionWith(contentDict.Keys);

            int[] mesaNumbers = new int[mesaSet.Count];
            MesaColorType[] mesaColors = new MesaColorType[mesaSet.Count];
            MesaData[] mesaContents = new MesaData[mesaSet.Count];

            int index = 0;
            foreach (int mesa in mesaSet)
            {
                mesaNumbers[index] = mesa;

                if (!mesaColorDict.TryGetValue(mesa, out mesaColors[index]))
                    mesaColors[index] = MesaColorType.Default;

                if (!contentDict.TryGetValue(mesa, out mesaContents[index]))
                    mesaContents[index] = new MesaData(0, new string[0], new string[0], new string[0], new string[0], new int[0], new string[0], new int[0]);

                if (mesaContents[index].notaPlato == null)
                    mesaContents[index].notaPlato = new string[mesaContents[index].nEspacios];

                if (mesaContents[index].ordenPlato == null)
                    mesaContents[index].ordenPlato = new int[mesaContents[index].nEspacios];

                index++;
            }

            // --- Handle previa mesas separately ---
            List<int> mesaNumbersPreviaList = new List<int>();
            List<MesaDataPrevia> mesaContentsPreviaList = new List<MesaDataPrevia>();

            foreach (var kvp in contentDictPrevia)
            {
                if (!mesaSet.Contains(kvp.Key)) // Only include if it's not already in mesaNumbers
                {
                    mesaNumbersPreviaList.Add(kvp.Key);
                    mesaContentsPreviaList.Add(kvp.Value);
                }
            }

            int[] mesaNumbersPrevia = mesaNumbersPreviaList.ToArray();
            MesaDataPrevia[] mesaContentsPrevia = mesaContentsPreviaList.ToArray();

            // --- Send combined RPC ---
            player.TargetReceiveMesaStates(conn, restaurantId, mesaNumbers, mesaNumbersPrevia, mesaColors, mesaContents, mesaContentsPrevia);
        }
    }

}

