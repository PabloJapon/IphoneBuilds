using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Collections;

public class MyPlayerController : NetworkBehaviour
{
    [SerializeField] private float floatValue = 0f;
    public Color colorPedir;
    public Color colorAtender;
    public Button buttonPedir;
    private Button buttonAtender;
    private GameObject atenderConfirmado;
    private TMP_Text inputMesa;

    private string[] nombrePlatoString;
    private string[] cantidadPlatoString;
    private string[] precioPlatoString;
    private int[] togglePlato;
    private string[] opcionesPlato; 
    private string[] notaPlato;
    private int[] ordenPlato;
    public GameObject espacioCamarero;
    public GameObject espacioBarra;
    public GameObject pedidosRealizados;
    public GameObject platoPedido;

    // Alerta
    public GameObject alertaCamarero;
    public static Dictionary<int, GameObject> alertasDict = new Dictionary<int, GameObject>();

    // Alerta borrar plato camarero 
    public GameObject AdvertenciaBorrarPlato;

    // Pagar
    public GameObject pagarPlato;
    public GameObject pagarTotal;
    public GameObject añadirPropina;
    public GameObject contentPagar;
    public GameObject contentPedido;
    public float totalSum = 0f;
    public TMP_Text amountText;
    public PaymentHandler PH;

    private GameObject prefabTotalInstance;
    private GameObject prefabPropinaInstance;

    // Cocina
    public GameObject contentCocina;
    public GameObject contentComanda;
    public GameObject cocinaPrefabComanda;
    public GameObject cocinaPrefabEspacio;
    private bool createComanda = false;
    public GameObject nCocina;
    public GameObject prefabOptionCocina;

    // Arrays to store the quantities and dish names
    public int[] quantities;  // Stores the quantity of each dish
    public string[] dishNames;  // Stores the name of each dish
    private int iprevio;

    public MenuPedir MP;

    private bool accionCamarero = false;

    // Disccionario platos pedidos
    public static Dictionary<float, GameObject> platosPedidosDictionary = new Dictionary<float, GameObject>();
    private Dictionary<int, MesaColorType> previousColorDict = new Dictionary<int, MesaColorType>();

    public GameObject prefabOptionPedido;

    public override void OnStartLocalPlayer()
    {
        if (SceneManager.GetActiveScene().name == "MobileScene" || SceneManager.GetActiveScene().name == "TPVScene")
        {
            if (!isLocalPlayer)
                return;

            // Find buttons even if inactive
            Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

            foreach (Button btn in allButtons)
            {
                if (btn.CompareTag("buttonPedir"))
                {
                    buttonPedir = btn;
                }
                else if (btn.CompareTag("buttonAtender"))
                {
                    buttonAtender = btn;
                }

                if (buttonPedir != null && buttonAtender != null)
                    break;
            }

            // Find other UI elements, including inactive ones
            atenderConfirmado = GameObject.FindGameObjectWithTag("atenderConfirmado");
            if (atenderConfirmado == null)
            {
                GameObject[] allAtenderConfirmado = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allAtenderConfirmado)
                {
                    if (obj.CompareTag("atenderConfirmado"))
                    {
                        atenderConfirmado = obj;
                        break;
                    }
                }
            }

            inputMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>();
            contentPagar = GameObject.FindGameObjectWithTag("contentPagar");
            contentCocina = GameObject.FindGameObjectWithTag("contentCocina");
            AdvertenciaBorrarPlato = GameObject.FindGameObjectWithTag("AdvertenciaBorrarPlato");
            AdvertenciaBorrarPlato.SetActive(false);

            contentPedido = GameObject.FindGameObjectWithTag("contentPedido");
            if (contentPedido == null)
            {
                GameObject[] allContentPedido = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allContentPedido)
                {
                    if (obj.CompareTag("contentPedido"))
                    {
                        contentPedido = obj;
                        break;
                    }
                }
            }

            amountText = GameObject.FindGameObjectWithTag("amountText")?.GetComponent<TMP_Text>();

            // Find PaymentHandler
            PH = GameObject.Find("Payment")?.GetComponent<PaymentHandler>();

            // Find MP
            MP = GameObject.Find("MenuPedir")?.GetComponent<MenuPedir>();

            if (MP == null)
            {
                Debug.LogError("menuPedir is null! Are you sure it’s in the scene?");
                return;
            }

            if (contentPagar == null)
            {
                Debug.Log("contentPagar = null");
            }
            //contentPedido.SetActive(false);

            if (buttonPedir != null)
            {
                buttonPedir.onClick.AddListener(SendFloatOnClick);
            }
            else
            {
                Debug.LogError("ButtonPedir not found.");
            }

            if (buttonAtender != null)
            {
                buttonAtender.onClick.AddListener(SendAtenderOnClick);
            }
            else
            {
                Debug.LogError("ButtonAtender not found.");
            }

