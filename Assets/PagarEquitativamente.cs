using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using TMPro;

public class PagarEquitativamente : MonoBehaviour
{
    public PaymentHandler PH;

    public TMP_Text totalPagar;
    public TMP_Text personas;
    public TMP_Text pagarResultante;
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;

    private void OnEnable()
    {
        ResultanteEquitativo(2);
        String amountTodoText = GameObject.FindGameObjectWithTag("amountText").GetComponent<TMP_Text>().text;
        totalPagar.text = amountTodoText;

        // Fuente
        string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
        if (fuente == null)
            fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
        totalPagar.font = fuente;
        text1.font = fuente;
        text2.font = fuente;
        text3.font = fuente;
    }

    public void masPersonas()
    {
        if (int.TryParse(personas.text, out int personasInt))
        {
            personasInt++;
            personas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);

            // Fuente
            string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
            TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
            if (fuente == null)
                fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
            personas.font = fuente;
        }
    }

    public void menosPersonas()
    {
        if (int.TryParse(personas.text, out int personasInt) && personasInt > 1)
        {
            personasInt--;
            personas.text = personasInt.ToString();
            ResultanteEquitativo(personasInt);

            // Fuente
            string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
            TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
            if (fuente == null)
                fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
            personas.font = fuente;
        }
    }

    private void ResultanteEquitativo(int numPersonas)
    {
        String amountTodoText = GameObject.FindGameObjectWithTag("amountText").GetComponent<TMP_Text>().text;
        if (float.TryParse(amountTodoText.Replace(",", "").Replace(" €", ""), out float totalPagar) && numPersonas > 0)
        {
            float aPagar = totalPagar / (100 * numPersonas);
            pagarResultante.text = $"Pagar {aPagar:F2} €";
        }
        else
        {
            pagarResultante.text = "Error en el cálculo";
        }

        // Fuente
            string rutaFuente = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
            TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(rutaFuente);
            if (fuente == null)
                fuente = Resources.Load<TMP_FontAsset>(rutaFuente + " SDF");
            pagarResultante.font = fuente;
    }

    public void RedirectToPaymentHandlerEquitativo()
    {
        if (PaymentHandler.Local != null)
        {
            PaymentHandler.Local.RedirectToPaymentPage("Equitativo", pagarResultante.text);
        }
        else
        {
            Debug.LogError("PaymentHandler.Local is not assigned yet.");
        }
    }

    public void RedirectToPaymentHandlerTodo()
    {
        String amountTodoText = GameObject.FindGameObjectWithTag("amountText").GetComponent<TMP_Text>().text;

        if (PaymentHandler.Local != null)
        {
            PaymentHandler.Local.RedirectToPaymentPage("Todo", amountTodoText);
        }
        else
        {
            Debug.LogError("PaymentHandler.Local is not assigned yet.");
        }
    }
}
