using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class DataBasePersonalizacionRespScene : MonoBehaviour
{
    
    // 1. Defeinimos los elementos para importar cada campo de la base de datos de pythonanywhere de personalizaci´´on
    public static string[] id;
    public static string[] id_connect;
    public static string[] nombre_rest;
    public static int[] num_mesas;
    public static string[] img_url_cabecero;
    private int downloadedImageCount = 0; // Counter for downloaded images
    public Image imageRest;
    public static Sprite[] spriteRest;
    public GameObject imageRestLoading;
    public static string[] letra_titulo;
    public static int[] size_letra_titulo;
    public static string[] col_letra_titulo;
    public static string[] col_fondo_titulo;
    public static string[] letra_gral;
    public static int[] size_letra_gral;
    public static string[] letra_titulos;
    public static int[] size_letra_titulos;
    public static float[] redondez_gral;
    public static string[] col_fondo;
    public static string[] col_botones;
    public static string[] col_fondo_gral;
    public static string[] col_fondo_titulos;
    public static int[] icono;
    public static string[] col_fondo_icono;
    public static string[] col_icono_base;
    public static string[] col_icono_pulsado;
    public static string[] col_ppal_botones;
    public static string[] col_sec_botones;
    public static string[] letra_empl;
    public static string[] col_ppal_empl;
    public static string[] col_sec_empl;
    public static string[] codigo_cocina;
    public static string[] cocinas;

    // parte QRs
    public static string[] mensaje_qr;
    public static string[] letra_qr;
    public static int[] size_letra_qr;
    public static string[] col_letra_qr;
    public static string[] col_marco_qr;
    public static string[] col_qr;
    public static string[] col_fondo_qr;

    // 2. Url de la base de datos
    public string url;
    private String restaurantId;
    
    public event Action OnDataLoaded; // Event to notify when data is loaded


    public AspectFill aspectFillImageRestaurante;


    public void Awake()
    {
        // PARA CASI TODO
        StartCoroutine(WaitForRestaurantIDResponsable());
		imageRestLoading.SetActive(true);
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
        {
            yield return null; // Wait for the next frame
            restaurantId = LoginManagerResponsable.restaurantID;
        }

        // Once we have a valid restaurant ID, start loading menu data
        StartCoroutine(LoadPersonalizacionDataCoroutine());
    }

    // Coroutine wrapper for loading personalizacion data asynchronously
    private IEnumerator LoadPersonalizacionDataCoroutine()
    {
        // Call the async version and wait for it to complete
        var loadTask = LoadPersonalizacionDataAsync();
        while (!loadTask.IsCompleted) // While the task is not completed, keep yielding
        {
            yield return null;
        }

        // Handle exceptions if any
        if (loadTask.Exception != null)
        {
            Debug.LogError(loadTask.Exception);
        }
    }

    public async Task LoadPersonalizacionDataAsync()
    {
        string itemUrl = $"{url}/restaurant/{restaurantId}"; // Construct the URL

        UnityWebRequest request = UnityWebRequest.Get(itemUrl);

        // Await the completion of the request using a manual async pattern
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield(); // Keep yielding to the main thread
        }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personalizacion items: " + request.error);
            return;
        }

        string PersonalizacionString = request.downloadHandler.text;
        //Debug.Log("Received JSON data PersonalizacionDataAsync: " + PersonalizacionString);

        if (PersonalizacionString.Length == 3) // The menu is empty - new user
        {
            // newUser
            List<PersonalizacionEntry2> PersonalizacionEntries = ParsePersonalizacion(PersonalizacionString);

            // Initialize arrays with the size of the PersonalizacionEntries list
            int count = PersonalizacionEntries.Count;
            id = new string[count];
            id_connect = new string[count];
            nombre_rest = new string[count];
            num_mesas = new int[count];
            img_url_cabecero = new string[count];
            spriteRest = new Sprite[count]; // ke
            letra_titulo = new string[count];
            size_letra_titulo = new int[count];
            col_letra_titulo = new string[count];
            col_fondo_titulo = new string[count];
            letra_gral = new string[count];
            size_letra_gral = new int[count];
            letra_titulos = new string[count];
            size_letra_titulos = new int[count];
            redondez_gral = new float[count];
            col_fondo = new string[count];
            col_botones = new string[count];
            col_fondo_gral = new string[count];
            col_fondo_titulos = new string[count];
            icono = new int[count];
            col_fondo_icono = new string[count];
            col_icono_base = new string[count];
            col_icono_pulsado = new string[count];
            col_ppal_botones = new string[count];
            col_sec_botones = new string[count];
            letra_empl = new string[count];
            col_ppal_empl = new string[count];
            col_sec_empl = new string[count];
            codigo_cocina = new string[count];
            cocinas = new string[count];

            // qrs
            mensaje_qr = new string[count];
            letra_qr = new string[count];
            size_letra_qr = new int[count];
            col_letra_qr = new string[count];
            col_marco_qr = new string[count];
            col_qr = new string[count];
            col_fondo_qr = new string[count];

            for (int i = 0; i < count; i++)
            {
                PersonalizacionEntry2 entry = PersonalizacionEntries[i];
                id[i] = entry.id;
                id_connect[i] = entry.id_connect;
                nombre_rest[i] = entry.nombre_rest;
                num_mesas[i] = entry.num_mesas;
                letra_titulo[i] = entry.letra_titulo;
                size_letra_titulo[i] = entry.size_letra_titulo;
                col_letra_titulo[i] = entry.col_letra_titulo;
                col_fondo_titulo[i] = entry.col_fondo_titulo;
                letra_gral[i] = entry.letra_gral;
                size_letra_gral[i] = entry.size_letra_gral;
                letra_titulos[i] = entry.letra_titulos;
                size_letra_titulos[i] = entry.size_letra_titulos;
                redondez_gral[i] = entry.redondez_gral;
                col_fondo[i] = entry.col_fondo;
                col_botones[i] = entry.col_botones;
                col_fondo_gral[i] = entry.col_fondo_gral;
                col_fondo_titulos[i] = entry.col_fondo_titulos;
                icono[i] = entry.icono;
                col_fondo_icono[i] = entry.col_fondo_icono;
                col_icono_base[i] = entry.col_icono_base;
                col_icono_pulsado[i] = entry.col_icono_pulsado;
                col_ppal_botones[i] = entry.col_ppal_botones;
                col_sec_botones[i] = entry.col_sec_botones;
                letra_empl[i] = entry.letra_empl;
                col_ppal_empl[i] = entry.col_ppal_empl;
                col_sec_empl[i] = entry.col_sec_empl;
                codigo_cocina[i] = entry.codigo_cocina;
                cocinas[i] = entry.cocinas;
                // QRs
                mensaje_qr[i] = entry.mensaje_qr;
                letra_qr[i] = entry.letra_qr;
                size_letra_qr[i] = entry.size_letra_qr;
                col_letra_qr[i] = entry.col_letra_qr;
                col_marco_qr[i] = entry.col_marco_qr;
                col_qr[i] = entry.col_qr;
                col_fondo_qr[i] = entry.col_fondo_qr;

                // Start a coroutine to download the image por defecto
                StartCoroutine(DownloadImage("https://drive.google.com/uc?id=1Fh9ZSQKZZlhZOPTRXMhPD7wLfzpBusUC",i));
            }

            // OnDataLoaded?.Invoke(); // Notify that all data is loaded
        }
        else
        {
            List<PersonalizacionEntry2> PersonalizacionEntries = ParsePersonalizacion(PersonalizacionString);

            // Initialize arrays with the size of the PersonalizacionEntries list
            int count = PersonalizacionEntries.Count;
            id = new string[count];
            id_connect = new string[count];
            nombre_rest = new string[count];
            num_mesas = new int[count];
            img_url_cabecero = new string[count];
            spriteRest = new Sprite[count]; // ke
            letra_titulo = new string[count];
            size_letra_titulo = new int[count];
            col_letra_titulo = new string[count];
            col_fondo_titulo = new string[count];
            letra_gral = new string[count];
            size_letra_gral = new int[count];
            letra_titulos = new string[count];
            size_letra_titulos = new int[count];
            redondez_gral = new float[count];
            col_fondo = new string[count];
            col_botones = new string[count];
            col_fondo_gral = new string[count];
            col_fondo_titulos = new string[count];
            icono = new int[count];
            col_fondo_icono = new string[count];
            col_icono_base = new string[count];
            col_icono_pulsado = new string[count];
            col_ppal_botones = new string[count];
            col_sec_botones = new string[count];
            letra_empl = new string[count];
            col_ppal_empl = new string[count];
            col_sec_empl = new string[count];
            codigo_cocina = new string[count];
            cocinas = new string[count];

            // qrs
            mensaje_qr = new string[count];
            letra_qr = new string[count];
            size_letra_qr = new int[count];
            col_letra_qr = new string[count];
            col_marco_qr = new string[count];
            col_qr = new string[count];
            col_fondo_qr = new string[count];

            for (int i = 0; i < count; i++)
            {
                PersonalizacionEntry2 entry = PersonalizacionEntries[i];
                id[i] = entry.id;
                id_connect[i] = entry.id_connect;
                nombre_rest[i] = entry.nombre_rest;
                num_mesas[i] = entry.num_mesas;
                letra_titulo[i] = entry.letra_titulo;
                size_letra_titulo[i] = entry.size_letra_titulo;
                col_letra_titulo[i] = entry.col_letra_titulo;
                col_fondo_titulo[i] = entry.col_fondo_titulo;
                letra_gral[i] = entry.letra_gral;
                size_letra_gral[i] = entry.size_letra_gral;
                letra_titulos[i] = entry.letra_titulos;
                size_letra_titulos[i] = entry.size_letra_titulos;
                redondez_gral[i] = entry.redondez_gral;
                col_fondo[i] = entry.col_fondo;
                col_botones[i] = entry.col_botones;
                col_fondo_gral[i] = entry.col_fondo_gral;
                col_fondo_titulos[i] = entry.col_fondo_titulos;
                icono[i] = entry.icono;
                col_fondo_icono[i] = entry.col_fondo_icono;
                col_icono_base[i] = entry.col_icono_base;
                col_icono_pulsado[i] = entry.col_icono_pulsado;
                col_ppal_botones[i] = entry.col_ppal_botones;
                col_sec_botones[i] = entry.col_sec_botones;
                letra_empl[i] = entry.letra_empl;
                col_ppal_empl[i] = entry.col_ppal_empl;
                col_sec_empl[i] = entry.col_sec_empl;
                codigo_cocina[i] = entry.codigo_cocina;
                cocinas[i] = entry.cocinas;
                // QRs
                mensaje_qr[i] = entry.mensaje_qr;
                letra_qr[i] = entry.letra_qr;
                size_letra_qr[i] = entry.size_letra_qr;
                col_letra_qr[i] = entry.col_letra_qr;
                col_marco_qr[i] = entry.col_marco_qr;
                col_qr[i] = entry.col_qr;
                col_fondo_qr[i] = entry.col_fondo_qr;

                // Start a coroutine to download the image
                if (entry.img_url_cabecero != "")
                    StartCoroutine(DownloadImage(entry.img_url_cabecero,i));
            }
            //OnDataLoaded?.Invoke();
        }
    }
    
    IEnumerator DownloadImage(string url, int index)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to download image from URL: " + url);
            Debug.LogError("Error message: " + request.error);
            yield break;
        }

        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        if (texture == null)
        {
            Debug.LogError("Failed to create texture from downloaded image for URL: " + url);
            yield break;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        if (sprite == null)
        {
            Debug.LogError("Failed to create sprite from downloaded image texture for URL: " + url);
            yield break;
        }
        spriteRest[index] = sprite;

        downloadedImageCount++; // Increment the counter

        if (downloadedImageCount == 1)
        {
            // Instancia la imagen del cabecero
            CreateImage();
		    imageRestLoading.SetActive(false);
            img_url_cabecero[0] = url;
            OnDataLoaded?.Invoke(); // Notify that all data is loaded

            // Manually call AdjustToCover after assigning the sprite
            aspectFillImageRestaurante.AdjustToCover();
        } 
        
    }

    private void CreateImage()
    {
        Sprite[] sprites = DataBasePersonalizacionRespScene.spriteRest;
        imageRest.sprite=sprites[0];
    }

    public List<PersonalizacionEntry2> ParsePersonalizacion(string PersonalizacionString)
    {
        List<PersonalizacionEntry2> PersonalizacionEntries = new List<PersonalizacionEntry2>();

        // Wrap the JSON array in a root object for JsonUtility
        string wrappedJson = "{ \"items\": " + PersonalizacionString + " }";

        PersonalizacionDataList2 PersonalizacionItems = JsonUtility.FromJson<PersonalizacionDataList2>(wrappedJson);

        foreach (var item in PersonalizacionItems.items)
        {
            if (item.id == LoginManagerResponsable.restaurantID)
            {
                PersonalizacionEntries.Add(new PersonalizacionEntry2(
                    item.id, item.id_connect, item.nombre_rest, item.num_mesas,
                    item.img_url_cabecero, item.letra_titulo, item.size_letra_titulo,
                    item.col_letra_titulo, item.col_fondo_titulo, item.letra_gral, item.size_letra_gral,item.letra_titulos, item.size_letra_titulos,
                    item.redondez_gral, item.col_fondo, item.col_botones, 
                    item.col_fondo_gral, item.col_fondo_titulos,
                    item.icono, item.col_fondo_icono, item.col_icono_base, item.col_icono_pulsado, item.col_ppal_botones, item.col_sec_botones,
                    item.letra_empl, item.col_ppal_empl, item.col_sec_empl, item.codigo_cocina, item.cocinas,
                    item.mensaje_qr, item.letra_qr, item.size_letra_qr,item.col_letra_qr, item.col_marco_qr, item.col_qr, item.col_fondo_qr
                    
                ));
            }
        }

        //Debug.Log("Cantidad de personalizaciones encontradas: " + PersonalizacionEntries.Count);

         // Si no se encontró ninguna coincidencia (nuevo restaurante)
        if (PersonalizacionEntries.Count == 0)
        {
            // Devolvemos unos ajustes predeterminados
            PersonalizacionEntries.Add(new PersonalizacionEntry2(
                LoginManagerResponsable.restaurantID, "0", "Restaurante Genérico", 10, 
                "https://drive.google.com/uc?id=1Fh9ZSQKZZlhZOPTRXMhPD7wLfzpBusUC", "OpenSans SDF", 14, 
                "#000000", "#FFFFFF", "OpenSans SDF", 12, "OpenSans SDF", 16,
                5, "#DDDDDD", "#FF5733", 
                "#EEEEEE", "#CCCCCC",
                0, "#FFFFFF", "#000000", "#333333", "#FF5733", "#FFC300",
                "OpenSans SDF", "#FF5733", "#FFC300", "0000", "",
                "Bienvenidos", "OpenSans SDF", 12, "#000000", "#CCCCCC", "#000000", "#FFFFFF"
            ));
        }

        return PersonalizacionEntries;
    }
}

