using Mirror;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MesaColorType
{
    Default,
    Yellow,
    Blue,
    Red,
    Grey
}

public class MesaStateManager : MonoBehaviour
{
    public static MesaStateManager instance;

    public string apiBase = "https://gastrali.tail634a78.ts.net";
    public string bridgeApiKey = "wuerjhakrguh7346873qkjrgbh985467uswfhiiargoiihy23r8yhrfnhrgq3lkm";
    private Dictionary<string, Dictionary<int, bool>> asistenciaActiveByRestaurant = new Dictionary<string, Dictionary<int, bool>>();

    private Dictionary<string, Dictionary<int, MesaColorType>> colorStatesByRestaurant = new Dictionary<string, Dictionary<int, MesaColorType>>();
    private Dictionary<string, Dictionary<int, MesaData>> mesaContentByRestaurant = new Dictionary<string, Dictionary<int, MesaData>>();
    private Dictionary<string, Dictionary<int, MesaDataPrevia>> mesaContentPreviaByRestaurant = new Dictionary<string, Dictionary<int, MesaDataPrevia>>();
    private Dictionary<string, int> nextBatchIdByMesa = new Dictionary<string, int>();

    private int GetNuevoBatchId(string restId, int mesaNumber)
    {
        string key = restId + "_" + mesaNumber;
        int current = nextBatchIdByMesa.TryGetValue(key, out int existingVal) ? existingVal + 1 : 0;
        nextBatchIdByMesa[key] = current;
        return current;
    }

    // Espacios Camarero
    public GameObject prefabEspacioCamarero;
    public GameObject prefabEspacioBarra;

    public GameObject prefabOptionPedido;
    public GameObject prefabOptionCocina;
    public GameObject imageNotificacion;

    // Bloqueo de TPV/Camarero cuando Camarero/TPV está cobrando
    private Dictionary<string, Dictionary<int, string>> pagoEnCursoByRestaurant = new Dictionary<string, Dictionary<int, string>>(); // 👈 AÑADIR, junto a los demás diccionarios

    // Intenta bloquear la mesa para un origen ("Camarero" o "TPV"). Devuelve false si ya la está cobrando el OTRO origen.
    public bool SetPagoEnCurso(string restId, int mesaNumber, string origen) // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] SetPagoEnCurso en servidor: restId={restId}, mesa={mesaNumber}, origen={origen}"); // 👈 AÑADIR temporal
        
        if (!pagoEnCursoByRestaurant.TryGetValue(restId, out var dict))
            dict = pagoEnCursoByRestaurant[restId] = new Dictionary<int, string>();

        if (dict.TryGetValue(mesaNumber, out string existente) && existente != origen)
        {
            Debug.Log($"[Server] SetPagoEnCurso RECHAZADO → Mesa {mesaNumber} ya la cobra {existente}, intentó {origen}");
            return false;
        }

