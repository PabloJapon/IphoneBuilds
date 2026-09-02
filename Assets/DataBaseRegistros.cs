using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DataBaseRegistros : MonoBehaviour
{
    public static string[] categoria;
    public static string[] fecha;
    public static string[] hora;
    public static string[] id;
    public static int[] mesa;
    public static string[] n;
    public static string[] nPedido;
    public static string[] plato;
    public static float[] precio;
    public static string[] precioPlato;

    public string url;
    public static bool isDataLoaded = false; // Add a flag to indicate when data is loaded

    private String restaurantId;

    void Awake()
    {
        StartCoroutine(WaitForRestaurantIDResponsable());
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
        {
            yield return null; // Wait for the next frame
            restaurantId = LoginManagerResponsable.restaurantID;
        }

        // Once we have a valid restaurant ID, start loading registros data
        StartCoroutine(LoadRegistrosData());
    }

    public IEnumerator LoadRegistrosData()
    {
        string itemUrl = $"{url}/restaurant/{restaurantId}"; // Construct the URL

        UnityWebRequest request = UnityWebRequest.Get(itemUrl);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Registros: " + request.error);
            yield break;
        }

        string registrosString = request.downloadHandler.text;
        //Debug.Log("Received JSON data: " + registrosString);

        List<RegistrosEntry> registrosEntries = ParseRegistros(registrosString);

        if (registrosEntries != null && registrosEntries.Count > 0)
        {
            // Initialize arrays with the size of the RegistrosEntries list
            int count = registrosEntries.Count;
            categoria = new string[count];
            fecha = new string[count];
            hora = new string[count];
            id = new string[count];
            mesa = new int[count];
            n = new string[count];
            nPedido = new string[count];
            plato = new string[count];
            precio = new float[count];
            precioPlato = new string[count];

            for (int i = 0; i < count; i++)
            {
                RegistrosEntry entry = registrosEntries[i];
                categoria[i] = entry.categoria;
                fecha[i] = entry.fecha;
                hora[i] = entry.hora;
                id[i] = entry.id;
                mesa[i] = entry.mesa;
                n[i] = entry.n;
                nPedido[i] = entry.nPedido;
                plato[i] = entry.plato;
                precio[i] = entry.precio;
                precioPlato[i] = entry.precioPlato;
            }
            isDataLoaded = true; // Mark data as loaded
        }
        else
        {
            Debug.Log("No hay registros disponibles.");
        }
    }

    public List<RegistrosEntry> ParseRegistros(string registrosString)
    {
        List<RegistrosEntry> registrosEntries = new List<RegistrosEntry>();

        try
        {
            // Wrap the JSON array in a root object for JsonUtility
            string wrappedJson = "{ \"items\": " + registrosString + " }";

            RegistrosDataList registrosItems = JsonUtility.FromJson<RegistrosDataList>(wrappedJson);

            foreach (var item in registrosItems.items)
            {
                // Only add records with id equal to restaurantId
                if (item.id == restaurantId)
                {
                    registrosEntries.Add(new RegistrosEntry(item.categoria, item.fecha, item.hora, item.id, item.mesa, item.n, item.nPedido, item.plato, item.precio, item.precioPlato));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing JSON data: " + ex.Message);
        }

        return registrosEntries;
    }
}

[Serializable]
public class RegistrosData
{
    public string categoria;
    public string fecha;
    public string hora;
    public string id;
    public int mesa;
    public string n;
    public string nPedido;
    public string plato;
    public float precio;
    public string precioPlato;
}

[Serializable]
public class RegistrosDataList
{
    public RegistrosData[] items;
}

public class RegistrosEntry
{
    public string categoria { get; private set; }
    public string fecha { get; private set; }
    public string hora { get; private set; }
    public string id { get; private set; }
    public int mesa { get; private set; }
    public string n { get; private set; }
    public string nPedido { get; private set; }
    public string plato { get; private set; }
    public float precio { get; private set; }
    public string precioPlato { get; private set; }

    public RegistrosEntry(string categoria, string fecha, string hora, string id, int mesa, string n, string nPedido, string plato, float precio, string precioPlato)
    {
        this.categoria = categoria;
        this.fecha = fecha;
        this.hora = hora;
        this.id = id;
        this.mesa = mesa;
        this.n = n;
        this.nPedido = nPedido;
        this.plato = plato;
        this.precio = precio;
        this.precioPlato = precioPlato;
    }
}

