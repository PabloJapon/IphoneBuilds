using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TamañoLetra : MonoBehaviour
{
    public int incremento = 2;
    public int tamañoMin = 8;
    public int tamañoMax = 30;

    private int delta = 0; // how much we've shifted from each text's original size

    public void AumentarTamañoLetra()
    {
        delta += incremento;
        AplicarDeltaATodos();
    }

    public void DisminuirTamañoLetra()
    {
        delta -= incremento;
        AplicarDeltaATodos();
    }

    private void AplicarDeltaATodos()
    {
        GameObject contentCocina = GameObject.FindGameObjectWithTag("contentCocina");
        if (contentCocina == null)
        {
            Debug.LogWarning("contentCocina not found.");
            return;
        }

        TMP_Text[] allTexts = contentCocina.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in allTexts)
        {
            float original = GetOriginalSize(t);
            float newSize = Mathf.Clamp(original + delta, tamañoMin, tamañoMax);
            t.fontSize = newSize;
        }

        Debug.Log($"[TamañoLetra] Delta={delta} applied across {allTexts.Length} TMP texts.");
    }

    public void AplicarTamañoAComanda(GameObject comanda)
    {
        TMP_Text[] texts = comanda.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in texts)
        {
            float original = GetOriginalSize(t);
            float newSize = Mathf.Clamp(original + delta, tamañoMin, tamañoMax);
            t.fontSize = newSize;
        }

        Debug.Log($"[TamañoLetra] Delta={delta} applied to {comanda.name}");
    }

    private float GetOriginalSize(TMP_Text t)
    {
        OriginalFontSize marker = t.GetComponent<OriginalFontSize>();
        if (marker == null)
        {
            marker = t.gameObject.AddComponent<OriginalFontSize>();
            marker.size = t.fontSize;
        }
        return marker.size;
    }
}

// Tiny marker component — no editor clutter, saves each text's original size
public class OriginalFontSize : MonoBehaviour
{
    public float size;
}