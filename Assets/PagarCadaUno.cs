using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using System;

public class PagarCadaUno : MonoBehaviour
{
    public PaymentHandler PH;

    public GameObject buttonElegirPagar;
    public GameObject contentPrincipal;
    public GameObject contentCadaUno;
    public TMP_Text numeroSeleccionados;
    public TMP_Text selecciona;

    private GameObject prefabElegirPagar2;
    private int n;

    public GameObject buttonPagarTotalCadaUno;
    private float totalSum;

    public static Dictionary<int, GameObject> buttonElegirPagarDictionary = new Dictionary<int, GameObject>();
    public static Dictionary<int, int> nTimesDictionary = new Dictionary<int, int>();

    public string id_payment;

    // Method to generate a unique random id_payment
    private string GenerateUniquePaymentId()
    {
        // Generate a GUID and convert it to a string
        string guidPart = Guid.NewGuid().ToString("N"); // "N" format gives a 32-character string without dashes

        // Get the current timestamp (seconds since the Unix epoch)
        string timestampPart = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Combine GUID part and timestamp part to form the id_payment
        return $"{guidPart}_{timestampPart}";
    }

    private void OnEnable()
    {
        // Fuente
        string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
        if (fuente == null)
            fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");

        // extraer de contentprincipal info
        for (int i = 0; i < contentPrincipal.transform.childCount - 2; i++)
            {
                GameObject child = contentPrincipal.transform.GetChild(i).gameObject;
                TMP_Text[] textsPrincipal = child.GetComponentsInChildren<TMP_Text>();

                for (int j = 0; j < int.Parse(textsPrincipal[1].text) ; j++)
                {
                    //Instanciar prefab button elegir pagar
                    GameObject prefabElegirPagar = Instantiate(buttonElegirPagar, transform.position, Quaternion.identity);

                    prefabElegirPagar.transform.SetParent(contentCadaUno.transform, false);

                    TMP_Text[] texts = prefabElegirPagar.GetComponentsInChildren<TMP_Text>();
                    texts[0].text = textsPrincipal[0].text; // name
                    float price = (ExtractFloat(textsPrincipal[2].text))/float.Parse(textsPrincipal[1].text);
                    texts[1].text = price.ToString("0.00") + " €"; // price

                    // Cambiar letras
                    texts[0].font = fuente;
                    texts[1].font = fuente;

                    // Create a local copy of i for the lambda
                    int currentIndex = i * 1000 + j;

                    buttonElegirPagarDictionary[currentIndex] = prefabElegirPagar;
                    nTimesDictionary[currentIndex] = 0;
                    prefabElegirPagar.GetComponent<Button>().onClick.AddListener(() => OnButtonSelected(texts[0].text, price, currentIndex));
                }
            }

        totalSum = 0;
        // Total a pagar
        buttonPagarTotalCadaUno.GetComponentInChildren<TMP_Text>().text = "Pagar";
        buttonPagarTotalCadaUno.GetComponent<Button>().interactable = false;

        // Cambiar letras
        numeroSeleccionados.font = fuente;
        selecciona.font = fuente;
        buttonPagarTotalCadaUno.GetComponentInChildren<TMP_Text>().font = fuente;
    }

    private void OnButtonSelected(string name, float price, int i)
    {
        buttonElegirPagarDictionary.TryGetValue(i, out prefabElegirPagar2);
        nTimesDictionary.TryGetValue(i, out n);
        n++;
        nTimesDictionary[i] = n;
        if(n % 2 == 0)
        {
            prefabElegirPagar2.GetComponent<Image>().color = Color.white;
            totalSum -= price;
        }
        else
        {
            prefabElegirPagar2.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#FFC368", out Color c) ? c : Color.white;
            totalSum += price;
        }

        // Contar los botones con color #FFC368
        int selectedCount = CountButtonsWithColor("#FFC368");
        numeroSeleccionados.text = selectedCount + " elementos seleccionados";

        // Total a pagar
        if (totalSum != 0)
        {
            buttonPagarTotalCadaUno.GetComponent<Button>().interactable = true;
            buttonPagarTotalCadaUno.GetComponentInChildren<TMP_Text>().text = "Pagar " + totalSum.ToString("0.00").Replace(".", ",") + " €";
        }
        else
        {
            buttonPagarTotalCadaUno.GetComponentInChildren<TMP_Text>().text = "Pagar";
            buttonPagarTotalCadaUno.GetComponent<Button>().interactable = false;
        }
    }

     private int CountButtonsWithColor(string hexColor)
    {
        int count = 0;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color targetColor))
        {
            foreach (var button in buttonElegirPagarDictionary.Values)
            {
                Image img = button.GetComponent<Image>();
                if (img != null && img.color == targetColor)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private float ExtractFloat(string input)
    {
        string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
        string sanitizedInput = Regex.Replace(input, @"[^\d" + Regex.Escape(decimalSeparator) + "]", "");
        if (float.TryParse(sanitizedInput, out float result))
        {
            return result;
        }
        Debug.LogError("Failed to extract float from input: " + input);
        return 0;
    }

    public void RedirectToPaymentHandlerElegir()
    {
        string amountPagar = (100 * totalSum).ToString();

        if (PaymentHandler.Local != null)
        {
            PaymentHandler.Local.RedirectToPaymentPage("Elegir", amountPagar);
        }
        else
        {
            Debug.LogError("PaymentHandler.Local is not assigned yet.");
        }
    }
}
