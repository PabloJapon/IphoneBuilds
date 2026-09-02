using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Linq;

public class DataBase : MonoBehaviour
{
    public static string[] id;
    public static int[] numeroMenu;
    public static string[] nombrePlatos;
    public static string[] descripcionPlatos;
    public static float[] precioPlatos;
    public static Sprite[] spritePlatos;
    public static string[] seccion;
    public static int[] toggle;
    public static string[] imageUrls;
    public static int[] alergs1;
    public static int[] alergs2;
    public static int[] alergs3;
    public static int[] alergs4;
    public static int[] alergs5;
    public static int[] alergs6;
    public static int[] alergs7;
    public static int[] alergs8;
    public static int[] alergs9;
    public static int[] alergs10;
    public static int[] alergs11;
    public static int[] alergs12;
    public static int[] alergs13;
    public static int[] alergs14;
    public static int[] vegs;
    public static string[] optionGroups;
    public static int[] disponible;
    public static int[] itemIds;
    public static string seccionesOrden = ""; // ";"-separated section names in saved order, for the active menu

    public static Dictionary<int, string> menuNamesById = new Dictionary<int, string>();
    public static Dictionary<int, string> seccionesOrdenById = new Dictionary<int, string>();

    public GameObject cuadroErrorConexion;
    
    public bool IsLoaded { get; private set; } = false; // para ver desde otros scripts si se ha cargado o no la DB (ej ButtonsColorsCode para el camarero)


    public string url; // Base URL for the API

    private int downloadedImageCount = 0; // Counter for downloaded images
    private int totalImages = 0; // Total number of images to download

    public event Action OnDataLoaded; // Event to notify when data is loaded

    public TMP_Text textId; // Text UI to display the restaurant ID or item ID
    public String restaurantId;

    public GameObject canvasIntro;
    public GameObject loadingSliderGO;
    public Slider loadingSlider;

    public static event Action<int, bool> OnDisponibleChanged;

    public static void NotificarCambioDisponibilidad(int platoIndex, bool nuevoValor)
    {
        disponible[platoIndex] = nuevoValor ? 1 : 0;
        OnDisponibleChanged?.Invoke(platoIndex, nuevoValor);
    }

    // Para el codigo ButtonsColorsCodeCamarero
    // public static bool DBIsLoaded = false;

