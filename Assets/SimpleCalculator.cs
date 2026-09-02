using UnityEngine;
using TMPro;
using System;
using System.Data;

public class SimpleCalculator : MonoBehaviour
{
    public TMP_Text displayText;

    private string input = "";
    private bool showingResult = false;

    public MenuPedir MP;

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (input.Length > 0 && !showingResult)
                    input = input.Substring(0, input.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                Calculate();
            }
            else
            {
                if (showingResult)
                {
                    input = "";
                    showingResult = false;
                }

                input += c;
            }
        }

        displayText.text = input;
    }

    public void AppendInput(string value)
    {
        if (showingResult)
        {
            input = "";
            showingResult = false;
        }

        input += value;
        displayText.text = input;
    }

    public void ClearAll()
    {
        input = "";
        showingResult = false;
        displayText.text = "";
    }

    public void Calculate()
    {
        //Debug.Log("Evaluating: " + input);

        // Replace user-friendly input with computable format
        string evaluable = input
            .Replace(",", ".")   // Convert decimal comma to dot
            .Replace("x", "*")   // Convert x to *
            .Replace("X", "*");  // (Optional: allow capital X)

        var result = new DataTable().Compute(evaluable, "");

        if (result != null)
        {
            string resultStr = Convert.ToDouble(result).ToString("0.00").Replace(".", ",");
            input = resultStr;
            showingResult = true;
            //Debug.Log("Result: " + resultStr);

            MP.SelectVarios(resultStr);
            ClearAll();
            gameObject.SetActive(false);
        }
        else
        {
            displayText.text = "Error";
        }
    }
}