        dict[mesaNumber] = origen;
        BroadcastPagoEnCurso(restId, mesaNumber, true, origen);
        return true;
    }

    public void ClearPagoEnCurso(string restId, int mesaNumber) // 👈 AÑADIR método completo
    {
        if (pagoEnCursoByRestaurant.TryGetValue(restId, out var dict))
            dict.Remove(mesaNumber);

        BroadcastPagoEnCurso(restId, mesaNumber, false, null);
    }

    public bool TryGetPagoEnCurso(string restId, int mesaNumber, out string origen) // 👈 AÑADIR método completo
    {
        origen = null;
        return pagoEnCursoByRestaurant.TryGetValue(restId, out var dict) && dict.TryGetValue(mesaNumber, out origen);
    }

    private void BroadcastPagoEnCurso(string restId, int mesaNumber, bool enCurso, string origen) // 👈 AÑADIR método completo
    {
        if (!(NetworkManager.singleton is MyRoomManager mgr) ||
            !mgr.restaurantConnections.TryGetValue(restId, out var connections))
        {
            Debug.LogWarning($"[PagoEnCurso] BroadcastPagoEnCurso: NO se encontraron conexiones para restId={restId}"); // 👈 AÑADIR temporal
            return;
        }
        Debug.Log($"[PagoEnCurso] Broadcast a {connections.Count} conexiones"); // 👈 AÑADIR temporal
        
        foreach (var conn in connections)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
                player.TargetUpdatePagoEnCurso(conn, mesaNumber, enCurso, origen);
        }
    }

    // Mesa
    public TMP_Text numeroMesaLocal;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(ClearStaleWebCacheOnBoot());
    }

    private IEnumerator ClearStaleWebCacheOnBoot()
    {
        using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get($"{apiBase}/mesa_state/all_keys"))
        {
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[MesaStateManager] Could not fetch cached mesa keys on boot: {req.error}");
                yield break;
            }

            var response = JsonUtility.FromJson<MesaStateKeysResponse>(req.downloadHandler.text);
            if (response?.keys == null) yield break;

            /* Debug.Log($"[MesaStateManager] Clearing {response.keys.Count} stale web cache entries on boot"); */
            foreach (var key in response.keys)
            {
                PushMesaStateToWeb(key.restaurant_id, key.mesa);
                yield return null; // spread requests across frames
            }
        }
    }
    //---------------------------------------------------------------------------//
    // COLOR
    private int GetColorPriority(MesaColorType color)
    {
        switch (color)
        {
            case MesaColorType.Blue: return 4;   // ya pagado
            case MesaColorType.Red: return 3;    // asistencia solicitada
            case MesaColorType.Yellow: return 2; // plato listo / pendiente
            case MesaColorType.Grey: return 1;   // entregado
            default: return 0;                   // Default
        }
    }

    public void SetMesaColor(string restaurantId, int mesaNumber, MesaColorType color, bool force = false)
    {
        if (!force && TryGetColorState(restaurantId, mesaNumber, out MesaColorType currentColor)
            && GetColorPriority(currentColor) > GetColorPriority(color))
        {
            Debug.Log($"[Server] SetMesaColor skipped (current {currentColor} outranks {color}) → Restaurant: {restaurantId}, Mesa: {mesaNumber}");
            return;
        }

        Debug.Log($"[Server] SetMesaColor → Restaurant: {restaurantId}, Mesa: {mesaNumber}, Color: {color}");

        if (!colorStatesByRestaurant.ContainsKey(restaurantId))
            colorStatesByRestaurant[restaurantId] = new Dictionary<int, MesaColorType>();

        colorStatesByRestaurant[restaurantId][mesaNumber] = color;

        if (!(NetworkManager.singleton is MyRoomManager mgr) ||
            !mgr.restaurantConnections.TryGetValue(restaurantId, out var connections) ||
            connections.Count == 0)
        {
            Debug.LogWarning($"[Server] No valid connections for restaurantId {restaurantId}.");
            return;
        }

        foreach (var conn in connections)
        {
            if (conn?.isReady != true) continue;

            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
            {
                player.TargetUpdateMesaColor(conn, mesaNumber, color);
            }
        }
    }
    // CONTENT
    public void SetMesaContent(string restId, int mesaNumber, MesaData newData)
    {
        // 1. Make sure the restaurant exists
        if (!mesaContentByRestaurant.TryGetValue(restId, out var mesaDict))
            mesaDict = mesaContentByRestaurant[restId] = new Dictionary<int, MesaData>();

        // 2. Get current data (if any)
        if (!mesaDict.TryGetValue(mesaNumber, out var existing))
            existing = new MesaData(); // This gives empty arrays if your constructor is safe

        // Asignar un batchId estable (uno por pedido real) a todos los platos de este pedido
        int batchId = GetNuevoBatchId(restId, mesaNumber);
        newData.batchIdPlato = new int[newData.nEspacios];
        for (int bi = 0; bi < newData.nEspacios; bi++) newData.batchIdPlato[bi] = batchId;

        // 3. Append new platos
        existing.nombrePlatoString = existing.nombrePlatoString.Concat(newData.nombrePlatoString).ToArray();
        existing.opcionesPlato = existing.opcionesPlato.Concat(newData.opcionesPlato).ToArray();
        existing.cantidadPlatoString = existing.cantidadPlatoString.Concat(newData.cantidadPlatoString).ToArray();
        existing.precioPlatoString = existing.precioPlatoString.Concat(newData.precioPlatoString).ToArray();
        existing.togglePlato = existing.togglePlato.Concat(newData.togglePlato).ToArray();
        existing.nEspacios += newData.nEspacios;

        existing.estadoPlato = existing.estadoPlato.Concat(newData.estadoPlato).ToArray(); 
        existing.notaPlato = existing.notaPlato.Concat(newData.notaPlato).ToArray();
        existing.ordenPlato = existing.ordenPlato.Concat(newData.ordenPlato).ToArray();
        existing.batchIdPlato = existing.batchIdPlato.Concat(newData.batchIdPlato).ToArray();

        // 4. Save back
        mesaDict[mesaNumber] = existing;
        PushMesaStateToWeb(restId, mesaNumber);

        // Broadcast to all clients in that restaurant
        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restId, out var conns))
            return;

        foreach (var conn in conns)
        {
            if (conn?.isReady != true) continue;

            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
            {
                player.TargetUpdateContentMesa(conn, mesaNumber, newData.nEspacios, newData.nombrePlatoString, newData.opcionesPlato, newData.cantidadPlatoString, newData.precioPlatoString, newData.togglePlato, newData.notaPlato, newData.ordenPlato, newData.batchIdPlato);

            }
        }
    }

    public void SetMesaContentPrevia(string restId, int mesaNumber, MesaDataPrevia newData, NetworkConnectionToClient senderConn = null)
    {
        // 1. Make sure the restaurant exists
        if (!mesaContentPreviaByRestaurant.TryGetValue(restId, out var mesaDict))
        {
            mesaDict = mesaContentPreviaByRestaurant[restId] = new Dictionary<int, MesaDataPrevia>();
        }

        // 2. Get current data (if any)
        if (!mesaDict.TryGetValue(mesaNumber, out var existing))
        {
            existing = new MesaDataPrevia();
        }

        // 3. Append new platos
        existing.nombrePlatoString = existing.nombrePlatoString.Concat(newData.nombrePlatoString).ToArray();
        existing.opcionesPlato = existing.opcionesPlato.Concat(newData.opcionesPlato).ToArray();
        existing.cantidadPlatoString = existing.cantidadPlatoString.Concat(newData.cantidadPlatoString).ToArray();
        existing.precioPlatoString = existing.precioPlatoString.Concat(newData.precioPlatoString).ToArray();
        existing.togglePlato = existing.togglePlato.Concat(newData.togglePlato).ToArray();
        int ownerId = senderConn.connectionId;
        existing.ownerConnectionId = existing.ownerConnectionId.Concat(new int[] { ownerId }).ToArray();
        // Save back
        mesaDict[mesaNumber] = existing;
        PushMesaStateToWeb(restId, mesaNumber);

        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restId, out var conns))
            return;

        foreach (var conn in conns)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
            {
                player.TargetUpdateContentMesaPrevia(conn, mesaNumber, existing.ownerConnectionId, existing.nombrePlatoString, existing.opcionesPlato, existing.cantidadPlatoString, existing.precioPlatoString, existing.togglePlato);
            }
        }
    }

    // UPDATE PLATO PREVIA
    public void UpdateMesaPlatoCantidad(string restId, int mesaNumber, int connId, string nombrePlato, string opciones, string newCantidad, string newPrecio, NetworkConnectionToClient senderConn = null)
    {
        if (!mesaContentPreviaByRestaurant.TryGetValue(restId, out var mesaDict)) return;
        if (!mesaDict.TryGetValue(mesaNumber, out var mesa)) return;

        Debug.Log($"[UpdateMesaPlatoCantidad] Looking for connId={connId} nombrePlato={nombrePlato} opciones={opciones}");
        for (int i = 0; i < mesa.ownerConnectionId.Length; i++)
        {
            Debug.Log($"[UpdateMesaPlatoCantidad] Entry[{i}] connId={mesa.ownerConnectionId[i]} nombre={mesa.nombrePlatoString[i]} opciones={mesa.opcionesPlato[i]}");
        }

        int arrayPos = -1;
        for (int i = 0; i < mesa.ownerConnectionId.Length; i++)
        {
            if (mesa.ownerConnectionId[i] == connId && mesa.nombrePlatoString[i] == nombrePlato && mesa.opcionesPlato[i] == opciones)
            {
                arrayPos = i;
                break;
            }
        }
        if (arrayPos < 0)
        {
            Debug.LogWarning($"[UpdateMesaPlatoCantidad] dish not found for connId={connId} in mesa {mesaNumber}");
            return;
        }

        mesa.cantidadPlatoString[arrayPos] = newCantidad;
        mesa.precioPlatoString[arrayPos] = newPrecio;
        mesaDict[mesaNumber] = mesa;
        PushMesaStateToWeb(restId, mesaNumber);

        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restId, out var conns)) return;

        foreach (var conn in conns)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
                player.TargetUpdateContentMesaPrevia(conn, mesaNumber, mesa.ownerConnectionId, mesa.nombrePlatoString, mesa.opcionesPlato, mesa.cantidadPlatoString, mesa.precioPlatoString, mesa.togglePlato);
        }
    }

    // DELETE PLATO FROM PREVIA
    public void DeletePlatoFromMesa(string restId, int mesaNumber, int connId, string nombrePlato, string opciones, NetworkConnectionToClient senderConn = null)
    {
        if (!mesaContentPreviaByRestaurant.TryGetValue(restId, out var mesaDict)) return;
        if (!mesaDict.TryGetValue(mesaNumber, out var data)) return;

        int arrayPos = -1;
        for (int i = 0; i < data.ownerConnectionId.Length; i++)
        {
            if (data.ownerConnectionId[i] == connId && data.nombrePlatoString[i] == nombrePlato && data.opcionesPlato[i] == opciones)
            {
                arrayPos = i;
                break;
            }
        }
        if (arrayPos < 0)
        {
            Debug.LogWarning($"[DeletePlatoFromMesa] dish not found for connId={connId} in mesa {mesaNumber}");
            return;
        }

        data.nombrePlatoString = data.nombrePlatoString.Where((_, i) => i != arrayPos).ToArray();
        data.opcionesPlato = data.opcionesPlato.Where((_, i) => i != arrayPos).ToArray();
        data.cantidadPlatoString = data.cantidadPlatoString.Where((_, i) => i != arrayPos).ToArray();
        data.precioPlatoString = data.precioPlatoString.Where((_, i) => i != arrayPos).ToArray();
        data.togglePlato = data.togglePlato.Where((_, i) => i != arrayPos).ToArray();
        data.ownerConnectionId = data.ownerConnectionId.Where((_, i) => i != arrayPos).ToArray();
        mesaDict[mesaNumber] = data;
        PushMesaStateToWeb(restId, mesaNumber);

        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restId, out var conns)) return;

        foreach (var conn in conns)
        {
            if (conn?.isReady != true) continue;
            MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
            if (player != null)
                player.TargetUpdateContentMesaPrevia(conn, mesaNumber, data.ownerConnectionId, data.nombrePlatoString, data.opcionesPlato, data.cantidadPlatoString, data.precioPlatoString, data.togglePlato);
        }
    }


    //---------------------------------------------------------------------------//
    // COLOR
    public void SetLocalMesaColor(string restaurantId, int mesaNumber, MesaColorType color)
    {
        if (!colorStatesByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            mesaDict = new Dictionary<int, MesaColorType>();
            colorStatesByRestaurant[restaurantId] = mesaDict;
        }

        mesaDict[mesaNumber] = color;
    }
    // CONTENT
    public void SetLocalMesaContent(string restaurantId, int mesaNumber, MesaData data)
    {
        Debug.Log($"[SetLocalMesaContent] Storing restId='{restaurantId}' mesa={mesaNumber}");

        if (!mesaContentByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            mesaDict = new Dictionary<int, MesaData>();
            mesaContentByRestaurant[restaurantId] = mesaDict;
        }

        mesaDict[mesaNumber] = data;
    }
    // CONTENT PREVIA
    public void SetLocalMesaContentPrevia(string restaurantId, int mesaNumber, MesaDataPrevia dataPrevia)
    {
        if (!mesaContentPreviaByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            mesaDict = new Dictionary<int, MesaDataPrevia>();
            mesaContentPreviaByRestaurant[restaurantId] = mesaDict;
        }

        mesaDict[mesaNumber] = dataPrevia;
    }

    //---------------------------------------------------------------------------//
    // COLOR
    public bool TryGetColorState(string restaurantId, int mesaNumber, out MesaColorType color)
    {
        color = MesaColorType.Default;

        if (colorStatesByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            return mesaDict.TryGetValue(mesaNumber, out color);
        }

        return false;
    }
    // CONTENT
    public bool TryGetContentState(string restaurantId, int mesaNumber, out MesaData mesaData)
    {
        mesaData = null;

        if (mesaContentByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            return mesaDict.TryGetValue(mesaNumber, out mesaData);
        }

        return false;
    }
    // CONTENT PREVIA
    public bool TryGetContentPreviaState(string restaurantId, int mesaNumber, out MesaDataPrevia mesaDataPrevia)
    {
        mesaDataPrevia = null;

        if (mesaContentPreviaByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            return mesaDict.TryGetValue(mesaNumber, out mesaDataPrevia);
        }

        return false;
    }
    //---------------------------------------------------------------------------//
    // COLOR
    public bool TryGetMesaColorStates(string restaurantId, out Dictionary<int, MesaColorType> mesaDict)
    {
        return colorStatesByRestaurant.TryGetValue(restaurantId, out mesaDict);
    }
    // CONTENT
    public bool TryGetContentStates(string restaurantId, out Dictionary<int, MesaData> mesaDict)
    {
        return mesaContentByRestaurant.TryGetValue(restaurantId, out mesaDict);
    }
    // CONTENT PREVIA
    public bool TryGetContentPreviaStates(string restaurantId, out Dictionary<int, MesaDataPrevia> mesaDictPrevia)
    {
        return mesaContentPreviaByRestaurant.TryGetValue(restaurantId, out mesaDictPrevia);
    }


    public void ResetMesaContentPrevia(string restaurantId, int mesaNumber)
    {
        if (mesaContentPreviaByRestaurant.TryGetValue(restaurantId, out var mesaDict))
            mesaDict.Remove(mesaNumber);
        PushMesaStateToWeb(restaurantId, mesaNumber);
    }

    public void ResetMesaContent(string restaurantId, int mesaNumber)
    {
        SetMesaColor(restaurantId, mesaNumber, MesaColorType.Default, force: true);
        SetAsistenciaActive(restaurantId, mesaNumber, false);

        if (mesaContentByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        {
            if (mesaDict.Remove(mesaNumber))
            {
                Debug.Log($"[Server] Reset mesa content for Restaurant: {restaurantId}, Mesa: {mesaNumber}");
                PushMesaStateToWeb(restaurantId, mesaNumber, isReset: true);

                // Notify clients about the reset, e.g. send empty MesaData or a reset command
                if (NetworkManager.singleton is MyRoomManager mgr &&
                    mgr.restaurantConnections.TryGetValue(restaurantId, out var connections))
                {
                    foreach (var conn in connections)
                    {
                        if (conn?.isReady != true) continue;

                        MyRoomPlayer player = conn.identity?.GetComponent<MyRoomPlayer>();
                        if (player != null)
                        {
                            player.TargetUpdateContentMesa(conn, mesaNumber, 0, new string[0], new string[0], new string[0], new string[0], new int[0], new string[0], new int[0], new int[0]);
                        }
                    }
                }
            }
        }
    }

    public void UpdateDishState(string restaurantId, int mesaNumber, int batchId, int localIndex, string nombrePlato, int cantidadPlato, string opciones, int newState, NetworkConnectionToClient senderConn = null)
    {
        if (!mesaContentByRestaurant.TryGetValue(restaurantId, out var mesaDict))
        { Debug.LogWarning("[UpdateDishState] Restaurant not found!"); return; }

        if (!mesaDict.TryGetValue(mesaNumber, out var data))
        { Debug.LogWarning($"[UpdateDishState] Mesa {mesaNumber} not found!"); return; }

        int dishIndex = ResolveGlobalIndex(data.batchIdPlato, batchId, localIndex);
        if (dishIndex < 0 || dishIndex >= data.estadoPlato.Length)
        { Debug.LogWarning($"[UpdateDishState] Could not resolve dishIndex for batchId={batchId} localIndex={localIndex} (mesa {mesaNumber})"); return; }

        data.estadoPlato[dishIndex] = newState;
        mesaDict[mesaNumber] = data;

        if (newState == 3 && !data.estadoPlato.Any(s => s == 2))
        {
            Debug.Log($"[UpdateDishState] No more ready dishes → Restaurant: {restaurantId}, Mesa: {mesaNumber}, forcing Grey");
            SetMesaColor(restaurantId, mesaNumber, MesaColorType.Grey, force: true);
        }

        PushMesaStateToWeb(restaurantId, mesaNumber);

        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restaurantId, out var conns))
        { Debug.LogWarning("[UpdateDishState] No connections found!"); return; }

        foreach (var conn in conns)
        {
            if (conn.isReady)
            {
                Debug.Log($"[UpdateDishState] Sending TargetRpc to conn {conn.connectionId}");
                MyRoomPlayer player = conn.identity.GetComponent<MyRoomPlayer>();
                player.TargetUpdateDishState(conn, mesaNumber, batchId, localIndex, nombrePlato, cantidadPlato, opciones, newState);
            }
        }
    }

    private int ResolveGlobalIndex(int[] batchIdPlato, int batchId, int localIndex)
    {
        if (batchIdPlato == null) return localIndex; // fallback datos legacy sin batch tracking

        int count = 0;
        for (int i = 0; i < batchIdPlato.Length; i++)
        {
            if (batchIdPlato[i] == batchId)
            {
                if (count == localIndex) return i;
                count++;
            }
        }
        return -1;
    }

    // Incoming pedido from the web app or from MyPlayerController
    public void ProcessIncomingPedido(string restId, int mesaNumber, int nEspacios, string[] nombrePlatoString, string[] opcionesPlato, string[] cantidadPlatoString, string[] precioPlatoString, int[] togglePlato, string[] notaPlato, int[] ordenPlato)
    {
        var data = new MesaData(nEspacios, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato, notaPlato, ordenPlato);

        for (int i = 0; i < nEspacios; i++)
        {
            if (togglePlato[i] == -1)
                data.estadoPlato[i] = 3; // treat Varios as already "delivered" — never appears as pending
            else if (togglePlato[i] == 0)
                data.estadoPlato[i] = 2;
        }

        SetMesaContent(restId, mesaNumber, data);
        ResetMesaContentPrevia(restId, mesaNumber);

        for (int i = 0; i < nEspacios; i++)
            if (togglePlato[i] == 0) // -1 (Varios) is skipped, never broadcast to cocina/camarero
                UpdateDishState(restId, mesaNumber, data.batchIdPlato[i], i, nombrePlatoString[i], int.Parse(cantidadPlatoString[i]), opcionesPlato[i], 2);

        bool hasToggleZero = System.Array.Exists(togglePlato, t => t == 0); // -1 no longer matches, so Varios never triggers Yellow
        if (hasToggleZero)
            SetMesaColor(restId, mesaNumber, MesaColorType.Yellow);
        else
        {
            TryGetColorState(restId, mesaNumber, out MesaColorType currentColor);
            if (currentColor != MesaColorType.Yellow)
                SetMesaColor(restId, mesaNumber, MesaColorType.Grey);
        }
    }

    public void PushMesaStateToWeb(string restId, int mesaNumber, bool isReset = false)
    {
        StartCoroutine(PushMesaStateCoroutine(restId, mesaNumber, isReset));
    }

    private IEnumerator PushMesaStateCoroutine(string restId, int mesaNumber, bool isReset = false)
    {
        MesaDataPrevia previa = null;
        MesaData confirmed = null;

        if (mesaContentPreviaByRestaurant.TryGetValue(restId, out var previaDict))
            previaDict.TryGetValue(mesaNumber, out previa);

        if (mesaContentByRestaurant.TryGetValue(restId, out var contentDict))
            contentDict.TryGetValue(mesaNumber, out confirmed);

        var payload = new MesaStatePayload
        {
            restaurant_id = restId,
            mesa = mesaNumber,
            previa = previa == null ? new WebDishState[0] : ToWebDishState(previa.nombrePlatoString, previa.opcionesPlato, previa.cantidadPlatoString, previa.precioPlatoString),
            confirmed = confirmed == null ? new WebDishStatus[0] : ToWebDishStatus(confirmed),
            asistencia_active = IsAsistenciaActive(restId, mesaNumber),
            is_reset = isReset
        };
        string json = JsonUtility.ToJson(payload);

        using (UnityEngine.Networking.UnityWebRequest req = new UnityEngine.Networking.UnityWebRequest($"{apiBase}/mesa_state/push", "POST"))
        {
            req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Bridge-Key", bridgeApiKey);
            req.timeout = 5;
            yield return req.SendWebRequest();
        }
    }

    private WebDishState[] ToWebDishState(string[] nombre, string[] opciones, string[] cantidad, string[] precio)
    {
        var list = new List<WebDishState>();
        for (int i = 0; i < nombre.Length; i++)
            list.Add(new WebDishState { name = nombre[i], options = opciones[i], quantity = cantidad[i], price = precio[i] });
        return list.ToArray();
    }

    private WebDishStatus[] ToWebDishStatus(MesaData data)
    {
        var list = new List<WebDishStatus>();
        for (int i = 0; i < data.nombrePlatoString.Length; i++)
            list.Add(new WebDishStatus { name = data.nombrePlatoString[i], options = data.opcionesPlato[i], quantity = data.cantidadPlatoString[i], price = data.precioPlatoString[i], state = i < data.estadoPlato.Length ? data.estadoPlato[i] : 0 });
        return list.ToArray();
    }

    public void RequestAsistencia(string restId, int mesaNumber)
    {
        var mgr = NetworkManager.singleton as MyRoomManager;
        if (mgr == null || !mgr.restaurantConnections.TryGetValue(restId, out var conns)) return;

        TryGetColorState(restId, mesaNumber, out MesaColorType previousColor);

        foreach (var conn in conns)
        {
            if (conn?.isReady != true) continue;
            MyPlayerController playerController = conn.identity?.GetComponent<MyPlayerController>();
            if (playerController != null)
                playerController.TargetBroadcastAtencion(conn, (float)mesaNumber, previousColor);
        }

        SetMesaColor(restId, mesaNumber, MesaColorType.Red);
        SetAsistenciaActive(restId, mesaNumber, true);
        PushMesaStateToWeb(restId, mesaNumber);
    }

    public void SetAsistenciaActive(string restId, int mesaNumber, bool active)
    {
        if (!asistenciaActiveByRestaurant.TryGetValue(restId, out var dict))
            dict = asistenciaActiveByRestaurant[restId] = new Dictionary<int, bool>();
        dict[mesaNumber] = active;
    }

    public bool IsAsistenciaActive(string restId, int mesaNumber)
    {
        return asistenciaActiveByRestaurant.TryGetValue(restId, out var dict)
            && dict.TryGetValue(mesaNumber, out var active) && active;
    }

    T[] CombineArrays<T>(T[] a, T[] b)
    {
        T[] result = new T[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }
}


