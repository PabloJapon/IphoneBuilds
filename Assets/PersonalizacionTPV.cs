using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

//using Nobi.UiRoundedCorners; // para las esquinas redondeadas

// La personalizacion del camarero se hace en el código CrearCamarero

using System.Collections.Generic; // Esto es necesario para usar Dictionary
using UnityEngine.SceneManagement;

public class PersonalizacionTPV : MonoBehaviour
{
    // DEFINICIONES
    public TMP_Text textId;

    // Definimos los objetos de unity que vamos a editar (lo que se arrastra desde fuera) (menos la parte de los botones de la barra inferior, apartado 5)
    public TMP_Text nombre_rest2;  // nombre rest en la barra de arriba
    public Image col_fondo_titulo; // color barra arriba (titulo restaurante). Lo ponemos a parte para que interactue como boton (que haya sombra al pulsarlo)
    public Image col_fondo_titulo2; // color barra arriba
    public TMP_Text sesionIniciada;  
    
    // Botones/imagenes/textos de arriba a los que cambiar el color en funcion del color de la barra
    public Image bordeBoton0; // Tres botones de Comandas, Facturas y Abrir caja
    public Image bordeBoton1; 
    public Image bordeBoton2;
    public Image bordeBoton3;
    public Image bordeBoton4;
    //public TMP_Text textBoton0; este ya se edita en NavigationTPV, ya que empieza pulsado (Comandas)
    public TMP_Text textBoton1;
    public TMP_Text textBoton2;
    public TMP_Text textBoton3;
    public TMP_Text textBoton4;
    //public Image botonArriba0;
    public Image botonArriba1;
    public Image botonArriba2;
    public Image botonArriba3;
    public Image botonArriba4;
    public Image botonCerrar; // Cerrar sesión
    public TMP_Text textbotonCerrar;
    public Image botonPedir;
    public TMP_Text textBotonPedir;
    public Image botonVolver; // de este necesitamos la imagen del boton (que sera blanca o negra) el borde y el texto
    public Image bordeBotonVolver;
    public TMP_Text textBotonVolver; // esto no está funcionando
    public Image fondoTomandoNota; // fondo 'Tomando nota mesa X'
    public TMP_Text textTomandoNota;
    public TMP_Text textOrdenTomandoNota;
    public TMP_Text textOrden2TomandoNota;
    public Image botonAnadir; // en detalle plato
    public TMP_Text textBotonAnadir;
                                     // botones secciones
    // en detalle plato 'Añadir'

    // Botones pantalla derecha están en CrearCamarero porque es un prefab

    // Botones Facturas
    public Image botonCrearFactura;
    public TMP_Text textBotonCrearFactura;
    public Image botonImprimirFactura;
    public TMP_Text textBotonImprimirFactura;
    public Image botonCancelarFactura;
    public TMP_Text textBotonCancelarFactura;

    // Botones Detalle Cliente
    public Image botonContinuarCliente;
    public TMP_Text textContinuarCliente;
    public Image botonCancelarCliente;
    public TMP_Text textCancelarCliente;

