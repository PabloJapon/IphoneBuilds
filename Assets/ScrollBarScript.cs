using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarScript : MonoBehaviour
{
    private Scrollbar scrollBar;
    private Image[] imagenes;

    private bool ocultar;

    // Start is called before the first frame update
    void Start()
    {
        scrollBar = gameObject.GetComponent<Scrollbar>();
        if (scrollBar == null)
        {
            Debug.LogError("Scrollbar component not found on the game object.");
        }

        imagenes = gameObject.GetComponentsInChildren<Image>();
        if (imagenes.Length == 0)
        {
            Debug.LogError("No Image components found in children.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollBar.size == 1)
        {
            ocultar = true;
            foreach (Image image in imagenes)
            {
                image.enabled = false;
            }
        }
        else if (ocultar && scrollBar.size < 1)
        {
            ocultar = false;
            foreach (Image image in imagenes)
            {
                image.enabled = true;
            }
        }
    }
}
