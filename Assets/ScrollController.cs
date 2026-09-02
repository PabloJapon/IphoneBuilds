using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    private Scrollbar scrollbar;
    public float scrollSpeed = 6f;
    private float targetScrollValue;

    public GameObject canvasEntrantes;
    public GameObject canvasPlatos;
    public GameObject canvasBebidas;
    public GameObject canvasPostres;
    private GameObject targetObject;
    public float    margenCabecera;
    public GameObject cabecera;

    private bool isScrolling = false;

    void Start()
    {
        scrollbar = this.gameObject.GetComponent<Scrollbar>();
        cabecera.SetActive(false);
    }

    void Update()
    {
        if (isScrolling)
        {
            // Lerp suavemente hacia el valor objetivo
            scrollbar.value = Mathf.Lerp(scrollbar.value, targetScrollValue, Time.deltaTime * scrollSpeed);

            // Detén el desplazamiento cuando esté lo suficientemente cerca del valor objetivo
            if (Mathf.Abs(scrollbar.value - targetScrollValue) < 0.001f)
            {
                scrollbar.value = targetScrollValue;
                isScrolling = false;
            }
        }

        if(scrollbar.value < 0.9f & cabecera.activeInHierarchy == false)
        {
            cabecera.SetActive(true);
        }

        if (scrollbar.value > 0.9f & cabecera.activeInHierarchy == true)
        {
            cabecera.SetActive(false);
        }
    }

    public void OnButtonClick(int seccion)
    {
        if (seccion==0)
        {
            targetObject = canvasEntrantes;
        }
        else if (seccion==1)
        {
            targetObject = canvasPlatos;
        }
        else if (seccion==2)
        {
            targetObject = canvasBebidas;
        }
        else
        {
            targetObject = canvasPostres;
        }

        RectTransform rectTransform = targetObject.GetComponent<RectTransform>();
        float yPosition = rectTransform.anchoredPosition.y;
        targetScrollValue = 1 - (yPosition/(-10000)) + 0.025f + margenCabecera;
        if (targetScrollValue < 0)
        {
            targetScrollValue = 0;
        }

        // Inicia el desplazamiento al hacer clic en el botón
        isScrolling = true;
    }
}