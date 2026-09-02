using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

//using Nobi.UiRoundedCorners; // para las esquinas redondeadas

// Personalizacion de escena movil con datos importados en DataBasePersonalizacion

// La personalizacion del camarero se hace en el código CrearCamarero

using System.Collections.Generic; // Esto es necesario para usar Dictionary
using UnityEngine.SceneManagement;

public class PersonalizacionMovil : MonoBehaviour
{
    // DEFINICIONES
    public TMP_Text textId;

    // 1. Definimos los objetos de unity que vamos a editar (lo que se arrastra desde fuera) (menos la parte de los botones de la barra inferior, apartado 5)
    public TMP_Text nombre_rest2; // este es el de dentro del scroll
    public TMP_Text nombre_rest3; // este es lo mismo pero fuera del scroll
    public Image col_fondo_titulo2;
    public Image col_fondo_titulo3;
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
    public Image puntitoPedidoCamarero;
    public Image puntitoAtencion;
    public Image fondoPedido;
    public TMP_Text textPedido;
    public Image fondoTomandoPedido;
    public TMP_Text textTomandoPedido;
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
    // 3. Url de la base de datos
    public string url;

    // 4. Esto es para que se actualicen las cosas cuando se haya cargado toda la base de datos
    private int downloadedImageCount = 0; // Counter for downloaded images BORRAR

    // 5. Parte de iconos (para escoger el tipo de iconos de la barra de abajo y editar textos -> aquí no estamos cambiando los colores, eso se hace en ButtonsColorsCode -> attached donde la barra)
    // 5.1. Aquí asignamos los iconos en el inspector de Unity (cada letra corresponde a un tipo de iconos: rellenos, finos, etc)
    public Sprite a1, a2, a3, a4;
    public Sprite b1, b2, b3, b4;
    public Sprite c1, c2, c3, c4;
    public Sprite d1, d2, d3, d4;

    // 5.2 Asignamos los iconos a los objetos de UI (los elementos como tal que van a contener los iconos en el proyecto)
    public Image icon1, icon2, icon3, icon4;
    public TMP_Text texto1, texto2, texto3, texto4; // Y los textos de debajo de los iconos


