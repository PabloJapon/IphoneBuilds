using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class DataBaseQrsRespScene : MonoBehaviour
{
    
    // 1. Defeinimos los elementos para importar cada campo de la base de datos de pythonanywhere de personalizaci´´on
    public static string[] id;
    public static string[] mensaje_qr;
    public static int[] if_mensaje_qr;
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

    void Awake()
    {
        // PARA CASI TODO
        StartCoroutine(WaitForRestaurantIDResponsable());
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
        {
            yield return null; // Wait for the next frame
            restaurantId = LoginManagerResponsable.restaurantID;
        }

        // Once we have a valid restaurant ID, start loading Qrs data
        StartCoroutine(LoadQrsDataCoroutine());
    }

    // Coroutine wrapper for loading personalizacion data asynchronously
    private IEnumerator LoadQrsDataCoroutine()
    {
        // Call the async version and wait for it to complete
        var loadTask = LoadQrsDataAsync();
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

    public async Task LoadQrsDataAsync()
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
            Debug.LogError("Failed to fetch Qrs items: " + request.error);
            return;
        }

        string QrsString = request.downloadHandler.text;

        // if (QrsString.Length == 3) // The menu in empty - new user
        // {
        //     // newUser
        //     OnDataLoaded?.Invoke();
        // }
        // else
        //{
            List<QrsEntry2> QrsEntries = ParseQrs(QrsString);

            // Initialize arrays with the size of the QrsEntries list
            int count = QrsEntries.Count;
            id = new string[count];
            mensaje_qr = new string[count];
            if_mensaje_qr = new int[count];
            letra_qr = new string[count];
            size_letra_qr = new int[count];
            col_letra_qr = new string[count];
            col_marco_qr = new string[count];
            col_qr = new string[count];
            col_fondo_qr = new string[count];

            for (int i = 0; i < count; i++)
            {
                QrsEntry2 entry = QrsEntries[i];
                id[i] = entry.id;
                mensaje_qr[i] = entry.mensaje_qr;
                if_mensaje_qr[i] = entry.if_mensaje_qr;
                letra_qr[i] = entry.letra_qr;
                size_letra_qr[i] = entry.size_letra_qr;
                col_letra_qr[i] = entry.col_letra_qr;
                col_marco_qr[i] = entry.col_marco_qr;
                col_qr[i] = entry.col_qr;
                col_fondo_qr[i] = entry.col_fondo_qr;
            }
            
            OnDataLoaded?.Invoke();
        //}
    }

    public List<QrsEntry2> ParseQrs(string QrsString)
    {
        List<QrsEntry2> QrsEntries = new List<QrsEntry2>();

        // Wrap the JSON array in a root object for JsonUtility
        string wrappedJson = "{ \"items\": " + QrsString + " }";

        QrsDataList2 QrsItems = JsonUtility.FromJson<QrsDataList2>(wrappedJson);

        foreach (var item in QrsItems.items)
        {
            if (item.id == LoginManagerResponsable.restaurantID)
            {
                QrsEntries.Add(new QrsEntry2(
                    item.id, 
                    item.mensaje_qr, item.if_mensaje_qr, item.letra_qr, item.size_letra_qr,item.col_letra_qr, item.col_marco_qr, item.col_qr, item.col_fondo_qr
                ));
            }
        }

        if (QrsEntries.Count == 0)
        {
            // Devolvemos unos ajustes predeterminados
            QrsEntries.Add(new QrsEntry2(
            LoginManagerResponsable.restaurantID, "Mensaje defecto", 1, "OpenSans SDF", 14, 
            "#000000", "#FFFFFF", "#DDDDDD", "#FF5733"
            ));
        }

        return QrsEntries;
    }
}

[Serializable]
public class QrsData2
{
    public string id;
    public string mensaje_qr;
    public int if_mensaje_qr;
    public string letra_qr;
    public int size_letra_qr;
    public string col_letra_qr;
    public string col_marco_qr;
    public string col_qr;
    public string col_fondo_qr;
}

[Serializable]
public class QrsDataList2
{
    public QrsData2[] items;
}

public class QrsEntry2
{
    public string id { get; private set; }
    public string mensaje_qr { get; private set; }
    public int if_mensaje_qr { get; private set; }
    public string letra_qr { get; private set; }
    public int size_letra_qr { get; private set; }
    public string col_letra_qr { get; private set; }
    public string col_marco_qr { get; private set; }
    public string col_qr { get; private set; }
    public string col_fondo_qr { get; private set; }

    public QrsEntry2(
        string id, 
        string mensaje_qr, int if_mensaje_qr,string letra_qr, int size_letra_qr, string col_letra_qr, string col_marco_qr, string col_qr, string col_fondo_qr)
    {
        this.id = id;
        this.mensaje_qr = mensaje_qr;
        this.if_mensaje_qr = if_mensaje_qr;
        this.letra_qr = letra_qr;
        this.size_letra_qr = size_letra_qr;
        this.col_letra_qr = col_letra_qr;
        this.col_marco_qr = col_marco_qr;
        this.col_qr = col_qr;
        this.col_fondo_qr = col_fondo_qr;
    }
}

