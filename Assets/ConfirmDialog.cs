using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmDialog : MonoBehaviour
{
    public static ConfirmDialog instance;

    public GameObject panelRaiz; // el fondo oscuro + caja del popup, todo desactivado por defecto
    public TMP_Text mensajeText;
    public Button botonOk;
    public Button botonCancelar;

    private Action onConfirmar;

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        panelRaiz.SetActive(false);
        botonOk.onClick.AddListener(Confirmar);
        botonCancelar.onClick.AddListener(Cancelar);
    }

    public void Mostrar(string mensaje, Action callbackSiConfirma)
    {
        onConfirmar = callbackSiConfirma;
        mensajeText.text = mensaje;
        panelRaiz.SetActive(true);
    }

    private void Confirmar()
    {
        panelRaiz.SetActive(false);
        onConfirmar?.Invoke();
        onConfirmar = null;
    }

    private void Cancelar()
    {
        panelRaiz.SetActive(false);
        onConfirmar = null;
    }
}