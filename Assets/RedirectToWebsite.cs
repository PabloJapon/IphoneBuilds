using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class RedirectToWebsite : MonoBehaviour, IPointerClickHandler
{
    public string websiteURL; // Aquí coloca la URL de tu sitio web

    public void OnPointerClick(PointerEventData eventData)
    {
        // Abre la URL en el navegador predeterminado del usuario
        Process.Start(websiteURL);
    }
}
