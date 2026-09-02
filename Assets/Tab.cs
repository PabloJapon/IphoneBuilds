using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Tab : MonoBehaviour
{
    public GameObject contentCanvas; // El canvas que debe activarse/desactivarse al hacer clic en el botón de esta pestaña
    public Button tabButton; // El botón de esta pestaña

    // Método para inicializar la pestaña
    public void Initialize(GameObject content, Button button)
    {
        contentCanvas = content;
        tabButton = button;
        tabButton.onClick.AddListener(OnTabButtonClick); // Agrega el listener al botón
    }

    // Método llamado cuando se hace clic en el botón de la pestaña
    private void OnTabButtonClick()
    {
        // Desactiva todos los canvas de las otras pestañas
        TabManager.Instance.DeactivateAllContentCanvases();

        // Activa el canvas de esta pestaña
        contentCanvas.SetActive(true);
    }
}