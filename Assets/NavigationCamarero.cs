using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class NavigationCamarero : MonoBehaviour
{
    public static NavigationCamarero Instance { get; private set; }

    public GameObject zonaCliente;
    public GameObject barraTomarNota;
    public TMP_Text tomandoNota;
    public GameObject canvasMenu;
    public GameObject canvasPedido;
    public GameObject contentPedido;
    public GameObject precioTotal;
    public Button botonMesas; 
    public GameObject verPedidosAnteriores;

    public TMP_Text nMesa;

    public Button buttonMesa;
    public Button buttonEditarMenu;           
    public GameObject canvasEditarMenu;
    public GameObject detallePlatoX;
    public GameObject detalleMesaX;
    public GameObject canvasIntro2;
    public ScrollRect scrollRect;

    // Elegir Menu
    public Dictionary<int, GameObject> contentMenus = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> zonaHorizontales = new Dictionary<int, GameObject>();

    public TMP_Text textMenuEmpresa;
    public GameObject contentMenu;
    public GameObject barraNavegacionDesactivarPlatos;

    // Barra nav camarero
    public Button buttonMenuNavCamarero;
    public ButtonsColorsCode BCCNavCamarero;

    private void Awake()
    {
        // Make sure there's only one instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Optional: prevent duplicates
            return;
        }
        Instance = this;
    }

    public void Mesas()
    {
        zonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        detalleMesaX.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000); // Hide it
        if (canvasEditarMenu != null) canvasEditarMenu.SetActive(false);
    }
    public void VolverMesas()
    {
        zonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        detalleMesaX.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000); // Hide it
        if (canvasEditarMenu != null) canvasEditarMenu.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            BCCNavCamarero.SelectButton(buttonMenuNavCamarero);
        }
    }
    public void TomarNota(int mesaNumber, string nombreCliente, string empresa, string menu)
    {
        if (SceneManager.GetActiveScene().name == "TPVScene" && mesaNumber > 999)
        {
            if (mesaNumber < 2000)
            {
                TomarNotaRecogerDelivery(true, mesaNumber - 1000, nombreCliente, empresa, menu);
                Debug.Log(menu);
            }
            else
            {
                TomarNotaRecogerDelivery(false, mesaNumber - 2000, nombreCliente, empresa, menu);
                Debug.Log(menu);
            }

            // Set Active Pedidos anteriores
            verPedidosAnteriores.SetActive(true);
        }

        else
        {
            ResetComanda();

            zonaCliente.SetActive(true);
            barraTomarNota.SetActive(true);
            tomandoNota.text = "Tomando nota de la Mesa " + mesaNumber;
            nMesa.text = mesaNumber.ToString();

            if (SceneManager.GetActiveScene().name == "MobileScene")
            {
                BCCNavCamarero.SelectButton(buttonMenuNavCamarero);
                canvasPedido.SetActive(false);
                if (CrearMenu.instance != null)
                    CrearMenu.instance.ResetToFirstSeccion();
            }
            //buttonMesa.onClick.Invoke();
            detallePlatoX.SetActive(false);
            detalleMesaX.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000); // Hide it
            scrollRect.verticalNormalizedPosition = 1f;

            canvasMenu.SetActive(true);

            if (SceneManager.GetActiveScene().name != "MobileScene")
            {
                canvasPedido.SetActive(true);

                // Enseñar Menu 1 por defecto si no es recoger o delivery
                int firstMenuID = contentMenus.Keys.Min();
                foreach (var kv in contentMenus) kv.Value.SetActive(kv.Key == firstMenuID);
                foreach (var kv in zonaHorizontales) kv.Value.SetActive(kv.Key == firstMenuID);
                textMenuEmpresa.text = DataBase.menuNamesById.ContainsKey(firstMenuID) ? DataBase.menuNamesById[firstMenuID] : "";
                scrollRect.content = contentMenus[firstMenuID].GetComponent<RectTransform>();
                zonaHorizontales[firstMenuID].GetComponentInChildren<Button>().onClick.Invoke();
                
                // Set Not Active Pedidos anteriores
                verPedidosAnteriores.SetActive(false);
            }

            if (canvasIntro2.activeInHierarchy == true)
            {
                canvasIntro2.SetActive(false);
            }
        }
    }

    public void TomarNotaRecogerDelivery(bool recoger, int n, string nombreCliente, string empresa, string menu) // Para la primera vez de momento
    {
        ResetComanda();

        zonaCliente.SetActive(true);
        barraTomarNota.SetActive(true);

        if (recoger)
        {
            tomandoNota.text = "Tomando nota R" + n + " (" + nombreCliente + ", " + empresa + ")";
            nMesa.text = (1000 + n).ToString();
        }
        else
        {
            tomandoNota.text = "Tomando nota D" + n + " (" + nombreCliente + ", " + empresa + ")";
            nMesa.text = (2000 + n).ToString();
        }

        //buttonMesa.onClick.Invoke();
        detallePlatoX.SetActive(false);
        detalleMesaX.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000); // Hide it
        scrollRect.verticalNormalizedPosition = 1f;

        canvasMenu.SetActive(true); //

        canvasPedido.SetActive(true);
        int targetMenuID = DataBase.menuNamesById.FirstOrDefault(x => x.Value == menu).Key;
        if (targetMenuID == 0) targetMenuID = contentMenus.Keys.Min(); // fallback
        foreach (var kv in contentMenus) kv.Value.SetActive(kv.Key == targetMenuID);
        foreach (var kv in zonaHorizontales) kv.Value.SetActive(kv.Key == targetMenuID);
        textMenuEmpresa.text = menu;
        scrollRect.content = contentMenus[targetMenuID].GetComponent<RectTransform>();
        zonaHorizontales[targetMenuID].GetComponentInChildren<Button>().onClick.Invoke();
    }

    public void Menu()
    {
        canvasMenu.SetActive(true);
        canvasPedido.SetActive(false);
    }
    public GameObject imageNotificacionCamarero; // debe apuntar al mismo objeto asignado en MenuPedir

    public void Pedido()
    {
        canvasMenu.SetActive(false);
        canvasPedido.SetActive(true);
        if (imageNotificacionCamarero != null)
            imageNotificacionCamarero.SetActive(false);
    }

    public void ResetComanda()
    {
        int childCount = contentPedido.transform.childCount;

        for (int i = 0; i < childCount - 2; i++)
        {
            Destroy(contentPedido.transform.GetChild(i).gameObject);
        }

        precioTotal.SetActive(false);
    }

    public void AbrirCanvasEditarMenu()
    {
        if (canvasEditarMenu != null) canvasEditarMenu.SetActive(true);
    }
}
