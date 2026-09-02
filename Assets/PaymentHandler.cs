using UnityEngine;
using System;
using System.Text;
using TMPro;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class PaymentHandler : NetworkBehaviour
{
    public string id_payment;
    public string tableNumber;

    public static PaymentHandler Local;

    void Start()
    {
        if (isLocalPlayer)
        {
            Local = this;
        }
    }


    // Method to generate a unique random id_payment
    private string GenerateUniquePaymentId()
    {
        // Generate a GUID and convert it to a string
        string guidPart = Guid.NewGuid().ToString("N"); // "N" format gives a 32-character string without dashes

        // Get the current timestamp (seconds since the Unix epoch)
        string timestampPart = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Combine GUID part and timestamp part to form the id_payment
        return $"{guidPart}_{timestampPart}";
    }

    public void RedirectToPaymentPage(string method, string amountText) // metodo seleccionado y monto
    {
        try
        {
            // id_Text
            String idText = GameObject.FindGameObjectWithTag("textID").GetComponent<TMP_Text>().text;

            // tableNumber
            tableNumber = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;

            // Generate a unique id_payment before proceeding
            id_payment = GenerateUniquePaymentId();

            // Remove currency symbols and commas
            amountText = amountText.Replace(",", "").Replace(" €", "").Replace("Pagar ", "");

            // Encode the amountText, idText, tableNumber and method  in base64
            string encodedAmount = Convert.ToBase64String(Encoding.UTF8.GetBytes(amountText.ToString()));
            string encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(idText));
            string encodedTableNumber = Convert.ToBase64String(Encoding.UTF8.GetBytes(tableNumber));
            string encodedMethod = Convert.ToBase64String(Encoding.UTF8.GetBytes(method));

            Debug.Log(idText);

            // Construct the URL with the encoded amount, idText, tableNumber, and the unique id_payment as query parameters
            string paymentUrl = $"https://gastrali.com/client_payment/?amount={encodedAmount}&id={encodedId}&table_number={encodedTableNumber}&method={encodedMethod}&id_payment={id_payment}";

            // Open the URL in the default browser
            Application.OpenURL(paymentUrl);

        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing payment: " + ex.Message);
        }

        // Confirmar metodo de pago
        if (isLocalPlayer)
        {
            ConfirmarMetodoDePago(method, int.Parse(tableNumber));
        }
    }


    [Command]
    void ConfirmarMetodoDePago(string method, int nMesa)
    {
        // 1) Find the player's RestaurantID from the server's perspective
        MyRoomPlayer myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;
        Debug.Log($"[CmdEstoyListoToServer] Received data from client with RestaurantID: {restId}");

        // 2) Grab the MyRoomManager singleton
        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        // 3) Look up the list of connections for that restaurant
        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            Debug.Log($"[CmdEstoyListoToServer] Found {conns.Count} connection(s) for restaurant {restId}");
            // 4) Send a [TargetRpc] to each connection in that restaurant
            foreach (NetworkConnectionToClient c in conns)
            {
                Debug.Log($"[CmdEstoyListoToServer] Sending TargetRpc to connection {c.connectionId}");
                RpcNotificarMetodoDePago(c, method, nMesa);
            }
        }
        else
        {
            Debug.LogError($"[CmdEstoyListoToServer] No connections found for restaurant {restId}");
        }
    }


    [TargetRpc]
    void RpcNotificarMetodoDePago(NetworkConnectionToClient conn, string method, int nMesa)
    {
        String inputMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;

        if (SceneManager.GetActiveScene().name == "MobileScene" && !isServer && !Navigation.camarero && int.Parse(inputMesa) == nMesa) // solo clientes y misma mesa
        {
            if (method == "Equitativo") // Forzar equitativo
            {
                GameObject canvasEquitativamenteForzado = null;
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "EquitativamenteForzado") // Check by name
                    {
                        canvasEquitativamenteForzado = obj;
                        break;
                    }
                }

                canvasEquitativamenteForzado.SetActive(true);
                canvasEquitativamenteForzado.transform.parent.gameObject.SetActive(true);
            }
            else if (method == "Elegir") // Forzar elegir
            {
                GameObject canvasElegirForzado = null;
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "ElegirForzado") // Check by name
                    {
                        canvasElegirForzado = obj;
                        break;
                    }
                }

                canvasElegirForzado.SetActive(true);
                canvasElegirForzado.transform.parent.gameObject.SetActive(true);
            }
            else // "Todo" // Echarlos de la app
            {
                Debug.Log("2");
                GameObject canvasClose = null;
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "CanvasClose") // Check by name
                    {
                        Debug.Log("3");
                        canvasClose = obj;
                        break;
                    }
                }

                canvasClose.SetActive(true);

                StartCoroutine(DelayedDisconnect());
            }
        }
    }

    private IEnumerator DelayedDisconnect()
    {
        // Wait a few seconds to let the canvas be visible
        yield return new WaitForSeconds(1f);
        if (NetworkClient.active)
        {
            // Disconnect the client from the server.
            NetworkManager.singleton.StopClient();
        }
    }
}
