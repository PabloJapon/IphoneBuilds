using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{
    // Zonas
    public GameObject zonaCliente;
    public GameObject zonaCamarero;

    // Zona Cliente
    public GameObject canvasQR;
    public GameObject canvasIntro;
    public GameObject canvasIntro2;
    public GameObject canvasMenu;
    public GameObject canvasPedido;
    public GameObject canvasAtendido;
    public GameObject imageNotificacion;
    public GameObject detallePlatoX;
    public GameObject detalleMesa;
    public GameObject buttonLlamar2;

    // Zona camarero
    public GameObject canvasBarraCamarero;
    public NavigationCamarero NC;
    private Coroutine camareroConnectCoroutine;
    private string idGuardadoCamarero;

    public GameObject canvasInicioSesion;        // canva para meter codigo camarero
    public GameObject canvasBienvenidaCamarero;
    public GameObject panelMisTurnos;
    public GameObject panelMisFichajes;
    public MiTurnoController miTurnoController;
    public MisFichajesController misFichajesController;

    public enum DestinoCamarero { Ninguno, Turnos, Fichajes }
    public static DestinoCamarero destinoPendiente = DestinoCamarero.Ninguno;

    // Pagar
    public GameObject canvasPrePagar;
    public GameObject realizarPago;
    public GameObject buttonPagar;

    // Check QR
    public TMP_Text nMesa;
    public TMP_Text id;
    public static string idRestaurante;
    public static string idNumeroMesa;

    // Bool camarero
    public static bool camarero = false;

    // ----- Sistema de botón atrás (Android) -----
    private readonly Stack<Action> historial = new Stack<Action>();
    private Action pantallaActual;
    private bool modalAtendidoAbierto = false;

    public void Start()
    {
        zonaCliente.SetActive(true);
        canvasIntro.SetActive(true);
        canvasPedido.SetActive(true);
        zonaCamarero.SetActive(true);
        canvasAtendido.SetActive(true);
        canvasPrePagar.SetActive(true);
        realizarPago.SetActive(true);
        buttonLlamar2.SetActive(true);
        canvasBarraCamarero.SetActive(false);
        buttonPagar.SetActive(false);
        StartCoroutine(CanvasPedidoTime());

        if (miTurnoController != null)
            miTurnoController.OnBackButtonPressed += () => Volver();

        if (misFichajesController != null)
            misFichajesController.OnBackButtonPressed += () => Volver();

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            if (PlayerPrefs.GetInt("es_dispositivo_camarero", 0) == 1)
            {
                idGuardadoCamarero = PlayerPrefs.GetString("camarero_restaurant_id", "");
                camarero = true;
                EstablecerRaiz(MostrarBienvenidaCamarero);
            }
            else
            {
                EstablecerRaiz(MostrarPantallaQR);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Volver();
        }
    }

    // ----- Motor de navegación / botón atrás -----

    // Navega a una pantalla nueva, guardando la actual en el historial
    // para poder volver a ella con el botón atrás.
    private void IrA(Action mostrarPantalla)
    {
        if (pantallaActual != null)
            historial.Push(pantallaActual);
        pantallaActual = mostrarPantalla;
        mostrarPantalla.Invoke();
    }

    // Cambia de pantalla SIN apilar la actual (pantallas de paso, como el
    // login, a las que no queremos volver al pulsar atrás).
    private void Reemplazar(Action mostrarPantalla)
    {
        pantallaActual = mostrarPantalla;
        mostrarPantalla.Invoke();
    }

    // Marca una pantalla como raíz: vacía el historial (no hay "atrás"
    // antes de ella salvo salir de la app).
    private void EstablecerRaiz(Action mostrarPantalla)
    {
        historial.Clear();
        pantallaActual = mostrarPantalla;
        mostrarPantalla.Invoke();
    }

    public void Volver()
    {
        if (modalAtendidoAbierto)
        {
            ButtonNoAtendido();
            return;
        }

        if (historial.Count > 0)
        {
            pantallaActual = historial.Pop();
            pantallaActual.Invoke();
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    IEnumerator CanvasPedidoTime()
    {
        yield return new WaitForSeconds(1f);
        canvasPedido.SetActive(false);
        canvasAtendido.SetActive(false);
        realizarPago.SetActive(false);
        buttonLlamar2.SetActive(false);
        canvasIntro.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MobileScene" && !camarero)
        {
            zonaCamarero.GetComponentInChildren<Canvas>().enabled = false;
        }
    }

    // ----- Botones del canvas de bienvenida (camarero recurrente) -----

    private void MostrarBienvenidaCamarero()
    {
        canvasQR?.SetActive(false);
        canvasIntro2?.SetActive(false);
        canvasMenu?.SetActive(false);
        canvasPedido?.SetActive(false);
        canvasAtendido?.SetActive(false);
        canvasInicioSesion.SetActive(false);
        panelMisTurnos.SetActive(false);
        panelMisFichajes.SetActive(false);
        zonaCliente.SetActive(false);
        canvasBienvenidaCamarero.SetActive(true);
    }

    private void MostrarQRDesdeBienvenida()
    {
        canvasBienvenidaCamarero.SetActive(false);
        canvasInicioSesion.SetActive(false);
        canvasBarraCamarero.SetActive(false);
        panelMisTurnos.SetActive(false);
        panelMisFichajes.SetActive(false);
        zonaCliente.SetActive(true);
        zonaCamarero.GetComponentInChildren<Canvas>().enabled = false;
        canvasQR.SetActive(true);
        camarero = false;
    }

    public void ButtonQRDesdeBienvenida()
    {
        // El flujo de QR es login real (requiere fichaje), no una consulta.
        destinoPendiente = DestinoCamarero.Ninguno;

        if (camareroConnectCoroutine != null)
        {
            StopCoroutine(camareroConnectCoroutine);
            camareroConnectCoroutine = null;
        }

        canvasBienvenidaCamarero.SetActive(false);
        TestButtonCamarero(idGuardadoCamarero);
    }

    private void MostrarLoginCamarero()
    {
        canvasBienvenidaCamarero.SetActive(false);
        canvasInicioSesion.SetActive(true);
    }

    public void ButtonMisTurnos()
    {
        destinoPendiente = DestinoCamarero.Turnos;
        idRestaurante = idGuardadoCamarero;
        id.text = idGuardadoCamarero;
        IrA(MostrarLoginCamarero);
    }

    public void ButtonMisFichajes()
    {
        destinoPendiente = DestinoCamarero.Fichajes;
        idRestaurante = idGuardadoCamarero;
        id.text = idGuardadoCamarero;
        IrA(MostrarLoginCamarero);
    }

    public void MostrarDestinoTrasLogin()
    {
        switch (destinoPendiente)
        {
            case DestinoCamarero.Turnos:
                Reemplazar(() =>
                {
                    panelMisTurnos.SetActive(true);
                    panelMisFichajes.SetActive(false);
                });
                break;
            case DestinoCamarero.Fichajes:
                Reemplazar(() =>
                {
                    panelMisFichajes.SetActive(true);
                    panelMisTurnos.SetActive(false);
                });
                break;
        }
        destinoPendiente = DestinoCamarero.Ninguno;
    }

    // Zonas
    private void MostrarZonaCliente()
    {
        zonaCliente.SetActive(true);
        canvasBarraCamarero.SetActive(false);
        zonaCamarero.GetComponentInChildren<Canvas>().enabled = false;
        camarero = false;
    }

    public void ZonaCliente()
    {
        IrA(MostrarZonaCliente);
    }

    private void MostrarZonaCamarero()
    {
        zonaCliente.SetActive(false);
        zonaCamarero.GetComponentInChildren<Canvas>().enabled = true;
        detalleMesa.transform.position = new Vector3(0, 60, 0); // Esconderlo
        canvasBarraCamarero.SetActive(true);
        camarero = true;
    }

    public void ZonaCamarero()
    {
        IrA(MostrarZonaCamarero);
    }

    public void ZonaResponsable()
    {
        zonaCliente.SetActive(false);
        zonaCamarero.GetComponentInChildren<Canvas>().enabled = false;
    }

    // Zona Cliente
    public void ProcessQRCodeResult(string qrCodeResult)
    {
        Debug.Log("QR Code Result: " + qrCodeResult);

        string[] splitData = qrCodeResult.Split(';');
        if (splitData.Length < 2)
        {
            Debug.LogError("QR Code result does not contain enough data!");
            return;
        }

        if (nMesa == null) Debug.LogError("nMesa is NULL!");
        else nMesa.text = splitData[0];

        if (id == null) Debug.LogError("id is NULL!");
        else id.text = splitData[1];

        idNumeroMesa = splitData[0];
        idRestaurante = splitData[1];

        if (idNumeroMesa == "Camarero")
        {
            if (camareroConnectCoroutine != null)
            {
                StopCoroutine(camareroConnectCoroutine);
                camareroConnectCoroutine = null;
            }

            idNumeroMesa = "0";

            bool esPrimeraVez = PlayerPrefs.GetInt("es_dispositivo_camarero", 0) == 0;

            GuardarDispositivoCamarero(idRestaurante);
            idGuardadoCamarero = idRestaurante;
            camarero = true;

            if (esPrimeraVez)
            {
                EstablecerRaiz(MostrarBienvenidaCamarero);
            }
            else
            {
                TestButtonCamarero(idRestaurante);
            }

            return;
        }

        camarero = false;

        StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante));

        IrA(MostrarPantallaMenu);
        canvasPrePagar?.SetActive(true);

        StartCoroutine(StartClientWithDelay());
    }

    private IEnumerator WaitForLocalPlayerAndSendID(string restaurantID)
    {
        float timeout = 5f;
        while ((NetworkClient.connection == null || NetworkClient.connection.identity == null) && timeout > 0f)
        {
            timeout -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (NetworkClient.connection == null || NetworkClient.connection.identity == null)
        {
            Debug.LogError("[WaitForLocalPlayerAndSendID] Timed out waiting for identity!");
            yield break;
        }

        MyRoomPlayer roomPlayer = NetworkClient.connection.identity.GetComponent<MyRoomPlayer>();
        if (roomPlayer != null && roomPlayer.isLocalPlayer)
        {
            roomPlayer.CmdSetRestaurantID(restaurantID);
        }
        else
        {
            Debug.LogError("[WaitForLocalPlayerAndSendID] MyRoomPlayer not found or not local after waiting.");
        }
    }

    private IEnumerator StartClientWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        var roomManager = FindObjectOfType<MyRoomManager>();
        if (roomManager != null && !NetworkClient.isConnected && !NetworkClient.active)
        {
            roomManager.StartClient();
        }
    }

    public void TestButton(string ID)
    {
        if (SceneManager.GetActiveScene().name != "TPVScene")
        {
            nMesa.text = "7";
            id.text = ID;
            idNumeroMesa = "7";
            idRestaurante = ID;
            canvasIntro2.SetActive(true);
            canvasMenu.SetActive(false);
            canvasPedido.SetActive(false);
            canvasAtendido.SetActive(false);
            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                canvasQR.SetActive(false);
            }
            camarero = false;

            var manager = FindObjectOfType<MyRoomManager>();
            if (manager != null)
            {
                bool isLocalHost = NetworkServer.active && NetworkClient.active;
                if (isLocalHost)
                {
                    StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante));
                    return;
                }

                if (NetworkClient.isConnected || NetworkClient.active)
                {
                    manager.StopClient();
                }

                StartCoroutine(RestartClient(manager));
            }
        }
        else
        {
            TestButtonCamarero(ID);
        }
    }

    private IEnumerator RestartClient(MyRoomManager manager)
    {
        yield return new WaitForSeconds(1.5f);

        if (HostClient.isHostMode)
            manager.StartHost();
        else
            manager.StartClient();

        float timeout = 5f;
        while (!NetworkClient.isConnected && timeout > 0f)
        {
            timeout -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (!NetworkClient.isConnected)
        {
            Debug.LogError("[RestartClient] Timed out waiting for connection!");
            yield break;
        }

        StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante));
    }

    public void TestButtonCamareroSimular()
    {
        TestButtonCamarero("MIsuhj");
    }

    public void TestButtonCamarero(string ID, bool mostrarLoginInmediato = true)
    {
        GuardarDispositivoCamarero(ID);

        nMesa.text = "0";
        id.text = ID;
        idNumeroMesa = "0";
        idRestaurante = ID;
        camarero = true;

        EstablecerRaiz(() =>
        {
            canvasIntro2.SetActive(false);
            canvasMenu.SetActive(true);
            canvasPedido.SetActive(false);
            canvasAtendido.SetActive(false);

            if (mostrarLoginInmediato)
                canvasInicioSesion.SetActive(true);

            if (SceneManager.GetActiveScene().name == "MobileScene")
                canvasQR.SetActive(false);

            zonaCliente.SetActive(false);
            zonaCamarero.GetComponentInChildren<Canvas>().enabled = true;
            detalleMesa.transform.position = new Vector3(0, 60, 0);
            canvasBarraCamarero.SetActive(true);
        });

        var manager = FindObjectOfType<MyRoomManager>();
        if (manager != null)
        {
            if (camareroConnectCoroutine != null)
            {
                StopCoroutine(camareroConnectCoroutine);
                camareroConnectCoroutine = null;
            }

            bool isLocalHost = NetworkServer.active && NetworkClient.active;
            if (isLocalHost)
            {
                camareroConnectCoroutine = StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante));
                return;
            }

            if (NetworkClient.isConnected || NetworkClient.active)
                manager.StopClient();

            camareroConnectCoroutine = StartCoroutine(RestartClientCamarero(manager));
        }
    }

    private IEnumerator RestartClientCamarero(MyRoomManager manager)
    {
        yield return new WaitForSeconds(1.5f);

        if (HostClient.isHostMode)
            manager.StartHost();
        else
            manager.StartClient();

        float timeout = 5f;
        while (!NetworkClient.isConnected && timeout > 0f)
        {
            timeout -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (!NetworkClient.isConnected)
        {
            Debug.LogError("[RestartClientCamarero] Timed out!");
            yield break;
        }

        yield return StartCoroutine(WaitForLocalPlayerAndSendID(idRestaurante));

        NC.Mesas();
    }

    private void GuardarDispositivoCamarero(string idRestauranteAGuardar)
    {
        if (SceneManager.GetActiveScene().name != "MobileScene") return;

        PlayerPrefs.SetInt("es_dispositivo_camarero", 1);
        PlayerPrefs.SetString("camarero_restaurant_id", idRestauranteAGuardar);
        PlayerPrefs.Save();
    }

    public void RegistrarDispositivoComoPersonal(string restauranteId)
    {
        GuardarDispositivoCamarero(restauranteId);
        idGuardadoCamarero = restauranteId;
        camarero = true;
        EstablecerRaiz(MostrarBienvenidaCamarero);
    }

    private void MostrarPantallaQR()
    {
        canvasQR.SetActive(true);
        canvasIntro2.SetActive(false);
        canvasMenu.SetActive(false);
        canvasPedido.SetActive(false);
        canvasAtendido.SetActive(false);
        buttonPagar.SetActive(false);
    }

    public void ButtonQR()
    {
        IrA(MostrarPantallaQR);
    }

    private void MostrarPantallaMenu()
    {
        canvasIntro2.SetActive(false);
        canvasMenu.SetActive(true);
        canvasPedido.SetActive(false);
        canvasAtendido.SetActive(false);
        detallePlatoX.SetActive(false);
        canvasPrePagar.GetComponent<Canvas>().sortingOrder = 1;
        buttonPagar.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            canvasQR.SetActive(false);
        }
    }

    public void ButtonMenu()
    {
        IrA(MostrarPantallaMenu);
    }

    private void MostrarPantallaPedido()
    {
        canvasIntro2.SetActive(false);
        canvasMenu.SetActive(false);
        canvasPedido.SetActive(true);
        canvasAtendido.SetActive(false);
        imageNotificacion.SetActive(false);
        canvasPrePagar.GetComponent<Canvas>().sortingOrder = 1;
        buttonPagar.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            canvasQR.SetActive(false);
        }
    }

    public void ButtonPedido()
    {
        IrA(MostrarPantallaPedido);
    }

    public void ButtonAtendido()
    {
        GameObject atencionImage = null;
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.CompareTag("AtencionImage")) { atencionImage = obj; break; }
        }

        if (atencionImage != null && atencionImage.activeSelf)
        {
            GameObject dialogoAlready = null;
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.name == "DialogoAlreadyAtendido") { dialogoAlready = obj; break; }
            }
            if (dialogoAlready != null)
                dialogoAlready.SetActive(true);
            return;
        }

        canvasAtendido.SetActive(true);
        modalAtendidoAbierto = true;
    }

    public void ButtonNoAtendido()
    {
        canvasAtendido.SetActive(false);
        modalAtendidoAbierto = false;
    }

    private void MostrarPantallaPagar()
    {
        zonaCliente.SetActive(true);
        canvasQR.SetActive(false);
        canvasIntro2.SetActive(false);
        canvasMenu.SetActive(false);
        canvasPedido.SetActive(false);
        canvasAtendido.SetActive(false);
        canvasPrePagar.GetComponent<Canvas>().sortingOrder = 2;
        buttonPagar.SetActive(true);

        foreach (Transform child in canvasPrePagar.transform)
        {
            if (child.name != "Principal" && child.name != "Logo")
                child.gameObject.SetActive(false);
            else
                child.gameObject.SetActive(true);
        }
    }

    public void ButtonPagar()
    {
        IrA(MostrarPantallaPagar);
    }

    public void RestartApp()
    {
        SceneManager.LoadScene(0);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (SceneManager.GetActiveScene().name != "MobileScene") return;
        if (GUI.Button(new Rect(10, 10, 160, 30), "Borrar sesión camarero"))
        {
            PlayerPrefs.DeleteKey("es_dispositivo_camarero");
            PlayerPrefs.DeleteKey("camarero_restaurant_id");
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs de camarero borrados. Reinicia la escena.");
        }
    }
#endif
}