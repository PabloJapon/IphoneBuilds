using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ConnectMirrorTPV : MonoBehaviour
{
    public TMP_Text idRestaurante;

    public void LoginStart()
    {
        // Ver si el cliente se ha iniciado ya
        var roomManager = FindObjectOfType<MyRoomManager>();
        if (roomManager != null && !NetworkClient.isConnected && !NetworkClient.active)
        {
            roomManager.StartClient();
        }
        StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante.text));
        StartCoroutine(StartClientWithDelay());
    }

    private IEnumerator WaitForLocalPlayerAndSendID(string restaurantID)
    {
        // Wait until NetworkClient.connection and its identity are available.
        while (NetworkClient.connection == null || NetworkClient.connection.identity == null)
        {
            //Debug.Log("Waiting for local player to spawn...");
            yield return new WaitForSeconds(0.1f);
        }

        // Once the local player is available:
        MyRoomPlayer roomPlayer = NetworkClient.connection.identity.GetComponent<MyRoomPlayer>();
        if (roomPlayer != null && roomPlayer.isLocalPlayer)
        {
            Debug.Log($"[WaitForLocalPlayerAndSendID] Sending RestaurantID: {restaurantID}");
            roomPlayer.CmdSetRestaurantID(restaurantID);
        }
        else
        {
            Debug.Log("[WaitForLocalPlayerAndSendID] MyRoomPlayer not found or not local even after waiting.");
        }
    }

    private IEnumerator StartClientWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Adjust delay if needed
        //Debug.Log("[StartClientWithDelay] Starting client after delay");
        // Ver si el cliente se ha iniciado ya
        var roomManager = FindObjectOfType<MyRoomManager>();
        if (roomManager != null && !NetworkClient.isConnected && !NetworkClient.active)
        {
            roomManager.StartClient();
        }
    }
}