[Serializable]
public class PersonalizacionData2
{
    public string id;
    public string id_connect;
    public string nombre_rest;
    public int num_mesas;
    public string img_url_cabecero;
    public string letra_titulo;
    public int size_letra_titulo;
    public string col_letra_titulo;
    public string col_fondo_titulo;
    public string letra_gral;
    public int size_letra_gral;
    public string letra_titulos;
    public int size_letra_titulos;
    public float redondez_gral;
    public string col_fondo;
    public string col_botones;
    public string col_fondo_gral;
    public string col_fondo_titulos;
    public int icono;
    public string col_fondo_icono;
    public string col_icono_base;
    public string col_icono_pulsado;
    public string col_ppal_botones;
    public string col_sec_botones;
    public string letra_empl;
    public string col_ppal_empl;
    public string col_sec_empl;
    public string codigo_cocina;
    public string cocinas;

    // QRs
    public string mensaje_qr;
    public string letra_qr;
    public int size_letra_qr;
    public string col_letra_qr;
    public string col_marco_qr;
    public string col_qr;
    public string col_fondo_qr;
}

[Serializable]
public class PersonalizacionDataList2
{
    public PersonalizacionData2[] items;
}

public class PersonalizacionEntry2
{
    public string id { get; private set; }
    public string id_connect { get; private set; }
    public string nombre_rest { get; private set; }
    public int num_mesas { get; private set; }
    public string img_url_cabecero { get; private set; }
    public string letra_titulo { get; private set; }
    public int size_letra_titulo { get; private set; }
    public string col_letra_titulo { get; private set; }
    public string col_fondo_titulo { get; private set; }
    public string letra_gral { get; private set; }
    public int size_letra_gral { get; private set; }
    public string letra_titulos { get; private set; }
    public int size_letra_titulos { get; private set; }
    public float redondez_gral { get; private set; }
    public string col_fondo { get; private set; }
    public string col_botones { get; private set; }
    public string col_fondo_gral { get; private set; }
    public string col_fondo_titulos { get; private set; }
    public int icono { get; private set; }
    public string col_fondo_icono { get; private set; }
    public string col_icono_base { get; private set; }
    public string col_icono_pulsado { get; private set; }
    public string col_ppal_botones { get; private set; }
    public string col_sec_botones { get; private set; }
    public string letra_empl { get; private set; }
    public string col_ppal_empl { get; private set; }
    public string col_sec_empl { get; private set; }
    public string codigo_cocina { get; private set; }
    public string cocinas { get; private set; }

