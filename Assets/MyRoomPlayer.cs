using Mirror;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MyRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnRestaurantIDChanged))]
    public string RestaurantID = "0";

    private TMP_Text infoClient;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[MyRoomPlayer] OnStartClient. My RestaurantID = {RestaurantID}, isLocalPlayer={isLocalPlayer}, isServer={isServer}");

        infoClient = GameObject.Find("InfoClientText")?.GetComponent<TMP_Text>();

        if (infoClient == null)
        {
            Debug.LogWarning("[MyRoomPlayer] InfoClientText not found in scene.");
        }
        else if (isLocalPlayer)
        {
            infoClient.text = $"ID: {RestaurantID} [OnStartClient]";
        }
    }

    void OnRestaurantIDChanged(string oldID, string newID)
    {
        if (infoClient == null)
            infoClient = GameObject.Find("InfoClientText")?.GetComponent<TMP_Text>();

        if (infoClient != null && isLocalPlayer)
            infoClient.text = $"Updated ID: {newID} [Hook]";
        else if (isLocalPlayer)
            Debug.LogWarning("[MyRoomPlayer] infoClient still null in OnRestaurantIDChanged");
    }

    [Command]
    public void CmdSetRestaurantID(string newID)
    {
        RestaurantID = newID;

        if (NetworkManager.singleton is MyRoomManager manager)
        {
            manager.RegisterPlayer(this);
            //Debug.Log("[CmdSetRestaurantID] Registered player with manager.");
        }
        else
        {
            Debug.LogError("[CmdSetRestaurantID] Could not find MyRoomManager!");
        }
    }

    [Command]
    public void CmdUpdateDishState(string restaurantId, int mesaNumber, int batchId, int localIndex, string nombrePlato, int cantidadPlato, string opciones, int newState)
    {
        MesaStateManager.instance.UpdateDishState(restaurantId, mesaNumber, batchId, localIndex, nombrePlato, cantidadPlato, opciones, newState, connectionToClient);
    }

    [Command]
    public void CmdSetMesaColor(string restaurantId, int mesaNumber, int colorType, bool force)
    {
        MesaStateManager.instance.SetMesaColor(restaurantId, mesaNumber, (MesaColorType)colorType, force);
    }

    [TargetRpc]
    public void TargetUpdateDishState(NetworkConnection target, int mesaNumber, int batchId, int dishIndex, string nombrePlato, int cantidadPlato, string opciones, int newState)
    {
        Debug.Log($"[TargetUpdateDishState] mesa={mesaNumber} dishIndex={dishIndex} nombrePlato={nombrePlato} cantidadPlato={cantidadPlato} newState={newState}");

        // CLIENTE
        if (SceneManager.GetActiveScene().name == "MobileScene" && Navigation.camarero == false)
        {
            string textNumeroMesa = GameObject.FindGameObjectWithTag("inputMesa").GetComponent<TMP_Text>().text;
            if (mesaNumber == int.Parse(textNumeroMesa))
            {
                var contentPedido = GameObject.FindGameObjectWithTag("contentPedido");
                if (contentPedido == null) return;

                foreach (Transform child in contentPedido.transform)
                {
                    DishTag tag = child.GetComponent<DishTag>();
                    if (tag == null || tag.batchId != batchId || tag.localIndex != dishIndex) continue;

                    TMP_Text[] tmpTexts = child.GetComponentsInChildren<TMP_Text>();
                    if (tmpTexts.Length < 4) continue;

                    if (newState == 2 && (tmpTexts[3].text == "En camino" || tmpTexts[3].text == "Entregado")) continue;
                    if (newState == 3 && tmpTexts[3].text == "Entregado") continue;

                    if (newState == 2) tmpTexts[3].text = "En camino";
                    else if (newState == 3) tmpTexts[3].text = "Entregado";
                    break;
                }
            }
        }

        // CAMARERO
        if ((SceneManager.GetActiveScene().name == "MobileScene" && Navigation.camarero) || SceneManager.GetActiveScene().name == "TPVScene")
        {
            if (!CrearCamarero.mesasDictionary.TryGetValue(mesaNumber, out GameObject mesa))
            {
                if (SceneManager.GetActiveScene().name == "TPVScene")
                    StartCoroutine(RetryUpdateDishState(mesaNumber, batchId, dishIndex, newState));
                return;
            }

            Transform content = mesa.transform.GetChild(0).GetChild(0).GetChild(0);
            ColorUtility.TryParseHtmlString("#FFC368", out Color amarillo);
            Color greyColor = new Color32(200, 200, 200, 255);

            if (newState == 2)
            {
                foreach (Transform child in content)
                {
                    DishTag tag = child.GetComponent<DishTag>();
                    if (tag == null || tag.batchId != batchId || tag.localIndex != dishIndex) continue;

                    Image image = child.GetComponent<Image>();
                    if (image == null) continue;
                    if (image.color == greyColor) continue;
                    if (image.color == amarillo) continue;

                    image.color = amarillo;
                    break;
                }
                CmdSetMesaColor(RestaurantID, mesaNumber, (int)MesaColorType.Yellow, false);
            }
            else if (newState == 3)
            {
                foreach (Transform child in content)
                {
                    DishTag tag = child.GetComponent<DishTag>();
                    if (tag == null || tag.batchId != batchId || tag.localIndex != dishIndex) continue;

                    Toggle toggle = child.GetComponentInChildren<Toggle>();
                    if (toggle != null && !toggle.interactable) continue;

                    if (toggle != null) { toggle.SetIsOnWithoutNotify(true); toggle.interactable = false; }
                    Image image = child.GetComponent<Image>();
                    if (image != null) image.color = greyColor;
                    break;
                }
            }
        }

        // COCINA
        if (SceneManager.GetActiveScene().name == "CocinaScene" && (newState == 2 || newState == 3))
        {
            GameObject contentCocina = GameObject.FindGameObjectWithTag("contentCocina");
            if (contentCocina == null) return;

            Color greyColor = new Color32(200, 200, 200, 255);

            // Vamos directos a LA comanda exacta (mesa+batch): nunca buscamos en otras comandas
            string comandaName = "CocinaComandaMesa" + mesaNumber + "Batch" + batchId;
            Transform comanda = contentCocina.transform.Find(comandaName);
            if (comanda == null) return; // ya destruida, o no corresponde a esta cocina

            GrupoCocinaUI[] grupos = comanda.GetComponentsInChildren<GrupoCocinaUI>(true);
            foreach (GrupoCocinaUI grupo in grupos)
            {
                if (grupo == null || grupo.dishesContainer == null) continue;

                Transform espacio = grupo.dishesContainer.Find("Espacio_" + dishIndex);
                if (espacio == null) continue;
                {

                    if (newState == 2)
                    {
                        Toggle toggle = espacio.GetComponentInChildren<Toggle>();
                        if (toggle == null) continue;

                        toggle.SetIsOnWithoutNotify(true);
                        toggle.interactable = false;

                        bool grupoCompleto = true;
                        foreach (Toggle tGrupo in grupo.dishesContainer.GetComponentsInChildren<Toggle>(true))
                        {
                            if (!tGrupo.isOn) { grupoCompleto = false; break; }
                        }

                        GameObject comandaGO = comanda.gameObject;
                        if (grupoCompleto) Destroy(grupo.gameObject);

                        // Solo contamos toggles de PLATOS (dentro de dishesContainer de cada grupo),
                        // nunca el toggleListo del grupo (ese es "cocina lista", no "plato hecho").
                        bool comandaCompleta = true;
                        foreach (GrupoCocinaUI g in comanda.GetComponentsInChildren<GrupoCocinaUI>(true))
                        {
                            if (g == null || g.dishesContainer == null) continue;
                            foreach (Toggle tDish in g.dishesContainer.GetComponentsInChildren<Toggle>(true))
                            {
                                if (!tDish.isOn) { comandaCompleta = false; break; }
                            }
                            if (!comandaCompleta) break;
                        }

                        // la segunda vez no llega AQUI
                        foreach (GrupoCocinaUI gDebug in comanda.GetComponentsInChildren<GrupoCocinaUI>(true))
                        {
                            if (gDebug == null || gDebug.dishesContainer == null) continue;
                            foreach (Toggle tDebug in gDebug.dishesContainer.GetComponentsInChildren<Toggle>(true))
                                Debug.Log($"[DEBUG comanda] grupo={gDebug.name} toggle={tDebug.name} isOn={tDebug.isOn} interactable={tDebug.interactable}");
                        }

                        if (comandaCompleta)
                        {
                            Debug.Log($"[Contador] DESTRUYENDO comanda '{comandaName}' (todos los platos marcados)");
                            Destroy(comandaGO);
                        }

                        return;
                    }

                    else // newState == 3
                    {
                        Image image = espacio.GetComponentInChildren<Image>();
                        if (image != null && image.color != greyColor)
                        {
                            image.color = greyColor;
                        }
                        return;
                    }
                }
            }
        }
    }

    IEnumerator RetryUpdateDishState(int mesaNumber, int batchId, int dishIndex, int newState)
    {
        float timeout = 3f;
        while (!CrearCamarero.mesasDictionary.ContainsKey(mesaNumber))
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0)
            {
                Debug.LogWarning($"[RetryUpdateDishState] Timed out waiting for mesa {mesaNumber}");
                yield break;
            }
            yield return null;
        }

        if (newState == 2)
        {
            if (!CrearCamarero.mesasDictionary.TryGetValue(mesaNumber, out GameObject mesa)) yield break;

            Transform content = mesa.transform.GetChild(0).GetChild(0).GetChild(0);
            ColorUtility.TryParseHtmlString("#FFC368", out Color amarillo);
            Color greyColor = new Color32(200, 200, 200, 255);
            foreach (Transform child in content)
            {
                DishTag tag = child.GetComponent<DishTag>();
                if (tag == null || tag.batchId != batchId || tag.localIndex != dishIndex) continue;

                Image image = child.GetComponent<Image>();
                if (image != null && image.color == greyColor) continue;
                if (image != null) { image.color = amarillo; break; }
            }
            CmdSetMesaColor(RestaurantID, mesaNumber, (int)MesaColorType.Yellow, false);
        }
    }

    [TargetRpc]
    public void TargetReceiveMesaStates(NetworkConnection target, string restaurantId, int[] mesaNumbers, int[] mesaNumbersPrevia, MesaColorType[] mesaColors, MesaData[] mesaContents, MesaDataPrevia[] mesaContentsPrevia)
    {
        if (SceneManager.GetActiveScene().name == "CocinaScene")
        {
            for (int i = 0; i < mesaNumbers.Length; i++)
            {
                int mesa = mesaNumbers[i];
                ConnectMirrorCocina.instance.pendingMesasFromServer.Add(mesa);
            }

            ConnectMirrorCocina.instance.RetrieveDataCocina(mesaNumbers, mesaContents);
        }
        else
        {
            CrearCamarero.instance.pendingMesasFromServer.Clear();
            CrearCamarero.instance.pendingMesasFromServerPrevia.Clear();

            for (int i = 0; i < mesaNumbers.Length; i++)
            {
                int mesa = mesaNumbers[i];
                MesaColorType color = mesaColors[i];
                MesaData content = mesaContents[i];

                CrearCamarero.instance.pendingMesasFromServer.Add(mesa);

                MesaStateManager.instance.SetLocalMesaColor(restaurantId, mesa, color);
                MesaStateManager.instance.SetLocalMesaContent(restaurantId, mesa, content);
            }

            for (int i = 0; i < mesaNumbersPrevia.Length; i++)
            {
                int mesa = mesaNumbersPrevia[i];
                MesaDataPrevia contentPrevia = mesaContentsPrevia[i];

                CrearCamarero.instance.pendingMesasFromServerPrevia.Add(mesa);

                MesaStateManager.instance.SetLocalMesaContentPrevia(restaurantId, mesa, contentPrevia);
            }

            // Atencion called?
            if (SceneManager.GetActiveScene().name == "MobileScene" && !Navigation.camarero)
            {
                string myMesa = GameObject.FindGameObjectWithTag("inputMesa")?.GetComponent<TMP_Text>()?.text;
                if (myMesa != null && MesaStateManager.instance.TryGetColorState(restaurantId, int.Parse(myMesa), out MesaColorType color))
                {
                    GameObject atencionImage = GameObject.Find("AtencionImage");
                    if (atencionImage != null)
                        atencionImage.SetActive(color == MesaColorType.Red);
                }
            }

            CrearCamarero.instance.TryRestoreIfLoaded();
        }
    }

    [TargetRpc]
    public void TargetUpdateMesaColor(NetworkConnection target, int mesaNumber, MesaColorType color)
    {
        if (SceneManager.GetActiveScene().name != "CocinaScene")
        {
            // exact same code as before
            var crear = CrearCamarero.instance;
            if (crear == null) { Debug.LogError("[Client] CrearCamarero.instance is null."); return; }
            if (!crear.mesaColorSyncDictionary.TryGetValue(mesaNumber, out var colorSync)) return;
            colorSync.SetColor(color);
        }
    }

    [TargetRpc]
    public void TargetUpdateContentMesa(NetworkConnection target, int mesaNumber, int nEspacios,
        string[] nombrePlatoString, string[] opcionesPlato,
        string[] cantidadPlatoString, string[] precioPlatoString, int[] togglePlato, string[] notaPlato, int[] ordenPlato, int[] batchIdPlato)
    {
        // exact same code as before
        var data = new MesaData(nEspacios, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato, notaPlato, ordenPlato, batchIdPlato);
        var crear = CrearCamarero.instance;
        if (SceneManager.GetActiveScene().name == "CocinaScene")
        {
            ConnectMirrorCocina.instance.SetContentCocina(data, mesaNumber);
        }
        else
        {
            if (!crear.mesaContentSyncDictionary.TryGetValue(mesaNumber, out var contentSync))
            {
                // In TPVScene the mesa may not be created yet (RpcCreateMesaOnTPV still in flight)
                if (SceneManager.GetActiveScene().name == "TPVScene")
                    StartCoroutine(RetryUpdateContentMesa(mesaNumber, data));
                return;
            }
            if (Navigation.camarero) contentSync.SetContentCamarero(data, mesaNumber);
            else if (int.Parse(MesaStateManager.instance.numeroMesaLocal.text) == mesaNumber)
                contentSync.SetContentCliente(data);
        }
    }

    IEnumerator RetryUpdateContentMesa(int mesaNumber, MesaData data)
    {
        // Wait until CreateMesa has registered this mesa
        float timeout = 3f;
        while (!CrearCamarero.instance.mesaContentSyncDictionary.ContainsKey(mesaNumber))
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0)
            {
                Debug.LogWarning($"[RetryUpdateContentMesa] Timed out waiting for mesa {mesaNumber}");
                yield break;
            }
            yield return null;
        }

        if (CrearCamarero.instance.mesaContentSyncDictionary.TryGetValue(mesaNumber, out var contentSync))
            contentSync.SetContentCamarero(data, mesaNumber);
    }

    [TargetRpc]
    public void TargetUpdateContentMesaPrevia(NetworkConnection target, int mesaNumber, int[] ownerConnectionId,
    string[] nombrePlatoString, string[] opcionesPlato,
    string[] cantidadPlatoString, string[] precioPlatoString, int[] togglePlato)
    {
        if (SceneManager.GetActiveScene().name == "TPVScene") return;

        // exact same code as before
        var data = new MesaDataPrevia(ownerConnectionId, nombrePlatoString, opcionesPlato, cantidadPlatoString, precioPlatoString, togglePlato); var crear = CrearCamarero.instance;
        if (crear == null) return;
        if (!crear.mesaContentPreviaSyncDictionary.TryGetValue(mesaNumber, out var contentSync)) return;
        if (int.Parse(MesaStateManager.instance.numeroMesaLocal.text) == mesaNumber)
        {
            contentSync.SetContentClientePrevia(data, mesaNumber);
            MesaStateManager.instance.imageNotificacion.SetActive(true);
        }
    }

    [TargetRpc]
    public void TargetUpdatePlatoQuantity(NetworkConnection target, int mesaNumber, int platoIndex, string newCantidad, string newPrecio)
    {
        if (int.Parse(MesaStateManager.instance.numeroMesaLocal.text) != mesaNumber) return;

        var contentPedido = GameObject.FindGameObjectWithTag("contentPedido");
        if (contentPedido == null) return;

        foreach (Transform child in contentPedido.transform)
        {
            if (child.name != "EspacioPrevia(Clone)" && child.name != "EspacioBarraPedido(Clone)") continue;

            TMP_Text[] texts = child.GetComponentsInChildren<TMP_Text>();

            // texts[6] stores platoIndex (set in SetContentClientePrevia)
            if (texts.Length > 6 && texts[6].text == platoIndex.ToString())
            {
                texts[1].text = newPrecio.Replace(".", ",");
                if (!texts[1].text.EndsWith("€")) texts[1].text += "€";
                texts[3].text = newCantidad;

                // Recalc total
                GameObject.Find("MenuPedir")?.GetComponent<MenuPedir>()?.HacerSumaPedidos();
                return;
            }
        }
    }

    [TargetRpc]
    public void TargetShowIncomingCall(NetworkConnection target, string numero)
    {
        Debug.Log($"[MyRoomPlayer] TargetShowIncomingCall RECEIVED on client. numero={numero}, CallPopupController.instance null? {CallPopupController.instance == null}");
        CallPopupController.NotifyIncomingCall(numero);
    }

    [TargetRpc]
    public void TargetCallAnswered(NetworkConnection target, string numero)
    {
        Debug.Log($"[MyRoomPlayer] TargetCallAnswered RECEIVED on client. numero={numero}");
        IncomingCallOrderRouter.NotifyCallAnswered(numero);
    }

    [TargetRpc]
    public void TargetHideIncomingCall(NetworkConnection target, string numero)
    {
        Debug.Log($"[MyRoomPlayer] TargetHideIncomingCall RECEIVED on client. numero={numero}");
        CallPopupController.NotifyCallEnded(numero);
    }

    [SyncVar]
    public int myConnectionId = -1;

    public override void OnStartServer()
    {
        base.OnStartServer();
        myConnectionId = connectionToClient.connectionId;
    }

    [Command]
    public void CmdSetPagoEnCurso(int mesaNumber, string origen) // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] CmdSetPagoEnCurso llamado: mesa={mesaNumber}, origen={origen}, restId={RestaurantID}"); // 👈 AÑADIR temporal
        bool ok = MesaStateManager.instance.SetPagoEnCurso(RestaurantID, mesaNumber, origen);
        if (!ok)
        {
            // El que lo pidió se queda con el estado real (por si acaso su UI local ya asumía que había bloqueado)
            MesaStateManager.instance.TryGetPagoEnCurso(RestaurantID, mesaNumber, out string origenReal);
            TargetUpdatePagoEnCurso(connectionToClient, mesaNumber, true, origenReal);
        }
    }

    [Command]
    public void CmdClearPagoEnCurso(int mesaNumber) // 👈 AÑADIR método completo
    {
        MesaStateManager.instance.ClearPagoEnCurso(RestaurantID, mesaNumber);
    }

    [TargetRpc]
    public void TargetUpdatePagoEnCurso(NetworkConnection target, int mesaNumber, bool enCurso, string origen) // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] TargetUpdatePagoEnCurso RECIBIDO en cliente: mesa={mesaNumber}, enCurso={enCurso}, origen={origen}, escena={SceneManager.GetActiveScene().name}"); // 👈 AÑADIR temporal
        
        if (SceneManager.GetActiveScene().name == "TPVScene")
        {
            // bloqueo visible en TPV cuando cobra el camarero
            var crear = CrearCamarero.instance;
            if (crear != null) crear.SetPagoEnCursoUI(mesaNumber, enCurso, origen);
        }
        else if (SceneManager.GetActiveScene().name == "MobileScene" && Navigation.camarero)
        {
            // bloqueo visible en camarero cuando cobra el TPV
            var cobro = CobrosCamarero.instance;
            if (cobro != null) cobro.SetPagoEnCursoUI(mesaNumber, enCurso, origen);
        }
    }
}
