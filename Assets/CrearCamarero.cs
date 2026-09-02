using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine.EventSystems;
using Mirror;
using System.Linq;

public class CrearCamarero : MonoBehaviour
{
    public static CrearCamarero instance;
    [HideInInspector]
    public List<int> pendingMesasFromServer = new List<int>();
    public List<int> pendingMesasFromServerPrevia = new List<int>();

    public Button printTicketWaiterButton;

    public GameObject contentMesas;
    public GameObject contentRecoger;
    public GameObject contentDelivery;
    public GameObject buttonMesaX;
    public GameObject buttonMesaXBarra;
    public GameObject buttonMesaXBarraRecogerDelivery;
    public DataBasePersonalizacion DBP;
    public DetalleMesa DM;
    public TMP_Text inputMesa;

    public GameObject detalleMesaX;
    public GameObject scrollMesaX;
    public GameObject scrollMesaXBarra;
    public GameObject[] mesas;
    public GameObject scrollAreaRecoger;
    public Button buttonPedir; 

    // Para TPV delivery o recoger
    public TMP_Text tomandoNota; // coger de aqui el nombre cliente y la empresa delivery
    public TMP_Text textMenuEmpresa; // coger de aqui el menu de empresa delivery

    // Espacios
    public GameObject espacioCamareroPrefab;
    public GameObject espacioTPVPrefab;

    // Options
    public GameObject prefabOptionPedido;
    public GameObject prefabOptionCocina;

    // Resetear Mesa
    public GameObject cuadroResetear;
    public GameObject cuadroResetearBarra;
    public GameObject canvasMesas;

    // Cobros
    private bool tarjeta;
    private float totalSum;
    private float totalSumEquitativo;
    private float totalSumCadaUno;
    private int numeroPersonas;
    private int personasPagadas;
    public GameObject contentTicket;
    public GameObject prefabPagarPlatoTPV;
    public GameObject prefabButtonElegirPagarTPV;
    public TMP_Text totalPrecioTicket;
    public TMP_Text totalTicket;
    public TMP_Text totalPrecioAPagar;
    public GameObject seleccioneMetodoPago;
    public GameObject comoVanAPagar;
    public GameObject ticket;
    public GameObject blurTicket;
    public GameObject entreCuantas;
    public GameObject calculadora;
    public CashCalculator CC;
    public TMP_Text textInfoPago; 

    //Recordar reparto elegido
    private enum TipoReparto { Junto, Equitativo, CadaUno }
    private TipoReparto tipoRepartoSeleccionado;    

    // Cobro Camarero
    public CobrosCamarero CobroC;

    // Equitativo
    public TMP_Text textNPersonas;
    public TMP_Text total;
    public TMP_Text totalCadaUno;

    // buttons ticket
    public GameObject buttonConfirmarPago;
    public GameObject buttonConfirmarPagoEquitativo;
    public GameObject buttonConfirmarPagoCadaUno;
    public GameObject buttonImprimirCadaUno;
    public GameObject buttonFinalizar;

    public GameObject CobrandoDesdeOtroSitio; // 👈 AÑADIR - arrastra el GameObject en el Inspector
    public TMP_Text textCobrandoDesdeOtroSitio; // 👈 AÑADIR - arrastra el TMP_Text hijo con el mensaje
    public Dictionary<int, string> pagoEnCursoDict = new Dictionary<int, string>(); // 👈 AÑADIR

    // LongPress
    public GameObject seleccionMesas;

    public TMP_Text textDetalleM;
    public TMP_Text textAdvertenciaBorrar1; // para cambiar fuente al cuadro de dialogo de seguro que quieres borrar este elemento?
    public TMP_Text textAdvertenciaBorrar2;
    public TMP_Text textAdvertenciaBorrar3;
    private string label;
    //private string labelNombre;
    //private string labelEmpresa;
    //private string labelMenu;

    public Image tomarNota2;
    public TMP_Text textTomarNota2;

    public Navigation N;
    public NavigationCamarero NC;
    public POSPrinterManager PPM;

    // MobileScene: Personalización botones cerrar mesa (no sé si estoy duplicando campos, no entiendo muy bien)
    public Image button_ticket; // sacar ticket
    public Image button_pago_tarj; // pago con tarjeta 
    public Image button_confirmar_pago; // confirmar pago
    public Image button_pago_junto; // todo junto
    public Image button_pago_equit; // equitativamente
    public Image button_pago_cadauno; // cada uno lo suyo 
    public Image button_pago_volver; // Volver
    public Image button_volver_todojunto; // Volver todo junto
    public Image button_confirmar_pago_equi;
    public Image button_finalizar_pago_equi; // Ahora es botón volver
    public Image button_confirmar_pago_cadauno; // falta poner de color sec el fondo de los elementos seleccionados
    public Image button_finalizar_pago_cadauno; // Ahora es botón volver

    // TPV Scene: Personalización botones cerrar mesa y pagos (no sé si estoy duplicando campos, no entiendo muy bien)
    public Image button_pago_tarj_tpv;
    public Image img_pago_tarj_tpv;
    public Image button_pago_ef_tpv;
    public Image img_pago_ef_tpv;
    public Image button_pago_volver_tpv;
    public Image button_pago_volver_tpv2;
    public Image button_pago_junto_tpv;
    public Image button_pago_equit_tpv;
    public Image button_pago_cadauno_tpv;
    public Image button_confirmar_pago_junto_tpv;
    public Image button_volver_pago_junto_tpv;
    public Button button_volver_confirmar_pago;
    public Image button_proceder_pago_equi_tpv;
    public Image button_confirmar_pago_equi_tpv;
    public Image button_finalizar_pago_equi_tpv;
    public Image button_volver_pago_equi_tpv;
    public Image button_confirmar_pago_cadauno_tpv;
    public Image button_imprimir_cadauno_tpv;
    public Image button_abrir_caja;
    public Image img_abrir_caja;
    public Image entrecuantos1_tpv;
    public Image entrecuantos2_tpv;
    public Image entrecuantos3_tpv;

    public Image button_volver_seleccion_items_tpv; // NUEVO: volver de la pantalla "elegir qué pagar"

    public RawImage BadgeMesas; // Puntito en Mesas / Recoger / Delivery

    // TPV Scene: Juntar / Separar mesas
    public Image button_juntar_mesas;
    public Image img_juntar_mesas;
    public Image button_separar_mesas;
    public Image img_separar_mesas;
    public Image button_cambiar_mesas;
    public Image img_cambiar_mesas;
    public Image button_volver_mesas;

    public static Dictionary<float, GameObject> buttonMesaDictionary = new Dictionary<float, GameObject>();
    public static Dictionary<float, GameObject> mesasDictionary = new Dictionary<float, GameObject>(); 

    // Sync
    public Dictionary<int, MesaColorSync> mesaColorSyncDictionary = new Dictionary<int, MesaColorSync>();
    public Dictionary<int, MesaContentSync> mesaContentSyncDictionary = new Dictionary<int, MesaContentSync>();
    public Dictionary<int, MesaContentPreviaSync> mesaContentPreviaSyncDictionary = new Dictionary<int, MesaContentPreviaSync>();

    private Dictionary<int, int> clickCounts = new Dictionary<int, int>();
    private Dictionary<int, GameObject> buttonElegirPagarDictionary = new Dictionary<int, GameObject>();
    private bool pagoParcialConfirmadoCadaUno = false; 
    private bool pagoParcialConfirmadoEquitativo = false;

    // Movimientos caja
    public DataBaseMovimientosCaja movimientosCaja;
    public TMP_Text textIdTurno;
    public GameObject advertenciaTurnoNoEmpezado;

    private bool dbpLoaded = false;

    // Fuentes
    private TMP_FontAsset fuenteCamarero;
    private TMP_FontAsset fuenteGeneral;


    [System.Serializable]
    public class MesaPrefabEntry
    {
        public string itemType;
        public GameObject buttonPrefab;
        public GameObject scrollPrefab;
    }

