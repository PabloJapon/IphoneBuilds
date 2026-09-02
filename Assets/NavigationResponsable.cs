
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class NavigationResponsable : MonoBehaviour
{
    // Canvases for different areas
    public GameObject canvasAreaPersonal;
    public GameObject canvasCliente;
    public GameObject canvasMenu;
    public GameObject canvasEstadisticas;
    public GameObject graficas;
    public GameObject canvasQrs;
    public GameObject canvasEmpleados;
    public GameObject canvasPedidos;
    public Transform menusRoot;


    // "Guardar" buttons
    public Button guardarButtonPA;
    public Button guardarButtonQR;

    // Confirmation dialog (seguroSinGuardar) containing one "Cancelar" button
    public GameObject seguroSinGuardar;

    // Action to store which navigation method should be executed after confirmation
    private System.Action confirmedNavigationAction;

    public RespDataBasePersonalizacion RDBPA;
    public RespDataBaseQrs RDBQR;
    public variasCocinas CocinasDB;
    public DataBasePersonalizacionRespScene DBPRS;

    public EnviarDatosPersonalizacion enviarPA;
    public EnviarDatosQrs enviarQR;

    void Start()
    {
        // Initial setup of canvases
        canvasAreaPersonal.SetActive(true);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);

        // Set initial state of guardar buttons and confirmation dialog
        guardarButtonPA.interactable = false;
        guardarButtonQR.interactable = false;
        seguroSinGuardar.SetActive(false);
    }

    // Navigation button methods
    public void ButtonAreaPersonal()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToPersonalArea;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToPersonalArea();
        }
    }

    public void ButtonCliente()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToCliente;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToCliente();
        }
    }

    public void ButtonMenu()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToMenu;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToMenu();
        }
    }

    public void ButtonEstadisticas()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToEstadisticas;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToEstadisticas();
        }
    }

    public void ButtonQrs()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToQrs;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToQrs();
        }
    }

    public void ButtonEmpleados()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToEmpleados;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToEmpleados();
        }
    }

    public void ButtonPedidos()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            confirmedNavigationAction = GoToPedidos;
            seguroSinGuardar.SetActive(true);
        }
        else
        {
            GoToPedidos();
        }
    }

    // Navigation actions
    private void GoToPersonalArea()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(true);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);
    }

    private void GoToCliente()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(true);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);
    }

    private void GoToMenu()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(true);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);
    }

    private void GoToEstadisticas()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(true);
        graficas.SetActive(true);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);
    }

    private void GoToQrs()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(true);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(false);
    }

    private void GoToEmpleados()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(true);
        canvasPedidos.SetActive(false);
    }

    private void GoToPedidos()
    {
        DeactivateAllCanvasMenus();
        canvasAreaPersonal.SetActive(false);
        canvasCliente.SetActive(false);
        canvasMenu.SetActive(false);
        canvasEstadisticas.SetActive(false);
        graficas.SetActive(false);
        canvasQrs.SetActive(false);
        canvasEmpleados.SetActive(false);
        canvasPedidos.SetActive(true);
    }

    public void NoGuardar()
    {
        RDBPA.RellenarCampos();
        RDBQR.RellenarCampos();
        CocinasDB.RellenarCocinas();

        ConfirmNavigation();
    }

    public void Guardar()
    {
        if (canvasCliente.activeSelf)
        {
            enviarPA.OnButtonClick1();   // Call your save method for canvasCliente
            guardarButtonPA.interactable = false;
        }
        else if (canvasQrs.activeSelf)
        {
            enviarQR.OnButtonClick2();   // Call your save method for canvasQrs
            guardarButtonQR.interactable = false;
        }

        ConfirmNavigation();
    }
    public void ConfirmNavigation()
    {
        DeactivateAllCanvasMenus();

        seguroSinGuardar.SetActive(false);
        if (confirmedNavigationAction != null)
        {
            confirmedNavigationAction();
            confirmedNavigationAction = null;

            guardarButtonPA.interactable = false;
            guardarButtonQR.interactable = false;
        }
    }

    private void DeactivateAllCanvasMenus()
    {
        foreach (Transform child in menusRoot)
        {
            child.gameObject.SetActive(false);
        }
    }


}
