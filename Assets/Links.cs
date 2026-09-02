using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Links : MonoBehaviour
{
    public void OpenProximoPago()
    {
        Application.OpenURL("https://gastrali.com/micuenta/"); // Reemplaza con el link deseado
    }
    public void OpenCambiarPlan()
    {
        Application.OpenURL("https://gastrali.com/precios/"); // Reemplaza con el link deseado
    }

    public void OpenFormaPago()
    {
        Application.OpenURL("https://gastrali.com/metododepago/"); // Reemplaza con el link deseado
    }

    public void OpenFacturas()
    {
        Application.OpenURL("https://gastrali.com/misfacturas/"); // Reemplaza con el link deseado
    }

    public void OpenStripe()
    {
        Application.OpenURL("https://gastrali.com/create_account_stripe/"); // Reemplaza con el link deseado
    }
}