    // Diccionario que almacenará las combinaciones de sprites
    private Dictionary<int, Sprite[]> spriteCombinations;

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
        Debug.Log("Base de datos cargada ✅");
        EditarUnity();
    }
    void Awake()
    {
        // PARA LOS ICONOS
        // Inicializamos el diccionario con combinaciones
        spriteCombinations = new Dictionary<int, Sprite[]>();

        // Añadimos las combinaciones de sprites con un número
        spriteCombinations.Add(0, new Sprite[] { a1, a2, a3, a4 });
        spriteCombinations.Add(1, new Sprite[] { b1, b2, b3, b4 });
        spriteCombinations.Add(2, new Sprite[] { c1, c2, c3, c4 });
        spriteCombinations.Add(3, new Sprite[] { d1, d2, d3, d4 });
    }

    public void EditarUnity()
    {
        // CAMBIOS DE LOS OBJETOS DE UNITY CON LOS DATOS DE LA DATABASE PERSONALIZACIÓN

        // 1. Contenido textos
        nombre_rest2.text = DataBasePersonalizacion.nombre_rest[0];
        nombre_rest3.text = DataBasePersonalizacion.nombre_rest[0];

        // 2. Colores
        ChangeImageColor();

        // 3. Tamaño textos
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            nombre_rest2.fontSize = DataBasePersonalizacion.size_letra_titulo[0];
            nombre_rest3.fontSize = DataBasePersonalizacion.size_letra_titulo[0];

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
        nombre_rest3.font = fuenteNombreRest;

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
        texto1.font = fuenteGral; // texto debajo de iconos
        texto2.font = fuenteGral;
        texto3.font = fuenteGral;
        texto4.font = fuenteGral;
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
        SetIcons(3); // Esto aplicará la combinación de iconos número 2.

        // 6. Cambio el color de la letra de los botones a blanco o negro en función de si el fondo es oscuro o claro 
        UpdateTextColor(boton_ppal1, text_boton_ppal1);
        UpdateTextColor(boton_ppal2, text_boton_ppal2);
        UpdateTextColor(boton_secundario1, text_boton_sec1);
        UpdateTextColor(boton_secundario2, text_boton_sec2);
        UpdateTextColor(boton_secundario3, text_boton_sec3);
        UpdateTextColor(image_detalle_plato, detalle_plato1);
        UpdateTextColor(image_detalle_plato, detalle_plato2);
        UpdateTextColor(boton_ppal3, text_boton_ppal3);
        UpdateTextColor(botonReparto1, repartirPago2);
        UpdateTextColor(botonReparto2, repartirPago3);
        UpdateTextColor(botonReparto3, repartirPago4);
        UpdateTextColor(fondoPedido, textPedido);
        UpdateTextColor(fondoTomandoPedido, textTomandoPedido);
        //UpdateTextColor(button_pagar1, text_pagar1);
        //UpdateTextColor(button_pagar2, text_pagar2);
        //UpdateTextColor(button_pagar3, text_pagar3);

        //// 7. Redondez esquinas
        //// Obtén la referencia al componente ImageWithRoundedCorners
        //ImageWithRoundedCorners roundedCorners1 = boton_ppal1.GetComponent<ImageWithRoundedCorners>();
        //ImageWithRoundedCorners roundedCorners2 = boton_ppal2.GetComponent<ImageWithRoundedCorners>();
        //ImageWithRoundedCorners roundedCornersSec1 = boton_secundario1.GetComponent<ImageWithRoundedCorners>();
        //ImageWithRoundedCorners roundedCornersSec2 = boton_secundario2.GetComponent<ImageWithRoundedCorners>();
        //ImageWithRoundedCorners roundedCornersSec3 = boton_secundario3.GetComponent<ImageWithRoundedCorners>();
        //ImageWithRoundedCorners roundedCorners3 = boton_ppal3.GetComponent<ImageWithRoundedCorners>();

        ////// Cambia el valor de la variable radius
        //float radio=redondez_gral[0];
        //roundedCorners1.radius = radio; // Cambia el valor como necesites
        //roundedCorners1.Refresh(); // Llama a Refresh para aplicar el cambio

        //roundedCorners2.radius = radio; // Cambia el valor como necesites
        //roundedCorners2.Refresh(); // Llama a Refresh para aplicar el cambio

        //roundedCornersSec1.radius = radio; // Cambia el valor como necesites
        //roundedCornersSec1.Refresh(); // Llama a Refresh para aplicar el cambio

        //roundedCornersSec2.radius = radio/4; // Cambia el valor como necesites
        //roundedCornersSec2.Refresh(); // Llama a Refresh para aplicar el cambio

        //roundedCornersSec3.radius = radio/4; // Cambia el valor como necesites
        //roundedCornersSec3.Refresh(); // Llama a Refresh para aplicar el cambio

        //roundedCorners3.radius = radio; // Cambia el valor como necesites
        //roundedCorners3.Refresh(); // Llama a Refresh para aplicar el cambio

    }

    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        Color newColorBarsecc;
        Color newColorFondoTitulo;
        Color textColorTitulo;
        Color textColorSecc;
        Color newColorFondoIconos;
        Color newColorFondoDetalle;
        Color newColorTextDetalle;
        Color newColorBotonPpal; // COLOR PPAL
        Color newColorBotonSec; // COLOR SEC

        // Convertimos el string hex a un Color
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo[0], out newColorBarsecc)) // Cambiamos color al fondo de la barra de secciones
        {
            // Asignamos el nuevo color al componente Image
            col_fondo2.color = newColorBarsecc;
            col_fondo3.color = newColorBarsecc;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_titulo[0], out newColorFondoTitulo)) // color fondo título
        {
            // Asignamos el nuevo color al componente Image
            col_fondo_titulo2.color = newColorFondoTitulo;
            col_fondo_titulo3.color = newColorFondoTitulo;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_letra_titulo[0], out textColorTitulo)) // color letra del título
        {
            // Asignamos el nuevo color al primer componente TMP_Text
            nombre_rest2.color = textColorTitulo;
            nombre_rest3.color = textColorTitulo;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_icono[0], out newColorFondoIconos))
        {
            col_fondo_icono2.color = newColorFondoIconos;
            col_fondo_iconoC2.color = newColorFondoIconos;
            col_fondo_iconoC3.color = newColorFondoIconos;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_gral[0], out newColorFondoDetalle))
        {
            col_fondo_detalle2.color = newColorFondoDetalle;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out newColorBotonPpal)) // Cambiamos color a los botones ppales
        {
            // Asignamos el nuevo color al componente Image
            boton_ppal1.color = newColorBotonPpal;
            boton_ppal2.color = newColorBotonPpal;
            boton_ppal3.color = newColorBotonPpal;

            botonReparto1.color = newColorBotonPpal;
            botonReparto2.color = newColorBotonPpal;
            botonReparto3.color = newColorBotonPpal;

            //button_pagar1.color = newColorBotonPpal;
            //button_pagar2.color = newColorBotonPpal;
            //button_pagar3.color = newColorBotonPpal;

            // puntitoPedido.color = newColorBotonPpal; // cambio el puntito aquí
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_botones[0], out newColorBotonSec)) // Cambiamos color a los botones secundarios
        {
            // Asignamos el nuevo color al componente Image
            boton_secundario1.color = newColorBotonSec;
            boton_secundario2.color = newColorBotonSec;
            boton_secundario3.color = newColorBotonSec;

            fondoPedido.color = newColorBotonSec;
            fondoTomandoPedido.color = newColorBotonSec;

            // propina.color = newColorBotonSec;
            // volver2.color = newColorBotonSec;
            // volver3.color = newColorBotonSec;
            // Si es necesario, ajusto el color secundario por si es demasiado claro y no se leen bien las letras de ese color (propina, volver, etc)
            //AdjustTextColorSec(newColorBotonSec, propina);
            //AdjustTextColorSec(newColorBotonSec, volver2);
            //AdjustTextColorSec(newColorBotonSec, volver3);

        }

        // El puntito del pedido siempre se tiene que distinguir, usamos lo que mejor se distinga: color ppal, sec o b/n) 
        SetPointColorFromHex(DataBasePersonalizacion.col_fondo_icono[0], DataBasePersonalizacion.col_ppal_botones[0], DataBasePersonalizacion.col_sec_botones[0], puntitoPedido);
        SetPointColorFromHex(DataBasePersonalizacion.col_fondo_icono[0], DataBasePersonalizacion.col_ppal_botones[0], DataBasePersonalizacion.col_sec_botones[0], puntitoPedidoCamarero);
        // Lo mismo con puntito atencion
        SetPointColorFromHex(DataBasePersonalizacion.col_fondo_icono[0], DataBasePersonalizacion.col_ppal_botones[0], DataBasePersonalizacion.col_sec_botones[0], puntitoAtencion);

        CreateImage();
    }

    
    private void CreateImage()
    {
        Sprite[] sprites = DataBasePersonalizacion.spriteRest;
       imageRest.sprite=sprites[0];
    }

    public void SetIcons(int number)
    {
        if (spriteCombinations.ContainsKey(number))
        {
            // Accedemos a la combinación de iconos según el número
            Sprite[] selectedIcons = spriteCombinations[number];

            // Asignamos los sprites a los objetos de la UI
            icon1.sprite = selectedIcons[0];
            icon2.sprite = selectedIcons[1];
            icon3.sprite = selectedIcons[2];
            icon4.sprite = selectedIcons[3];
        }
        else
        {
            Debug.LogWarning("Número fuera de rango. No hay combinación asociada.");
        }
    }

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