    // QRs
    public string mensaje_qr { get; private set; }
    public string letra_qr { get; private set; }
    public int size_letra_qr { get; private set; }
    public string col_letra_qr { get; private set; }
    public string col_marco_qr { get; private set; }
    public string col_qr { get; private set; }
    public string col_fondo_qr { get; private set; }

    public PersonalizacionEntry2(
        string id, string id_connect, string nombre_rest, int num_mesas,
        string img_url_cabecero, string letra_titulo, int size_letra_titulo,
        string col_letra_titulo, string col_fondo_titulo, string letra_gral, int size_letra_gral, string letra_titulos, int size_letra_titulos,
        float redondez_gral, string col_fondo, string col_botones, 
        string col_fondo_gral, string col_fondo_titulos, 
        int icono, string col_fondo_icono, string col_icono_base, string col_icono_pulsado, string col_ppal_botones, string col_sec_botones,
        string letra_empl, string col_ppal_empl, string col_sec_empl, string codigo_cocina, string cocinas,
        string mensaje_qr,string letra_qr, int size_letra_qr, string col_letra_qr, string col_marco_qr, string col_qr, string col_fondo_qr)
    {
        this.id = id;
        this.id_connect = id_connect;
        this.nombre_rest = nombre_rest;
        this.num_mesas = num_mesas;
        this.img_url_cabecero = img_url_cabecero;
        this.letra_titulo = letra_titulo;
        this.size_letra_titulo = size_letra_titulo;
        this.col_letra_titulo = col_letra_titulo;
        this.col_fondo_titulo = col_fondo_titulo;
        this.letra_gral = letra_gral;
        this.size_letra_gral = size_letra_gral;
        this.letra_titulos = letra_titulos;
        this.size_letra_titulos = size_letra_titulos;
        this.redondez_gral = redondez_gral;
        this.col_fondo = col_fondo;
        this.col_botones = col_botones;
        this.col_fondo_gral = col_fondo_gral;
        this.col_fondo_titulos = col_fondo_titulos;
        this.icono = icono;
        this.col_fondo_icono = col_fondo_icono;
        this.col_icono_base = col_icono_base;
        this.col_icono_pulsado = col_icono_pulsado;
        this.col_ppal_botones = col_ppal_botones;
        this.col_sec_botones = col_sec_botones;
        this.letra_empl = letra_empl;
        this.col_ppal_empl = col_ppal_empl;
        this.col_sec_empl = col_sec_empl;
        this.codigo_cocina = codigo_cocina;
        this.cocinas = cocinas;
        this.mensaje_qr = mensaje_qr;
        this.letra_qr = letra_qr;
        this.size_letra_qr = size_letra_qr;
        this.col_letra_qr = col_letra_qr;
        this.col_marco_qr = col_marco_qr;
        this.col_qr = col_qr;
        this.col_fondo_qr = col_fondo_qr;
    }
}