            platosPedidosDictionary.Clear();
        }
    }

    void SendFloatOnClick()
    {
        // Only run this on the local client that actually has UI
        if (!isLocalPlayer) return;

        floatValue = float.Parse(inputMesa.text);
        int totalEspacios = 0;

        // Primer paso: contar cuántos espacios hay
        foreach (Transform espacio in contentPedido.transform)
        {
            if (espacio.name == "Espacio(Clone)" || espacio.name == "EspacioBarraPedido(Clone)" || espacio.name == "EspacioPrevia(Clone)")
            {
                totalEspacios++;
            }
        }

        // Segundo paso: inicializar los arrays
        nombrePlatoString = new string[totalEspacios];
        precioPlatoString = new string[totalEspacios];
        cantidadPlatoString = new string[totalEspacios];
        togglePlato = new int[totalEspacios];
        opcionesPlato = new string[totalEspacios];
        notaPlato = new string[totalEspacios];
        ordenPlato = new int[totalEspacios];

        // Tercer paso: llenar los arrays
        int i = 0;
        foreach (Transform espacio in contentPedido.transform)
        {
            if (espacio.name == "Espacio(Clone)" || espacio.name == "EspacioBarraPedido(Clone)" || espacio.name == "EspacioPrevia(Clone)")
            {
                TMP_Text[] tmpTexts = espacio.GetComponentsInChildren<TMP_Text>();
                nombrePlatoString[i] = tmpTexts[0].text;
                precioPlatoString[i] = tmpTexts[1].text;

                // Cantidad: always read from FixedContainer regardless of options
                Transform numero = espacio.Find("FixedContainer/Cantidad/Numero1");

                if (numero == null)
                {
                    // With options: Cantidad is reparented into the last OptionTextEspacio inside OptionsContainer
                    Transform optCont = espacio.Find("OptionsContainer");
                    if (optCont != null && optCont.childCount > 0)
                    {
                        Transform lastOption = optCont.GetChild(optCont.childCount - 1);
                        numero = lastOption.Find("Cantidad/Numero1") ?? lastOption.Find("Cantidad");
                    }
                }

                if (numero == null)
                    Debug.LogWarning($"[BuildPedidoData] Could not find Cantidad for '{espacio.name}' at index {i}. Defaulting to 1.");

                cantidadPlatoString[i] = numero?.GetComponent<TMP_Text>()?.text ?? "1";

                // Toggle: always read from FixedContainer
                int.TryParse(espacio.Find("FixedContainer/Text Toggle")
                    ?.GetComponent<TMP_Text>()?.text, out togglePlato[i]);

                // Options
                Transform optionsContainer = espacio.Find("OptionsContainer");
                List<string> toppings = new List<string>();
                foreach (Transform child in optionsContainer)
                {
                    TMP_Text txt = child.GetComponent<TMP_Text>();
                    if (txt != null) toppings.Add(txt.text);
                }
                opcionesPlato[i] = string.Join(", ", toppings);

                // Note (waiter/TPV only — read from a TMP_InputField tagged "notaPlato" inside the espacio)
                TMP_InputField notaField = espacio.GetComponentInChildren<TMP_InputField>();
                notaPlato[i] = notaField != null ? notaField.text : "";

                // Read orden from hidden TMP_Text named "OrdenPlato" on the espacio
                Transform ordenText = espacio.Find("FixedContainer/OrdenPlato");
                int.TryParse(ordenText?.GetComponent<TMP_Text>()?.text ?? "0", out ordenPlato[i]);

                i++;
            }
        }

        CmdSendPedidoToServer(floatValue, totalEspacios, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato, notaPlato, ordenPlato);

        MP.PedidoLocal();

        if (SceneManager.GetActiveScene().name != "TPVScene")
        {
            MP.BajarBotonPedido();
        }

        if (Navigation.camarero == true) // Si camarero, volver a mesas cuando pides
        {
            NavigationCamarero.Instance.Mesas();
        }
    }
    
    [Command]
    public void CmdSendPreviaToServer(float valueMesa, string[] nombrePlatoString, string[] opcionesPlato, string[] cantidadPlatoString, string[] precioPlatoString, int[] togglePlato)
    {
        var myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;

        int[] dummyindexPlato = new int[0];

        var data = new MesaDataPrevia(dummyindexPlato, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato);
        MesaStateManager.instance.SetMesaContentPrevia(restId, (int)valueMesa, data, connectionToClient);
    }

    [Command]
    public void CmdUpdateCantidad(int mesaNumber, string nombrePlato, string opciones, string newCantidad, string newPrecio)
    {
        var myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;
        int connId = connectionToClient.connectionId;
        MesaStateManager.instance.UpdateMesaPlatoCantidad(restId, mesaNumber, connId, nombrePlato, opciones, newCantidad, newPrecio, connectionToClient);
    }

    [Command]
    public void CmdDeletePlato(int mesaNumber, string nombrePlato, string opciones)
    {
        var myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;
        int connId = connectionToClient.connectionId;
        MesaStateManager.instance.DeletePlatoFromMesa(restId, mesaNumber, connId, nombrePlato, opciones, connectionToClient);
    }

    [Command]
    void CmdSendPedidoToServer(float valueMesa, int nEspacios, string[] nombrePlatoString, string[] opcionesPlato, string[] cantidadPlatoString, string[] precioPlatoString, int[] togglePlato, string[] notaPlato, int[] ordenPlato)
    {
        MyRoomPlayer myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;

        MesaStateManager.instance.ProcessIncomingPedido(restId, (int)valueMesa, nEspacios, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato, notaPlato, ordenPlato);

        if (valueMesa > 999)
            RpcCreateMesaOnTPV((int)valueMesa);

        if (valueMesa > 999 && TPV_DataManager.instance != null &&
            TPV_DataManager.mesaCustomerMap.TryGetValue((int)valueMesa, out int customerId))
        {
            string tipo = TPV_DataManager.mesaTipoMap.TryGetValue((int)valueMesa, out string t) ? t : "";
            TPV_DataManager.instance.SaveOrderToHistory(customerId, (int)valueMesa, tipo, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString);
        }
    }

    [ClientRpc]
    void RpcCreateMesaOnTPV(int mesaNumber)
    {
        if (SceneManager.GetActiveScene().name != "TPVScene") return;

        CrearCamarero crearCamarero = FindObjectOfType<CrearCamarero>();
        if (crearCamarero == null) return;

        crearCamarero.CreateMesa(mesaNumber);
    }


    void SendAtenderOnClick()
    {
        floatValue = float.Parse(inputMesa.text);
        atenderConfirmado.SetActive(true);
        CmdSendAtencionToServer(floatValue);
    }

    [Command]
    void CmdSendAtencionToServer(float valueMesa)
    {
        // 1) Find the player's RestaurantID from the server's perspective
        MyRoomPlayer myRoomPlayer = connectionToClient.identity.GetComponent<MyRoomPlayer>();
        string restId = myRoomPlayer.RestaurantID;
        Debug.Log($"[CmdSendAtencionToServer] Received data from client with RestaurantID: {restId}");

        // 2) Grab the MyRoomManager singleton
        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;

        // 3) Look up the list of connections for that restaurant
        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            Debug.Log($"[CmdSendAtencionToServer] Found {conns.Count} connection(s) for restaurant {restId}");
            // 4) Send a TargetRpc to each connection individually
            MesaStateManager.instance.TryGetColorState(restId, (int)valueMesa, out MesaColorType previousColor);
            foreach (NetworkConnectionToClient c in conns)
            {
                Debug.Log($"[CmdSendAtencionToServer] Sending TargetRpc to connection {c.connectionId}");
                TargetBroadcastAtencion(c, valueMesa, previousColor);
            }
        }
        else
        {
            Debug.LogError($"[CmdSendAtencionToServer] No connections found for restaurant {restId}");
        }

        // Colourize button mesa
        MesaStateManager.instance.SetMesaColor(restId, (int)valueMesa, MesaColorType.Red);  // or Yellow, Red, etc.
    }

    [TargetRpc]
    public void TargetBroadcastAtencion(NetworkConnection target, float valueMesa, MesaColorType previousColor)
    {
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            int mesaKey = (int)valueMesa;
            previousColorDict[mesaKey] = previousColor;

            CrearCamarero.mesasDictionary.TryGetValue(mesaKey, out GameObject mesa);
            GameObject prefabEspacioInstance = Instantiate(alertaCamarero, transform.position, Quaternion.identity);
            prefabEspacioInstance.transform.SetParent(mesa.transform.GetChild(0).GetChild(0).GetChild(0), false);
            alertasDict[mesaKey] = prefabEspacioInstance;

            // Fuente 
            TMP_Text[] texts = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
            string rutafuenteGeneral = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
            TMP_FontAsset fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral);
            if (fuenteGeneral == null)
                fuenteGeneral = Resources.Load<TMP_FontAsset>(rutafuenteGeneral + " SDF");
            texts[0].font = fuenteGeneral;
            texts[1].font = fuenteGeneral;

            Button buttonAlerta = prefabEspacioInstance.GetComponentInChildren<Button>();
            if (buttonAlerta != null)
            {
                buttonAlerta.onClick.AddListener(() =>
                {
                    MesaColorType prev = previousColorDict.ContainsKey(mesaKey) ? previousColorDict[mesaKey] : MesaColorType.Default;
                    var playerController = NetworkClient.connection.identity.GetComponent<MyPlayerController>();
                    playerController.CmdSendYaAtendidoToServer((int)valueMesa, (int)prev);
                });
            }
            else
            {
                Debug.LogError("NO BUTTON");
            }

            // Image atencion (only for clients)
            if (!Navigation.camarero)
            {
                string myMesa = GameObject.FindGameObjectWithTag("inputMesa")?.GetComponent<TMP_Text>()?.text;
                if (myMesa != null && int.Parse(myMesa) == mesaKey)
                {
                    GameObject atencionImage = null;
                    foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if (obj.CompareTag("AtencionImage")) { atencionImage = obj; break; }
                    }
                    if (atencionImage != null)
                        atencionImage.SetActive(true);
                }
            }
        }
    }

    public void YaAtendido(float valueMesa)
    {
        MesaColorType prev = previousColorDict.ContainsKey((int)valueMesa) ? previousColorDict[(int)valueMesa] : MesaColorType.Default;
        CmdSendYaAtendidoToServer(valueMesa, (int)prev);
    }

    [Command]
    void CmdSendYaAtendidoToServer(float valueMesa, int previousColor)
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
                TargetYaAtendido(c, valueMesa, previousColor);
            }
        }
        else
        {
            Debug.LogError($"[CmdSendPedidoToServer] No connections found for restaurant {restId}");
        }

        // Colourize button mesa
        MesaStateManager.instance.SetMesaColor(restId, (int)valueMesa, (MesaColorType)previousColor, force: true);
        MesaStateManager.instance.SetAsistenciaActive(restId, (int)valueMesa, false);
        MesaStateManager.instance.PushMesaStateToWeb(restId, (int)valueMesa);
    }

    [TargetRpc]
    void TargetYaAtendido(NetworkConnection target, float valueMesa, int previousColor)
    {
        Debug.Log($"[TargetYaAtendido] Instance ID: {this.GetInstanceID()}");


        int mesaKey = (int)valueMesa;
        Debug.Log($"[TargetYaAtendido] Received valueMesa: {valueMesa}, converted to int: {mesaKey}");

        // Log keys for debugging
        Debug.Log("Current keys in alertasDict:");
        foreach (var key in alertasDict.Keys)
        {
            Debug.Log($"Key: {key}");
        }

        if (alertasDict.TryGetValue(mesaKey, out GameObject alertPrefab))
        {
            Debug.Log("2");
            Destroy(alertPrefab);
            alertasDict.Remove(mesaKey);
        }
        else
        {
            Debug.LogError($"Key {mesaKey} not found in alertasDict.");
        }

        // Image atencion
        string myMesa = GameObject.FindGameObjectWithTag("inputMesa")?.GetComponent<TMP_Text>()?.text;
        if (myMesa != null && int.Parse(myMesa) == mesaKey)
        {
            GameObject atencionImage = null;
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.CompareTag("AtencionImage")) { atencionImage = obj; break; }
            }
            if (atencionImage != null)
                atencionImage.SetActive(false);
        }
    }

    [Command]
    public void CmdRequestPrintTicket(int mesaNumber)
    {
        RpcPrintTicket(mesaNumber);
    }

    [ClientRpc]
    void RpcPrintTicket(int mesaNumber)
    {
        if (POSPrinterManager.instance != null)
            POSPrinterManager.instance.PrintTestRemote(mesaNumber);
    }

    // Función eliminar plato por el camarero
    void ButtonClicked(string nombre, string cantidad, float mesa)
    {

        AdvertenciaBorrarPlato.GetComponentsInChildren<TMP_Text>()[0].text = "¿Seguro que quieres cancelar el pedido del elemento '" + nombre + "'?";

        AdvertenciaBorrarPlato.SetActive(true);

        Button[] buttons = AdvertenciaBorrarPlato.GetComponentsInChildren<Button>();

        // Si cancelas se cierra el cuadro de dialogo
        buttons[0].onClick.AddListener(() =>
        {
            AdvertenciaBorrarPlato.gameObject.SetActive(false); // Activa el Canvas
        });

        // Si aceptas se cierra el cuadro de dialogo y se manda la info
        buttons[1].onClick.AddListener(() =>
        {
            AdvertenciaBorrarPlato.gameObject.SetActive(false); // Activa el Canvas
        });
    }

    private float ExtractFloat(string input)
    {
        // Set to a specific culture that uses a comma as a decimal separator (for example, Spanish)
        CultureInfo culture = new CultureInfo("es-ES");
        string decimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator;

        // Use regex to remove all characters except digits and the decimal separator
        string sanitizedInput = Regex.Replace(input, @"[^\d" + Regex.Escape(decimalSeparator) + "]", "");

        if (float.TryParse(sanitizedInput, NumberStyles.Float, culture, out float result))
        {
            return result;
        }
        Debug.LogError("Failed to extract float from input: " + input);
        return 0;
    }
}
