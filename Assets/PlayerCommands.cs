using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerCommands : NetworkBehaviour
{
    private GameObject objectMesa;
    private GameObject objectButtonMesa;
    private GameObject content;
    private GameObject[] platosEspacios;

    [Command]
    public void CmdSendRecogerToServer(int valueMesa, int quantity, string dishName)
    {
        RpcBroadcastRecoger(valueMesa, quantity, dishName);
    }

    [ClientRpc]
    void RpcBroadcastRecoger(int valueMesa, int quantity, string dishName)
    {
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            // CAMARERO
            // Get the table object
            CrearCamarero.mesasDictionary.TryGetValue(valueMesa, out objectMesa);

            // Get the content GameObject (parent of all dish slots)
            Transform abuelo = objectMesa.transform.GetChild(0); // este es el abuelo de content: Sroll View
            Transform padreDePlatos = abuelo.GetChild(0); // padre de content
            content = padreDePlatos.GetChild(0).gameObject; // Content

            Debug.Log("content: "+content.name);

            // Get all dish slots (child GameObjects)
            platosEspacios = new GameObject[content.transform.childCount];

            for (int i = 0; i < content.transform.childCount; i++)
            {
                platosEspacios[i] = content.transform.GetChild(i).gameObject;
            }

            // vamos a buscar el cuadradito de la mesa para cambiarle el color a amarillo tb (SI NO ESTÁ EN ROJO)
            CrearCamarero.buttonMesaDictionary.TryGetValue(valueMesa, out objectButtonMesa);

            // Loop through all dish slots and check for the correct dish name
            foreach (GameObject slot in platosEspacios)
            {
                TMP_Text[] textComponents = slot.GetComponentsInChildren<TMP_Text>();
                // Sacamos la posicion del slot, para que al tachar se tache el que se tiene que tachar

                if (textComponents.Length >= 2)
                {
                    string dishText = textComponents[0].text;  // First TMP_Text (Dish Name)
                    TMP_Text dishText2 = textComponents[0]; // para coger el color del texto y ver que no esté tachado
                    Color dishTextColor = dishText2.color;
                    Color gris;
                    int slotQuantity = int.Parse(textComponents[1].text);  // Second TMP_Text (Quantity)

                    Debug.Log("plato: "+dishText+", plato que llega"+dishName);
                    // para que no se raye cuando haya dos platos y cantidades iguales, vamos a hacer que no tenga en cuenta los que ya estén tachados
                    // ademas que no tenga en cuenta los que ya estan en amarillo, por si hay varios
                    if (ColorUtility.TryParseHtmlString("#c3c3c4", out gris))
                    {
                        Color newColor;
                        Color newColor2;
                        if (ColorUtility.TryParseHtmlString("#FFC368", out newColor))
                        {
                            if (dishText == dishName && slotQuantity == quantity && dishTextColor != gris && slot.GetComponentInChildren<Image>().color != newColor)
                            {
                                slot.GetComponentInChildren<Image>().color = newColor;
                                // ponemos tb en amarillo el cuadradito de la mesa (SI NO ESTÁ EN ROJO)
                                if (ColorUtility.TryParseHtmlString("#CA0000", out newColor2))
                                {
                                    if (objectButtonMesa.GetComponent<Image>().color !=newColor2)
                                    {
                                        objectButtonMesa.GetComponent<Image>().color = newColor;
                                    }
                                }
                                // Detener la búsqueda si encontramos una coincidencia, solo se quede con el primero
                                break; 
                            }
                        }
                            
                    }
                }
            }

            // CLIENTE
            string textNumeroMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;

            if (Navigation.camarero == false && valueMesa == int.Parse(textNumeroMesa))
            {
                foreach (GameObject plato in MyPlayerController.platosPedidosDictionary.Values)
                {
                    TMP_Text[] tmpTexts = plato.GetComponentsInChildren<TMP_Text>();

                    if (tmpTexts.Length >= 2 && tmpTexts[0].text == dishName && tmpTexts[2].text == quantity.ToString() && tmpTexts[3].text == "En proceso")
                    {
                        tmpTexts[3].text = "En camino";
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSendEntregadoToServer(int valueMesa, int quantity, string dishName, int index)
    {
        // 1) Find the player's RestaurantID from the server's perspective
        MyRoomPlayer myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;
        Debug.Log($"[CmdSendPedidoToServer] Received data from client with RestaurantID: {restId}");

        // 2) Grab the MyRoomManager singleton
        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        // 3) Look up the list of connections for that restaurant
        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            Debug.Log($"[CmdSendPedidoToServer] Found {conns.Count} connection(s) for restaurant {restId}");
            // 4) Send a [TargetRpc] to each connection in that restaurant
            foreach (NetworkConnectionToClient c in conns)
            {
                Debug.Log($"[CmdSendPedidoToServer] Sending TargetRpc to connection {c.connectionId}");
                TargetBroadcastEntregado(c, valueMesa, quantity, dishName, index);
            }
        }
        else
        {
            Debug.LogError($"[CmdSendPedidoToServer] No connections found for restaurant {restId}");
        }
    }

    [TargetRpc]
    void TargetBroadcastEntregado(NetworkConnectionToClient conn, int valueMesa, int quantity, string dishName, int index)
    {
        bool algunoAmarillo = false;

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            // CAMARERO
            // Get the table object
            CrearCamarero.mesasDictionary.TryGetValue(valueMesa, out objectMesa);

            // Get the content GameObject (parent of all dish slots)
            content = objectMesa.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

            // Get all dish slots (child GameObjects)
            platosEspacios = new GameObject[content.transform.childCount];

            for (int i = 0; i < content.transform.childCount; i++)
            {
                platosEspacios[i] = content.transform.GetChild(i).gameObject;
            }

            bool updated = false;  // Flag to track if we've updated the dish already

            // Loop through all dish slots and check for the correct dish name
            
            foreach (GameObject slot in platosEspacios)
            {
                TMP_Text[] textComponents = slot.GetComponentsInChildren<TMP_Text>();
                // posicion del slot para tachar bien en camarero
                int n = slot.transform.GetSiblingIndex();

                if (textComponents.Length >= 2)
                {
                    string dishText = textComponents[0].text;  // First TMP_Text (Dish Name)
                    int slotQuantity = int.Parse(textComponents[1].text);  // Second TMP_Text (Quantity)                 
                    Color newColor; 
                    // al tachar no tenemos en cuenta los objetos ya tachada
                    if (ColorUtility.TryParseHtmlString("#c3c3c4", out newColor))
                    {
                        Debug.Log("n: "+n+", n que llega: "+index+". Nombre: "+dishText+", nombre que llega: ");
                        if (dishText == dishName && slotQuantity == quantity && textComponents[0].color != newColor && n==index)
                        {
                            if (!updated)  // Update the first matching dish
                            {
                                
                                textComponents[0].color = newColor;
                                textComponents[1].color = newColor;
                            
                                slot.GetComponentInChildren<Image>().color = Color.white;
                                updated = true;  // Mark as updated, so we only update the first match
                            }
                            // If this slot was already updated, skip it, but allow the second match to trigger
                            break;
                        }
                    }
                }
            }

            // Además, si al tachar ese plato no queda ningún otro en amarillo, que la mesa se ponga gris (si no está roja)
            // vamos a buscar el cuadradito de la mesa para cambiarle el color a amarillo tb (SI NO ESTÁ EN ROJO)
            CrearCamarero.buttonMesaDictionary.TryGetValue(valueMesa, out objectButtonMesa);

            foreach (GameObject slot in platosEspacios)
            {
                Color amarillo;
                if (ColorUtility.TryParseHtmlString("#FFC368", out amarillo))
                {
                    if(slot.GetComponentInChildren<Image>().color == amarillo)
                    {
                        algunoAmarillo = true; // todavia queda alguno en amarillo
                        Debug.Log("queda alguno en amarillo");
                    }
                }
            }
            if (algunoAmarillo == false) // si no hay platos en amarillo ponemos la mesa en gris
            {
                Debug.Log("no quedan en amarillo, debería ponerse gris");
                Color gris;
                Color rojo;
                if (ColorUtility.TryParseHtmlString("#F5F5F5", out gris))
                {
                    if (ColorUtility.TryParseHtmlString("#CA0000", out rojo))
                    {
                        if (objectButtonMesa.GetComponent<Image>().color !=rojo)
                        {
                            objectButtonMesa.GetComponent<Image>().color = gris;
                            // Cambiar el color del texto hijo a negro
                            TextMeshProUGUI textComponent = objectButtonMesa.GetComponentInChildren<TextMeshProUGUI>();
                            if (textComponent != null)
                            {
                                textComponent.color = Color.black;  // Cambiar el texto a negro
                            }
                        }
                    }
                }
            }

            // CLIENTE
            string textNumeroMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;

            if (Navigation.camarero == false && valueMesa == int.Parse(textNumeroMesa))
            {
                foreach (GameObject plato in MyPlayerController.platosPedidosDictionary.Values)
                {
                    TMP_Text[] tmpTexts = plato.GetComponentsInChildren<TMP_Text>();
                    Debug.Log("Nombre: "+tmpTexts[0].text+", nombre que llega: "+dishName+". Estado:"+tmpTexts[3].text);
                       
                    if (tmpTexts.Length >= 2 && tmpTexts[0].text == dishName && tmpTexts[2].text == quantity.ToString() && tmpTexts[3].text != "Entregado")
                    {
                        tmpTexts[3].text = "Entregado";
                        break;

                    }
                }
            }
        }
    }
}