    // Hasta aqui Check
    public Image col_fondo2; // no scroll
    public Image col_fondo3; // sí scroll
    public Image col_fondo_icono2; // fondo barra cliente
    public Image col_fondo_iconoC2; // col fondo barra camarero
    public Image col_fondo_iconoC3; // col fondo barra superior camarero
    public Image col_fondo_detalle2;
    public Image imageRest;
    public Image boton_ppal1; //para los colores de los botones de añadir plato
    public TMP_Text text_boton_ppal1;
    public Image boton_ppal2; //para los colores de botón pedir
    public TMP_Text text_boton_ppal2;
    public Image boton_secundario1; //para los colores de los botones de añadir plato
    public Image boton_secundario2; //para los colores de los botones de añadir plato
    public Image boton_secundario3; //para los colores de los botones de añadir plato
    public TMP_Text text_boton_sec1;
    public TMP_Text text_boton_sec2;
    public TMP_Text text_boton_sec3; 
    public TMP_Text detalle_plato1;
    public TMP_Text detalle_plato2; 
    public Image image_detalle_plato;
    public Image boton_ppal3; //para los colores de botón pagar 
    public TMP_Text text_boton_ppal3;
    public TMP_Text asistencia1; // textos del cuadro de dialogo de si quieres llamar al camarero
    public TMP_Text asistencia2;
    public TMP_Text asistencia3;
    public TMP_Text asistencia4;
    public TMP_Text asistencia5;
    public TMP_Text pagar1;
    public TMP_Text advertenciaPago1;
    public TMP_Text advertenciaPago2;
    public TMP_Text advertenciaPago3;
    public TMP_Text repartirPago1;
    public TMP_Text repartirPago2;
    public TMP_Text repartirPago3;
    public TMP_Text repartirPago4;
    public Image botonReparto1; // Botones sobre como repartir la cuenta
    public Image botonReparto2; 
    public Image botonReparto3; 
    public Image puntitoPedido; 
    public Image fondoPedido;
    public TMP_Text textPedido;
    // public Image tick; // tick de las opciones dentro de un planto (salsas etc)
    public Image button_pagar1; // primer boton de pagar
    public TMP_Text text_pagar1;
    public TMP_Text propina; // boton añadir propina
    public Image button_pagar2; // boton pagar en 'equitativamente'
    public TMP_Text text_pagar2;
    public Image button_pagar3; // boton pagar en 'cada uno lo suyo'
    public TMP_Text text_pagar3;
    public TMP_Text volver2; // texto volver en 'equitativamente'
    public TMP_Text volver3; // texto volver en 'cada uno lo suyo'
    // Botones de secciones de 'Ajustar Caja'
    public Image buttonAbrirCaja1; // Aceptar
    public TMP_Text textButtonAbrirCaja1;
    public Image buttonAbrirCaja2; // Cancelar
    public Image bordeButtonAbrirCaja2;  
    public TMP_Text textButtonAbrirCaja2;
    public Image buttonCerrarCaja1; // Aceptar
    public TMP_Text textButtonCerrarCaja1;
    public Image buttonCerrarCaja2; // Cancelar
    public Image bordeButtonCerrarCaja2;  
    public TMP_Text textButtonCerrarCaja2;
    public Image buttonMovimientos1; 
    public TMP_Text textButtonMovimientos1; 
    public Image buttonMovimientos2;
    public TMP_Text textButtonMovimientos2; 
    public Image imageButtonMovimientos2;
    public Image buttonAñadirMovimiento1; // Aceptar
    public TMP_Text textButtonAñadirMovimiento1;
    public Image buttonAñadirMovimiento2; // Cancelar
    public Image bordeButtonAñadirMovimiento2;
    public TMP_Text textButtonAñadirMovimiento2;
    public Image buttonRepZ1;
    public TMP_Text textButtonRepZ1; 
    public Image imageButtonRepZ1;  
    public Image buttonRepZ2;
    public TMP_Text textButtonRepZ2; 
    public Image imageButtonRepZ2;   
    public Image buttonRepX1;
    public TMP_Text textButtonRepX1;
    public Image imageButtonRepX1;    
    public Image buttonRepX2;
    public TMP_Text textButtonRepX2; 
    public Image imageButtonRepX2;   

    // 3. Url de la base de datos
    public string url;

    // 5. Parte de iconos (para escoger el tipo de iconos de la barra de abajo y editar textos -> aquí no estamos cambiando los colores, eso se hace en ButtonsColorsCode -> attached donde la barra)
    // 5.1. Aquí asignamos los iconos en el inspector de Unity (cada letra corresponde a un tipo de iconos: rellenos, finos, etc)
    // public Sprite a1, a2, a3, a4;
    // public Sprite b1, b2, b3, b4;
    // public Sprite c1, c2, c3, c4;
    // public Sprite d1, d2, d3, d4;

