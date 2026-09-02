using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CobrosCamarero : MonoBehaviour
{
    public TMP_Text inputMesa;

    public DetalleMesa DM;

    // Gameobject Canvas
    public GameObject detalleMesaX;
    public GameObject cerrarMesa;
    public GameObject pagoConTarjeta;
    public GameObject contentTicket;
    public GameObject equitativamente;
    public GameObject contentTicketCadaUno;
    public GameObject cadaUnoLoSuyo;

    // Prefabs
    public GameObject prefabPagarPlato;
    public GameObject prefabButtonElegirPagar;

    // Sums
    private float totalSum;
    private float totalSumEquitativo;
    private float totalSumElegir;

    // TMP_Texts Pagar
    public TMP_Text totalPrecio;
    public TMP_Text totalPrecioElegir;
    public TMP_Text totalPrecioAPagarElegir;

    public static bool pagoConfirmadoEnCurso = false;

    public GameObject canvasBloqueo; // Canvas que bloquea la barra de abajo al pagar

    public GameObject buttonSacarTicket;      // 👈 AÑADIR - arrastra "ButtonSacarTicket" en el Inspector
    public GameObject buttonPagoConTarjetaBtn; // 👈 AÑADIR - arrastra "ButtonPagoConTarjeta" (el botón, no el panel "pagoConTarjeta" que ya tienes)
    public GameObject buttonConfirmarPagoJunto; // 👈 AÑADIR - arrastra "ButtonConfirmarPago"
    public GameObject buttonVolverTodoJunto; 

    // Equitativo
    public TMP_Text textNPersonas;
    public GameObject buttonMasPersonas;   
    public GameObject buttonMenosPersonas; 
    public TMP_Text total;
    public TMP_Text totalCadaUno;
    private int numeroPersonas;
    private int personasPagadas;

    // cada uno
    public TMP_Text textElementosSeleccionados; 

    // Buttons
    public GameObject buttonConfirmarPagoEquitativo;
    public GameObject buttonFinalizarEquitativo;
    public GameObject buttonConfirmarPagoElegir;
    public GameObject buttonFinalizarElegir;

    // Movimientos caja
    public DataBaseMovimientosCaja movimientosCaja;
    public TMP_Text textIdTurno;

    public static CobrosCamarero instance;
    public GameObject CobrandoDesdeOtroSitio; // 👈 AÑADIR
    public TMP_Text textCobrandoDesdeOtroSitio; // 👈 AÑADIR

    public static Dictionary<float, GameObject> buttonMesaDictionary = new Dictionary<float, GameObject>();
    public static Dictionary<float, GameObject> mesasDictionary = new Dictionary<float, GameObject>();

    private Dictionary<int, int> clickCounts = new Dictionary<int, int>();
    private Dictionary<int, GameObject> buttonElegirPagarDictionary = new Dictionary<int, GameObject>();

    void Awake() // 👈 AÑADIR método completo
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void CerrarMesa()
    {
        buttonSacarTicket.SetActive(true);        // 👈 AÑADIR
        buttonPagoConTarjetaBtn.SetActive(true);  // 👈 AÑADIR
        buttonConfirmarPagoJunto.SetActive(false); // 👈 AÑADIR
        buttonVolverTodoJunto.SetActive(false);    // 👈 AÑADIR

        cerrarMesa.SetActive(true);
        CrearTicket(false, false);
    }

    public void SacarTicket()
    {
        Debug.Log("SacarTicketPorHacer");
    }

    public void PagoConTarjeta()
    {
        cerrarMesa.SetActive(false);
        pagoConTarjeta.SetActive(true);
    }

    public void VolverDesdePagoConTarjeta()
    {
        pagoConTarjeta.SetActive(false);
        cerrarMesa.SetActive(true);
    }

    public void TodoJunto()
    {
        pagoConTarjeta.SetActive(false);
        cerrarMesa.SetActive(true);

        buttonSacarTicket.SetActive(false);        // 👈 CAMBIADO (reemplaza los if/count)
        buttonPagoConTarjetaBtn.SetActive(false);  // 👈 CAMBIADO
        buttonConfirmarPagoJunto.SetActive(true);  // 👈 CAMBIADO
        buttonVolverTodoJunto.SetActive(true);     // 👈 CAMBIADO
    }

    public void VolverDesdeTodoJunto()
    {
        cerrarMesa.SetActive(false);
        pagoConTarjeta.SetActive(true);

        buttonSacarTicket.SetActive(true);        // 👈 CAMBIADO
        buttonPagoConTarjetaBtn.SetActive(true);  // 👈 CAMBIADO
        buttonConfirmarPagoJunto.SetActive(false); // 👈 CAMBIADO
        buttonVolverTodoJunto.SetActive(false);    // 👈 CAMBIADO
    }

    public void Equitativamente()
    {
        pagoConTarjeta.SetActive(false);
        equitativamente.SetActive(true);
        textNPersonas.text = "2"; // 👈 AÑADIR
        ResultanteEquitativo(2);
        buttonFinalizarEquitativo.SetActive(true);
        buttonFinalizarEquitativo.GetComponent<Button>().interactable = true;
        buttonConfirmarPagoEquitativo.GetComponent<Button>().interactable = true;
        buttonMasPersonas.GetComponent<Button>().interactable = true;
        buttonMenosPersonas.GetComponent<Button>().interactable = true;
        personasPagadas = 0;
        buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
    }

    public void VolverDesdeEquitativo()
    {
        equitativamente.SetActive(false);
        pagoConTarjeta.SetActive(true);
    }

    public void CadaUno()
    {
        pagoConTarjeta.SetActive(false);
        cadaUnoLoSuyo.SetActive(true);

        buttonConfirmarPagoElegir.GetComponentInChildren<TMP_Text>().text = "Confirmar pago 0,00€";
        textElementosSeleccionados.text = "0 elementos seleccionados"; // 👈 AÑADIR
        buttonConfirmarPagoElegir.GetComponent<Button>().interactable = false;
        buttonFinalizarElegir.GetComponent<Button>().interactable = true;
    }

    public void VolverDesdeCadaUno()
    {
        foreach (var pair in buttonElegirPagarDictionary)
        {
            int index = pair.Key;
            GameObject buttonObj = pair.Value;

            if (clickCounts.TryGetValue(index, out int count) && count % 2 == 1)
            {
                var image = buttonObj.GetComponent<Image>();
                if (image != null) image.color = Color.white;
            }
        }
        clickCounts.Clear();

        totalSumElegir = 0f;
        totalPrecioAPagarElegir.text = "0,00€";
        textElementosSeleccionados.text = "0 elementos seleccionados"; // 👈 AÑADIR
        buttonConfirmarPagoElegir.GetComponentInChildren<TMP_Text>().text = "Confirmar pago 0,00€";
        buttonConfirmarPagoElegir.GetComponent<Button>().interactable = false;

        cadaUnoLoSuyo.SetActive(false);
        pagoConTarjeta.SetActive(true);
    }

    private void CrearTicket(bool equitativo, bool cadaUno)
    {
        // Recoger platos en mesa y asignar en contentTicket como prefabs nuevos
        Transform sixActive = null;
        int count = 0;
        foreach (Transform child in detalleMesaX.transform)
        {
            if (child.gameObject.activeSelf && ++count == 6)
            {
                sixActive = child;
                break;
            }
        }

        GameObject scrollSpecificMesa = sixActive.gameObject;
        GameObject contentSpecificMesa = scrollSpecificMesa.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;

        // Deactivate scrollMesa - whe dont need it anymore
        sixActive.gameObject.SetActive(false);

        totalSum = 0;
        totalSumElegir = 0;
        int currentIndex = 0;
        foreach (Transform espacioTransform in contentSpecificMesa.transform)
        {
            var espacio = espacioTransform.GetChild(0).gameObject; // inside Toggle

            var textEspacio1 = espacio.transform.GetChild(1);
            var cantidadEspacio1 = espacio.transform.GetChild(2);
            var precioEspacio = espacio.transform.GetChild(3);

            int cantidad = int.Parse(cantidadEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text);

            // crear prefab Normal en contentTicket asignando textos
            var pagarPlato = Instantiate(prefabPagarPlato, transform.position, Quaternion.identity);
            pagarPlato.transform.SetParent(contentTicket.transform, false);
            pagarPlato.transform.SetSiblingIndex(contentTicket.transform.childCount - 2); // Dejar total el ultimo

            var childTexts = pagarPlato.GetComponentsInChildren<TMP_Text>();
            childTexts[0].text = textEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text;
            childTexts[1].text = cantidadEspacio1.GetComponent<TMPro.TextMeshProUGUI>().text;
            childTexts[2].text = precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text;
            float floatVal = ExtractFloat(precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text);
            totalSum += floatVal;

            float precioLineaTotal = ExtractFloat(precioEspacio.GetComponent<TMPro.TextMeshProUGUI>().text); // 👈 AÑADIR
            float precioUnidad = cantidad > 0 ? precioLineaTotal / cantidad : precioLineaTotal;               // 👈 AÑADIR

            for (int i = 0; i < cantidad; i++)
            {
                var pagarElegirPlato = Instantiate(prefabButtonElegirPagar, transform.position, Quaternion.identity);
                pagarElegirPlato.transform.SetParent(contentTicketCadaUno.transform, false);
                pagarElegirPlato.transform.SetSiblingIndex(contentTicketCadaUno.transform.childCount - 3);

                var childElegirTexts = pagarElegirPlato.GetComponentsInChildren<TMP_Text>();
                childElegirTexts[0].text = textEspacio1.GetComponent<TMP_Text>().text;
                childElegirTexts[1].text = precioUnidad.ToString("0.00").Replace(".", ",") + "€"; // 👈 CAMBIADO

                int indexCopy = currentIndex;
                float priceFloat = precioUnidad; // 👈 CAMBIADO
                pagarElegirPlato.GetComponent<Button>().onClick.AddListener(() => OnButtonSelected(childTexts[0].text, priceFloat, indexCopy));
                buttonElegirPagarDictionary[indexCopy] = pagarElegirPlato;
                currentIndex++;

                totalPrecioElegir.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
                totalPrecioAPagarElegir.text = totalSumElegir.ToString("0.00").Replace(".", ",") + "€";
            }
        }
        
        //else if (cadaUno)
        //{
        //    totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";

        //    personasPagadas = 1;
        //    buttonConfirmarPagoCadaUno.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + personasPagadas;
        //}
        //else
        //{
        //    totalPrecioAPagar.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
        //}
        totalPrecio.text = totalSum.ToString("0.00").Replace(".", ",") + "€";
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
            totalSumElegir -= price;
        }
        else
        {
            image.color = ColorUtility.TryParseHtmlString("#FFC368", out Color c) ? c : Color.white;
            totalSumElegir += price;
        }

        totalPrecioAPagarElegir.text = totalSumElegir.ToString("0.00").Replace(".", ",") + "€";

        // 👇 AÑADIR: contar y mostrar cuántos elementos hay seleccionados
        int seleccionados = 0;
        foreach (var count in clickCounts.Values)
            if (count % 2 == 1) seleccionados++;
        textElementosSeleccionados.text = seleccionados + (seleccionados == 1 ? " elemento seleccionado" : " elementos seleccionados");

        buttonConfirmarPagoElegir.GetComponent<Button>().interactable = true;
        buttonConfirmarPagoElegir.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + totalSumElegir.ToString("0.00").Replace(".", ",") + "€";
    }

    public void ConfirmarPagoCadaUno()
    {
        NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "Camarero"); // 👈 AÑADIR
        foreach (var pair in buttonElegirPagarDictionary)
        {
            int index = pair.Key;
            GameObject buttonObj = pair.Value;

            if (clickCounts.TryGetValue(index, out int count) && count % 2 == 1)
            {
                ColorUtility.TryParseHtmlString("#8C8C8C", out Color greyColor); // 👈 CAMBIADO: un pelín más claro que #787878

                Transform child0 = buttonObj.transform.GetChild(0);
                Transform child1 = buttonObj.transform.GetChild(1);

                if (child0.TryGetComponent<TMP_Text>(out TMP_Text text0))
                {
                    text0.color = greyColor;
                    text0.fontStyle |= FontStyles.Strikethrough;
                }

                if (child1.TryGetComponent<TMP_Text>(out TMP_Text text1))
                {
                    text1.color = greyColor;
                    text1.fontStyle |= FontStyles.Strikethrough; // 👈 AÑADIR: tachar también el precio
                }

                var image = buttonObj.GetComponent<Image>();
                image.color = Color.white;

                buttonObj.GetComponent<Button>().interactable = false;
            }
        }

        personasPagadas++;
        pagoConfirmadoEnCurso = true; 
        canvasBloqueo.SetActive(true);
        buttonFinalizarElegir.GetComponent<Button>().interactable = false; // 👈 AÑADIR: ya no se puede "volver" tras el primer pago
        totalSumElegir = 0f;
        textElementosSeleccionados.text = "0 elementos seleccionados"; 
                buttonConfirmarPagoElegir.GetComponentInChildren<TMP_Text>().text = "Confirmar pago 0,00€";
        totalPrecioAPagarElegir.text = "0,00€";

        if (!AreAllButtonsPaid())
        {
            buttonConfirmarPagoElegir.GetComponent<Button>().interactable = true;
        }
        else  // Todos los platos pagados
        {
            buttonConfirmarPagoElegir.GetComponent<Button>().interactable = false;
            clickCounts.Clear();
            FinalizarPagado(); // 👈 CAMBIADO: se dispara solo, ya no hace falta pulsar "Finalizar"
        }
    }

    private bool AreAllButtonsPaid()
    {
        foreach (var button in buttonElegirPagarDictionary.Values)
        {
            var btn = button.GetComponent<Button>(); // 👈 CAMBIADO
            if (btn == null || btn.interactable)      // 👈 CAMBIADO: si sigue interactable, no está pagado
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
            localReceiver.SendColorizeButtonPagado("Todo", 0, int.Parse(inputMesa.text));
            NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdClearPagoEnCurso(int.Parse(inputMesa.text)); // 👈 AÑADIR 

            Debug.Log("1");

            if (movimientosCaja != null && textIdTurno != null)
            {
                string tipo = "IngresoTarjeta";

                StartCoroutine(movimientosCaja.AddMovimientoCaja(
                    tipo,
                    "Pago cliente mesa " + inputMesa.text,
                    totalSum
                ));
            }
            else
            {
                Debug.LogError("MovimientosCaja or textIdTurno missing!");
            }

            pagoConfirmadoEnCurso = false;
            canvasBloqueo.SetActive(false);
            ClearTicket();
            DM.clickClose();
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

    public void ClearTicket()
    {
        if (pagoConfirmadoEnCurso) return; 
        foreach (Transform child in contentTicket.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in contentTicketCadaUno.transform)
        {
            Destroy(child.gameObject);
        }
        buttonElegirPagarDictionary.Clear(); 
        clickCounts.Clear();  

        totalSum = 0;
        totalSumElegir = 0;
        totalPrecioElegir.text = "0,00€";
        buttonFinalizarEquitativo.SetActive(false);

        cadaUnoLoSuyo.SetActive(false);
        pagoConTarjeta.SetActive(false);
        cerrarMesa.SetActive(false);
        equitativamente.SetActive(false);
    }

    public void ConfirmarPagoEquitativo()
    {
        NetworkClient.localPlayer.GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "Camarero"); // 👈 AÑADIR
        personasPagadas++;
        pagoConfirmadoEnCurso = true; 
        canvasBloqueo.SetActive(true);

        // 👇 AÑADIR: una vez confirmado un pago, ya no se puede "volver" ni cambiar personas
        buttonFinalizarEquitativo.GetComponent<Button>().interactable = false;
        buttonMasPersonas.GetComponent<Button>().interactable = false;
        buttonMenosPersonas.GetComponent<Button>().interactable = false;

        if (personasPagadas >= numeroPersonas)
        {
            buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + numeroPersonas + "/" + numeroPersonas;
            buttonConfirmarPagoEquitativo.GetComponent<Button>().interactable = false;
            FinalizarPagado(); // 👈 AÑADIR: se dispara solo en el último pago, ya no hace falta el click de "Finalizar"
        }
        else
        {
            buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
        }
    }

    public void masPersonas()
    {
        if (int.TryParse(textNPersonas.text, out int personasInt))
        {
            personasInt++;
            textNPersonas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);

            // Fuente
            string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
            TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
            if (fuente == null)
                fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
            textNPersonas.font = fuente;
        }
        buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
    }

    public void menosPersonas()
    {
        if (int.TryParse(textNPersonas.text, out int personasInt) && personasInt > 1)
        {
            personasInt--;
            textNPersonas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);

            // Fuente
            string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
            TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
            if (fuente == null)
                fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
            textNPersonas.font = fuente;
        }
        buttonConfirmarPagoEquitativo.GetComponentInChildren<TMP_Text>().text = "Confirmar pago " + (personasPagadas + 1) + "/" + numeroPersonas;
    }

    private void ResultanteEquitativo(int numPersonas)
    {
        totalSumEquitativo = totalSum / numPersonas;
        total.text = $"{totalSum:F2} €";
        totalCadaUno.text = $"{totalSumEquitativo:F2} €";
        numeroPersonas = numPersonas;

        // Fuente
        string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
        if (fuente == null)
            fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
        totalCadaUno.font = fuente;
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

    public void SetPagoEnCursoUI(int mesaNumber, bool enCurso, string origen) // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] SetPagoEnCursoUI llamado: mesa={mesaNumber}, enCurso={enCurso}, origen={origen}, mesaAbiertaActual={inputMesa.text}"); // 👈 AÑADIR temporal
        if (!int.TryParse(inputMesa.text, out int mesaAbierta) || mesaAbierta != mesaNumber) return;

        bool bloqueadaPorTPV = enCurso && origen == "TPV";
        CobrandoDesdeOtroSitio.SetActive(bloqueadaPorTPV);
        if (bloqueadaPorTPV)
            textCobrandoDesdeOtroSitio.text = "Se está cobrando esta mesa desde el TPV";

        buttonSacarTicket.SetActive(!bloqueadaPorTPV); // reutiliza referencias que ya añadimos antes
        buttonPagoConTarjetaBtn.SetActive(!bloqueadaPorTPV);
    }

    public void OnClickConfirmarPagoTodoJunto() // 👈 AÑADIR método completo
    {
        Debug.Log($"[PagoEnCurso] Entrando en OnClickConfirmarPagoTodoJunto. localPlayer null? {NetworkClient.localPlayer == null}"); // 👈 AÑADIR temporal
    
        NetworkClient.localPlayer.GetComponent<MyPlayerController>().GetComponent<MyRoomPlayer>().CmdSetPagoEnCurso(int.Parse(inputMesa.text), "Camarero");
        FinalizarPagado();
    }
}
