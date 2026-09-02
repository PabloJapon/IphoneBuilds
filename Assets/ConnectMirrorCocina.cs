using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using System;

public class ConnectMirrorCocina : MonoBehaviour
{
    public TMP_Text idRestaurante;

    public static ConnectMirrorCocina instance;
    [HideInInspector]
    public List<int> pendingMesasFromServer = new List<int>();

    public Dictionary<int, MesaContentSync> mesaContentSyncDictionary = new Dictionary<int, MesaContentSync>();

    // Gameobjects
    public GameObject prefabCocinaComanda;
    public GameObject prefabCocinaEspacio;
    private GameObject contentComanda;
    public GameObject prefabOptionCocina;
    public GameObject prefabCocinaOrdenHeader;
    public GameObject prefabCocinaGrupo; // nuevo prefab con GrupoCocinaUI
    public Color colorNotaCocina;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void LoginStart()
    {
        FindObjectOfType<MyRoomManager>().StartClient();
        StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante.text));
        StartCoroutine(StartClientWithDelay());
    }

    private IEnumerator WaitForLocalPlayerAndSendID(string restaurantID)
    {
        while (NetworkClient.connection == null || NetworkClient.connection.identity == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

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

    public void RetrieveDataCocina(int[] mesaNumbers, MesaData[] mesaContents)
    {
        for (int i = 0; i < mesaNumbers.Length; i++)
        {
            SetContentCocina(mesaContents[i], mesaNumbers[i]);
        }
    }

    private IEnumerator StartClientWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("[StartClientWithDelay] Starting client after delay");
        FindObjectOfType<MyRoomManager>().StartClient();
    }

    public void SetContentCocina(MesaData data, int mesaNumber)
    {
        Debug.Log($"[SetContentCocina] mesa={mesaNumber} nEspacios={data.nEspacios}");

        GameObject nCocina = GameObject.FindGameObjectWithTag("nCocina");
        TMP_Text textoTMP = nCocina.GetComponentInChildren<TMP_Text>();
        int numeroCocina = int.Parse(textoTMP.text);

        // Agrupar por batchId REAL (uno por pedido, asignado por el servidor).
        // Una sola llamada puede traer varios pedidos distintos (reconexión tardía).
        var indicesPorBatch = new Dictionary<int, List<int>>();
        for (int i = 0; i < data.nEspacios; i++)
        {
            int bId = (data.batchIdPlato != null && i < data.batchIdPlato.Length) ? data.batchIdPlato[i] : 0;
            if (!indicesPorBatch.TryGetValue(bId, out var list))
            {
                list = new List<int>();
                indicesPorBatch[bId] = list;
            }
            list.Add(i);
        }

        foreach (var kvp in indicesPorBatch.OrderBy(k => k.Key))
        {
            int thisBatch = kvp.Key;
            List<int> indicesBatch = kvp.Value;

            if (!indicesBatch.Any(i => data.togglePlato[i] == numeroCocina && data.estadoPlato[i] < 2))
                continue; // esta cocina no tiene nada pendiente en este pedido concreto

            BuildComandaCocina(data, mesaNumber, numeroCocina, thisBatch, indicesBatch);
        }
    }

    private void BuildComandaCocina(MesaData data, int mesaNumber, int numeroCocina, int thisBatch, List<int> indicesBatch)
    {
        GameObject contentCocina = GameObject.FindGameObjectWithTag("contentCocina");

        string comandaName = "CocinaComandaMesa" + mesaNumber + "Batch" + thisBatch;
        if (contentCocina.transform.Find(comandaName) != null)
        {
            Debug.LogWarning($"[BuildComandaCocina] {comandaName} ya existe en esta pantalla, evitando duplicado");
            return;
        }

        GameObject cocinaComanda = Instantiate(prefabCocinaComanda, transform.position, Quaternion.identity);
        cocinaComanda.transform.SetParent(contentCocina.transform, false);
        cocinaComanda.name = comandaName;

        Transform[] childrenTransforms = cocinaComanda.GetComponentsInChildren<Transform>();
        foreach (Transform childTransform in childrenTransforms)
        {
            if (childTransform.name == "Content")
            {
                contentComanda = childTransform.gameObject;
                break;
            }
        }

        TMP_Text[] childrenTexts = cocinaComanda.GetComponentsInChildren<TMP_Text>();

        TMP_Text textComponent = childrenTexts[0];
        if (mesaNumber >= 2000)
            textComponent.text = "D" + (mesaNumber - 2000);
        else if (mesaNumber >= 1000)
            textComponent.text = "R" + (mesaNumber - 1000);
        else
            textComponent.text = "Mesa " + mesaNumber;

        string rutaFuenteEmpl = "Fonts/" + DataBasePersonalizacionCocinaScene.letra_empl[0].Replace(" ", "");
        TMP_FontAsset fuenteEmpl = Resources.Load<TMP_FontAsset>(rutaFuenteEmpl);
        if (fuenteEmpl == null)
            fuenteEmpl = Resources.Load<TMP_FontAsset>(rutaFuenteEmpl + " SDF");
        textComponent.font = fuenteEmpl;

        HashSet<int> diferentes = new HashSet<int>();
        foreach (int i in indicesBatch)
        {
            int num = data.togglePlato[i];
            if (num != 0) diferentes.Add(num);
        }
        Debug.Log("totCocinas: " + diferentes.Count);

        // ── DISH LOOP (por grupos: 1º/2º/3º/sin orden) ──────────────────────
        bool hasOrden = data.ordenPlato != null && indicesBatch.Any(i => data.ordenPlato[i] > 0);
        string[] ordenLabels = { "-------   Sin orden   -------", "-----------   1º   -----------", "-----------   2º   -----------", "-----------   3º   -----------" };

        Dictionary<int, HashSet<int>> cocinasPorOrden = new Dictionary<int, HashSet<int>>();
        Dictionary<int, int> totalPlatosPorOrden = new Dictionary<int, int>();

        foreach (int i in indicesBatch)
        {
            int ordenKey = hasOrden ? data.ordenPlato[i] : 0;
            if (!cocinasPorOrden.ContainsKey(ordenKey))
            {
                cocinasPorOrden[ordenKey] = new HashSet<int>();
                totalPlatosPorOrden[ordenKey] = 0;
            }
            if (data.togglePlato[i] != 0) cocinasPorOrden[ordenKey].Add(data.togglePlato[i]);
            totalPlatosPorOrden[ordenKey]++;
        }

        List<int> ordenesOrdenados = cocinasPorOrden.Keys
            .OrderBy(k => (k == 0 ? 999 : k))
            .ToList();

        int totalDishesInComanda = indicesBatch.Count(i => data.togglePlato[i] == numeroCocina);
        int[] dishesClickedTotal = { 0 };

        for (int idx = 0; idx < ordenesOrdenados.Count; idx++)
        {
            int ordenKey = ordenesOrdenados[idx];
            List<int> indicesDeEstaCocina = indicesBatch
                .Where(i => data.togglePlato[i] == numeroCocina && (hasOrden ? data.ordenPlato[i] : 0) == ordenKey)
                .ToList();

            if (indicesDeEstaCocina.Count == 0) continue;

            GameObject grupoObj = Instantiate(prefabCocinaGrupo, contentComanda.transform, false);
            GrupoCocinaUI grupoUI = grupoObj.GetComponent<GrupoCocinaUI>();

            grupoUI.mesaNumber = mesaNumber;
            grupoUI.batchIndex = thisBatch;
            grupoUI.ordenGrupo = ordenKey;
            grupoUI.totCocinasGrupo = cocinasPorOrden[ordenKey].Count;
            grupoUI.desbloqueado = (idx == 0);
            grupoUI.AsignarNombreUnico();

            if (grupoUI.headerText != null)
            {
                if (hasOrden)
                {
                    string label = (ordenKey > 0 && ordenKey < ordenLabels.Length) ? ordenLabels[ordenKey] : ordenLabels[0];
                    grupoUI.headerText.text = label;
                    grupoUI.headerText.font = fuenteEmpl;
                    grupoUI.headerText.gameObject.SetActive(true);
                }
                else
                {
                    grupoUI.headerText.gameObject.SetActive(false);
                }
            }

            grupoUI.RefrescarVisual(0);

            if (grupoUI.toggleListo != null)
            {
                grupoUI.toggleListo.onValueChanged.AddListener((bool isOn) =>
                {
                    if (!isOn) return;
                    if (!grupoUI.desbloqueado) return;

                    if (NetworkClient.connection == null || NetworkClient.connection.identity == null) return;
                    var comunicacionCocinas = NetworkClient.connection.identity.GetComponent<ComunicacionCocinas>();
                    if (comunicacionCocinas == null || !comunicacionCocinas.isActiveAndEnabled) return;

                    grupoUI.toggleListo.interactable = false;
                    comunicacionCocinas.CmdGrupoListo(mesaNumber, thisBatch, ordenKey, numeroCocina, grupoUI.totCocinasGrupo, totalPlatosPorOrden[ordenKey]);
                });
            }

            if (grupoUI.toggleLabel != null) grupoUI.toggleLabel.font = fuenteEmpl;

            int totalDishesInGrupo = indicesDeEstaCocina.Count;
            int dishesClickedInGrupo = 0;

            foreach (int i in indicesDeEstaCocina)
            {
                GameObject prefabEspacioInstance = Instantiate(prefabCocinaEspacio, transform.position, Quaternion.identity);
                prefabEspacioInstance.transform.SetParent(grupoUI.dishesContainer, false);

                TMP_Text[] texts = prefabEspacioInstance.GetComponentsInChildren<TMP_Text>();
                texts[0].text = data.nombrePlatoString[i];
                texts[1].text = data.cantidadPlatoString[i];
                texts[0].font = fuenteEmpl;
                texts[1].font = fuenteEmpl;

                int capturedIndex = i;
                int capturedLocalIndex = GetLocalIndexInBatch(data.batchIdPlato, capturedIndex, thisBatch);
                prefabEspacioInstance.name = "Espacio_" + capturedLocalIndex;
                int capturedOrdenKey = ordenKey;
                string capturedNombre = data.nombrePlatoString[i];
                int capturedCantidad = int.Parse(data.cantidadPlatoString[i]);
                int capturedTotalPlatosGrupo = totalPlatosPorOrden[ordenKey];

                Toggle toggleDish = prefabEspacioInstance.GetComponentInChildren<Toggle>();

                bool yaHecho = data.estadoPlato != null && i < data.estadoPlato.Length && data.estadoPlato[i] >= 2;
                if (yaHecho)
                {
                    toggleDish.SetIsOnWithoutNotify(true);
                    toggleDish.interactable = false;
                    dishesClickedInGrupo++;
                    dishesClickedTotal[0]++;
                }

                toggleDish.onValueChanged.AddListener((bool isOn) =>
                {
                    if (!isOn) { toggleDish.SetIsOnWithoutNotify(true); return; }

                    toggleDish.SetIsOnWithoutNotify(false);

                    Action ejecutarCheckeo = () =>
                    {
                        toggleDish.SetIsOnWithoutNotify(true);
                        toggleDish.interactable = false;

                        var player = NetworkClient.connection.identity.GetComponent<MyRoomPlayer>();
                        if (player == null) return;

                        int localIndexInBatch = GetLocalIndexInBatch(data.batchIdPlato, capturedIndex, thisBatch);
                        player.CmdUpdateDishState(player.RestaurantID, mesaNumber, thisBatch, localIndexInBatch, capturedNombre, capturedCantidad, data.opcionesPlato[capturedIndex], 2);
                        player.CmdSetMesaColor(player.RestaurantID, mesaNumber, (int)MesaColorType.Yellow, false);

                        var comunicacionCocinas = NetworkClient.connection.identity.GetComponent<ComunicacionCocinas>();
                        if (comunicacionCocinas != null && comunicacionCocinas.isActiveAndEnabled)
                        {
                            comunicacionCocinas.CmdPlatoCompletadoGrupo(mesaNumber, thisBatch, capturedOrdenKey, capturedTotalPlatosGrupo);
                        }

                        // El borrado del grupo/comanda ya no se decide aquí: lo hace
                        // TargetUpdateDishState (eco del servidor) en TODAS las pantallas por igual.
                    };

                    if (grupoUI.NecesitaConfirmacion())
                    {
                        ConfirmDialog.instance.Mostrar(
                            "Este plato pertenece a un grupo que aún no debería prepararse. ¿Confirmas que quieres marcarlo como hecho igualmente?",
                            ejecutarCheckeo
                        );
                    }
                    else
                    {
                        ejecutarCheckeo();
                    }
                });

                string[] opciones = data.opcionesPlato[i].Split(',');
                foreach (string opcion in opciones)
                {
                    if (!string.IsNullOrWhiteSpace(opcion))
                    {
                        GameObject optionPedido = Instantiate(prefabOptionCocina, prefabEspacioInstance.transform.GetChild(1));
                        optionPedido.GetComponent<TMP_Text>().text = StripPrice(opcion.Trim());
                        optionPedido.GetComponent<TMP_Text>().font = fuenteEmpl;

                        RectTransform espacioRect = prefabEspacioInstance.GetComponent<RectTransform>();
                        espacioRect.sizeDelta = new Vector2(espacioRect.sizeDelta.x, espacioRect.sizeDelta.y + 27);
                    }
                }

                Transform notaDisplay = null;
                foreach (Transform t in prefabEspacioInstance.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "NotaDisplay") { notaDisplay = t; break; }
                }

                if (data.notaPlato != null && i < data.notaPlato.Length && !string.IsNullOrWhiteSpace(data.notaPlato[i]))
                {
                    if (notaDisplay != null)
                    {
                        notaDisplay.SetAsLastSibling();
                        notaDisplay.gameObject.SetActive(true);
                        TMP_Text notaTxt = notaDisplay.GetComponent<TMP_Text>();
                        notaTxt.text = "Nota: " + data.notaPlato[i];
                        notaTxt.font = fuenteEmpl;
                        notaTxt.color = colorNotaCocina;

                        float currentHeight = prefabEspacioInstance.GetComponent<RectTransform>().sizeDelta.y;
                        prefabEspacioInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(
                            prefabEspacioInstance.GetComponent<RectTransform>().sizeDelta.x,
                            currentHeight + 30);
                    }
                }
                else if (notaDisplay != null)
                {
                    notaDisplay.gameObject.SetActive(false);
                }
            }

            var comunicacionCocinasReg = NetworkClient.connection?.identity?.GetComponent<ComunicacionCocinas>();
            if (comunicacionCocinasReg != null)
            {
                comunicacionCocinasReg.CmdRegistrarGrupo(mesaNumber, thisBatch, ordenKey, grupoUI.totCocinasGrupo, totalPlatosPorOrden[ordenKey], idx == 0);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(grupoObj.GetComponent<RectTransform>());

            if (dishesClickedInGrupo >= totalDishesInGrupo)
            {
                Destroy(grupoObj);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentComanda.GetComponent<RectTransform>());
        // ── END DISH LOOP ──────────────────────────────────────────────────
        
        if (dishesClickedTotal[0] >= totalDishesInComanda)
        {
            Destroy(cocinaComanda);
            return;
        }

        Image[] childrenImages = cocinaComanda.GetComponentsInChildren<Image>();
        Color newColorMesas;

        foreach (Image img in childrenImages)
        {
            if (img.gameObject.name.ToLower() != "borde")
            {
                if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionCocinaScene.col_ppal_empl[0], out newColorMesas))
                {
                    img.color = newColorMesas;
                    UpdateTextColor(img, textComponent);
                }
                break;
            }
        }

        TamañoLetra tamañoLetra = FindObjectOfType<TamañoLetra>();
        if (tamañoLetra != null)
            tamañoLetra.AplicarTamañoAComanda(cocinaComanda);
    }

    private string StripPrice(string option)
    {
        int plusIndex = option.LastIndexOf('+');
        int minusIndex = option.LastIndexOf('-');

        int cutIndex = -1;
        if (plusIndex > 0) cutIndex = plusIndex;
        if (minusIndex > cutIndex) cutIndex = minusIndex;

        if (cutIndex > 0)
            return option.Substring(0, cutIndex).Trim();

        return option;
    }

    private int GetLocalIndexInBatch(int[] batchIdPlato, int globalIndex, int batchId)
    {
        int count = 0;
        for (int k = 0; k < globalIndex; k++)
            if (batchIdPlato[k] == batchId) count++;
        return count;
    }

    void UpdateTextColor(Image boton, TMP_Text text)
    {
        Color backgroundColor = boton.color;
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        if (luminance > 0.5f)
        {
            text.color = Color.black;
        }
        else
        {
            text.color = Color.white;
            Debug.Log("white)");
        }
    }
}