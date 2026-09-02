using UnityEngine;
using UnityEngine.UI;

public class QuitManager : MonoBehaviour
{
    public GameObject quitConfirmationPanel;  // Assign your UI GameObject in the Inspector

    // "Guardar" buttons
    public Button guardarButtonPA;
    public Button guardarButtonQR;

    public GameObject canvasCliente;
    public GameObject canvasQrs;

    public EnviarDatosPersonalizacion enviarPA;
    public EnviarDatosQrs enviarQR;

    public RespDataBasePersonalizacion RDBPA;
    public RespDataBaseQrs RDBQR;

    private void Awake()
    {
        Application.wantsToQuit += OnAttemptQuit;
    }

    private bool OnAttemptQuit()
    {
        if ((canvasCliente.activeSelf && guardarButtonPA.interactable) ||
            (canvasQrs.activeSelf && guardarButtonQR.interactable))
        {
            quitConfirmationPanel.SetActive(true);  // Show the quit confirmation panel
            return false; // Prevent quitting until the user confirms
        }

        return true; // Allow quitting if no conditions are met
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

        ConfirmQuit();
    }

    public void NoGuardar()
    {
        RDBPA.RellenarCampos();
        RDBQR.RellenarCampos();

        ConfirmQuit();
    }

    public void ConfirmQuit()
    {
        Application.wantsToQuit -= OnAttemptQuit; // Remove event listener to allow quitting
        Application.Quit(); // Quit the application
    }
}