    void Awake()
    {
        OnDataLoaded += HandleDataLoaded;
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "ResponsableScene")
        {
            StartCoroutine(WaitForRestaurantIDResponsable());
        }
        else
        {
            StartCoroutine(WaitForRestaurantIDMovil());
        }
    }

    private void HandleDataLoaded()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "MobileScene" || currentScene.name == "TPVScene")
        {
            canvasIntro.SetActive(false);
        }
    }

    void OnDestroy()
    {
        OnDataLoaded -= HandleDataLoaded;
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
        {
            yield return null; // Wait for the next frame
        }

        // Once we have a valid restaurant ID, start loading menu data
        StartCoroutine(LoadMenuDataCoroutine()); // Start coroutine to load data
    }

    private IEnumerator WaitForRestaurantIDMovil()
    {
        while (string.IsNullOrEmpty(textId.text))
        {
            yield return null; // Wait for the next frame
        }

        // Once we have a valid restaurant ID, start loading menu data
        StartCoroutine(LoadMenuDataCoroutine()); // Start coroutine to load data
        canvasIntro.SetActive(true);
        loadingSliderGO.SetActive(true);
    }

    // Coroutine wrapper for loading menu data asynchronously
    private IEnumerator LoadMenuDataCoroutine()
    {
        // Call the async version and wait for it to complete
        var loadTask = LoadMenuDataAsync();
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

    public async Task LoadMenuDataAsync()
    {
        // Construct the URL to fetch menu items by restaurant ID
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "ResponsableScene")
        {
            restaurantId = LoginManagerResponsable.restaurantID;
        }
        else
        {
            restaurantId = textId.text;
        }

        string itemUrl = $"{url}/menu/restaurant/{restaurantId}"; // Construct the URL

        UnityWebRequest request = UnityWebRequest.Get(itemUrl);

        // Await the completion of the request using a manual async pattern
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield(); // Keep yielding to the main thread
        }

        if (request.isNetworkError || request.isHttpError)
        {
            cuadroErrorConexion.SetActive(true);
            Debug.LogError("Failed to fetch menu items: " + request.error);
            return;
        }

        string menuString = request.downloadHandler.text;

        if (menuString.Length == 3) // The menu in empty - new user
        {
            // newUser
            IsLoaded = true;
            OnDataLoaded?.Invoke();
        }
        else
        {
            List<MenuEntry> menuEntries = ParseMenu(menuString);

            // Initialize arrays with the size of the menuEntries list
            int count = menuEntries.Count;
            id = new string[count];
            numeroMenu = new int[count];
            nombrePlatos = new string[count];
            descripcionPlatos = new string[count];
            precioPlatos = new float[count];
            spritePlatos = new Sprite[count];
            seccion = new string[count];
            toggle = new int[count];
            alergs1 = new int[count];
            alergs2 = new int[count];
            alergs3 = new int[count];
            alergs4 = new int[count];
            alergs5 = new int[count];
            alergs6 = new int[count];
            alergs7 = new int[count];
            alergs8 = new int[count];
            alergs9 = new int[count];
            alergs10 = new int[count];
            alergs11 = new int[count];
            alergs12 = new int[count];
            alergs13 = new int[count];
            alergs14 = new int[count];
            vegs = new int[count];
            optionGroups = new string[count];
            imageUrls = new string[count];
            disponible = new int[count];
            itemIds = new int[count];
            totalImages = count; // Set the total number of images to download

            for (int i = 0; i < count; i++)
            {
                MenuEntry entry = menuEntries[i];
                id[i] = entry.Id;
                numeroMenu[i] = entry.MenuNumber;
                nombrePlatos[i] = entry.Name;
                descripcionPlatos[i] = entry.Description;
                precioPlatos[i] = entry.Price;
                seccion[i] = entry.Section;
                if (entry.Toggle == null)
                {
                    toggle[i] = 0;
                }
                else
                {
                    toggle[i] = entry.Toggle;
                }
                optionGroups[i] = entry.OptionGroups;
                alergs1[i] = entry.Alerg1;
                alergs2[i] = entry.Alerg2;
                alergs3[i] = entry.Alerg3;
                alergs4[i] = entry.Alerg4;
                alergs5[i] = entry.Alerg5;
                alergs6[i] = entry.Alerg6;
                alergs7[i] = entry.Alerg7;
                alergs8[i] = entry.Alerg8;
                alergs9[i] = entry.Alerg9;
                alergs10[i] = entry.Alerg10;
                alergs11[i] = entry.Alerg11;
                alergs12[i] = entry.Alerg12;
                alergs13[i] = entry.Alerg13;
                alergs14[i] = entry.Alerg14;
                vegs[i] = entry.Veg;
                disponible[i] = entry.Disponible;
                itemIds[i] = entry.ItemId;
                imageUrls[i] = entry.ImageUrl; // Collect image URLs
            }

            // Fetch the saved section order for the active menu
            await LoadSeccionesOrdenAsync();

            // Download all images concurrently using async/await
            await StartImageDownloadsAsync(imageUrls);
        }
       // DBIsLoaded = true;
    }

    private async Task LoadSeccionesOrdenAsync()
    {
        string menusUrl = $"{url}/menus/{restaurantId}";

        UnityWebRequest request = UnityWebRequest.Get(menusUrl);
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogWarning("Failed to fetch menus for secciones_orden: " + request.error);
            seccionesOrden = "";
            return;
        }

        string menusString = request.downloadHandler.text;

        try
        {
            List<MenuListEntry> menus = JsonConvert.DeserializeObject<List<MenuListEntry>>(menusString);
            if (menus != null)
            {
                menuNamesById.Clear();
                seccionesOrdenById.Clear();
                foreach (var m in menus)
                {
                    menuNamesById[m.id] = m.menu_name;
                    seccionesOrdenById[m.id] = m.secciones_orden ?? "";
                }
                seccionesOrden = seccionesOrdenById.Count > 0 ? seccionesOrdenById[menuNamesById.Keys.Min()] : "";
            }
            else
            {
                seccionesOrden = "";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing menus JSON for secciones_orden: " + e.Message);
            seccionesOrden = "";
        }
    }
    
    public void ReintentarConexion()
    {
        cuadroErrorConexion.SetActive(false);

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "ResponsableScene")
        {
            StartCoroutine(WaitForRestaurantIDResponsable());
        }
        else
        {
            StartCoroutine(WaitForRestaurantIDMovil());
        }
    }

    async Task DownloadImageAsync(string url, int index)
    {
        if (!string.IsNullOrEmpty(url))
        {
            int maxRetries = 3;
            int retryCount = 0;
            UnityWebRequest request = null;

            while (retryCount < maxRetries)
            {
                request = UnityWebRequestTexture.GetTexture(url);
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogWarning($"Attempt {retryCount + 1}/{maxRetries} failed: {request.error}");
                    retryCount++;
                    await Task.Delay(1000);
                    continue;
                }
                break;
            }

            if (retryCount == maxRetries)
            {
                Debug.LogError($"Failed after {maxRetries} attempts: {url}");
                // ⚠️ No hacemos return — dejamos que caiga al contador
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        Vector2.zero
                    );
                    spritePlatos[index] = sprite; // null si falla, que es válido
                }
            }
        }

        // ✅ Siempre llega aquí, sin importar éxito o fallo
        spritePlatos[index] = spritePlatos[index]; // ya asignado o queda null

        downloadedImageCount++;
        float loadValue = (float)downloadedImageCount / totalImages;
        if (loadingSlider != null)
            loadingSlider.value = loadValue;

        if (downloadedImageCount == totalImages)
        {
            IsLoaded = true;
            OnDataLoaded?.Invoke(); // ✅ Este es el único punto de control
        }
    }



    // This method starts multiple image downloads concurrently
    public async Task StartImageDownloadsAsync(string[] urls)
    {
        List<Task> downloadTasks = new List<Task>();

        for (int i = 0; i < urls.Length; i++)
        {
            downloadTasks.Add(DownloadImageAsync(urls[i], i)); // Add tasks for concurrent downloads
        }

        // Wait for all downloads to complete
        await Task.WhenAll(downloadTasks);

         //Scene currentScene = SceneManager.GetActiveScene();

        //if (currentScene.name == "MobileScene" || currentScene.name == "TPVScene")
        //{
        //    canvasIntro.SetActive(false);
        //}
    }

    // Parse the menu items from the JSON string
    public List<MenuEntry> ParseMenu(string menuString)
    {
        List<MenuEntry> menuEntries = new List<MenuEntry>();

        // Check if the menuString is null or empty
        if (string.IsNullOrEmpty(menuString))
        {
            Debug.LogError("Menu string is null or empty.");
            return menuEntries;
        }

        try
        {
            // Unity's JsonUtility does not support JSON arrays directly
            // So we need a wrapper class or switch to Newtonsoft.Json
            menuEntries = JsonConvert.DeserializeObject<List<MenuEntry>>(menuString);  // Use Newtonsoft.Json instead of JsonUtility
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing menu JSON: " + e.Message);
        }

        return menuEntries;
    }
}

