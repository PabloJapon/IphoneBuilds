using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Globalization;
using System.Text.RegularExpressions;

public class CashCalculator : MonoBehaviour
{
    public TMP_Text cuentaEntregado;
    public TMP_Text totalPrecioADevolver;

    private Dictionary<float, int> denominationCounts = new Dictionary<float, int>();
    private float totalDelivered = 0f;

    public TMP_Text totalToPay;

    void Start()
    {
        UpdateUI();
    }

    public void AddAmount(float amount)
    {
        if (!denominationCounts.ContainsKey(amount))
            denominationCounts[amount] = 0;

        denominationCounts[amount]++;
        totalDelivered += amount;

        UpdateUI();
    }

    public void Clear()
    {
        denominationCounts.Clear();
        totalDelivered = 0f;
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Update delivered breakdown
        string breakdown = "";
        foreach (var entry in denominationCounts)
        {
            breakdown += $"{entry.Key:0.00}€ x{entry.Value} + ";
        }

        if (breakdown.Length > 3)
            breakdown = breakdown.Substring(0, breakdown.Length - 3);

        cuentaEntregado.text = $"{breakdown} = {totalDelivered:0.00}€";
        totalPrecioADevolver.text = $"{(totalDelivered - ExtractFloat(totalToPay.text)):0.00} €";
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
            // Debug.LogWarning("No float value found in the input string.");
            return float.NaN; // Return NaN (Not a Number) to indicate failure
        }
    }
}