    // // 5.2 Asignamos los iconos a los objetos de UI (los elementos como tal que van a contener los iconos en el proyecto)
    // public Image icon1, icon2, icon3, icon4;
    // public TMP_Text texto1, texto2, texto3, texto4; // Y los textos de debajo de los iconos

    


    // Para asegurar que se haya importado ya la DB Personalizacion
    public DataBasePersonalizacion DBP; // Reference to the DataBase component
    private bool isDBLoaded = false;

    public AspectFill aspectFillImageRestaurante;

    public bool IsLoaded { get; private set; } = false; // para ver desde otros scripts si se ha cargado o no la DB (ej ButtonsColorsCode para el camarero)


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

        // Aquí ya puedes arrancar tu lógica principal
        //Debug.Log("Base de datos cargada ✅");
        EditarUnity();
    }

    public void EditarUnity()
    {
        // CAMBIOS DE LOS OBJETOS DE UNITY CON LOS DATOS DE LA DATABASE PERSONALIZACIÓN

        // 1. Contenido textos
        nombre_rest2.text = DataBasePersonalizacion.nombre_rest[0];

        // 2. Colores
        ChangeImageColor();
        // iconos de los tres botones con borde();

        // 3. Tamaño textos
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            nombre_rest2.fontSize = DataBasePersonalizacion.size_letra_titulo[0];

            text_boton_ppal1.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            text_boton_ppal2.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            text_boton_sec1.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            text_boton_ppal2.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            text_boton_ppal3.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            detalle_plato1.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // detalle_plato2.fontSize = size_letra_gral[0].Replace(" ", "");
            // asistencia1.fontSize = size_letra_gral[0].Replace(" ", "");
            // asistencia2.fontSize = size_letra_gral[0].Replace(" ", "");
            // asistencia3.fontSize = size_letra_gral[0].Replace(" ", "");
            // asistencia4.fontSize = size_letra_gral[0].Replace(" ", "");
            // asistencia5.fontSize = size_letra_gral[0].Replace(" ", "");
            pagar1.fontSize = DataBasePersonalizacion.size_letra_gral[0];
            // advertenciaPago1.fontSize = size_letra_gral[0].Replace(" ", "");
            // advertenciaPago2.fontSize = size_letra_gral[0].Replace(" ", "");
            // advertenciaPago3.fontSize = size_letra_gral[0].Replace(" ", "");
            // repartirPago1.fontSize = size_letra_gral[0].Replace(" ", "");
            // repartirPago2.fontSize = size_letra_gral[0].Replace(" ", "");
            // repartirPago3.fontSize = size_letra_gral[0].Replace(" ", "");
            // repartirPago4.fontSize = size_letra_gral[0].Replace(" ", "");
            textPedido.fontSize = DataBasePersonalizacion.size_letra_gral[0];
        }

        // 4. Tipo letra
        // Construimos la ruta con el nombre de la fuente del título para poder cargarla (tiene que estar en la dirección Resources/Fonts)
        string rutaFuenteNombreRest = "Fonts/" + DataBasePersonalizacion.letra_titulo[0].Replace(" ", "");
        TMP_FontAsset fuenteNombreRest = Resources.Load<TMP_FontAsset>(rutaFuenteNombreRest);
        if (fuenteNombreRest == null)
            fuenteNombreRest = Resources.Load<TMP_FontAsset>(rutaFuenteNombreRest + " SDF");
        nombre_rest2.font = fuenteNombreRest;

        // Construimos la ruta con el nombre de la fuente general para poder cargarla (tiene que estar en la dirección Resources/Fonts)
        string rutaFuenteGral = "Fonts/" + DataBasePersonalizacion.letra_gral[0].Replace(" ", "");
        TMP_FontAsset fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral);
        if (fuenteGral == null)
            fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral + " SDF");
        text_boton_ppal1.font = fuenteGral;
        text_boton_ppal2.font = fuenteGral;
        text_boton_sec1.font = fuenteGral;
        text_boton_ppal2.font = fuenteGral;
        text_boton_ppal3.font = fuenteGral;
        // texto1.font = fuenteGral; // texto debajo de iconos
        // texto2.font = fuenteGral;
        // texto3.font = fuenteGral;
        // texto4.font = fuenteGral;
        detalle_plato1.font = fuenteGral;
        detalle_plato2.font = fuenteGral;
        asistencia1.font = fuenteGral;
        asistencia2.font = fuenteGral;
        asistencia3.font = fuenteGral;
        asistencia4.font = fuenteGral;
        asistencia5.font = fuenteGral;
        pagar1.font = fuenteGral;
        advertenciaPago1.font = fuenteGral;
        advertenciaPago2.font = fuenteGral;
        advertenciaPago3.font = fuenteGral;
        repartirPago1.font = fuenteGral;
        repartirPago2.font = fuenteGral;
        repartirPago3.font = fuenteGral;
        repartirPago4.font = fuenteGral;
        textPedido.font = fuenteGral;
        //text_pagar1.font = fuenteGral;
        //text_pagar2.font = fuenteGral;
        //text_pagar3.font = fuenteGral;
        //propina.font = fuenteGral;
        //volver2.font = fuenteGral;
        //volver3.font = fuenteGral;

        // 5. Tipo iconos (esto viene del código CombinacionesIconos)
        // SetIcons(3); // Esto aplicará la combinación de iconos número 2.

        // 6. Cambio el color de la letra de los botones a blanco o negro en función de si el fondo es oscuro o claro 
        // UpdateTextColor(boton_ppal1, text_boton_ppal1);
        // UpdateTextColor(boton_ppal2, text_boton_ppal2);
        // UpdateTextColor(boton_secundario1, text_boton_sec1);
        // UpdateTextColor(boton_secundario2, text_boton_sec2);
        // UpdateTextColor(boton_secundario3, text_boton_sec3);
        // UpdateTextColor(image_detalle_plato, detalle_plato1);
        // UpdateTextColor(image_detalle_plato, detalle_plato2);
        // UpdateTextColor(boton_ppal3, text_boton_ppal3);
        // UpdateTextColor(botonReparto1, repartirPago2);
        // UpdateTextColor(botonReparto2, repartirPago3);
        // UpdateTextColor(botonReparto3, repartirPago4);
        // UpdateTextColor(fondoPedido, textPedido);
        //UpdateTextColor(button_pagar1, text_pagar1);
        //UpdateTextColor(button_pagar2, text_pagar2);
        //UpdateTextColor(button_pagar3, text_pagar3);

        //7. Cambio color a botones e imagenes en funcion del fondo (como en la barra de arriba) y textos
        UpdateTextColor(col_fondo_titulo2, nombre_rest2);
        UpdateTextColor(col_fondo_titulo2, sesionIniciada);
        UpdateImageColor(col_fondo_titulo2, bordeBoton0);
        UpdateImageColor(col_fondo_titulo2, bordeBoton1);
        UpdateImageColor(col_fondo_titulo2, bordeBoton2);
        UpdateImageColor(col_fondo_titulo2, bordeBoton3);
        UpdateImageColor(col_fondo_titulo2, bordeBoton4);
        // UpdateTextColor(col_fondo_titulo2, textBoton0); 
        UpdateTextColor(col_fondo_titulo2, textBoton1); 
        UpdateTextColor(col_fondo_titulo2, textBoton2);
        UpdateTextColor(col_fondo_titulo2, textBoton3);
        UpdateTextColor(col_fondo_titulo2, textBoton4);
        UpdateImageColor(col_fondo_titulo2, botonCerrar); // cerrar sesion 
        UpdateTextColor(botonPedir, textBotonPedir); 
        UpdateImageColor(col_fondo_titulo2, botonVolver);
        UpdateTextColor(fondoTomandoNota, textTomandoNota);
        UpdateTextColor(fondoTomandoNota, textOrdenTomandoNota);
        UpdateTextColor(fondoTomandoNota, textOrden2TomandoNota);
        UpdateTextColor(botonAnadir, textBotonAnadir);  
        UpdateTextColor(col_fondo_titulo2, textBotonCrearFactura); // botones factura
        UpdateTextColor(col_fondo_titulo2, textBotonImprimirFactura);
        UpdateTextColor(col_fondo_titulo2, textBotonCancelarFactura);
        // Botones Detalle Clientes
        UpdateTextColor(col_fondo_titulo2, textContinuarCliente);
        UpdateTextColor(col_fondo_titulo2, textCancelarCliente);
        // botones secciones 'Ajustar Caja'
        UpdateTextColor(buttonAbrirCaja1, textButtonAbrirCaja1);
        UpdateImageColor(buttonAbrirCaja1, buttonAbrirCaja2);
        UpdateTextColor(buttonAbrirCaja1, textButtonCerrarCaja1);
        UpdateImageColor(buttonAbrirCaja1, buttonCerrarCaja2);
        UpdateTextColor(buttonAñadirMovimiento1, textButtonAñadirMovimiento1);
        UpdateImageColor(buttonAñadirMovimiento1, buttonAñadirMovimiento2);
        UpdateTextColor(buttonMovimientos1, textButtonMovimientos1);
        UpdateTextColor(buttonMovimientos1, textButtonMovimientos2);
        UpdateImageColor(buttonMovimientos1, imageButtonMovimientos2);
        UpdateTextColor(buttonRepZ1, textButtonRepZ1);
        UpdateImageColor(buttonRepZ1, imageButtonRepZ1);
        UpdateTextColor(buttonRepZ1, textButtonRepZ2);
        UpdateImageColor(buttonRepZ1, imageButtonRepZ2);
        UpdateTextColor(buttonRepX1, textButtonRepX1);
        UpdateImageColor(buttonRepX1, imageButtonRepX1);
        UpdateTextColor(buttonRepX1, textButtonRepX2);
        UpdateImageColor(buttonRepX1, imageButtonRepX2);
    }

    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        // Color newColorBarsecc;
        // Color newColorFondoTitulo;
        // Color textColorTitulo;
        // Color textColorSecc;
        // Color newColorFondoIconos;
        // Color newColorFondoDetalle;
        // Color newColorTextDetalle;
        Color newColorBotonPpal; // COLOR PPAL
        Color newColorBotonSec; // COLOR SEC

        // Debug.Log("color ppal:" + DataBasePersonalizacion.col_ppal_empl[0]);

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out newColorBotonPpal)) // color fondo título lo cambio por color ppal empleado
        {
            // Asignamos el nuevo color al componente Image de la barra de arriba
            col_fondo_titulo.color = newColorBotonPpal;
            col_fondo_titulo2.color = newColorBotonPpal;
            // Y tambien a los tres botones
            // botonArriba0.color = newColorBotonPpal; 
            botonArriba1.color = newColorBotonPpal; //
            botonArriba2.color = newColorBotonPpal;
            botonArriba3.color = newColorBotonPpal;
            botonArriba4.color = newColorBotonPpal;
            textbotonCerrar.color = newColorBotonPpal;
            // botones pedir y volver (al tomar nota)
            botonPedir.color = newColorBotonPpal;
            bordeBotonVolver.color = newColorBotonPpal;
            textBotonVolver.color = newColorBotonPpal;
            // boton añadir
            botonAnadir.color = newColorBotonPpal;
            // botones facturas
            botonCrearFactura.color = newColorBotonPpal;
            botonImprimirFactura.color = newColorBotonPpal;
            botonCancelarFactura.color = newColorBotonPpal;
            // botones detalle clientes
            botonContinuarCliente.color = newColorBotonPpal;
            botonCancelarCliente.color = newColorBotonPpal;

            // botones secciones 'Ajustar Caja'  
            buttonAbrirCaja1.color = newColorBotonPpal;  
            bordeButtonAbrirCaja2.color = newColorBotonPpal; 
            textButtonAbrirCaja2.color = newColorBotonPpal; 
            buttonCerrarCaja1.color = newColorBotonPpal;  
            bordeButtonCerrarCaja2.color = newColorBotonPpal; 
            textButtonCerrarCaja2.color = newColorBotonPpal; 
            buttonMovimientos1.color = newColorBotonPpal;  
            buttonMovimientos2.color = newColorBotonPpal;
            buttonAñadirMovimiento1.color = newColorBotonPpal;
            bordeButtonAñadirMovimiento2.color = newColorBotonPpal;
            textButtonAñadirMovimiento2.color = newColorBotonPpal;
            buttonRepZ1.color = newColorBotonPpal;  
            buttonRepZ2.color = newColorBotonPpal;   
            buttonRepX1.color = newColorBotonPpal;  
            buttonRepX2.color = newColorBotonPpal;  

        }

        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_empl[0], out newColorBotonSec)) // color fondo título lo cambio por color ppal empleado
        {
            fondoTomandoNota.color = newColorBotonSec;
        }
        // if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_letra_titulo[0], out textColorTitulo)) // color letra del título ---> titulo va a pasar a b/n
        // {
        //     // Asignamos el nuevo color al primer componente TMP_Text
        //     nombre_rest2.color = textColorTitulo;
        // // }
        // if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_icono[0], out newColorFondoIconos))
        // {
        //     col_fondo_icono2.color = newColorFondoIconos;
        //     col_fondo_iconoC2.color = newColorFondoIconos;
        //     col_fondo_iconoC3.color = newColorFondoIconos;
        // }
        // if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_gral[0], out newColorFondoDetalle))
        // {
        //     col_fondo_detalle2.color = newColorFondoDetalle;
        // }
        // if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out newColorBotonPpal)) // Cambiamos color a los botones ppales
        // {
        //     // Asignamos el nuevo color al componente Image
        //     boton_ppal1.color = newColorBotonPpal;
        //     boton_ppal2.color = newColorBotonPpal;
        //     boton_ppal3.color = newColorBotonPpal;

        //     botonReparto1.color = newColorBotonPpal;
        //     botonReparto2.color = newColorBotonPpal;
        //     botonReparto3.color = newColorBotonPpal;

        //     //button_pagar1.color = newColorBotonPpal;
        //     //button_pagar2.color = newColorBotonPpal;
        //     //button_pagar3.color = newColorBotonPpal;

        //     // puntitoPedido.color = newColorBotonPpal; // cambio el puntito aquí
        // }
        // if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_botones[0], out newColorBotonSec)) // Cambiamos color a los botones secundarios
        // {
        //     // Asignamos el nuevo color al componente Image
        //     boton_secundario1.color = newColorBotonSec;
        //     boton_secundario2.color = newColorBotonSec;
        //     boton_secundario3.color = newColorBotonSec;

        //     fondoPedido.color = newColorBotonSec;

        //     // propina.color = newColorBotonSec;
        //     // volver2.color = newColorBotonSec;
        //     // volver3.color = newColorBotonSec;
        //     // Si es necesario, ajusto el color secundario por si es demasiado claro y no se leen bien las letras de ese color (propina, volver, etc)
        //     //AdjustTextColorSec(newColorBotonSec, propina);
        //     //AdjustTextColorSec(newColorBotonSec, volver2);
        //     //AdjustTextColorSec(newColorBotonSec, volver3);

        // }

        // El puntito del pedido siempre se tiene que distinguir, usamos lo que mejor se distinga: color ppal, sec o b/n) 
        SetPointColorFromHex(DataBasePersonalizacion.col_fondo_icono[0], DataBasePersonalizacion.col_ppal_botones[0], DataBasePersonalizacion.col_sec_botones[0], puntitoPedido);
    }

    
    private void CreateImage()
    {
        Sprite[] sprites = DataBasePersonalizacion.spriteRest;
       imageRest.sprite=sprites[0];
    }

    // public void SetIcons(int number)
    // {
    //     if (spriteCombinations.ContainsKey(number))
    //     {
    //         // Accedemos a la combinación de iconos según el número
    //         Sprite[] selectedIcons = spriteCombinations[number];

    //         // Asignamos los sprites a los objetos de la UI
    //         icon1.sprite = selectedIcons[0];
    //         icon2.sprite = selectedIcons[1];
    //         icon3.sprite = selectedIcons[2];
    //         icon4.sprite = selectedIcons[3];
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Número fuera de rango. No hay combinación asociada.");
    //     }
    // }

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
    
    void AdjustTextColorSec(Color inputColor, TMP_Text text) // por si el color secundario es demasiado claro y no se leen bien las letras de ese color (propina, volver, etc)
    {
        // Calcular la luminancia del color de entrada
        float luminance = 0.299f * inputColor.r + 0.587f * inputColor.g + 0.114f * inputColor.b;

        // Umbral para considerar que el color es "demasiado claro"
        float threshold = 0.7f;

        if (luminance > threshold)
        {
            // Oscurecer el color si es demasiado claro
            float darkenFactor = 0.5f;
            inputColor.r *= darkenFactor;
            inputColor.g *= darkenFactor;
            inputColor.b *= darkenFactor;
        }

        // Aplicar el color ajustado al texto
        text.color = inputColor;
    }

    // ******* Funciones para que el puntito del pedido siempre se distinga (con color ppal, sec o b/n)// Calcula la distancia euclidiana entre dos colores en RGB *******
    // Calcula la distancia euclidiana entre dos colores en RGB
    static float ColorDistance(Color c1, Color c2)
    {
        float dr = c1.r - c2.r;
        float dg = c1.g - c2.g;
        float db = c1.b - c2.b;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db);
    }

    // Calcula la luminancia de un color
    static float Luminance(Color c)
    {
        return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
    }

    // Devuelve el color (negro o blanco) que mejor contraste con baseColor
    static Color GetBestContrastBlackOrWhite(Color baseColor)
    {
        float lum = Luminance(baseColor);
        return (lum > 0.5f) ? Color.black : Color.white;
    }

    // Función que recibe strings hex y el Image y asigna el color correcto al texto "punto"
    public void SetPointColorFromHex(string hexColorBarra, string hexColorPpal, string hexColorSec, Image punto)
    {
        Color colorBarra, colorPpal, colorSec;

        if (!ColorUtility.TryParseHtmlString(hexColorBarra, out colorBarra))
        {
            Debug.LogWarning("No se pudo parsear hexColorBarra, usando blanco");
            colorBarra = Color.white;
        }
        if (!ColorUtility.TryParseHtmlString(hexColorPpal, out colorPpal))
        {
            Debug.LogWarning("No se pudo parsear hexColorPpal, usando negro");
            colorPpal = Color.black;
        }
        if (!ColorUtility.TryParseHtmlString(hexColorSec, out colorSec))
        {
            Debug.LogWarning("No se pudo parsear hexColorSec, usando negro");
            colorSec = Color.black;
        }

        float umbralDistancia = 0.4f;

        if (ColorDistance(colorBarra, colorPpal) > umbralDistancia)
        {
            punto.color = colorPpal;
            return;
        }

        if (ColorDistance(colorBarra, colorSec) > umbralDistancia)
        {
            punto.color = colorSec;
            return;
        }

        punto.color = GetBestContrastBlackOrWhite(colorBarra);
    }

    // ******* Fin de cambio de color puntito *******
}
