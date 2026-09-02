// Código para que aparezcan rellenados los campos de personalización en la interfaz del responsable con lo que haya en la base de datos

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class RespDataBaseQrs : MonoBehaviour
{
    // 1. Definimos los objetos de unity que vamos a editar (lo que se arrastra desde fuera)
    public TMP_InputField  mensaje_qr2; 
    public TMP_Dropdown letra_qr2;
    public TMP_Dropdown size_letra_qr2;
    public Image col_letra_qr2; 
    public Image col_fondo_qr2;
    public Toggle if_mensaje;
    public TMP_Text nMesas;
    public TMP_Text nMesasResp;
    private string lastNumMesas;
    private Color lastColLetra;
    private Color lastColFondo;
    public GameObject canva;
    
    public DataBaseQrsRespScene DB;
    public DataBasePersonalizacionRespScene DB2;
    private bool isCanvasActive = false;  // Controla el estado de la activación del Canvas

    // Lista de opciones (tipografías)
    public FontImageList fontImageList; // Referencia al ScriptableObject
    public event Action OnDataLoaded; // Event to notify when data is loaded

    void Start()
    {
        DB.OnDataLoaded += OnDatabaseLoaded;
        DB2.OnDataLoaded += OnDatabaseLoaded;

    }
    
    void OnDestroy()
    {
        DB.OnDataLoaded -= OnDatabaseLoaded;
        DB2.OnDataLoaded -= OnDatabaseLoaded;
    }

    private void OnDatabaseLoaded()
    {
        // estas cosas no las podemos poner en RellenarCampos() para que luego se actualice todo bien
        lastNumMesas = DataBasePersonalizacionRespScene.num_mesas[0].ToString();

        // Apply the initial values to make sure everything is in sync
        UpdateNumMesas(lastNumMesas);


        // lo mismo con los colores, para que se actualicen bien aunque cambies de canva
        Color newColorQr1;
        Color newColorQr4;
        
        if (ColorUtility.TryParseHtmlString(DataBaseQrsRespScene.col_letra_qr[0], out newColorQr1)) 
        {
            // Asignamos inicialmente el valor de la base de datos
            lastColLetra = newColorQr1;
        }
        if (ColorUtility.TryParseHtmlString(DataBaseQrsRespScene.col_fondo_qr[0], out newColorQr4)) 
        {
            // Asignamos inicialmente el valor de la base de datos
            lastColFondo = newColorQr4;
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
        while (!mensaje_qr2.gameObject.activeInHierarchy)
        {
            yield return null; // Esperar un frame antes de verificar nuevamente
        }

        // Una vez activo, actualizar los campos
        isCanvasActive = true;
        RellenarCampos();
    }

    public void RellenarCampos() // Equivalente a CreatePrefabs de EditarMenu
    {
        if (DataBaseQrsRespScene.mensaje_qr == null) // New user
        {
            Debug.Log("New User - RespDataBaseQrs");
        }
        else
        {
            // 1. Textos
            mensaje_qr2.text = DataBaseQrsRespScene.mensaje_qr[0];
            // nmesas se actualiza antes

            // 2. Colores
            ChangeImageColor();

            // 3. Dropdowns
            // 3.1. Tamaños letras
            int size4=-1; // Tamaño letra qr
            if (DataBaseQrsRespScene.size_letra_qr[0] == 100)
            {
                size4 = 0;
            }
            else if (DataBaseQrsRespScene.size_letra_qr[0] == 130)
            {
                size4 = 1;
            }
            else if (DataBaseQrsRespScene.size_letra_qr[0] == 160)
            {
                size4 = 2;
            }
            size_letra_qr2.value = size4;

            // 3.2. Tipo letra (creamos dropdown y luego asignamos opcion de bd)
            letra_qr2.ClearOptions(); // Limpia las opciones existentes en el Dropdown
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(); // Crea una lista para almacenar las opciones personalizadas

            // Itera sobre las imágenes en fontImageList
            for (int i = 0; i < fontImageList.fontImages.Count; i++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.image = fontImageList.fontImages[i]; // Asigna la imagen como opción desde fontImageList
                options.Add(option); // Agrega la opción a la lista
            }
            
            letra_qr2.AddOptions(options); // Asigna las opciones al TMP_Dropdown            
            letra_qr2.captionImage.sprite = fontImageList.fontImages[letra_qr2.value]; 
            letra_qr2.captionImage.preserveAspect = true;
            
            // Asignamos el valor que viene de la base de datos
            letra_qr2.value=fontImageList.fontNames.IndexOf(DataBaseQrsRespScene.letra_qr[0]);

            // 4. Toggle mensaje
            if (DataBaseQrsRespScene.if_mensaje_qr[0]==1)
            {
                if_mensaje.isOn = true;
            }
            else
            {
                if_mensaje.isOn = false;

            }
        }
        OnDataLoaded?.Invoke(); // Notify that all data is loaded
    }

    
    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        col_letra_qr2.color = lastColLetra;

        col_fondo_qr2.color = lastColFondo;

        // esto ademas se lo asignamos al botón que activa el canva QRs para que se actualicen los colores nuevos en los gameobjects al volver a cava QRs
    }

    public void OnButtonClickedUpdateNmesa() // asignamos esto al 'guardar' de personalizar para que se acualice el numero de mesa
    {
        if (nMesasResp.text != lastNumMesas)
        {
            lastNumMesas = nMesasResp.text;
            UpdateNumMesas(lastNumMesas);
        }
    }

    public void OnButtonClickedUpdate () // para que se guarden los colores nuevos en las variables internas al pulsar 'guardar'
    {
        if (col_letra_qr2.color != lastColLetra)
        {
            lastColLetra = col_letra_qr2.color;
            //ChangeImageColor();
        }
        if (col_fondo_qr2.color != lastColFondo)
        {
            lastColFondo = col_fondo_qr2.color;
            //ChangeImageColor();
        }
    } 

     void UpdateNumMesas(string newText)
    {
        nMesas.text = newText;
    }
}
