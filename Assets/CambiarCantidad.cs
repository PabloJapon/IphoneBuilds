using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine.SceneManagement;

public class CambiarCantidad : MonoBehaviour
{
    public static CambiarCantidad Instance { get; private set; }

    public GameObject menuPedir;
    MenuPedir menuPedirScript;

    public TMP_Text cantidadDetallePlatoX;
    public TMP_Text textDetallePrecio;

    public TMP_Text[] textPrecios;
    public TMP_Text[] textNPlatos;
    public TMP_Text[] textNumeros;

    void Awake()
    {
        // Ensure only one instance of CambiarCantidad exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        menuPedirScript = menuPedir.GetComponent<MenuPedir>();
    }

    public void OnClickDetalle()
    {
        int platoNumber = SceneManager.GetActiveScene().name == "TPVScene" ? DetallePlatoUI.xPlato : DetallePlato.xPlato;
        cantidadDetallePlatoX.text = menuPedirScript.platoCount[platoNumber].ToString();
    }

    public void StartCounting()
    {
        int platoNumber = SceneManager.GetActiveScene().name == "TPVScene" ? DetallePlatoUI.xPlato : DetallePlato.xPlato;
        int platoCount = menuPedirScript.platoCount[platoNumber];
        cantidadDetallePlatoX.text = platoCount.ToString();

        float basePrice = DataBase.precioPlatos[platoNumber - 1];
        float extraTotal = 0f;
        bool esTPV = SceneManager.GetActiveScene().name == "TPVScene";
        var opciones = esTPV ? DetallePlatoUI.Instance.GetOptionSelections() : DetallePlato.Instance.GetOptionSelections();
        foreach (var pair in opciones)
            extraTotal += esTPV ? DetallePlatoUI.Instance.ExtractOptionExtraPrice(pair.Value) : DetallePlato.Instance.ExtractOptionExtraPrice(pair.Value);

        float totalPrice = (basePrice + extraTotal) * platoCount;
        textDetallePrecio.text = "Añadir   " + totalPrice.ToString("0.00").Replace(".", ",") + " €";
        if (SceneManager.GetActiveScene().name == "TPVScene")
            DetallePlatoUI.yPlato = basePrice + extraTotal;
        else
            DetallePlato.yPlato = basePrice + extraTotal;
    }

    public void AddCantidad()
    {
        int platoNumber = SceneManager.GetActiveScene().name == "TPVScene" ? DetallePlatoUI.xPlato : DetallePlato.xPlato;
        menuPedirScript.platoCount[platoNumber] = menuPedirScript.platoCount[platoNumber] + 1;
        StartCounting();
    }
    public void QuitarCantidad()
    {
        int platoNumber = SceneManager.GetActiveScene().name == "TPVScene" ? DetallePlatoUI.xPlato : DetallePlato.xPlato;
        if (menuPedirScript.platoCount[platoNumber] > 1) // ← > 1 not > 0 since we sync now
        {
            menuPedirScript.platoCount[platoNumber] = menuPedirScript.platoCount[platoNumber] - 1;
        }
        StartCounting();
    }

    public void AddCantidadPedido(int index)
    {
        int platoNumber = int.Parse(textNPlatos[index].text);
        int platoQuantity = int.Parse(textNumeros[index].text);

        menuPedirScript.UpdatePlatoCountPedido(index, platoQuantity + 1, platoNumber);
    }


    public void QuitarCantidadPedido(int index)
    {
        int platoNumber = int.Parse(textNPlatos[index].text);
        int platoQuantity = int.Parse(textNumeros[index].text);

        if (platoQuantity > 1)
        {
            menuPedirScript.UpdatePlatoCountPedido(index, platoQuantity - 1, platoNumber);
        }
    }

    float ExtractFloat(string input)
    {
        Match match = Regex.Match(input, @"(\d+,\d+)");
        if (match.Success)
        {
            string val = match.Groups[1].Value.Replace(',', '.');
            return float.Parse(val, CultureInfo.InvariantCulture);
        }
        return 0f;
    }
}