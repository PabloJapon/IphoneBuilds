using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecogerDeliveryManager : MonoBehaviour
{
    public GameObject chooseDelivery;
    public GameObject detalleCliente;
    public NavigationCamarero NC;

    private bool recoger = false;
    private int nrecoger;
    private int ndelivery;

    public void DetalleClienteRecoger()
    {
        chooseDelivery.SetActive(true);

        recoger = true;
    }

    public void DetalleClienteDelivery()
    {
        chooseDelivery.SetActive(true);

        recoger = false;
    }

    public void ContinuarPedido()
    {
        string nombreCliente = detalleCliente.GetComponentInChildren<TMP_InputField>().text;

        int mesaNumber;
        string tipo;
        if (recoger)
        {
            nrecoger++;
            mesaNumber = 1000 + nrecoger;
            tipo = "Recoger";
        }
        else
        {
            ndelivery++;
            mesaNumber = 2000 + ndelivery;
            tipo = "Delivery";
        }

        if (TPV_DataManager.instance != null && !string.IsNullOrWhiteSpace(nombreCliente))
        {
            int customerId = TPV_DataManager.instance.GetOrCreateCustomerId(nombreCliente);
            TPV_DataManager.mesaCustomerMap[mesaNumber] = customerId;
            TPV_DataManager.mesaTipoMap[mesaNumber] = tipo;
        }

        NC.TomarNota(mesaNumber, nombreCliente, DataBaseEmpresasDeliveryTPV.nameEmpresa, DataBaseEmpresasDeliveryTPV.menuEmpresa);
        detalleCliente.SetActive(false);

        IncomingCallOrderRouter.NotifyFlowFinished();
    }
}
