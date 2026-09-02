// vale lo que tengo que hacer es
// hay dos colores: pulsado (blanco) y no pulsado. en personalizacion todos los botones adquieren el color 'no pulsado'
// y aquí pondremos que por defecto aparezca pulsado el botón 'Comandas', y que cuando se pulse otro, se hagan los cambios correspondientes

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NavigationBarraTPV : MonoBehaviour
{
    // canvas
    public GameObject canvasZonaCliente;          // Comandas
    public GameObject barraTomarNota;
    public GameObject canvasFacturas;       
    public GameObject canvasCrearFacturas;  
    public GameObject canvasAjusteCaja;

    public GameObject canvasListadoMovimientos;
    public GameObject canvasMovimientosCaja;
    public GameObject canvasReporteX;
    public GameObject canvasReporteZ;

    public GameObject canvasMenuDesactivar;
    public GameObject canvasPanelTurnos;
    public GameObject canvasFichajes;

    // botones
    public Button buttonComandas;
    public Button buttonAjusteCaja;
    public Button buttonReservas;
    public MenuMasController menuMas;
    public Color ColorPrincipalPersonalizacion => normalBackgroundColor;

    // barra arriba (referencia de color para elegir entre negro y blanco)
    public Image barra;

    // Color dinámico
    private Color normalBackgroundColor;

    // para asegurar que se haya importado ya la db personalizacion
    public DataBasePersonalizacion DBP; // reference to the database component
    private bool isDBLoaded = false;

    void Start()
    {
        // Nos suscribimos al evento de que la DB se cargó
        DBP.OnDataLoaded += OnDBLoaded;
    }

    private void OnDestroy()
    {
        // Siempre es buena práctica desuscribirse
        DBP.OnDataLoaded -= OnDBLoaded;
    }

    private void OnDBLoaded()
    {
        isDBLoaded = true;

        LoadColorsFromDatabase();
        ActivateComandas(); // por defecto comandas activo
    }

    // 🔹 cargar colores desde la db
    public void LoadColorsFromDatabase()
    {
        Color colorPpal;
        //color colorsec;

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out colorPpal))
        {
            normalBackgroundColor = colorPpal; //Este color está bien, rojo
        }

         //     if (colorutility.tryparsehtmlstring(DataBasePersonalizacion.col_sec_empl[0], out colorsec))
         //     {
         //         normalbackgroundcolor = colorsec;
         //     }

    }

    void ResetButtons()
    {
        ResetButtonStyle(buttonComandas);
        ResetButtonStyle(buttonAjusteCaja);
        ResetButtonStyle(buttonReservas);
        if (menuMas != null) menuMas.SetSeleccionado(false);   // ← AQUÍ
    }

    void ResetButtonStyle(Button button)
    {
        // fondo
        button.image.color = normalBackgroundColor;

        // texto (tmp)
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        UpdateTextColor(barra, text); 
    }

    void SelectButton(Button button)
    {
        UpdateImageColor(barra, button.image);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.color = normalBackgroundColor;
    }

    public void ActivateComandas()
    {        
        canvasFacturas.SetActive(false); 
        canvasCrearFacturas.SetActive(false);
        canvasAjusteCaja.SetActive(false);
        canvasMenuDesactivar.SetActive(false);

        HideAllFacturasCanvases();

        canvasPanelTurnos.SetActive(false);
        canvasFichajes.SetActive(false);
        ResetButtons();
        SelectButton(buttonComandas);
    }

    public void ActivateFacturas()
    {        
        canvasZonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        canvasFacturas.SetActive(true);
        canvasAjusteCaja.SetActive(false);
        canvasCrearFacturas.SetActive(false);
        canvasMenuDesactivar.SetActive(false);

        HideAllFacturasCanvases();

        canvasPanelTurnos.SetActive(false);
        canvasFichajes.SetActive(false);
        ResetButtons();
        if (menuMas != null) menuMas.SetSeleccionado(true);
    }

    public void ActivateAjustarCaja()
    {
        canvasZonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        canvasAjusteCaja.SetActive(true);
        canvasFacturas.SetActive(false);
        canvasCrearFacturas.SetActive(false);
        canvasMenuDesactivar.SetActive(false);

        HideAllFacturasCanvases();
        canvasPanelTurnos.SetActive(false);
        canvasFichajes.SetActive(false);
        ResetButtons();
        SelectButton(buttonAjusteCaja);
    }

    public void ActivateReservas()
    {
        ResetButtons();
        SelectButton(buttonReservas);
    }

    // Funciones cambio de color
    void UpdateTextColor(Image boton, TMP_Text text)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        // Cambiar el color del texto basado en la luminancia
        if (luminance > 0.5f)
        {
            // Fondo claro, texto negro
            text.color = Color.black;
        }
        else
        {
            // Fondo oscuro, texto blanco
            text.color = Color.white;
        }
    }

    // Lo mismo pero para imagenes (botones de barra de arriba etc)
    void UpdateImageColor(Image boton, Image imageCambiaColor)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton.color;

        // Calcular la luminancia usando la fórmula estándar
        float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

        // Cambiar el color de la imagen basado en la luminancia
        if (luminance > 0.5f)
        {
            // Fondo claro, imagen negro
            imageCambiaColor.color = Color.black;
        }
        else
        {
            // Fondo oscuro, imagen blanco
            imageCambiaColor.color = Color.white;
        }
    }

        public void ActivateGestionCarta()
    {
        canvasZonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        canvasAjusteCaja.SetActive(false);
        canvasFacturas.SetActive(false);
        canvasCrearFacturas.SetActive(false);
        canvasMenuDesactivar.SetActive(true);

        HideAllFacturasCanvases();
        canvasPanelTurnos.SetActive(false);
        canvasFichajes.SetActive(false);
        ResetButtons();
        if (menuMas != null) menuMas.SetSeleccionado(true);
    }

    public void ActivatePanelTurnos()
    {
        canvasZonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        canvasFacturas.SetActive(false);
        canvasCrearFacturas.SetActive(false);
        canvasAjusteCaja.SetActive(false);
        canvasMenuDesactivar.SetActive(false);
        canvasFichajes.SetActive(false);
        HideAllFacturasCanvases();

        canvasPanelTurnos.SetActive(true);

        ResetButtons();
        if (menuMas != null) menuMas.SetSeleccionado(true);
    }

    public void ActivateFichajes()
    {
        canvasZonaCliente.SetActive(false);
        barraTomarNota.SetActive(false);
        canvasFacturas.SetActive(false);
        canvasCrearFacturas.SetActive(false);
        canvasAjusteCaja.SetActive(false);
        canvasMenuDesactivar.SetActive(false);
        canvasPanelTurnos.SetActive(false);
        HideAllFacturasCanvases();

        canvasFichajes.SetActive(true);

        ResetButtons();
        if (menuMas != null) menuMas.SetSeleccionado(true);
    }

    public void HideAllFacturasCanvases()
    {
        canvasListadoMovimientos.SetActive(false);
        canvasMovimientosCaja.SetActive(false);
        canvasReporteX.SetActive(false);
        canvasReporteZ.SetActive(false);
    }

}
