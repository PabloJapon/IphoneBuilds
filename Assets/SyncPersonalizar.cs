using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class SyncPersonalizar : MonoBehaviour
{
    // References
    public TMP_InputField nombreEstablecimientoSource;
    public Image colorFondoSource;
    public Image colorLetraSource;
    public Image colorFondoBarraSeccionesSource;
    public Image colorEtiquetasBarraSeccionesSource;
    public Image colorFondoSeccionSource;
    public Image colorFondoPlatosSource;
    public Image colorBarraSource;
    public Image colorIcono1Source;
    public Image colorIcono2Source;
    public TMP_Dropdown dropdownFuente1;
    public TMP_Dropdown dropdownFuente2;
    public TMP_Dropdown dropdownFuente3;
    public TMP_Dropdown dropdownFuente4;
    public TMP_Dropdown dropdownSize1;
    public TMP_Dropdown dropdownSize2;
    public TMP_Dropdown dropdownSize3;
    public TMP_Dropdown dropdownIconos;
    public Image colorPpalSource;
    public Image colorSecSource;
    // Empleados
    public Image color1Source;
    public Image color2Source;

    // To sync
    public TMP_Text nombreEstablecimiento;
    public Image colorFondo;
    public Image colorFondoBarraSecciones;
    public TMP_Text LetraEtiqueta1;
    public TMP_Text LetraEtiqueta2;
    public TMP_Text LetraEtiqueta3;
    public TMP_Text LetraEtiqueta4;
    public Image colorEtiqueta1;
    public Image colorEtiqueta2;
    public Image colorEtiqueta3;
    public Image colorEtiqueta4;
    public Image colorFondoSeccion;
    public Image colorPlato1;
    public Image colorPlato2;
    public Image colorPlato3;
    public Image colorBarra;
    public Image colorBarra2;
    public Image Puntito;
    // Textos iconos cliente
    public TMP_Text Icono1;
    public TMP_Text Icono2;
    public TMP_Text Icono3;
    public TMP_Text Icono4;
    // Textos camarero
    public TMP_Text Numero1;
    public TMP_Text Numero2;
    public TMP_Text Numero3;
    public TMP_Text Mesa;
    public TMP_Text IconoC1;
    public TMP_Text IconoC2;
    public TMP_Text IconoC3;
    public TMP_Text IconoC4;
    // Más textos
    public TMP_Text LetraSec;
    public TMP_Text LetraPlato1;
    public TMP_Text LetraPlato2;
    public TMP_Text LetraPlato3;
    public TMP_Text LetraPlato4;
    public TMP_Text LetraPlato5;
    public TMP_Text LetraPlato6;
    public TMP_Text LetraPlato7;
    public TMP_Text LetraPlato8;
    // Empleados
    public Image colorComanda1;
    public Image colorComanda2;
    public Image colorComanda3;
    public Image colorComanda4;
    public Image colorComanda5;
    public Image colorComanda6;
    public Image colorBarraCocina;
    // Más textos
    public TMP_Text LetraMesa1;
    public TMP_Text LetraMesa2;
    public TMP_Text LetraMesa3;
    public TMP_Text LetraMesa4;
    public TMP_Text LetraMesa5;
    public TMP_Text LetraMesa6;
    public TMP_Text LetraBarra1;
    public TMP_Text LetraBarra2;
    public TMP_Text LetraBarra3;
    // Todos los textos de las comandas
    public TMP_Text TextC1, TextC2, TextC3, TextC4, TextC5, TextC6, TextC7, TextC8,
                TextC9, TextC10, TextC11, TextC12, TextC13, TextC14, TextC15, TextC16,
                TextC17, TextC18, TextC19, TextC20, TextC21, TextC22, TextC23, TextC24,
                TextC25, TextC26, TextC27, TextC28, TextC29, TextC30, TextC31, TextC32;
    // Imagenes iconos
    public Image imageIcono1;
    public Image imageIcono2;
    public Image imageIcono3;
    public Image imageIcono4;
    public Image imageIconoC1;
    public Image imageIconoC2;
    public Image imageIconoC3;
    public Image imageIconoC4;
    // Aquí asignamos los iconos en el inspector de Unity (cada letra corresponde a un tipo de iconos: rellenos, finos, etc)
    public Sprite a1, a2, a3, a4;
    public Sprite b1, b2, b3, b4;
    public Sprite c1, c2, c3, c4;
    public Sprite d1, d2, d3, d4;
    // Diccionario que almacenará las combinaciones de sprites
    private Dictionary<int, Sprite[]> spriteCombinations;

    // Internal parameters to store last values
    private string lastNombreEstablecimiento;
    private Color lastColorFondo;
    private Color lastColorLetra;
    private Color lastColorFondoBarraSecciones;
    private Color lastColorEtiquetasBarraSecciones;
    private Color lastColorFondoSeccion;
    private Color lastColorFondoPlato;
    private Color lastColorBarra;
    private Color lastColorIcono1;
    private Color lastColorIcono2;
    private String lastTextFuente1; // fuente nombre rest
    private String lastTextFuente2; // fuente titulos
    private String lastTextFuente3; // fuente gral
    private String lastTextFuente4; // fuente empleados
    private int lastTextSize1; // fuente nombre rest
    private int lastTextSize2; // fuente titulos
    private int lastTextSize3; // fuente gral
    private int lastIcon;
    private Color lastColorPpal;
    private Color lastColorSec;

    // Empleados
    private Color lastColor1;
    private Color lastColor2;

    // mas objetos a cambiar de las pantallas de vista previa (detalle plato y pagar)
    // colores
    public Image botonSec;
    public Image botonPpal1;
    public Image botonPpal2;
    public Image botonPpal3;
    public Image botonPpal4;
    public Image botonPpal5;
    public Image iconoPagarPpal1;
    public Image iconoPagarPpal2;
    public Image fondoSec1;
    public Image fondoSec2;
    public Image fondoSec3;
    public Image fondoSec4;
    public Image fondoSecciones;
    public Image barraInferior1;
    public Image barraInferior2;
    public Image barraInferior3;
    public Image iconoSelec1;
    public Image iconoSelec2;
    public Image iconoSelec3;
    public Image iconoNoSelec1;
    public Image iconoNoSelec2;
    public Image iconoNoSelec3;
    public Image iconoNoSelec4;
    public Image iconoNoSelec5;
    public Image iconoNoSelec6;
    public Image iconoNoSelec7;
    public Image iconoNoSelec8;
    public Image iconoNoSelec9;
    public Image plato1;
    public Image plato2;
    public Image platito1;
    public Image platito2;
    // Textos (solo fuente)
    public TMP_Text texto1;
    public TMP_Text texto2;
    public TMP_Text texto3;
    public TMP_Text texto4;
    public TMP_Text texto5;
    public TMP_Text texto6;
    // textos (fuente y color)
    public TMP_Text textPlato1;
    public TMP_Text textPlato2;
    public TMP_Text textSecc1;
    public TMP_Text textSecc2;
    public TMP_Text textSecc3;
    public TMP_Text textSecc4;
    public TMP_Text textSec1;
    public TMP_Text textSec2;
    public TMP_Text textPlatito1;
    public TMP_Text textPlatito2;
    public TMP_Text textPlatito3;
    public TMP_Text textPpal1;
    public TMP_Text textPpal2;
    public TMP_Text textPpal3;
    public TMP_Text textPpal4;
    public TMP_Text textPpal5;
    public TMP_Text textIconoSelect1;
    public TMP_Text textIconoSelect2;
    public TMP_Text textIconoSelect3;
    public TMP_Text textIconoNoSelect1;
    public TMP_Text textIconoNoSelect2;
    public TMP_Text textIconoNoSelect3;
    public TMP_Text textIconoNoSelect4;
    public TMP_Text textIconoNoSelect5;
    public TMP_Text textIconoNoSelect6;
    public TMP_Text textIconoNoSelect7;
    public TMP_Text textIconoNoSelect8;
    public TMP_Text textIconoNoSelect9;

    // para cambiar fuente
    public FontImageList fontImageList; // Referencia al ScriptableObject

    void Start()
    {
        // PARA LOS ICONOS
        // Inicializamos el diccionario con combinaciones
        spriteCombinations = new Dictionary<int, Sprite[]>();

        // Añadimos las combinaciones de sprites con un número
        spriteCombinations.Add(0, new Sprite[] { a1, a2, a3, a4 });
        spriteCombinations.Add(1, new Sprite[] { b1, b2, b3, b4 });
        spriteCombinations.Add(2, new Sprite[] { c1, c2, c3, c4 });
        spriteCombinations.Add(3, new Sprite[] { d1, d2, d3, d4 });

        // Initialize the UI with the source values
        SyncUI();

        // Add listener for text changes
        nombreEstablecimientoSource.onValueChanged.AddListener(OnTextChanged);
    }

    void Update()
    {
        // Check and update colors each frame
        OnColorChanged();

        OnFontChanged();

        OnSizeChanged();

        OnIconChanged();
    }

    void SyncUI()
    {
        // Sync all elements at once
        UpdateText(nombreEstablecimientoSource.text);
        UpdateColorFondo(colorFondoSource.color);
        UpdateColorLetra(colorLetraSource.color);
        UpdateColorFondoBarraSecciones(colorFondoBarraSeccionesSource.color);
        UpdateColorEtiquetasBarraSecciones(colorEtiquetasBarraSeccionesSource.color); 
        UpdateColorSeccion(colorFondoSeccionSource.color);
        UpdateColorPlato(colorFondoPlatosSource.color);
        UpdateColorBarra(colorBarraSource.color);
        UpdateColorIcono1(colorIcono1Source.color);
        UpdateColorIcono2(colorIcono2Source.color);
        UpdateColor1(colorPpalSource.color);
        UpdateColor2(colorSecSource.color);

        // Empleados
        UpdateColor1(color1Source.color);
        UpdateColor2(color2Source.color);

        // Store initial values
        lastNombreEstablecimiento = nombreEstablecimientoSource.text;
        lastColorFondo = colorFondoSource.color;
        lastColorLetra = colorLetraSource.color;
        lastColorFondoBarraSecciones = colorFondoBarraSeccionesSource.color;
        lastColorEtiquetasBarraSecciones = colorEtiquetasBarraSeccionesSource.color;
        lastColorFondoSeccion = colorFondoSeccionSource.color;
        lastColorFondoPlato = colorFondoPlatosSource.color;
        lastColorBarra = colorBarraSource.color;
        lastColorIcono1 = colorIcono1Source.color;
        lastColorIcono2 = colorIcono2Source.color;
        lastTextFuente1 = fontImageList.fontNames[dropdownFuente1.value];
        lastTextFuente2 = fontImageList.fontNames[dropdownFuente2.value];
        lastTextFuente3 = fontImageList.fontNames[dropdownFuente3.value];
        lastTextFuente4 = fontImageList.fontNames[dropdownFuente4.value];
        lastTextSize1 = dropdownSize1.value;
        lastTextSize2 = dropdownSize2.value;
        lastTextSize3 = dropdownSize3.value;
        lastColorPpal = colorPpalSource.color;
        lastColorSec = colorSecSource.color;
        // Empleados
        lastColor1 = color1Source.color;
        lastColor2 = color2Source.color;
        // Iconos
        lastIcon = dropdownIconos.value;

        // Textos app (que se ponga el texto blanco o negro en funcion del fondo)
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color,LetraEtiqueta1); // color texto etiquetas
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color,LetraEtiqueta2);
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color,LetraEtiqueta3);
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color,LetraEtiqueta4);
        UpdateTextColor(colorFondoSeccionSource.color,LetraSec); // color texto titulo seccion
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato1); // color texto plato
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato2);
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato3);
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato4); // color texto plato
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato5);
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato6);
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato7); // color texto plato
        UpdateTextColor(colorFondoPlatosSource.color,LetraPlato8);

        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color, textSecc1);
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color, textSecc2);
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color, textSecc3);
        UpdateTextColor(colorEtiquetasBarraSeccionesSource.color, textSecc4);
        UpdateTextColor(colorFondoPlatosSource.color, textPlatito1);
        UpdateTextColor(colorFondoPlatosSource.color, textPlatito2);
        UpdateTextColor(colorFondoPlatosSource.color, textPlatito3);
        UpdateTextColor(colorFondoPlatosSource.color, textPlato1);
        UpdateTextColor(colorFondoPlatosSource.color, textPlato2);
        UpdateTextColor(colorSecSource.color, textSec1);
        UpdateTextColor(colorSecSource.color, textSec2);
        UpdateTextColor(colorPpalSource.color, textPpal1);
        UpdateTextColor(colorPpalSource.color, textPpal2);
        UpdateTextColor(colorPpalSource.color, textPpal3);
        UpdateTextColor(colorPpalSource.color, textPpal4);
        UpdateTextColor(colorPpalSource.color, textPpal5);
        // Fuentes
        UpdateFuente(lastTextFuente1,nombreEstablecimiento); // título restaurante
        UpdateFuente(lastTextFuente3,LetraEtiqueta1); // fuente
        UpdateFuente(lastTextFuente3,LetraEtiqueta2);
        UpdateFuente(lastTextFuente3,LetraEtiqueta3);
        UpdateFuente(lastTextFuente3,LetraEtiqueta4);
        UpdateFuente(lastTextFuente2,LetraSec); // fuente texto titulo seccion
        UpdateFuente(lastTextFuente3,LetraPlato1);
        UpdateFuente(lastTextFuente3,LetraPlato2);
        UpdateFuente(lastTextFuente3,LetraPlato3);
        UpdateFuente(lastTextFuente3,LetraPlato4);
        UpdateFuente(lastTextFuente3,LetraPlato5);
        UpdateFuente(lastTextFuente3,LetraPlato6);
        UpdateFuente(lastTextFuente3,LetraPlato7);
        UpdateFuente(lastTextFuente3,LetraPlato8);
        UpdateFuente(lastTextFuente3,Icono1);
        UpdateFuente(lastTextFuente3,Icono2);
        UpdateFuente(lastTextFuente3,Icono3);
        UpdateFuente(lastTextFuente3,Icono4);

        UpdateFuente(lastTextFuente3, textSecc1);
        UpdateFuente(lastTextFuente3, textSecc2);
        UpdateFuente(lastTextFuente3, textSecc3);
        UpdateFuente(lastTextFuente3, textSecc4);
        UpdateFuente(lastTextFuente3, textPlatito1);
        UpdateFuente(lastTextFuente3, textPlatito2);
        UpdateFuente(lastTextFuente3, textPlatito3);
        UpdateFuente(lastTextFuente3, textPlato1);
        UpdateFuente(lastTextFuente3, textPlato2);
        UpdateFuente(lastTextFuente3, textSec1);
        UpdateFuente(lastTextFuente3, textSec2);
        UpdateFuente(lastTextFuente3, textPpal1);
        UpdateFuente(lastTextFuente3, textPpal2);
        UpdateFuente(lastTextFuente3, textPpal3);
        UpdateFuente(lastTextFuente3, textPpal4);
        UpdateFuente(lastTextFuente3, textPpal5);
        UpdateFuente(lastTextFuente3, textIconoSelect1);
        UpdateFuente(lastTextFuente3, textIconoSelect2);
        UpdateFuente(lastTextFuente3, textIconoSelect3);
        UpdateFuente(lastTextFuente3, textIconoNoSelect1);
        UpdateFuente(lastTextFuente3, textIconoNoSelect2);
        UpdateFuente(lastTextFuente3, textIconoNoSelect3);
        UpdateFuente(lastTextFuente3, textIconoNoSelect4);
        UpdateFuente(lastTextFuente3, textIconoNoSelect5);
        UpdateFuente(lastTextFuente3, textIconoNoSelect6);
        UpdateFuente(lastTextFuente3, textIconoNoSelect7);
        UpdateFuente(lastTextFuente3, textIconoNoSelect8);
        UpdateFuente(lastTextFuente3, textIconoNoSelect9);
        UpdateFuente(lastTextFuente3, texto1);
        UpdateFuente(lastTextFuente3, texto2);
        UpdateFuente(lastTextFuente3, texto3);
        UpdateFuente(lastTextFuente3, texto4);
        UpdateFuente(lastTextFuente3, texto5);
        UpdateFuente(lastTextFuente3, texto6);

        // Tamaño letra
        UpdateSize1(lastTextSize1,nombreEstablecimiento); // título restaurante
        UpdateSize3(lastTextSize3,LetraEtiqueta1); // fuente
        UpdateSize3(lastTextSize3,LetraEtiqueta2);
        UpdateSize3(lastTextSize3,LetraEtiqueta3);
        UpdateSize3(lastTextSize3,LetraEtiqueta4);
        UpdateSize2(lastTextSize2,LetraSec); // fuente texto titulo seccion
        UpdateSize3(lastTextSize3,LetraPlato1);
        UpdateSize3(lastTextSize3,LetraPlato2);
        UpdateSize3(lastTextSize3,LetraPlato3);
        UpdateSize3(lastTextSize3,LetraPlato4);
        UpdateSize3(lastTextSize3,LetraPlato5);
        UpdateSize3(lastTextSize3,LetraPlato6);
        UpdateSize3(lastTextSize3,LetraPlato7);
        UpdateSize3(lastTextSize3,LetraPlato8);

        // Textos cocina
        UpdateTextColor(color1Source.color,LetraMesa1); // color texto plato
        UpdateTextColor(color1Source.color,LetraMesa2);
        UpdateTextColor(color1Source.color,LetraMesa3);
        UpdateTextColor(color1Source.color,LetraMesa4); // color texto plato
        UpdateTextColor(color1Source.color,LetraMesa5);
        UpdateTextColor(color1Source.color,LetraMesa6);
        UpdateTextColor(color2Source.color,LetraBarra1); // color texto plato
        UpdateTextColor(color2Source.color,LetraBarra2);
        UpdateTextColor(color2Source.color,LetraBarra3);

        // Fuentes cocina
        UpdateFuente(lastTextFuente4,LetraMesa1);
        UpdateFuente(lastTextFuente4,LetraMesa2);
        UpdateFuente(lastTextFuente4,LetraMesa3);
        UpdateFuente(lastTextFuente4,LetraMesa4);
        UpdateFuente(lastTextFuente4,LetraMesa5);
        UpdateFuente(lastTextFuente4,LetraMesa6);
        UpdateFuente(lastTextFuente4,LetraBarra1);
        UpdateFuente(lastTextFuente4,LetraBarra2);
        UpdateFuente(lastTextFuente4,LetraBarra3);
        UpdateFuente(lastTextFuente4,TextC1); // textos comandas
        UpdateFuente(lastTextFuente4, TextC2);
        UpdateFuente(lastTextFuente4, TextC3);
        UpdateFuente(lastTextFuente4, TextC4);
        UpdateFuente(lastTextFuente4, TextC5);
        UpdateFuente(lastTextFuente4, TextC6);
        UpdateFuente(lastTextFuente4, TextC7);
        UpdateFuente(lastTextFuente4, TextC8);
        UpdateFuente(lastTextFuente4, TextC9);
        UpdateFuente(lastTextFuente4, TextC10);
        UpdateFuente(lastTextFuente4, TextC11);
        UpdateFuente(lastTextFuente4, TextC12);
        UpdateFuente(lastTextFuente4, TextC13);
        UpdateFuente(lastTextFuente4, TextC14);
        UpdateFuente(lastTextFuente4, TextC15);
        UpdateFuente(lastTextFuente4, TextC16);
        UpdateFuente(lastTextFuente4, TextC17);
        UpdateFuente(lastTextFuente4, TextC18);
        UpdateFuente(lastTextFuente4, TextC19);
        UpdateFuente(lastTextFuente4, TextC20);
        UpdateFuente(lastTextFuente4, TextC21);
        UpdateFuente(lastTextFuente4, TextC22);
        UpdateFuente(lastTextFuente4, TextC23);
        UpdateFuente(lastTextFuente4, TextC24);
        UpdateFuente(lastTextFuente4, TextC25);
        UpdateFuente(lastTextFuente4, TextC26);
        UpdateFuente(lastTextFuente4, TextC27);
        UpdateFuente(lastTextFuente4, TextC28);
        UpdateFuente(lastTextFuente4, TextC29);
        UpdateFuente(lastTextFuente4, TextC30);
        UpdateFuente(lastTextFuente4, TextC31);
        UpdateFuente(lastTextFuente4, TextC32);

        // Fuentes camarero
        UpdateFuente(lastTextFuente4,Numero1);
        UpdateFuente(lastTextFuente4,Numero2);
        UpdateFuente(lastTextFuente4,Numero3);
        UpdateFuente(lastTextFuente4,Mesa);
        UpdateFuente(lastTextFuente4,IconoC1);
        UpdateFuente(lastTextFuente4,IconoC2);
        UpdateFuente(lastTextFuente4,IconoC3);
        UpdateFuente(lastTextFuente4,IconoC4);

        // Imagenes iconos
        UpdateIcon(lastIcon);

    }

    void OnTextChanged(string newText)
    {
        if (newText != lastNombreEstablecimiento)
        {
            lastNombreEstablecimiento = newText;
            UpdateText(newText);
        }
    }

    void OnColorChanged()
    {
        // Check and update each color if needed
        if (colorFondoSource.color != lastColorFondo)
        {
            lastColorFondo = colorFondoSource.color;
            UpdateColorFondo(lastColorFondo);
        }

        if (colorLetraSource.color != lastColorLetra)
        {
            lastColorLetra = colorLetraSource.color;
            UpdateColorLetra(lastColorLetra);
        }

        if (colorFondoBarraSeccionesSource.color != lastColorFondoBarraSecciones)
        {
            lastColorFondoBarraSecciones = colorFondoBarraSeccionesSource.color;
            UpdateColorFondoBarraSecciones(lastColorFondoBarraSecciones);
        }

        if (colorEtiquetasBarraSeccionesSource.color != lastColorEtiquetasBarraSecciones)
        {
            lastColorEtiquetasBarraSecciones = colorEtiquetasBarraSeccionesSource.color;
            UpdateColorEtiquetasBarraSecciones(lastColorEtiquetasBarraSecciones);

            UpdateTextColor(lastColorEtiquetasBarraSecciones,LetraEtiqueta1);
            UpdateTextColor(lastColorEtiquetasBarraSecciones,LetraEtiqueta2);
            UpdateTextColor(lastColorEtiquetasBarraSecciones,LetraEtiqueta3);
            UpdateTextColor(lastColorEtiquetasBarraSecciones,LetraEtiqueta4);

            UpdateTextColor(lastColorEtiquetasBarraSecciones, textSecc1);
            UpdateTextColor(lastColorEtiquetasBarraSecciones, textSecc2);
            UpdateTextColor(lastColorEtiquetasBarraSecciones, textSecc3);
            UpdateTextColor(lastColorEtiquetasBarraSecciones, textSecc4);
        }

        if (colorFondoSeccionSource.color != lastColorFondoSeccion)
        {
            lastColorFondoSeccion = colorFondoSeccionSource.color;
            UpdateColorSeccion(lastColorFondoSeccion);
            UpdateTextColor(lastColorFondoSeccion,LetraSec);
        }

        if (colorFondoPlatosSource.color != lastColorFondoPlato)
        {
            lastColorFondoPlato = colorFondoPlatosSource.color;
            UpdateColorPlato(lastColorFondoPlato);
            UpdateTextColor(lastColorFondoPlato,LetraPlato1); // color texto plato
            UpdateTextColor(lastColorFondoPlato,LetraPlato2);
            UpdateTextColor(lastColorFondoPlato,LetraPlato3);
            UpdateTextColor(lastColorFondoPlato,LetraPlato4); // color texto plato
            UpdateTextColor(lastColorFondoPlato,LetraPlato5);
            UpdateTextColor(lastColorFondoPlato,LetraPlato6);
            UpdateTextColor(lastColorFondoPlato,LetraPlato7); // color texto plato
            UpdateTextColor(lastColorFondoPlato,LetraPlato8);

            UpdateTextColor(lastColorFondoPlato, textPlatito1);
            UpdateTextColor(lastColorFondoPlato, textPlatito2);
            UpdateTextColor(lastColorFondoPlato, textPlatito3);
            UpdateTextColor(lastColorFondoPlato, textPlato1);
            UpdateTextColor(lastColorFondoPlato, textPlato2);
        }

        if (colorBarraSource.color != lastColorBarra)
        {
            lastColorBarra = colorBarraSource.color;
            UpdateColorBarra(lastColorBarra);
        }

        if (color1Source.color != lastColor1)
        {
            lastColor1 = color1Source.color;
            UpdateColor1(lastColor1);
            UpdateTextColor(lastColor1,LetraMesa1); // color texto plato 
            UpdateTextColor(lastColor1,LetraMesa2); 
            UpdateTextColor(lastColor1,LetraMesa3); 
            UpdateTextColor(lastColor1,LetraMesa4); 
            UpdateTextColor(lastColor1,LetraMesa5); 
            UpdateTextColor(lastColor1,LetraMesa6);
        }

        if (colorPpalSource.color != lastColorPpal)
        {
            lastColorPpal = colorPpalSource.color;
            UpdateColorPpal(lastColorPpal);
            UpdateTextColor(lastColorPpal, textPpal1);
            UpdateTextColor(lastColorPpal, textPpal2);
            UpdateTextColor(lastColorPpal, textPpal3);
            UpdateTextColor(lastColorPpal, textPpal4);
            UpdateTextColor(lastColorPpal, textPpal5);
        }

        if (color2Source.color != lastColor2)
        {
            lastColor2 = color2Source.color;
            UpdateColor2(lastColor2);
            UpdateTextColor(lastColor2,LetraBarra1); 
            UpdateTextColor(lastColor2,LetraBarra2); 
            UpdateTextColor(lastColor2,LetraBarra3);
        }

        if (colorSecSource.color != lastColorSec)
        {
            lastColorSec = colorSecSource.color;
            UpdateColorSec(lastColorSec);
            UpdateTextColor(lastColorSec, textSec1);
            UpdateTextColor(lastColorSec, textSec2);
        }

        if (colorIcono1Source.color != lastColorIcono1)
        {
            lastColorIcono1 = colorIcono1Source.color;
            UpdateColorIcono1(lastColorIcono1);
        }

        if (colorIcono2Source.color != lastColorIcono2)
        {
            lastColorIcono2 = colorIcono2Source.color;
            UpdateColorIcono2(lastColorIcono2);
        }
    }

    void OnFontChanged()
    {

        if (fontImageList.fontNames[dropdownFuente1.value] != lastTextFuente1)
        {
            lastTextFuente1 = fontImageList.fontNames[dropdownFuente1.value];
            UpdateFuente(lastTextFuente1,nombreEstablecimiento);
        }

        if (fontImageList.fontNames[dropdownFuente2.value] != lastTextFuente2)
        {
            lastTextFuente2 = fontImageList.fontNames[dropdownFuente2.value];
            UpdateFuente(lastTextFuente2,LetraSec);
        }

        if (fontImageList.fontNames[dropdownFuente3.value] != lastTextFuente3)
        {
            lastTextFuente3 = fontImageList.fontNames[dropdownFuente3.value];
            UpdateFuente(lastTextFuente3,LetraEtiqueta1);
            UpdateFuente(lastTextFuente3,LetraEtiqueta2);
            UpdateFuente(lastTextFuente3,LetraEtiqueta3);
            UpdateFuente(lastTextFuente3,LetraEtiqueta4);
            UpdateFuente(lastTextFuente3,LetraPlato1);
            UpdateFuente(lastTextFuente3,LetraPlato2);
            UpdateFuente(lastTextFuente3,LetraPlato3);
            UpdateFuente(lastTextFuente3,LetraPlato4);
            UpdateFuente(lastTextFuente3,LetraPlato5);
            UpdateFuente(lastTextFuente3,LetraPlato6);
            UpdateFuente(lastTextFuente3,LetraPlato7);
            UpdateFuente(lastTextFuente3,LetraPlato8);
            UpdateFuente(lastTextFuente3,Icono1);
            UpdateFuente(lastTextFuente3,Icono2);
            UpdateFuente(lastTextFuente3,Icono3);
            UpdateFuente(lastTextFuente3,Icono4);

            UpdateFuente(lastTextFuente3, textSecc1);
            UpdateFuente(lastTextFuente3, textSecc2);
            UpdateFuente(lastTextFuente3, textSecc3);
            UpdateFuente(lastTextFuente3, textSecc4);
            UpdateFuente(lastTextFuente3, textPlatito1);
            UpdateFuente(lastTextFuente3, textPlatito2);
            UpdateFuente(lastTextFuente3, textPlatito3);
            UpdateFuente(lastTextFuente3, textPlato1);
            UpdateFuente(lastTextFuente3, textPlato2);
            UpdateFuente(lastTextFuente3, textSec1);
            UpdateFuente(lastTextFuente3, textSec2);
            UpdateFuente(lastTextFuente3, textPpal1);
            UpdateFuente(lastTextFuente3, textPpal2);
            UpdateFuente(lastTextFuente3, textPpal3);
            UpdateFuente(lastTextFuente3, textPpal4);
            UpdateFuente(lastTextFuente3, textPpal5);
            UpdateFuente(lastTextFuente3, textIconoSelect1);
            UpdateFuente(lastTextFuente3, textIconoSelect2);
            UpdateFuente(lastTextFuente3, textIconoSelect3);
            UpdateFuente(lastTextFuente3, textIconoNoSelect1);
            UpdateFuente(lastTextFuente3, textIconoNoSelect2);
            UpdateFuente(lastTextFuente3, textIconoNoSelect3);
            UpdateFuente(lastTextFuente3, textIconoNoSelect4);
            UpdateFuente(lastTextFuente3, textIconoNoSelect5);
            UpdateFuente(lastTextFuente3, textIconoNoSelect6);
            UpdateFuente(lastTextFuente3, textIconoNoSelect7);
            UpdateFuente(lastTextFuente3, textIconoNoSelect8);
            UpdateFuente(lastTextFuente3, textIconoNoSelect9);
            UpdateFuente(lastTextFuente3, texto1);
            UpdateFuente(lastTextFuente3, texto2);
            UpdateFuente(lastTextFuente3, texto3);
            UpdateFuente(lastTextFuente3, texto4);
            UpdateFuente(lastTextFuente3, texto5);
            UpdateFuente(lastTextFuente3, texto6);
        }

        if (fontImageList.fontNames[dropdownFuente4.value] != lastTextFuente4)
        {
            // Cocina
            lastTextFuente4 = fontImageList.fontNames[dropdownFuente4.value];
            UpdateFuente(lastTextFuente4,LetraMesa1);
            UpdateFuente(lastTextFuente4,LetraMesa2);
            UpdateFuente(lastTextFuente4,LetraMesa3);
            UpdateFuente(lastTextFuente4,LetraMesa4);
            UpdateFuente(lastTextFuente4,LetraMesa5);
            UpdateFuente(lastTextFuente4,LetraMesa6);
            UpdateFuente(lastTextFuente4,LetraBarra1); 
            UpdateFuente(lastTextFuente4,LetraBarra2); 
            UpdateFuente(lastTextFuente4,LetraBarra3);     
            UpdateFuente(lastTextFuente4,TextC1); // textos comandas
            UpdateFuente(lastTextFuente4, TextC2);
            UpdateFuente(lastTextFuente4, TextC3);
            UpdateFuente(lastTextFuente4, TextC4);
            UpdateFuente(lastTextFuente4, TextC5);
            UpdateFuente(lastTextFuente4, TextC6);
            UpdateFuente(lastTextFuente4, TextC7);
            UpdateFuente(lastTextFuente4, TextC8);
            UpdateFuente(lastTextFuente4, TextC9);
            UpdateFuente(lastTextFuente4, TextC10);
            UpdateFuente(lastTextFuente4, TextC11);
            UpdateFuente(lastTextFuente4, TextC12);
            UpdateFuente(lastTextFuente4, TextC13);
            UpdateFuente(lastTextFuente4, TextC14);
            UpdateFuente(lastTextFuente4, TextC15);
            UpdateFuente(lastTextFuente4, TextC16);
            UpdateFuente(lastTextFuente4, TextC17);
            UpdateFuente(lastTextFuente4, TextC18);
            UpdateFuente(lastTextFuente4, TextC19);
            UpdateFuente(lastTextFuente4, TextC20);
            UpdateFuente(lastTextFuente4, TextC21);
            UpdateFuente(lastTextFuente4, TextC22);
            UpdateFuente(lastTextFuente4, TextC23);
            UpdateFuente(lastTextFuente4, TextC24);
            UpdateFuente(lastTextFuente4, TextC25);
            UpdateFuente(lastTextFuente4, TextC26);
            UpdateFuente(lastTextFuente4, TextC27);
            UpdateFuente(lastTextFuente4, TextC28);
            UpdateFuente(lastTextFuente4, TextC29);
            UpdateFuente(lastTextFuente4, TextC30);
            UpdateFuente(lastTextFuente4, TextC31);
            UpdateFuente(lastTextFuente4, TextC32);
            // Camarero
            UpdateFuente(lastTextFuente4,Numero1);  
            UpdateFuente(lastTextFuente4,Numero2);  
            UpdateFuente(lastTextFuente4,Numero3);
            UpdateFuente(lastTextFuente4,Mesa);  
            UpdateFuente(lastTextFuente4,IconoC1);  
            UpdateFuente(lastTextFuente4,IconoC2);  
            UpdateFuente(lastTextFuente4,IconoC3);  
            UpdateFuente(lastTextFuente4,IconoC4);    
        }

    }

    void OnSizeChanged()
    {

        if (dropdownSize1.value != lastTextSize1)
        {
            lastTextSize1 = dropdownSize1.value-10;
            UpdateSize1(lastTextSize1,nombreEstablecimiento);
        }

        if (dropdownSize2.value != lastTextSize2)
        {
            lastTextSize2 = dropdownSize2.value;
            UpdateSize2(lastTextSize2,LetraSec);
        }
        if (dropdownSize3.value != lastTextSize3)
        {
            lastTextSize3 = dropdownSize3.value;
            UpdateSize3(lastTextSize3,LetraEtiqueta1);
            UpdateSize3(lastTextSize3,LetraEtiqueta2);
            UpdateSize3(lastTextSize3,LetraEtiqueta3);
            UpdateSize3(lastTextSize3,LetraEtiqueta4);
            UpdateSize3(lastTextSize3,LetraPlato1);
            UpdateSize3(lastTextSize3,LetraPlato2);
            UpdateSize3(lastTextSize3,LetraPlato3);
            UpdateSize3(lastTextSize3,LetraPlato4);
            UpdateSize3(lastTextSize3,LetraPlato5);
            UpdateSize3(lastTextSize3,LetraPlato6);
            UpdateSize3(lastTextSize3,LetraPlato7);
            UpdateSize3(lastTextSize3,LetraPlato8);
        }

    }

    void OnIconChanged()
    {
        if (dropdownIconos.value != lastIcon)
        {
            lastIcon = dropdownIconos.value;
            UpdateIcon(lastIcon);
        }
    }

    void UpdateText(string newText)
    {
        nombreEstablecimiento.text = newText;
    }

    void UpdateColorFondo(Color newColor)
    {
        colorFondo.color = newColor;
    }

    void UpdateColorLetra(Color newColor)
    {
        nombreEstablecimiento.color = newColor;
    }

    void UpdateColorFondoBarraSecciones(Color newColor)
    {
        colorFondoBarraSecciones.color = newColor;
        fondoSecciones.color = newColor;
    }

    void UpdateColorEtiquetasBarraSecciones(Color newColor)
    {
        colorEtiqueta1.color = newColor;
        colorEtiqueta2.color = newColor;
        colorEtiqueta3.color = newColor;
        colorEtiqueta4.color = newColor;
        fondoSec1.color = newColor;
        fondoSec2.color = newColor;
        fondoSec3.color = newColor;
        fondoSec4.color = newColor;
    }

    void UpdateColorSeccion(Color newColor)
    {
        colorFondoSeccion.color = newColor;
    }

    void UpdateColorPlato(Color newColor)
    {
        colorPlato1.color = newColor;
        colorPlato2.color = newColor;
        colorPlato3.color = newColor;
        platito1.color = newColor;
        platito2.color = newColor;
        // plato1.color = newColor;
        plato2.color = newColor;
    }

    void UpdateColorBarra(Color newColor)
    {
        colorBarra.color = newColor;
        colorBarra2.color = newColor;
        barraInferior1.color = newColor;
        barraInferior2.color = newColor;
        barraInferior3.color = newColor;
    }

    void UpdateColor1(Color newColor)
    {
        colorComanda1.color = newColor;
        colorComanda2.color = newColor;
        colorComanda3.color = newColor;
        colorComanda4.color = newColor;
        colorComanda5.color = newColor;
        colorComanda6.color = newColor;
    }

    void UpdateColorPpal(Color newColor)
    {
        // botones principales
        botonPpal1.color = newColor;
        botonPpal2.color = newColor;
        botonPpal3.color = newColor;
        botonPpal4.color = newColor;
        botonPpal5.color = newColor;
        iconoPagarPpal1.color = newColor;
        iconoPagarPpal2.color = newColor;

    }

    void UpdateColor2(Color newColor)
    {
        colorBarraCocina.color = newColor;
    }

    void UpdateColorSec(Color newColor)
    {
        // puntito
        Puntito.color = newColor;
        // botón secundario
        botonSec.color = newColor;
    }

    void UpdateColorIcono1(Color newColor)
    {
        // imagenes iconos seleccionados
        imageIcono1.color = newColor;
        imageIconoC2.color = newColor;
        // textos iconos selecionados
        Icono1.color = newColor;
        IconoC2.color = newColor;
        // imagenes iconos seleccionados (detalle y pagar)
        iconoSelec1.color = newColor;
        iconoSelec2.color = newColor;
        iconoSelec3.color = newColor;
        // textos iconos seleccionados (detalle y pagar)
        textIconoSelect1.color = newColor;
        textIconoSelect2.color = newColor;
        textIconoSelect3.color = newColor;
    }

    void UpdateColorIcono2(Color newColor)
    {   
        // imagenes iconos base
        imageIcono2.color = newColor;
        imageIcono3.color = newColor;
        imageIcono4.color = newColor;
        imageIconoC1.color = newColor;
        imageIconoC3.color = newColor;
        imageIconoC4.color = newColor;
        // textos iconos base
        Icono2.color = newColor;
        Icono3.color = newColor;
        Icono4.color = newColor;
        IconoC1.color = newColor;
        IconoC3.color = newColor;
        IconoC4.color = newColor;
        // imagenes iconos base (detalle y pagar)
        iconoNoSelec1.color = newColor;
        iconoNoSelec2.color = newColor;
        iconoNoSelec3.color = newColor;
        iconoNoSelec4.color = newColor;
        iconoNoSelec5.color = newColor;
        iconoNoSelec6.color = newColor;
        iconoNoSelec7.color = newColor;
        iconoNoSelec8.color = newColor;
        iconoNoSelec9.color = newColor;
        // texts iconos base (detalle y pagar)
        textIconoNoSelect1.color = newColor;
        textIconoNoSelect2.color = newColor;
        textIconoNoSelect3.color = newColor;
        textIconoNoSelect4.color = newColor;
        textIconoNoSelect5.color = newColor;
        textIconoNoSelect6.color = newColor;
        textIconoNoSelect7.color = newColor;
        textIconoNoSelect8.color = newColor;
        textIconoNoSelect9.color = newColor;
    }

    void UpdateTextColor(Color boton, TMP_Text text)
    {
        // Obtener el color de fondo
        Color backgroundColor = boton;

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

    void UpdateFuente(String newFont, TMP_Text text)
    {
        TMP_FontAsset newFont2 = Resources.Load<TMP_FontAsset>("Fonts/" + newFont);
        text.font = newFont2;
    }

    void UpdateSize1(int newSize, TMP_Text text)
    {
        if (newSize == 0) // Pequeño
        {
            text.fontSize = 15;
        }
        if (newSize == 1) // Mediano
        {
            text.fontSize = 20;
        }
        if (newSize == 2) // Grande
        {
            text.fontSize = 25;
        }
    }

    void UpdateSize2(int newSize, TMP_Text text)
    {
        if (newSize == 0) // Pequeño
        {
            text.fontSize = 10;
        }
        if (newSize == 1) // Mediano
        {
            text.fontSize = 13;
        }
        if (newSize == 2) // Grande
        {
            text.fontSize = 16;
        }
    }

    void UpdateSize3(int newSize, TMP_Text text)
    {
        if (newSize == 0) // Pequeño
        {
            text.fontSize = 9;
        }
        if (newSize == 1) // Mediano
        {
            text.fontSize = 10;
        }
        if (newSize == 2) // Grande
        {
            text.fontSize = 11;
        }
    }

    void UpdateIcon(int newIcon) 
    {
        if (spriteCombinations.ContainsKey(newIcon))
        {
            // Accedemos a la combinación de iconos según el número
            Sprite[] selectedIcons = spriteCombinations[newIcon];

            // Asignamos los sprites a los objetos de la UI
            imageIcono1.sprite = selectedIcons[0];
            imageIcono2.sprite = selectedIcons[1];
            imageIcono3.sprite = selectedIcons[2];
            imageIcono4.sprite = selectedIcons[3];
            
            imageIconoC1.sprite = selectedIcons[0];
            imageIconoC2.sprite = selectedIcons[1];
            imageIconoC3.sprite = selectedIcons[2];
            imageIconoC4.sprite = selectedIcons[3];

            iconoSelec1.sprite = selectedIcons[0];
            iconoNoSelec1.sprite = selectedIcons[1];
            iconoNoSelec2.sprite = selectedIcons[2];
            iconoNoSelec3.sprite = selectedIcons[3];

            iconoNoSelec4.sprite = selectedIcons[0];
            iconoNoSelec5.sprite = selectedIcons[1];
            iconoNoSelec6.sprite = selectedIcons[2];
            iconoSelec2.sprite = selectedIcons[3];

            iconoNoSelec7.sprite = selectedIcons[0];
            iconoNoSelec8.sprite = selectedIcons[1];
            iconoNoSelec9.sprite = selectedIcons[2];
            iconoSelec3.sprite = selectedIcons[3];
        }
        else
        {
            Debug.LogWarning("Número fuera de rango. No hay combinación asociada.");
        }
    }
}
