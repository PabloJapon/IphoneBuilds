using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArregloAsistencia : MonoBehaviour
{
    public ButtonsColorsCode BCC;

    public Button[] buttons;

    public GameObject canvasMenu;
    public GameObject canvasPedido;
    public GameObject canvasPagar;

    public void QuitarAsistencia()
    {
        if (canvasMenu.activeInHierarchy)
        {
            BCC.SelectButton(buttons[0]);
        }
        else if (canvasPedido.activeInHierarchy)
        {
            BCC.SelectButton(buttons[1]);
        }
        else
        {
            BCC.SelectButton(buttons[2]);
        }
    }
}