[Serializable]
public class MenuEntryList
{
    public MenuEntry[] items; // Note: The property name must match the JSON structure
}

[Serializable]
public class MenuData
{
    public string id;
    public int menuNumber;
    public string name;
    public string description;
    public float price;
    public string imageUrl;
    public string seccion;
    public string toggle;
    public int alerg1;
    public int alerg2;
    public int alerg3;
    public int alerg4;
    public int alerg5;
    public int alerg6;
    public int alerg7;
    public int alerg8;
    public int alerg9;
    public int alerg10;
    public int alerg11;
    public int alerg12;
    public int alerg13;
    public int alerg14;
    public int veg;
    public string OptionGroups;
    public int disponible;
}

public class MenuEntry
{
    public string Id { get; private set; }
    public int MenuNumber { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public float Price { get; private set; }
    public string ImageUrl { get; private set; }
    public string Section { get; private set; }
    public int Toggle { get; private set; }
    public int Alerg1 { get; private set; }
    public int Alerg2 { get; private set; }
    public int Alerg3 { get; private set; }
    public int Alerg4 { get; private set; }
    public int Alerg5 { get; private set; }
    public int Alerg6 { get; private set; }
    public int Alerg7 { get; private set; }
    public int Alerg8 { get; private set; }
    public int Alerg9 { get; private set; }
    public int Alerg10 { get; private set; }
    public int Alerg11 { get; private set; }
    public int Alerg12 { get; private set; }
    public int Alerg13 { get; private set; }
    public int Alerg14 { get; private set; }
    public int Veg { get; private set; }
    public string OptionGroups { get; private set; }
    public int Disponible { get; private set; }
    public int ItemId { get; private set; }

    public MenuEntry(string id, int menuNumber, string name, string description, float price, string imageUrl, string seccion, int toggle, int alerg1, int alerg2, int alerg3, int alerg4, int alerg5, int alerg6, int alerg7, int alerg8, int alerg9, int alerg10, int alerg11, int alerg12, int alerg13, int alerg14, int veg, string optionGroups, int disponible, int itemId)
    {
        ItemId = itemId;
        Id = id;
        MenuNumber = menuNumber;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        Section = seccion;
        if (toggle == null)
        {
            Toggle = 0;
        }
        else 
        {
            Toggle = toggle;
        }
        Alerg1 = alerg1;
        Alerg2 = alerg2;
        Alerg3 = alerg3;
        Alerg4 = alerg4;
        Alerg5 = alerg5;
        Alerg6 = alerg6;
        Alerg7 = alerg7;
        Alerg8 = alerg8;
        Alerg9 = alerg9;
        Alerg10 = alerg10;
        Alerg11 = alerg11;
        Alerg12 = alerg12;
        Alerg13 = alerg13;
        Alerg14 = alerg14;
        Veg = veg;
        OptionGroups = optionGroups;
        Disponible = disponible;
    }
}

[Serializable]
public class MenuListEntry
{
    public int id;
    public string menu_name;
    public string secciones_orden;
}