    public List<MesaPrefabEntry> mesaPrefabs;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        buttonMesaDictionary.Clear();
        mesasDictionary.Clear();
        DBP.OnDataLoaded += OnDBPLoaded;

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            // Button print ticket waiter
            printTicketWaiterButton.onClick.AddListener(() => NetworkClient.localPlayer.GetComponent<MyPlayerController>().CmdRequestPrintTicket(int.Parse(inputMesa.text)));
        }
    }

    private void OnDBPLoaded()
    {
        // (1) Create all your database’s mesas
        int totalMesas = DataBasePersonalizacion.num_mesas[0];
        for (int i = 1; i <= totalMesas; i++)
        {
            CreateMesa(i);
        }

        // (2) Apply saved colors and contents to mesas
        string restId = GameObject.FindGameObjectWithTag("textID").GetComponent<TMP_Text>().text;

        // Create recoger/delivery mesa buttons for late joiners (TPVScene only)
        if (SceneManager.GetActiveScene().name == "TPVScene")
        {
            foreach (int mesaNumber in pendingMesasFromServer)
            {
                if (mesaNumber > 999)
                    CreateMesa(mesaNumber);
            }
        }

        // Retrieve Saved Data
        foreach (int mesaNumber in pendingMesasFromServer)
        {
            MesaData savedData = null;
            // CONTENT MESAS
            if (mesaContentSyncDictionary.TryGetValue(mesaNumber, out var contentSync))
            {
                if (MesaStateManager.instance.TryGetContentState(restId, mesaNumber, out MesaData tmpData))
                {
                    savedData = tmpData;

                    Debug.Log($"[LateJoin] Mesa {mesaNumber} content restored: " +
                        $"nEspacios={savedData.nEspacios}, " +
                        $"nombrePlato={string.Join(",", savedData.nombrePlatoString)}, " +
                        $"opcionesPlato={string.Join(",", savedData.opcionesPlato)}, " +
                        $"cantidadPlato={string.Join(",", savedData.cantidadPlatoString)}, " +
                        $"precioPlato={string.Join(",", savedData.precioPlatoString)}, " +
                        $"togglePlato={string.Join(",", savedData.togglePlato)}"
                    );
                }
            }

            // Sync data for camareros and button colors and TPV
            if (Navigation.camarero || SceneManager.GetActiveScene().name == "TPVScene")
            {
                if (mesaColorSyncDictionary.TryGetValue(mesaNumber, out var colorSync))
                {
                    if (MesaStateManager.instance.TryGetColorState(restId, mesaNumber, out MesaColorType savedColor))
                    {
                        colorSync.SetColor(savedColor, false);
                    }
                }
                if (savedData != null)
                    contentSync.SetContentCamarero(savedData, mesaNumber);
            }
            // Sync data for clients
            if (SceneManager.GetActiveScene().name == "TPVScene" && !Navigation.camarero && int.TryParse(inputMesa.text, out int localMesaNumber) && localMesaNumber == mesaNumber) // solo para clientes en una mesa que tenga datos
            {
                if (savedData != null)
                    contentSync.SetContentCliente(savedData);
            }
        }
        foreach (int mesaNumber in pendingMesasFromServerPrevia) // Retrieve antes de pedir data
        {
            // Sync data for clients
            if (SceneManager.GetActiveScene().name != "TPVScene" && !Navigation.camarero && int.TryParse(inputMesa.text, out int localMesaNumber) && localMesaNumber == mesaNumber) // solo para clientes en una mesa que tenga datos
            {
                // Content previa mesa
                if (mesaContentPreviaSyncDictionary.TryGetValue(mesaNumber, out var contentSyncPrevia))
                {
                    if (MesaStateManager.instance.TryGetContentPreviaState(restId, mesaNumber, out MesaDataPrevia savedDataPrevia))
                    {
                        Debug.Log($"[LateJoin] Mesa PREVIA{mesaNumber} content restored: " +
                            $"ownerConnectionId={string.Join(",", savedDataPrevia.ownerConnectionId)}, " +
                            $"nombrePlato={string.Join(",", savedDataPrevia.nombrePlatoString)}, " +
                            $"opcionesPlato={string.Join(",", savedDataPrevia.opcionesPlato)}, " +
                            $"cantidadPlato={string.Join(",", savedDataPrevia.cantidadPlatoString)}, " +
                            $"precioPlato={string.Join(",", savedDataPrevia.precioPlatoString)}, " +
                            $"togglePlato={string.Join(",", savedDataPrevia.togglePlato)}"
                        );

                        contentSyncPrevia.SetContentClientePrevia(savedDataPrevia, mesaNumber);
                    }
                }
            }
        }
        pendingMesasFromServer.Clear();
        pendingMesasFromServerPrevia.Clear();

        // (3) Font/color tweaks
        var rutaFont = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
        fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFont);
        if (fuenteCamarero == null)
            fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFont + " SDF");
        textTomarNota2.font = fuenteCamarero;
        UpdateTextColor(tomarNota2, textTomarNota2);

        // Fuente general
        var rutaFont2 = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFont2);
        if (fuenteGeneral == null)
            fuenteGeneral = Resources.Load<TMP_FontAsset>(rutaFont2 + " SDF");

        dbpLoaded = true;

        ChangeColorBotonesPpal(); //Mobile Scene

        ChangeColorBotonesPpalTPV(); // TPV Scene
        ChangeColorBotonesSecTPV(); // TPV Scene
    }

    public void TryRestoreIfLoaded()
    {
        if (dbpLoaded)
            OnDBPLoaded();
    }


    public void CreateMesa(int mesaNumber)
    {
        string nombreCliente;
        string empresa;
        string menuEmpresa;

        if (buttonMesaDictionary.ContainsKey(mesaNumber))
            return;

        bool isMobile = SceneManager.GetActiveScene().name == "MobileScene";

        GameObject buttonPrefab;

        if (mesaNumber < 1000)
        {
            buttonPrefab = isMobile ? buttonMesaX : buttonMesaXBarra;
        }
        else
        {
            buttonPrefab = isMobile ? buttonMesaX : buttonMesaXBarraRecogerDelivery;
        }

        var btnGO = Instantiate(buttonPrefab, transform.position, Quaternion.identity);
        btnGO.name = "buttonMesa" + mesaNumber;

        var colorSync = btnGO.GetComponent<MesaColorSync>();
        if (colorSync != null)
        {
            colorSync.mesaNumber = mesaNumber;
            mesaColorSyncDictionary[mesaNumber] = colorSync;
            string restId = GameObject.FindGameObjectWithTag("textID").GetComponent<TMP_Text>().text;
            if (MesaStateManager.instance.TryGetColorState(restId, mesaNumber, out MesaColorType savedColor))
                colorSync.SetColor(savedColor, false);
            else
                colorSync.SetColor(MesaColorType.Default, false);
        }

        if (mesaNumber >= 1000 && mesaNumber < 2000)
        {
            btnGO.transform.SetParent(contentRecoger.transform, false);
            btnGO.transform.SetSiblingIndex(contentRecoger.transform.childCount - 2);
            label = "R" + (mesaNumber - 1000);

            // Sacar empresa y nombre cliente
            string inside = tomandoNota.text.Split('(', ')')[1];
            string[] data = inside.Split(',');

            nombreCliente = data[0].Trim();
            empresa = data[1].Trim();
            menuEmpresa = textMenuEmpresa.text;
        }
        else if (mesaNumber > 2000)
        {
            btnGO.transform.SetParent(contentDelivery.transform, false);
            btnGO.transform.SetSiblingIndex(contentDelivery.transform.childCount - 2);
            label = "D" + (mesaNumber - 2000);

            // Sacar empresa y nombre cliente
            string inside = tomandoNota.text.Split('(', ')')[1];
            string[] data = inside.Split(',');

            nombreCliente = data[0].Trim();
            empresa = data[1].Trim();
            menuEmpresa = textMenuEmpresa.text;
        }
        else
        {
            btnGO.transform.SetParent(contentMesas.transform, false);
            label = mesaNumber.ToString();
            nombreCliente = "";
            empresa = "";
            menuEmpresa = "";
        }

        // Lazy-load font if not yet available
        if (fuenteCamarero == null)
        {
            var rutaFont = "Fonts/" + DataBasePersonalizacion.letra_empl[0].Replace(" ", "");
            fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFont);
            if (fuenteCamarero == null)
                fuenteCamarero = Resources.Load<TMP_FontAsset>(rutaFont + " SDF");
        }

        var btnText = btnGO.GetComponentsInChildren<TMP_Text>();
        btnText[0].text = label;
        btnText[0].font = fuenteCamarero;

        btnText[1].text = nombreCliente;
        btnText[2].text = empresa;
        btnText[3].text = menuEmpresa;

        // wire up click → DetalleMesaClick
        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            // 🛑 Prevent click if in selection mode (i.e. from long press)
            if (LongPressDebug.selectionModeActive)
                return;

            TMP_Text label = btnGO.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                string text = label.text;

                int baseValue = 0;

                // Check if it starts with R or D
                if (text.StartsWith("R") || text.StartsWith("D"))
                {
                    char prefix = text[0];              // 'R' or 'D'
                    string numberPart = text.Substring(1); // everything after the letter

                    if (int.TryParse(numberPart, out int num))
                    {
                        if (prefix == 'R')
                            baseValue = 1000 + num;
                        else if (prefix == 'D')
                            baseValue = 2000 + num;

                        DetalleMesaClick(baseValue, nombreCliente, empresa, menuEmpresa);
                    }
                }
                // Normal number case (no letter)
                else if (int.TryParse(text, out int currentNum))
                {
                    DetalleMesaClick(currentNum, "", "", "");
                }
            }
        });

        // Assign canvas seleccionMesas to the script on the new button
        LongPressDebug script = btnGO.GetComponent<LongPressDebug>();
        if (script != null)
        {
            script.canvasToShow = seleccionMesas; // 👈 assign your panel here
        }

        buttonMesaDictionary[mesaNumber] = btnGO;

        // —— 2) Scroll‐detail panel —— 
        var scrollPrefab = isMobile ? scrollMesaX : scrollMesaXBarra;
        var panelGO = Instantiate(scrollPrefab, transform.position, Quaternion.identity);
        panelGO.name = "ScrollMesa" + mesaNumber;
        panelGO.transform.SetParent(detalleMesaX.transform, false);
        panelGO.SetActive(false);

        var myContentSync = panelGO.GetComponent<MesaContentSync>();
        var myContentPreviaSync = panelGO.GetComponent<MesaContentPreviaSync>();
        mesaContentSyncDictionary[mesaNumber] = myContentSync;
        mesaContentPreviaSyncDictionary[mesaNumber] = myContentPreviaSync;


        // Buttons
        if (!isMobile)
        {
            // hook up its buttons
            var scrollBtns = panelGO.GetComponentsInChildren<Button>();
            TMP_Text btnLabel = btnGO.GetComponentInChildren<TMP_Text>(true);

            scrollBtns[0].onClick.AddListener(() =>
            {
                if (btnLabel != null)
                {
                    string text = btnLabel.text;
                    int mesaNumberTomarNota = 0;

                    if (text.StartsWith("R") || text.StartsWith("D"))
                    {
                        char prefix = text[0];            // 'R' or 'D'
                        string numberPart = text.Substring(1);

                        if (int.TryParse(numberPart, out int num))
                        {
                            mesaNumberTomarNota = (prefix == 'R') ? 1000 + num : 2000 + num;
                        }
                        else
                        {
                            Debug.LogWarning($"Cannot parse mesa number from {text}");
                            return;
                        }

                        // hacer transparente el texto de 'Mesa X' cuando se abre el panel de pedir nota
                        // Lo volvemos a hacer opaco en DetalleMesa
                        //SetDetalleMesaTextAlpha(0f);

                        NC.TomarNota(mesaNumberTomarNota, nombreCliente, empresa, menuEmpresa);
                    }
                    else if (int.TryParse(text, out int num))
                    {
                        mesaNumberTomarNota = num;

                        // hacer transparente el texto de 'Mesa X' cuando se abre el panel de pedir nota
                        //SetDetalleMesaTextAlpha(0f);

                        NC.TomarNota(mesaNumberTomarNota, "", "", "");
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot parse mesa number from {text}");
                        return;
                    }
                }
            });

            scrollBtns[1].onClick.AddListener(() =>
            {
                NetworkClient.localPlayer.GetComponent<MyPlayerController>().CmdRequestPrintTicket(int.Parse(inputMesa.text));
            });
            scrollBtns[2].onClick.AddListener(() => Cobrar());
            scrollBtns[3].onClick.AddListener(() => SeguroResetear());

            // Interactable
            //scrollBtns[1].interactable = true;

            // Personalizar buttons
            var scrollbuttontexts = panelGO.GetComponentsInChildren<TMP_Text>();
            scrollbuttontexts[0].font = fuenteCamarero;
            scrollbuttontexts[1].font = fuenteCamarero;
            scrollbuttontexts[2].font = fuenteCamarero;
            scrollbuttontexts[3].font = fuenteCamarero;
            // scrollbuttontexts[0].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // scrollbuttontexts[1].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // scrollbuttontexts[2].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // scrollbuttontexts[3].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // textTomarNota2.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // color botones
            var images = panelGO.GetComponentsInChildren<Image>()
                .Where(img => img.gameObject.name != "Disable")
                .ToArray();
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c))
            {
                images[4].color = images[6].color = images[9].color = images[12].color = c; // boton pedir y bordes
                // iconos de los tres botones con borde
                images[8].color = images[11].color = images[14].color = c; 
                // textos de los tres botones con borde
                scrollbuttontexts[1].color = c;
                scrollbuttontexts[2].color = c;
                scrollbuttontexts[3].color = c;
            
            }

            // Boton pedir: cambiar icono y letra a blanco/negro dependiendo del fondo
            UpdateTextColor(images[4], scrollbuttontexts[0]); // texto 
            UpdateImageColor(images[4], images[5]); // icono 
        }
        else
        {
            // hook up its buttons
            var scrollBtns = panelGO.GetComponentsInChildren<Button>();
            TMP_Text btnLabel = btnGO.GetComponentInChildren<TMP_Text>(true);

            scrollBtns[0].onClick.AddListener(() =>
            {
                if (label != null && int.TryParse(btnLabel.text, out int currentNum))
                    NC.TomarNota(currentNum, "", "", "");
            });

            scrollBtns[1].onClick.AddListener(() => CobroC.CerrarMesa());
            scrollBtns[1].interactable = false; // no interactable cobro
            scrollBtns[2].onClick.AddListener(() => SeguroResetear());
            scrollBtns[2].gameObject.SetActive(false); // Desactivar button resetearmesa

            // Personalizar buttons
            var scrollbuttontexts = panelGO.GetComponentsInChildren<TMP_Text>();
            scrollbuttontexts[0].font = fuenteCamarero;
            scrollbuttontexts[1].font = fuenteCamarero;
            scrollbuttontexts[0].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            scrollbuttontexts[1].fontSize = DataBasePersonalizacion.size_letra_gral[0];
            textTomarNota2.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // color botones
            var images = panelGO.GetComponentsInChildren<Image>()
                .Where(img => img.gameObject.name != "Disable")
                .ToArray();
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c))
            {
                images[4].color = images[6].color = c;
            }
        }

        // track it
        mesasDictionary[mesaNumber] = panelGO;
        if (mesaNumber - 1 >= 0 && mesaNumber - 1 < mesas.Length)
            mesas[mesaNumber - 1] = panelGO;
    }

    public void CreateScrollPedidoInMesa(int mesaNumber)
    {
        if (mesasDictionary.TryGetValue(mesaNumber, out GameObject scrollPanel))
        {
            scrollPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[CrearCamarero] Tried to show scroll for mesa {mesaNumber}, but it doesn't exist.");
        }
    }


    public void SetMesaButtonsInteractable(int mesaNumber, bool interactable)
    {
        if (mesasDictionary.TryGetValue(mesaNumber, out GameObject panelGO))
        {
            var buttons = panelGO.GetComponentsInChildren<Button>();
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {
                SetButtonInteractable(buttons[1], interactable);
                SetButtonInteractable(buttons[2], interactable);
                SetButtonInteractable(buttons[3], interactable);
            }
            else // Mobile
            {
                // Button cerrar mesa
                SetButtonInteractable(buttons[1], interactable);
            }
        }
        else
        {
            Debug.LogWarning("Mesa not found in dictionary for interactable update: " + mesaNumber);
        }
    }

    public void ResetMesaButtonsInteractable(int mesaNumber)
    {
        if (mesasDictionary.TryGetValue(mesaNumber, out GameObject panelGO))
        {
            var buttons = panelGO.GetComponentsInChildren<Button>();
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {
                SetButtonInteractable(buttons[0], true);
                SetButtonInteractable(buttons[1], false);
                SetButtonInteractable(buttons[2], false);
                SetButtonInteractable(buttons[3], false);
            }
            else // Mobile
            {
                // Button cerrar mesa
                SetButtonInteractable(buttons[1], false);
            }
        }
        else
        {
            Debug.LogWarning("Mesa not found in dictionary for interactable update: " + mesaNumber);
        }
    }

    public void SetMesaButtonsPaidInteractable(int mesaNumber, bool interactable)
    {
        if (mesasDictionary.TryGetValue(mesaNumber, out GameObject panelGO))
        {
            var buttons = panelGO.GetComponentsInChildren<Button>();
            if (SceneManager.GetActiveScene().name == "TPVScene")
            {   
                // solo dejar interactuable resetear tras pagar
                SetButtonInteractable(buttons[0], interactable);
                SetButtonInteractable(buttons[1], interactable);
                SetButtonInteractable(buttons[2], interactable);
            }
            else // Mobile REVISAR
            {
                //buttons[1].interactable = interactable;
            }
        }
        else
        {
            Debug.LogWarning("Mesa not found in dictionary for interactable update: " + mesaNumber);
        }
    }

    public void SeguroResetear()
    {
        if (SceneManager.GetActiveScene().name == "TPVScene")
        {
            GameObject prefabEspacioInstance = Instantiate(cuadroResetearBarra, transform.position, Quaternion.identity);
            prefabEspacioInstance.transform.SetParent(canvasMesas.transform, false);
        }
        else
        {
            GameObject prefabEspacioInstance = Instantiate(cuadroResetear, transform.position, Quaternion.identity);
            prefabEspacioInstance.transform.SetParent(canvasMesas.transform, false);
        }
    }
    private void DetalleMesaClick(int mesaNumber, string nombreCliente, string empresa, string menu)
    {
        // hide all existing panels
        foreach (var kv in mesasDictionary)
            kv.Value.SetActive(false);

        // move the container onscreen
        var rtCont = detalleMesaX.GetComponent<RectTransform>();
        if (SceneManager.GetActiveScene().name == "MobileScene")
            rtCont.anchoredPosition = new Vector2(0, 370f);
        else
        {
            rtCont.offsetMin = new Vector2(1350, 0);
            rtCont.offsetMax = Vector2.zero;
        }

        // set title
        if (mesaNumber > 2000)
            textDetalleM.text = "MESA D" + (mesaNumber - 2000) + " (" + nombreCliente + ", " + empresa + ")";
        else if (mesaNumber > 1000)
            textDetalleM.text = "MESA R" + (mesaNumber - 1000) + " (" + nombreCliente + ", " + empresa + ")";
        else
            textDetalleM.text = "MESA " + mesaNumber;

        textDetalleM.font = fuenteCamarero;
        textAdvertenciaBorrar1.font = textAdvertenciaBorrar2.font = textAdvertenciaBorrar3.font = fuenteCamarero;

        // show the one we want
        if (mesasDictionary.TryGetValue(mesaNumber, out var go))
        {
            go.SetActive(true);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = new Vector2(0, -500);
            }
            else
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = new Vector2(0, -250);
            }

            //changetextNumeroMesa
            inputMesa.text = mesaNumber.ToString();
        }
        else
        {
            Debug.LogWarning($"Mesa {mesaNumber} not found in dictionary");
        }

        // Set button pedir not interactable by default when starting a pedido
        buttonPedir.interactable = false;

        // 👇 AÑADIR: comprobar si esta mesa ya está siendo cobrada por el camarero
        if (pagoEnCursoDict.TryGetValue(mesaNumber, out string origenBloqueo) && origenBloqueo == "Camarero")
        {
            CobrandoDesdeOtroSitio.SetActive(true);
            textCobrandoDesdeOtroSitio.text = "Un camarero está cobrando esta mesa";
            SetMesaButtonsInteractable(mesaNumber, false);
        }
        else
        {
            CobrandoDesdeOtroSitio.SetActive(false);
        }
    }
    public void DetalleMesaClickRecogerDelivery() // De momento en desuso
    {
        foreach (var kv in mesasDictionary)
        {
            GameObject mesaGO = kv.Value;

            if (mesaGO == null)
                continue;

            RectTransform rt = mesaGO.GetComponent<RectTransform>();
            if (rt == null)
                continue;

            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin += new Vector2(4000, 0);
            rt.offsetMax += new Vector2(4000, 0);
        }

        RectTransform rt2 = detalleMesaX.GetComponent<RectTransform>();
        rt2.offsetMin = new Vector2(1150, 0);
        rt2.offsetMax = new Vector2(0, 0);

        textDetalleM.text = "MESA Recoger/domicilio";
        textDetalleM.font = fuenteCamarero;
        textAdvertenciaBorrar1.font = fuenteCamarero;
        textAdvertenciaBorrar2.font = fuenteCamarero;
        textAdvertenciaBorrar3.font = fuenteCamarero;

        // Activate the corresponding mesa
        scrollAreaRecoger.SetActive(true);
    }

    public void clickClose()
    {
        detalleMesaX.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000); // Hide it
    }
    public void clickOpen()
    {
        detalleMesaX.GetComponent<RectTransform>().offsetMin = new Vector2(1350, 0);
        detalleMesaX.GetComponent<RectTransform>().offsetMax = Vector2.zero;
    }

    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        Color newColorBotonSec;

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_empl[0], out newColorBotonSec)) // Cambiamos color a los botones secundarios
        {
            // Asignamos el nuevo color al componente Image
            tomarNota2.color = newColorBotonSec;
        }
    }
    void UpdateTextColor(Image boton, TMP_Text text)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        // Cambiar el color del texto basado en la luminancia
        if (luminance > 0.5f)
        {
            // Fondo claro, texto negro
            text.color = Color.black;
        }
        else
        {
            // Fondo oscuro, texto blanco
            text.color = Color.white;
        }
    }

    void ChangeColorBotonesPpal()
    {
        if (SceneManager.GetActiveScene().name != "MobileScene")
            return;

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c))
        {
            button_ticket.color = c;
            button_pago_tarj.color = c;
            button_confirmar_pago.color = c;
            button_pago_junto.color = c;
            button_pago_equit.color = c;
            button_pago_cadauno.color = c;
            button_confirmar_pago_equi.color = c;
            button_confirmar_pago_cadauno.color = c;
            button_confirmar_pago_cadauno.color = c;

            UpdateTextColor(button_ticket, button_ticket.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_pago_tarj, button_pago_tarj.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_confirmar_pago, button_confirmar_pago.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_pago_junto, button_pago_junto.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_pago_equit, button_pago_equit.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_pago_cadauno, button_pago_cadauno.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_confirmar_pago_equi, button_confirmar_pago_equi.GetComponentInChildren<TMP_Text>());
            UpdateTextColor(button_confirmar_pago_cadauno, button_confirmar_pago_cadauno.GetComponentInChildren<TMP_Text>());

            // Botón volver: no se toca el fondo, solo el texto
            button_pago_volver.GetComponentInChildren<TMP_Text>().color = c;
            button_volver_todojunto.GetComponentInChildren<TMP_Text>().color = c;
            button_finalizar_pago_equi.GetComponentInChildren<TMP_Text>().color = c;
            button_finalizar_pago_cadauno.GetComponentInChildren<TMP_Text>().color = c;
        }
    }

    void ChangeColorBotonesPpalTPV()
    {
        if (SceneManager.GetActiveScene().name != "TPVScene")
            return;

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c))
        {
            button_pago_tarj_tpv.color = c;
            button_pago_ef_tpv.color = c;
            button_pago_volver_tpv.color = c;
            button_pago_volver_tpv2.color = c;
            button_pago_junto_tpv.color = c;
            button_pago_equit_tpv.color = c;
            button_pago_cadauno_tpv.color = c;
            button_confirmar_pago_junto_tpv.color = c;
            button_volver_pago_junto_tpv.color = c;
            button_proceder_pago_equi_tpv.color = c;
            button_confirmar_pago_equi_tpv.color = c;
            button_finalizar_pago_equi_tpv.color = c;
            button_volver_pago_equi_tpv.color = c;
            button_confirmar_pago_cadauno_tpv.color = c;
            button_imprimir_cadauno_tpv.color = c;
            button_abrir_caja.color = c;
            entrecuantos1_tpv.color = c;
            entrecuantos2_tpv.color = c;
            entrecuantos3_tpv.color = c;
            button_juntar_mesas.color = c;
            button_separar_mesas.color = c;
            button_cambiar_mesas.color = c;
            button_volver_mesas.color = c;

            UpdateTextColor(button_pago_tarj_tpv, button_pago_tarj_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_pago_tarj_tpv, img_pago_tarj_tpv);
            UpdateTextColor(button_pago_ef_tpv, button_pago_ef_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_pago_ef_tpv, img_pago_ef_tpv);
            UpdateTextColor(button_pago_volver_tpv, button_pago_volver_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_pago_volver_tpv2, button_pago_volver_tpv2.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_pago_junto_tpv, button_pago_junto_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_pago_equit_tpv, button_pago_equit_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_pago_cadauno_tpv, button_pago_cadauno_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_confirmar_pago_junto_tpv, button_confirmar_pago_junto_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_volver_pago_junto_tpv, button_volver_pago_junto_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_proceder_pago_equi_tpv, button_proceder_pago_equi_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_confirmar_pago_equi_tpv, button_confirmar_pago_equi_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_finalizar_pago_equi_tpv, button_finalizar_pago_equi_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_volver_pago_equi_tpv, button_volver_pago_equi_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_volver_pago_equi_tpv, button_volver_pago_equi_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_imprimir_cadauno_tpv, button_imprimir_cadauno_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_abrir_caja, button_abrir_caja.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_abrir_caja, img_abrir_caja);
            UpdateTextColor(entrecuantos2_tpv, entrecuantos2_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(entrecuantos3_tpv, entrecuantos3_tpv.GetComponentInChildren<TMP_Text>(true));
            UpdateTextColor(button_juntar_mesas, button_juntar_mesas.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_juntar_mesas, img_juntar_mesas);
            UpdateTextColor(button_separar_mesas, button_separar_mesas.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_separar_mesas, img_separar_mesas);
            UpdateTextColor(button_cambiar_mesas, button_cambiar_mesas.GetComponentInChildren<TMP_Text>(true));
            UpdateImageColor(button_cambiar_mesas, img_cambiar_mesas);
            UpdateTextColor(button_volver_mesas, button_volver_mesas.GetComponentInChildren<TMP_Text>(true));
        }
    }

    void ChangeColorBotonesSecTPV() 
    {
        if (SceneManager.GetActiveScene().name != "TPVScene")
            return;

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_empl[0], out Color c))
        {
            BadgeMesas.color = c;
            // ... los que quieras que usen col_sec_empl

            // UpdateTextColor(button_X, button_X.GetComponentInChildren<TMP_Text>(true));
        }
    }

    void UpdateImageColor(Image boton, Image icono)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        // Cambiar el color del texto basado en la luminancia
        if (luminance > 0.5f)
        {
            // Fondo claro, texto negro
            icono.color = Color.black;
        }
        else
        {
            // Fondo oscuro, texto blanco
            icono.color = Color.white;
        }
    }

    Color LightenColor(Color baseColor, float blendWithWhite = 0.75f)
    {
        // blendWithWhite: 0 = color puro, 1 = blanco puro. 0.75 = 75% blanco, 25% color original
        return Color.Lerp(baseColor, Color.white, blendWithWhite);
    }

    // Cambiar colores botones cuando se deshabilitan
    void SetButtonInteractable(Button button, bool interactable)
    {
        button.interactable = interactable;

        Transform disableOverlay = button.transform.Find("Disable");
        if (disableOverlay != null)
            disableOverlay.gameObject.SetActive(!interactable);
    }

    /// <summary>
    /// Cobros
    /// </summary>
    public void Cobrar()
    {
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            N.ButtonPagar();
        }
        else
        {
            comoVanAPagar.SetActive(true); 
            blurTicket.SetActive(true);
        }
    }

        public void ComoVanAPagar(bool tarjetaInput)
    {
        textInfoPago.gameObject.SetActive(false); 
        tarjeta = tarjetaInput;

        switch (tipoRepartoSeleccionado)
        {
            case TipoReparto.Junto:
                ticket.SetActive(true);
                CrearTicket(false, false);
                if (!tarjeta) calculadora.SetActive(true);

                button_volver_confirmar_pago.onClick.RemoveAllListeners(); // NUEVO
                button_volver_confirmar_pago.onClick.AddListener(VolverDesdeConfirmarPagoJunto); // NUEVO
                break;

            case TipoReparto.Equitativo:
                ticket.SetActive(true);
                ClearTicket();
                CrearTicket(true, false);
                if (!tarjeta) calculadora.SetActive(true);

                button_volver_confirmar_pago.onClick.RemoveAllListeners(); // NUEVO
                button_volver_confirmar_pago.onClick.AddListener(VolverDesdeConfirmarPagoEquitativo); // NUEVO
                break;

            case TipoReparto.CadaUno:
                MostrarTicketSoloSeleccionados();
                button_volver_seleccion_items_tpv.gameObject.SetActive(true);
                ticket.SetActive(true);
                if (!tarjeta) calculadora.SetActive(true);

                totalPrecioTicket.gameObject.SetActive(false); 
                totalTicket.gameObject.SetActive(false); 

                SetButtonInteractable(buttonImprimirCadaUno.GetComponent<Button>(), true);
                var btnImprimir = buttonImprimirCadaUno.GetComponent<Button>();
                btnImprimir.onClick.RemoveAllListeners();
                btnImprimir.onClick.AddListener(ImprimirTicketCadaUnoSeleccionado);

                var btnConfirm = buttonConfirmarPagoCadaUno.GetComponent<Button>();
                btnConfirm.onClick.RemoveAllListeners();
                btnConfirm.onClick.AddListener(ConfirmarPagoCadaUnoRonda);
                buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Confirmar pago";

                button_volver_confirmar_pago.onClick.RemoveAllListeners(); // NUEVO
                button_volver_confirmar_pago.onClick.AddListener(VolverDesdeConfirmarPagoCadaUno); // NUEVO
                break;
        }
    }

    public void VolverDesdeSeleccionarMetodoPago()
    {
        seleccioneMetodoPago.SetActive(false);

        if (tipoRepartoSeleccionado == TipoReparto.Equitativo)
        {
            totalSum = CalcularTotalMesa();
            ResultanteEquitativo(numeroPersonas);
        }
        else if (tipoRepartoSeleccionado == TipoReparto.CadaUno) 
        {
            textInfoPago.gameObject.SetActive(false);
            ticket.SetActive(true);
            SetButtonInteractable(buttonConfirmarPagoCadaUno.GetComponent<Button>(), AnyItemSelected());

            button_volver_confirmar_pago.gameObject.SetActive(!pagoParcialConfirmadoCadaUno); // NUEVO ← esta es la línea que faltaba

            button_volver_confirmar_pago.onClick.RemoveAllListeners();
            button_volver_confirmar_pago.onClick.AddListener(VolverDesdeSeleccionArticulosCadaUno);

            var btnConfirm = buttonConfirmarPagoCadaUno.GetComponent<Button>();
            btnConfirm.onClick.RemoveAllListeners();
            btnConfirm.onClick.AddListener(ContinuarSeleccionCadaUno);
            buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Continuar";
        }
    }

        public void VolverDesdeConfirmarPagoCadaUno()
    {
        ticket.SetActive(false);
        calculadora.SetActive(false);

        foreach (var pair in buttonElegirPagarDictionary)
        {
            GameObject btnObj = pair.Value;
            Transform child2 = btnObj.transform.GetChild(2); // "Pagado"
            bool yaPagado = child2 != null && child2.gameObject.activeSelf; // NUEVO
            bool seleccionado = clickCounts.TryGetValue(pair.Key, out int c) && c % 2 == 1;

            btnObj.SetActive(true);

            var btn = btnObj.GetComponent<Button>();
            if (btn != null) btn.interactable = !yaPagado; // CAMBIADO: antes era "= true"

            var img = btnObj.GetComponent<Image>();
            if (img != null)
            {
                if (yaPagado) // NUEVO: los ya pagados se quedan en gris, ignorando selección
                {
                    ColorUtility.TryParseHtmlString("#787878", out Color grey);
                    img.color = grey;
                }
                else if (seleccionado && ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c2))
                    img.color = LightenColor(c2);
                else
                    img.color = Color.white;
            }
        }

        seleccioneMetodoPago.SetActive(true);
        textInfoPago.gameObject.SetActive(true);
        int seleccionados = 0;
        foreach (var count in clickCounts.Values)
            if (count % 2 == 1) seleccionados++;
        textInfoPago.text = $"Pago de {seleccionados} elemento{(seleccionados > 1 ? "s" : "")}";
        button_pago_volver_tpv.gameObject.SetActive(true);
    }

    public void VolverDesdeSeleccionArticulosCadaUno()
    {
        ticket.SetActive(false);
        calculadora.SetActive(false);

        if (pagoParcialConfirmadoCadaUno) // CAMBIADO: antes era "personasPagadas > 1"
        {
            button_volver_confirmar_pago.gameObject.SetActive(false);
        }
    }

    // NUEVO: volver desde "confirmar pago" en modo Todo Junto
    public void VolverDesdeConfirmarPagoJunto()
    {
        ticket.SetActive(false);
        calculadora.SetActive(false);
        seleccioneMetodoPago.SetActive(true);
        button_pago_volver_tpv.gameObject.SetActive(true);
    }

    // NUEVO: volver desde "confirmar pago" en modo Equitativo
    public void VolverDesdeConfirmarPagoEquitativo()
    {
        ticket.SetActive(false);
        calculadora.SetActive(false);
        seleccioneMetodoPago.SetActive(true);
        textInfoPago.gameObject.SetActive(true);
        button_pago_volver_tpv.gameObject.SetActive(!pagoParcialConfirmadoEquitativo); // CAMBIADO: antes era "= true" fijo
    }

    public void TodoJunto()
    {
        //comoVanAPagar.SetActive(false);
        tipoRepartoSeleccionado = TipoReparto.Junto;
        textInfoPago.gameObject.SetActive(false); 
        seleccioneMetodoPago.SetActive(true);
    }

    public void EntreCuantas()
    {
        tipoRepartoSeleccionado = TipoReparto.Equitativo;
        totalSum = CalcularTotalMesa(); 
        entreCuantas.SetActive(true);
        textNPersonas.text = "2"; 
        ResultanteEquitativo(2);
        pagoParcialConfirmadoEquitativo = false; // NUEVO
        button_pago_volver_tpv.gameObject.SetActive(true);
    }


    public void ProcederAlPagoEquitativo()
    {
        //entreCuantas.SetActive(false);
        personasPagadas = 0;
        MostrarSeleccionMetodoPagoEquitativo();
    }

    private float CalcularTotalMesa()
    {
        Transform thirdActive = null;
        int count = 0;
        foreach (Transform child in detalleMesaX.transform)
        {
            if (child.gameObject.activeSelf && ++count == 3)
            {
                thirdActive = child;
                break;
            }
        }

        if (thirdActive == null) return 0f;

        GameObject scrollSpecificMesa = thirdActive.gameObject;
        GameObject contentSpecificMesa = scrollSpecificMesa.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        float sum = 0f;
        foreach (Transform espacioTransform in contentSpecificMesa.transform)
        {
            if (espacioTransform.name.Contains("CamareroTPVOrdenHeader")) continue; // 👈 ignorar separadores de orden

            var espacio = espacioTransform.GetChild(0).gameObject;
            var precioEspacio = espacio.transform.GetChild(3);
            sum += ExtractFloat(precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text);
        }
        return sum;
    }

    private void MostrarSeleccionMetodoPagoEquitativo()
    {
        seleccioneMetodoPago.SetActive(true);
        textInfoPago.gameObject.SetActive(true); 
        textInfoPago.text = $"Pago {personasPagadas + 1}/{numeroPersonas}";
    }

    public void CadaUnoLoSuyo()
    {
        //comoVanAPagar.SetActive(false);
        tipoRepartoSeleccionado = TipoReparto.CadaUno;
        clickCounts.Clear();
        buttonElegirPagarDictionary.Clear(); 
        pagoParcialConfirmadoCadaUno = false; 
        ClearTicket();
        CrearTicket(false, true);

        ticket.SetActive(true);
        calculadora.SetActive(false);

        totalPrecioTicket.gameObject.SetActive(true);
        totalTicket.gameObject.SetActive(true);

        buttonConfirmarPago.SetActive(false);
        buttonFinalizar.SetActive(false);

        buttonConfirmarPagoCadaUno.SetActive(true);
        SetButtonInteractable(buttonConfirmarPagoCadaUno.GetComponent<Button>(), false); 

        button_volver_confirmar_pago.gameObject.SetActive(true); // NUEVO
        button_volver_confirmar_pago.onClick.RemoveAllListeners(); // NUEVO
        button_volver_confirmar_pago.onClick.AddListener(VolverDesdeSeleccionArticulosCadaUno); // NUEVO

        var btnConfirm = buttonConfirmarPagoCadaUno.GetComponent<Button>();
        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.AddListener(ContinuarSeleccionCadaUno);
        buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Continuar";
    }

    public void ContinuarSeleccionCadaUno()
    {
        int seleccionados = 0;
        foreach (var count in clickCounts.Values)
            if (count % 2 == 1) seleccionados++;

        if (seleccionados == 0)
        {
            Debug.LogWarning("⚠️ No se ha seleccionado ningún elemento.");
            return;
        }

        ticket.SetActive(false);

        seleccioneMetodoPago.SetActive(true);
        textInfoPago.gameObject.SetActive(true); 
        textInfoPago.text = $"Pago de {seleccionados} elemento{(seleccionados > 1 ? "s" : "")}";
        button_pago_volver_tpv.gameObject.SetActive(true); // REVERTIDO: siempre visible aquí, según pediste
    }

    private void MostrarTicketSoloSeleccionados()
    {
        foreach (var pair in buttonElegirPagarDictionary)
        {
            int index = pair.Key;
            GameObject btnObj = pair.Value;
            bool seleccionado = clickCounts.TryGetValue(index, out int count) && count % 2 == 1;

            btnObj.SetActive(seleccionado);
            var btn = btnObj.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            var img = btnObj.GetComponent<Image>();
            if (img != null) img.color = Color.white; // 👈 resetear el naranja
        }

        totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
    }

   public void ImprimirTicketCadaUnoSeleccionado()
    {
        var productos = new List<POSPrinterManager.Producto>();

        foreach (var pair in buttonElegirPagarDictionary)
        {
            GameObject btnObj = pair.Value;

            if (!btnObj.activeSelf) continue; // solo los de esta ronda

            var childTexts = btnObj.GetComponentsInChildren<TMP_Text>();
            string descripcion = childTexts[0].text;
            float precio = ExtractFloat(childTexts[1].text);

            productos.Add(new POSPrinterManager.Producto
            {
                Cantidad = 1,
                Descripcion = descripcion,
                PrecioUnitario = (decimal)precio,
                Opciones = ""
            });
        }

        if (productos.Count == 0)
        {
            Debug.LogWarning("No hay elementos seleccionados para imprimir.");
            return;
        }

        PPM.PrintTicketParcial(int.Parse(inputMesa.text), productos);
    }

    public void ConfirmarPagoCadaUnoRonda()
    {        
        NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "TPV"); // 👈 AÑADIR
    
        pagoParcialConfirmadoCadaUno = true; 
        foreach (var pair in buttonElegirPagarDictionary)
        {
            int index = pair.Key;
            GameObject buttonObj = pair.Value;

            if (clickCounts.TryGetValue(index, out int count) && count % 2 == 1)
            {
                Transform child2 = buttonObj.transform.GetChild(2); // "Pagado"
                if (child2 != null)
                    child2.gameObject.SetActive(true);

                clickCounts[index] = 0; // ya pagado
            }
        }

        totalSum = 0f;
        totalPrecioAPagar.text = "0,00€";
        CC.Clear();
        ticket.SetActive(false);
        calculadora.SetActive(false);

        if (!AreAllButtonsPaid())
        {
            CadaUnoLoSuyoSiguienteRonda();
        }
        else
        {
            buttonConfirmarPagoCadaUno.SetActive(false);
            clickCounts.Clear();
            FinalizarPagado();
        }
    }

    private void CadaUnoLoSuyoSiguienteRonda()
    {
        foreach (var pair in buttonElegirPagarDictionary)
        {
            GameObject itemGO = pair.Value;
            Transform child2 = itemGO.transform.GetChild(2);
            bool yaPagado = child2 != null && child2.gameObject.activeSelf;

            itemGO.SetActive(true);

            var img = itemGO.GetComponent<Image>();
            if (img != null) img.color = Color.white;

            var btnPago = itemGO.GetComponent<Button>();
            if (btnPago != null)
                btnPago.interactable = !yaPagado;
        }

        ticket.SetActive(true);
        buttonConfirmarPago.SetActive(false);
        buttonConfirmarPagoCadaUno.SetActive(true);

        SetButtonInteractable(buttonImprimirCadaUno.GetComponent<Button>(), false); // 👈 deshabilitado también aquí

        totalPrecioTicket.gameObject.SetActive(true);
        totalTicket.gameObject.SetActive(true);

        var btnConfirmar = buttonConfirmarPagoCadaUno.GetComponent<Button>();
        btnConfirmar.onClick.RemoveAllListeners();
        btnConfirmar.onClick.AddListener(ContinuarSeleccionCadaUno);
        buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Continuar";

        button_volver_seleccion_items_tpv.gameObject.SetActive(false); 
        SetButtonInteractable(buttonConfirmarPagoCadaUno.GetComponent<Button>(), AnyItemSelected()); 
        button_volver_confirmar_pago.gameObject.SetActive(false); 
    }

    public void OnClickConfirmarPago()
    {
        NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "TPV"); // 👈 AÑADIR

        if (tipoRepartoSeleccionado == TipoReparto.CadaUno)
            ConfirmarPagoCadaUnoRonda();
        else
            FinalizarPagado();
    }

    private void CrearTicket(bool equitativo, bool cadaUno)
    {
        // Recoger platos en mesa y asignar en contentTicket como prefabs nuevos
        Transform thirdActive = null;
        int count = 0;
        foreach (Transform child in detalleMesaX.transform)
        {
            if (child.gameObject.activeSelf && ++count == 3)
            {
                thirdActive = child;
                break;
            }
        }

        GameObject scrollSpecificMesa = thirdActive.gameObject;
        GameObject contentSpecificMesa = scrollSpecificMesa.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        totalSum = 0;
        totalSumCadaUno = 0;
        int currentIndex = 0;
        foreach (Transform espacioTransform in contentSpecificMesa.transform)
        {
            if (espacioTransform.name.Contains("CamareroTPVOrdenHeader")) continue; // 👈 ignorar separadores de orden

            var espacio = espacioTransform.GetChild(0).gameObject; // inside Toggle

            var textEspacio1 = espacio.transform.GetChild(1);
            var cantidadEspacio1 = espacio.transform.GetChild(2);
            var precioEspacio = espacio.transform.GetChild(3);

            int cantidad = int.Parse(cantidadEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text);


            if (cadaUno)
            {
                float precioLineaTotal = ExtractFloat(precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text);
                float precioUnidad = cantidad > 0 ? precioLineaTotal / cantidad : precioLineaTotal;

                for (int i = 0; i < cantidad; i++)
                {
                    // Crear prefab por cada unidad
                    var pagarPlato = Instantiate(prefabButtonElegirPagarTPV, transform.position, Quaternion.identity);
                    pagarPlato.transform.SetParent(contentTicket.transform, false);

                    var childTexts = pagarPlato.GetComponentsInChildren<TMP_Text>();
                    childTexts[0].text = textEspacio1.GetComponent<TMP_Text>().text;
                    childTexts[1].text = precioUnidad.ToString("0.00").Replace(".", ",") + "€";

                    // 👇 Nuevo: asegurarnos de que "Pagado" empieza oculto
                    Transform child2 = pagarPlato.transform.GetChild(2);
                    if (child2 != null)
                        child2.gameObject.SetActive(false);

                    int indexCopy = currentIndex;
                    float priceFloat = precioUnidad;
                    pagarPlato.GetComponent<Button>().onClick.AddListener(() => OnButtonSelected(childTexts[0].text, priceFloat, indexCopy));
                    buttonElegirPagarDictionary[indexCopy] = pagarPlato;
                    currentIndex++;

                    totalSumCadaUno += priceFloat;
                    totalPrecioTicket.text = totalSumCadaUno.ToString("0.00").Replace(".", ",") + "€";
                }
            }

            else
            {
                // crear prefab Normal en contentTicket asignando textos
                var pagarPlato = Instantiate(prefabPagarPlatoTPV, transform.position, Quaternion.identity);
                pagarPlato.transform.SetParent(contentTicket.transform, false);

                var childTexts = pagarPlato.GetComponentsInChildren<TMP_Text>();
                childTexts[0].text = textEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text;
                childTexts[1].text = cantidadEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text;
                childTexts[2].text = precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text;
                float floatVal = ExtractFloat(precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text);
                totalSum += floatVal;
                totalPrecioTicket.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
            }
        }

        if (equitativo)
        {
            totalPrecioAPagar.text = totalSumEquitativo.ToString("0.00").Replace(".", ",") + "€";
            // Cambiar botones
            buttonConfirmarPago.SetActive(false);
            buttonConfirmarPagoEquitativo.SetActive(true);

            buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
        }
        else if (cadaUno)
        {
            totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
            buttonConfirmarPago.SetActive(false);
            buttonConfirmarPagoCadaUno.SetActive(true);
            buttonImprimirCadaUno.SetActive(true);
            SetButtonInteractable(buttonImprimirCadaUno.GetComponent<Button>(), false); // 👈 deshabilitado en pantalla de selección
            buttonFinalizar.SetActive(true);

            personasPagadas = 1;
            buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + personasPagadas;
        }
        else
        {
            totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
        }
    }

    public void ClearTicket()
    {
        foreach (Transform child in contentTicket.transform)
        {
            Destroy(child.gameObject);
        }

        totalSum = 0;
        totalSumCadaUno = 0;
        totalPrecioTicket.text = "0,00€";
        totalPrecioAPagar.text = "0,00€";
        buttonConfirmarPago.SetActive(true);
        buttonConfirmarPagoCadaUno.SetActive(false);
        buttonImprimirCadaUno.SetActive(false);
        buttonConfirmarPagoEquitativo.SetActive(false);
        buttonFinalizar.SetActive(false);
    }

    private void OnButtonSelected(string name, float price, int i)
    {
        var buttonObj = EventSystem.current.currentSelectedGameObject;
        if (buttonObj == null) return;

        if (!clickCounts.ContainsKey(i)) clickCounts[i] = 0;
        clickCounts[i]++;

        var image = buttonObj.GetComponent<Image>();
        if (image == null) return;

        if (clickCounts[i] % 2 == 0)
        {
            image.color = Color.white;
            totalSum -= price;
        }
        else
        {
            if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out Color c))
                image.color = LightenColor(c); // 👈 aclarado, en vez del color puro
            else
                image.color = Color.white;
            totalSum += price;
        }

        totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
        CC.UpdateUI();
        SetButtonInteractable(buttonConfirmarPagoCadaUno.GetComponent<Button>(), AnyItemSelected()); 
    }

    private bool AnyItemSelected()
    {
        foreach (var count in clickCounts.Values)
            if (count % 2 == 1) return true;
        return false;
    }

    public void ConfirmarPagoEquitativo()
    {             
        NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "TPV"); // 👈 AÑADIR
    
        personasPagadas++;
        pagoParcialConfirmadoEquitativo = true; // NUEVO

        ticket.SetActive(false);
        calculadora.SetActive(false);
        CC.Clear();

        if (personasPagadas < numeroPersonas )
        {
            buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
            MostrarSeleccionMetodoPagoEquitativo();
            button_pago_volver_tpv.gameObject.SetActive(false); // esto ya lo tenías, se queda igual
        }
        else
        {
            buttonConfirmarPagoEquitativo.SetActive(false);
            FinalizarPagado();
        }
    }

    public void ConfirmarPagoCadaUno()
    {
        foreach (var pair in buttonElegirPagarDictionary)
        {
            int index = pair.Key;
            GameObject buttonObj = pair.Value;

            if (clickCounts.TryGetValue(index, out int count) && count % 2 == 1)
            {
                ColorUtility.TryParseHtmlString("#787878", out Color greyColor);

                Transform child0 = buttonObj.transform.GetChild(0);
                Transform child1 = buttonObj.transform.GetChild(1);
                Transform child2 = buttonObj.transform.GetChild(2); // "Pagado"

                if (child0.TryGetComponent<TMP_Text>(out TMP_Text text0))
                    text0.color = greyColor;

                if (child1.TryGetComponent<TMP_Text>(out TMP_Text text1))
                    text1.color = greyColor;

                if (child2 != null)
                    child2.gameObject.SetActive(true); // ✅ Show "Pagado"

                // Volver boton a blanco
                var image = buttonObj.GetComponent<Image>();
                image.color = Color.white;
            }
        }

        personasPagadas++;
        buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + personasPagadas;
        totalSum = 0f;
        totalPrecioAPagar.text = "0,00€";
        CC.Clear();

        if (!AreAllButtonsPaid())
        {
            buttonConfirmarPagoCadaUno.GetComponent<Button>().interactable = true;
            buttonFinalizar.GetComponent<Button>().interactable = false;

        }
        else  // Finalizar payment step
        {
            buttonConfirmarPagoCadaUno.GetComponent<Button>().interactable = false;
            buttonFinalizar.GetComponent<Button>().interactable = true;

            clickCounts.Clear();
        }
    }

    private bool AreAllButtonsPaid()
    {
        foreach (var button in buttonElegirPagarDictionary.Values)
        {
            Transform child2 = button.transform.GetChild(2); // "Pagado"
            if (child2 == null || !child2.gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }

    public void FinalizarPagado()
    {
        var localReceiver = FindLocalReceiver();
        if (localReceiver != null)
        {
            if (movimientosCaja != null && !string.IsNullOrWhiteSpace(textIdTurno.text))
            {
                localReceiver.SendColorizeButtonPagado("Todo", 0, int.Parse(inputMesa.text));
                NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdClearPagoEnCurso(int.Parse(inputMesa.text));

                // de momento solo consideramos Movimientos de Caja a cash no relacionado con pagos de comandas, quito esto:
                // string tipo = "IngresoEfectivo";
                // if (tarjeta)
                // {
                //    tipo = "IngresoTarjeta";
                // }

                // StartCoroutine(movimientosCaja.AddMovimientoCaja(
                //    tipo,
                //    "Pago cliente mesa " + inputMesa.text,
                //    totalSum
                // ));


                // Dejar botones no interactuables excepto resetear
                SetMesaButtonsPaidInteractable(int.Parse(inputMesa.text), false);

                ClearTicket();
                ticket.SetActive(false);      
                calculadora.SetActive(false);

                seleccioneMetodoPago.SetActive(false);
                entreCuantas.SetActive(false);
                comoVanAPagar.SetActive(false);

                blurTicket.SetActive(false); 
            }
            else
            {
                Debug.LogError("MovimientosCaja or textIdTurno missing!");
                advertenciaTurnoNoEmpezado.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Local PaymentConfirmationReceiver not found.");
        }
    }

    PaymentConfirmationReceiver FindLocalReceiver()
    {
        foreach (var receiver in FindObjectsOfType<PaymentConfirmationReceiver>())
        {
            var netBehaviour = receiver.GetComponent<NetworkBehaviour>();
            if (netBehaviour != null && netBehaviour.isLocalPlayer)
            {
                return receiver;
            }
        }
        return null;
    }

    public void masPersonas()
    {
        if (int.TryParse(textNPersonas.text, out int personasInt))
        {
            personasInt++;
            textNPersonas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);
            textNPersonas.font = fuenteGeneral;
        }
    }

    public void menosPersonas()
    {
        if (int.TryParse(textNPersonas.text, out int personasInt) && personasInt > 1)
        {
            personasInt--;
            textNPersonas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);
            textNPersonas.font = fuenteGeneral;
        }
    }

    private void ResultanteEquitativo(int numPersonas)
    {
        totalSumEquitativo = totalSum / numPersonas;
        total.text = $"{totalSum:F2} €";
        totalCadaUno.text = $"{totalSumEquitativo:F2} €";
        numeroPersonas = numPersonas;
        totalCadaUno.font = fuenteGeneral;
    }

    float ExtractFloat(string input)
    {
        // Using regular expressions to find the float value
        Match match = Regex.Match(input, @"(\d+,\d+)");
        if (match.Success)
        {
            // Convert comma to dot for parsing the float value
            string floatValueString = match.Groups[0].Value.Replace(',', '.');
            return float.Parse(floatValueString, CultureInfo.InvariantCulture);
        }
        else
        {
            return float.NaN; // Return NaN (Not a Number) to indicate failure
        }
    }

    void SetDetalleMesaTextAlpha(float alpha)
    {
        Color c = textDetalleM.color;
        c.a = alpha;
        textDetalleM.color = c;
    }

    public void SetPagoEnCursoUI(int mesaNumber, bool enCurso, string origen) // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] SetPagoEnCursoUI llamado: mesa={mesaNumber}, enCurso={enCurso}, origen={origen}, mesaAbiertaActual={inputMesa.text}"); // 👈 AÑADIR temporal
        if (enCurso)
            pagoEnCursoDict[mesaNumber] = origen;
        else
            pagoEnCursoDict.Remove(mesaNumber);

        // Solo actualizar visualmente si es la mesa que tienes abierta ahora mismo
        if (int.TryParse(inputMesa.text, out int mesaAbierta) && mesaAbierta == mesaNumber)
        {
            bool bloqueadaPorCamarero = enCurso && origen == "Camarero";
            CobrandoDesdeOtroSitio.SetActive(bloqueadaPorCamarero);
            if (bloqueadaPorCamarero)
                textCobrandoDesdeOtroSitio.text = "Un camarero está cobrando esta mesa";

            SetMesaButtonsInteractable(mesaNumber, !bloqueadaPorCamarero); // reutiliza el método que ya tienes
        }
    }

}
