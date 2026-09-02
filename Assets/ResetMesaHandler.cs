using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ResetMesaHandler : NetworkBehaviour
{
    private GameObject ContentPedido;
    private MenuPedir MP;
    private GameObject ContentPagar;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MobileScene" || SceneManager.GetActiveScene().name == "TPVScene")
        {
            MP = GameObject.Find("MenuPedir").GetComponent<MenuPedir>();
            ContentPedido = GameObject.FindWithTag("contentPedido");
            ContentPagar = GameObject.FindWithTag("contentPagar");
        }
    }

    [Command]
    public void CmdResetearMesa(string id, int tableNumber)
    {
        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        if (manager.restaurantConnections.TryGetValue(id, out List<NetworkConnectionToClient> conns))
        {
            Debug.Log($"[CmdResetearMesa] Found {conns.Count} connection(s) for restaurant {id}");
            foreach (NetworkConnectionToClient c in conns)
            {
                //Debug.Log($"[CmdResetearMesa] Sending TargetRpc to connection {c.connectionId}");
                TargetClearOrderItems(c, tableNumber);
            }
            //Debug.Log("numero mesa en CmdResearMesa: "+tableNumber+" y la id "+id);
        }
        else
        {
            Debug.LogError($"[CmdResetearMesa] No connections found for restaurant {id}");
        }

        // Reset Mesa MesaData - Revisar el target porque alomejor se simplifica
        MyRoomPlayer myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;

        MesaStateManager.instance.ResetMesaContent(restId, (int)tableNumber);
    }

    [TargetRpc]
    private void TargetClearOrderItems(NetworkConnection target, int tableNumber)
    {
        if (SceneManager.GetActiveScene().name == "MobileScene" || SceneManager.GetActiveScene().name == "TPVScene")
        {
            // ── 1. Clear dishes via dictionary ──
            if (CrearCamarero.mesasDictionary.TryGetValue(tableNumber, out GameObject scrollPanel))
            {
                Transform content = scrollPanel.transform
                    .GetChild(0)
                    .GetChild(0)
                    .GetChild(0);

                var toDestroy = new List<GameObject>();
                foreach (Transform child in content)
                    toDestroy.Add(child.gameObject);
                foreach (var go in toDestroy)
                    Destroy(go);
            }
            else
            {
                Debug.LogWarning($"[Reset] ScrollMesa {tableNumber} not in mesasDictionary");
            }

            // ── 2. Reset buttons ──
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {
                CrearCamarero crearCamarero = FindObjectOfType<CrearCamarero>();
                crearCamarero.ResetMesaButtonsInteractable(tableNumber);
            }
            else
            {
                if (CrearCamarero.mesasDictionary.TryGetValue(tableNumber, out GameObject panel))
                {
                    panel.transform.GetChild(2).gameObject.SetActive(true);
                    panel.transform.GetChild(3).gameObject.SetActive(true);
                    panel.transform.GetChild(4).gameObject.SetActive(false);
                    panel.transform.GetChild(3).GetComponent<Button>().interactable = false;
                }
            }

            // ── 3. Reset button color ──
            if (CrearCamarero.buttonMesaDictionary.TryGetValue(tableNumber, out GameObject buttonObject))
            {
                if (tableNumber > 999)
                {
                    Destroy(buttonObject);
                }
                else
                {
                    var image = buttonObject.GetComponent<Image>();
                    var text = buttonObject.GetComponentInChildren<TMP_Text>();
                    if (image != null) image.color = ColorUtility.TryParseHtmlString("#F0F0F0", out Color c) ? c : Color.white;
                    if (text != null) text.color = ColorUtility.TryParseHtmlString("#323232", out Color c2) ? c2 : Color.black;
                }
            }
            else
            {
                Debug.LogWarning("No button found for mesa " + tableNumber);
            }

            // ── 4. Clear client order content ──
            ContentPedido = GameObject.FindGameObjectWithTag("contentPedido");
            if (ContentPedido == null)
            {
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.CompareTag("contentPedido"))
                    {
                        ContentPedido = obj;
                        break;
                    }
                }
            }

            foreach (Transform child in ContentPedido.transform)
            {
                if (!child.name.Contains("Total"))
                    Destroy(child.gameObject);
            }

            MP.totalSum = 0;
            MP.precioTotal.SetActive(false);
            MP.precioTotal2.SetActive(false);
            MP.primerPedidoHecho = false;
            MP.primerPedidoHecho2 = false;

            if (SceneManager.GetActiveScene().name == "MobileScene" && Navigation.camarero == false)
            {
                foreach (Transform child in ContentPagar.transform)
                    Destroy(child.gameObject);
            }
        }

        // ── 5. Disconnect client if it's their table ──
        if (SceneManager.GetActiveScene().name == "MobileScene" && !Navigation.camarero && !isServer)
        {
            int mesaNumber = int.Parse(GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text);
            if (mesaNumber == tableNumber)
            {
                GameObject canvasClose = null;
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "CanvasClose")
                    {
                        canvasClose = obj;
                        break;
                    }
                }

                if (canvasClose != null)
                    canvasClose.SetActive(true);
                else
                    Debug.LogWarning("CanvasClose not found in scene.");

                StartCoroutine(DelayedDisconnect());
            }
        }
        else if (SceneManager.GetActiveScene().name == "CocinaScene")
        {
            GameObject contentCocina = GameObject.FindWithTag("contentCocina");
            if (contentCocina != null)
            {
                foreach (Transform child in contentCocina.transform)
                {
                    if (child.name.Contains("CocinaComandaMesa" + tableNumber))
                        Destroy(child.gameObject);
                }
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
