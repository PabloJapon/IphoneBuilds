using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    public static TabManager Instance; // Singleton

    public List<Tab> tabs = new List<Tab>(); // Lista para almacenar las pestañas

    private void Awake()
    {
        Instance = this;
    }

    // Método para desactivar todos los canvas de las pestañas
    public void DeactivateAllContentCanvases()
    {
        foreach (Tab tab in tabs)
        {
            if (tab.contentCanvas != null)
            {
                tab.contentCanvas.SetActive(false);
            }
        }
    }
}