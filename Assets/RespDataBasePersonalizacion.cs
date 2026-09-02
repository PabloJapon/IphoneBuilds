// Código para que aparezcan rellenados los campos de personalización en la interfaz del responsable con lo que haya en la base de datos

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class RespDataBasePersonalizacion : MonoBehaviour
{
    // 1. Definimos los objetos de unity que vamos a editar (lo que se arrastra desde fuera)
    public TMP_InputField  nombre_rest2; 
    public TMP_InputField  num_mesas2;
    public TMP_Text url_imagenEncabezado;
    public Image col_fondo_titulo2;
    public Image col_letra_titulo2;
    public Image col_fondo2; 
    public Image col_botones2;
    public TMP_Dropdown size_letra_titulo2;
    public TMP_Dropdown letra_titulo2;
    public TMP_Dropdown letra_gral2;
    public TMP_Dropdown letra_titulos2;
    public TMP_Dropdown size_letra_gral2;
    public TMP_Dropdown size_letra_titulos2;
    public Image col_fondo_gral2; 
    public Image col_fondo_titulos2;
    public Image col_fondo_icono2; 
    public Image col_ppal_botones2;
    public Image col_sec_botones2; 
    public TMP_Dropdown icono2;
    public Image col_icono_base2;
    public Image col_icono_pulsado2; 
    public TMP_Dropdown redondez_gral2;

    // creamos variables internas para todos los colores para poder actualizarlos correctamente cuando pasemos de un canva a otro
    Color lastColorBarsecc;
    Color lastColorFondoTitulo;
    Color lasttextColorTitulo;
    Color lastColorBotonesBarsecc;
    Color lasttextColorFondoGral;
    Color lasttextColorFondoTitulos;
    Color lastColorFondoIconos;
    Color lastColorIconoBase;
    Color lastColorIconoPulsado;
    Color lastColorBotonPpal;
    Color lastColorBotonSec;
    Color lastColorPpalEmpl;
    Color lastColorSecEmpl;

    // Empleados
    public TMP_Dropdown letra_empl2;
    public Image col_ppal_empl2;
    public Image col_sec_empl2;

    // Codigo cocina
    public TMP_Text codigo_cocina;

    // Listas de opciones 
    public FontImageList fontImageList; // Referencia al ScriptableObject tipografías
    public IconosList iconosList; // Referencia al ScriptableObject iconos

    // string letra_titulo_i="aa"; // esto parece que hay que definirlo aquí fuera

    public DataBasePersonalizacionRespScene DB;

    private bool isCanvasActive = false;  // Variable para rastrear si el Canvas está activo
    public event Action OnDataLoaded; // Event to notify when data is loaded

    void Start()
    {
        DB.OnDataLoaded += OnDatabaseLoaded;
    }
    
    void OnDestroy()
    {
        DB.OnDataLoaded -= OnDatabaseLoaded;
    }

    private void OnDatabaseLoaded()
    {
        Color newColorBarsecc;
        Color newColorFondoTitulo;
        Color textColorTitulo;
        Color newColorBotonesBarsecc;
        Color textColorFondoGral;
        Color textColorFondoTitulos;
        Color newColorFondoIconos;
        Color newColorIconoPulsado;
        Color newColorIconoBase;
        Color newColorBotonPpal;
        Color newColorBotonSec;
        Color newColorPpalEmpl;
        Color newColorSecEmpl;

        // antes de nada asignamos a las variables internas de los colores los valores de la base de datos
        // Convertimos el string hex a un Color
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_fondo[0], out newColorBarsecc)) // Cambiamos color al fondo de la barra de secciones
        {
            // Asignamos el nuevo color al componente Image
            lastColorBarsecc = newColorBarsecc;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_fondo_titulo[0], out newColorFondoTitulo)) // color fondo título
        {
            // Asignamos el nuevo color al componente Image
            lastColorFondoTitulo = newColorFondoTitulo;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_letra_titulo[0], out textColorTitulo)) // color letra del título
        {
            // Asignamos el nuevo color al primer componente TMP_Text
            lasttextColorTitulo = textColorTitulo;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_botones[0], out newColorBotonesBarsecc))
        {
            lastColorBotonesBarsecc = newColorBotonesBarsecc;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_fondo_gral[0], out textColorFondoGral))
        {
            // Asignamos el nuevo color al primer componente TMP_Text
            lasttextColorFondoGral = textColorFondoGral;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_fondo_titulos[0], out textColorFondoTitulos))
        {
            // Asignamos el nuevo color al primer componente TMP_Text
            lasttextColorFondoTitulos = textColorFondoTitulos;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_fondo_icono[0], out newColorFondoIconos))
        {
            lastColorFondoIconos = newColorFondoIconos;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_icono_pulsado[0], out newColorIconoPulsado))
        {
            lastColorIconoPulsado = newColorIconoPulsado;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_icono_base[0], out newColorIconoBase))
        {
            lastColorIconoBase = newColorIconoBase;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_ppal_botones[0], out newColorBotonPpal)) // Cambiamos color a los botones ppales
        {
            // Asignamos el nuevo color al componente Image
            lastColorBotonPpal = newColorBotonPpal;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_sec_botones[0], out newColorBotonSec)) // Cambiamos color a los botones secundarios
        {
            // Asignamos el nuevo color al componente Image
            lastColorBotonSec = newColorBotonSec;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_ppal_empl[0], out newColorPpalEmpl)) // Cambiamos color a los botones secundarios
        {
            // Asignamos el nuevo color al componente Image
            lastColorPpalEmpl = newColorPpalEmpl;
        }
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacionRespScene.col_sec_empl[0], out newColorSecEmpl)) // Cambiamos color a los botones secundarios
        {
            // Asignamos el nuevo color al componente Image
            lastColorSecEmpl = newColorSecEmpl;
        }
        StartCoroutine(CheckAndUpdate());
    }

    // Se ejecuta cada vez que el GameObject (o Canvas) padre es activado
    void OnEnable()
    {
        if (isCanvasActive) // Solo actualizar si el Canvas está activo
        {
            RellenarCampos();
        }
    }

    // Chequear si los elementos están activos antes de rellenar
    private IEnumerator CheckAndUpdate()
    {
        // Asegurarte de que el Canvas esté activo antes de proceder
        while (!nombre_rest2.gameObject.activeInHierarchy)
        {
            yield return null; // Esperar un frame antes de verificar nuevamente
        }

        // Una vez activo, actualizar los campos
        isCanvasActive = true;
        RellenarCampos();
    }

    public void RellenarCampos() // Equivalente a CreatePrefabs de EditarMenu
    {
        if (DataBasePersonalizacionRespScene.nombre_rest == null) // New user
        {
            Debug.Log("New User - RespDataBasePersonalizacion");
        }
        else
        {
            // 1. Textos
            nombre_rest2.text = DataBasePersonalizacionRespScene.nombre_rest[0];
            num_mesas2.text = DataBasePersonalizacionRespScene.num_mesas[0].ToString();
            url_imagenEncabezado.text = DataBasePersonalizacionRespScene.img_url_cabecero[0];

            // 2. Colores
            ChangeImageColor();

            // 3. Dropdowns
            // 3.1. Tamaños letras
            int size1=-1; // Tamaño letra título
            if (DataBasePersonalizacionRespScene.size_letra_titulo[0] == 100)
            {
                size1 = 0;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_titulo[0] == 130)
            {
                size1 = 1;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_titulo[0] == 160)
            {
                size1 = 2;
            }
            size_letra_titulo2.value = size1;

            int size2=-1; // Tamaño letra general
            if (DataBasePersonalizacionRespScene.size_letra_gral[0] == 100)
            {
                size2 = 0;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_gral[0] == 130)
            {
                size2 = 1;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_gral[0] == 160)
            {
                size2 = 2;
            }
            size_letra_gral2.value = size2;

            int size3=-1; // Tamaño letra títulos secciones
            if (DataBasePersonalizacionRespScene.size_letra_titulos[0] == 100)
            {
                size3 = 0;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_titulos[0] == 130)
            {
                size3 = 1;
            }
            else if (DataBasePersonalizacionRespScene.size_letra_titulos[0] == 160)
            {
                size3 = 2;
            }
            size_letra_titulos2.value = size3;

            // 3.2 Tipo letras -> primero creamos el dropdown y luego le asignamos la opción que viene de la DB
            
            //// Tipo letra título
            letra_titulo2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en fontImageList
            for (int i = 0; i < fontImageList.fontImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = fontImageList.fontImages[i]; // Asigna la imagen como opción desde fontImageList
                options.Add(option); // Agrega la opción a la lista
            }
            
            letra_titulo2.AddOptions(options); // Asigna las opciones al TMP_Dropdown            
            letra_titulo2.captionImage.sprite = fontImageList.fontImages[letra_titulo2.value]; 
            letra_titulo2.captionImage.preserveAspect = true;
            // Cambiar fondo de los items a blanco
            SetDropdownItemBackground(letra_titulo2);
            
            // Asignamos el valor que viene de la base de datos
            letra_titulo2.value=fontImageList.fontNames.IndexOf(DataBasePersonalizacionRespScene.letra_titulo[0]);

            //// Tipo letra secciones general
            letra_gral2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> options2 = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en fontImageList
            for (int i = 0; i < fontImageList.fontImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = fontImageList.fontImages[i]; // Asigna la imagen como opción desde fontImageList
                options2.Add(option);
            }
            
            letra_gral2.AddOptions(options2); // Asigna las opciones al TMP_Dropdown            
            letra_gral2.captionImage.sprite = fontImageList.fontImages[letra_gral2.value];// Actualiza la imagen inicial seleccionada
            letra_gral2.captionImage.preserveAspect = true;

            // Asignamos el valor que viene de la base de datos
            letra_gral2.value=fontImageList.fontNames.IndexOf(DataBasePersonalizacionRespScene.letra_gral[0].Replace(" ", ""));

            //// Tipo letra secciones títulos
            letra_titulos2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> options3 = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en fontImageList
            for (int i = 0; i < fontImageList.fontImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = fontImageList.fontImages[i]; // Asigna la imagen como opción desde fontImageList
                options3.Add(option);
            }
            
            letra_titulos2.AddOptions(options3); // Asigna las opciones al TMP_Dropdown            
            letra_titulos2.captionImage.sprite = fontImageList.fontImages[letra_titulos2.value];// Actualiza la imagen inicial seleccionada
            letra_titulos2.captionImage.preserveAspect = true;

            // Asignamos el valor que viene de la base de datos
            letra_titulos2.value=fontImageList.fontNames.IndexOf(DataBasePersonalizacionRespScene.letra_titulos[0]);

            //// Tipo letra empleados
            letra_empl2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> options4 = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en fontImageList
            for (int i = 0; i < fontImageList.fontImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = fontImageList.fontImages[i]; // Asigna la imagen como opción desde fontImageList
                options4.Add(option);
            }
            
            letra_empl2.AddOptions(options4); // Asigna las opciones al TMP_Dropdown            
            letra_empl2.captionImage.sprite = fontImageList.fontImages[letra_empl2.value];// Actualiza la imagen inicial seleccionada
            letra_empl2.captionImage.preserveAspect = true;

            // Asignamos el valor que viene de la base de datos
            letra_empl2.value=fontImageList.fontNames.IndexOf(DataBasePersonalizacionRespScene.letra_empl[0]);

            // 3.3. Tipo iconos
            // creamos dropdown
            icono2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> optionsI = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en iconosList
            for (int i = 0; i < iconosList.iconoImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = iconosList.iconoImages[i]; // Asigna la imagen como opción desde iconosList
                optionsI.Add(option); // Agrega la opción a la lista
            }
            
            icono2.AddOptions(optionsI); // Asigna las opciones al TMP_Dropdown            
            icono2.captionImage.sprite = iconosList.iconoImages[icono2.value]; 
            icono2.captionImage.preserveAspect = true;


            // asignamos opcion
            icono2.value = DataBasePersonalizacionRespScene.icono[0];

            // 3.3 Redondez esquinas
            int redondez=-1; // Tipo letra secciones títulos
            if (DataBasePersonalizacionRespScene.redondez_gral[0] == 0)
            {
                redondez = 0;
            }
            else if (DataBasePersonalizacionRespScene.redondez_gral[0] == 65)
            {
                redondez = 1;
            }
            else if (DataBasePersonalizacionRespScene.redondez_gral[0] == 130)
            {
                redondez = 2;
            }
            //redondez_gral2.value = redondez;

            // Codigo cocina
            codigo_cocina.text = DataBasePersonalizacionRespScene.codigo_cocina[0];

            // Cocinas
            Debug.Log(DataBasePersonalizacionRespScene.cocinas[0]);
        }

        OnDataLoaded?.Invoke(); // Notify that all data is loaded
    }

    
    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        col_fondo2.color = lastColorBarsecc;
        col_fondo_titulo2.color = lastColorFondoTitulo;
        col_letra_titulo2.color = lasttextColorTitulo;
        col_botones2.color = lastColorBotonesBarsecc;
        col_fondo_gral2.color = lasttextColorFondoGral;
        col_fondo_titulos2.color = lasttextColorFondoTitulos;
        col_fondo_icono2.color = lastColorFondoIconos;
        col_icono_pulsado2.color = lastColorIconoPulsado;
        col_icono_base2.color = lastColorIconoBase;
        col_ppal_botones2.color = lastColorBotonPpal;
        col_sec_botones2.color = lastColorBotonSec;
        col_ppal_empl2.color = lastColorPpalEmpl;
        col_sec_empl2.color = lastColorSecEmpl;
        // esto tambien lo asignamos al botón que activa el canvasPersonalizar
    }

        public void OnButtonClickedUpdateColoresPers () // para que se guarden los colores nuevos en las variables internas al pulsar 'guardar'
    {
        if (col_fondo2.color != lastColorBarsecc)
        {
            lastColorBarsecc = col_fondo2.color;
        }
        if (col_fondo_titulo2.color != lastColorFondoTitulo)
        {
            lastColorFondoTitulo = col_fondo_titulo2.color;
        }
        if (col_letra_titulo2.color != lasttextColorTitulo)
        {
            lasttextColorTitulo = col_letra_titulo2.color;
        }
        if (col_botones2.color != lastColorBotonesBarsecc)
        {
            lastColorBotonesBarsecc = col_botones2.color;
        }
        if (col_fondo_gral2.color != lasttextColorFondoGral)
        {
            lasttextColorFondoGral = col_fondo_gral2.color;
        }
        if (col_fondo_titulos2.color != lasttextColorFondoTitulos)
        {
            lasttextColorFondoTitulos = col_fondo_titulos2.color;
        }
        if (col_fondo_icono2.color != lastColorFondoIconos)
        {
            lastColorFondoIconos = col_fondo_icono2.color;
        }
        if (col_icono_pulsado2.color != lastColorIconoPulsado)
        {
            lastColorIconoPulsado = col_icono_pulsado2.color;
        }
        if (col_icono_base2.color != lastColorIconoBase)
        {
            lastColorIconoBase = col_icono_base2.color;
        }
        if (col_ppal_botones2.color != lastColorBotonPpal)
        {
            lastColorBotonPpal = col_ppal_botones2.color;
        }
        if (col_sec_botones2.color != lastColorBotonSec)
        {
            lastColorBotonSec = col_sec_botones2.color;
        }
        if (col_ppal_empl2.color != lastColorPpalEmpl)
        {
            lastColorPpalEmpl = col_ppal_empl2.color;
        }
        if (col_sec_empl2.color != lastColorSecEmpl)
        {
            lastColorSecEmpl = col_sec_empl2.color;
        }
    } 


    public void SetDropdownItemBackground(TMP_Dropdown dropdown) // NO FUNCIONA
    {
        // Recorre todos los items del dropdown
        Transform dropdownListTransform = dropdown.transform.Find("Dropdown List");
        if (dropdownListTransform != null)
        {
            // Se obtiene el contenedor de las opciones (los "items")
            Transform viewportTransform = dropdownListTransform.Find("Viewport");
            if (viewportTransform != null)
            {
                Transform contentTransform = viewportTransform.Find("Content");
                if (contentTransform != null)
                {
                    // Itera sobre todos los elementos dentro del contenedor
                    for (int i = 0; i < contentTransform.childCount; i++)
                    {
                        Transform itemTransform = contentTransform.GetChild(i);
                        if (itemTransform != null)
                        {
                            // Obtener el fondo específico de cada item (Item Background)
                            Transform itemBackgroundTransform = itemTransform.Find("Item Background");

                            if (itemBackgroundTransform != null)
                            {
                                // Cambiar el color de fondo a blanco
                                Image itemBackgroundImage = itemBackgroundTransform.GetComponent<Image>();
                                if (itemBackgroundImage != null)
                                {
                                    itemBackgroundImage.color = Color.white;  // Cambia el fondo a blanco
                                }
                            }
                        }
                    }
                }
            }
        }
    }
                            
}
