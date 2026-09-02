using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;

using System.Collections.Generic; // Esto es necesario para usar Dictionary
using Newtonsoft.Json;

public class DataBasePersonalizacionCocinaScene : MonoBehaviour
{
    // DEFINICIONES
    public TMP_Text codCocinaText;
    public TMP_Text idOk;

    // 1. Defeinimos los elementos para importar cada campo de la base de datos de pythonanywhere de personalizaci´´on
    public static string[] id;
    public static string[] letra_empl;
    public static string[] col_ppal_empl;
    public static string[] col_sec_empl;
    public static string[] codigo_cocina;
    public static string[] cocinas;
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;
    public TMP_Text text4;


    // 2. Definimos los objetos de unity que vamos a editar (lo que se arrastra desde fuera)
    public Image barra_abajo;

    // 3. Url de la base de datos
    public string url;
    public string urlBase;

    public static Dictionary<string, bool> Features = new Dictionary<string, bool>();
    public static bool HasFeature(string clave) => Features.TryGetValue(clave, out var v) && v;

    void Awake()
    {
        // PARA CASI TODO
        StartCoroutine(WaitForRestaurantCodCocina());
    }

    private IEnumerator WaitForRestaurantCodCocina()
    {
        while (string.IsNullOrEmpty(idOk.text))
        {
            yield return null; // Wait for the next frame
        }

        // Once we have a valid restaurant ID, start loading menu data
        StartCoroutine(LoadPersonalizacionData3());
    }

    public IEnumerator LoadPersonalizacionData3()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personalizacion: " + request.error);
            yield break;
        }

        string PersonalizacionString = request.downloadHandler.text;
        Debug.Log("Received JSON data: " + PersonalizacionString);

        List<PersonalizacionEntry3> PersonalizacionEntries = ParsePersonalizacion(PersonalizacionString);

        // Initialize arrays with the size of the PersonalizacionEntries list
        int count = PersonalizacionEntries.Count;
        id = new string[count];
        letra_empl = new string[count];
        col_ppal_empl = new string[count];
        col_sec_empl = new string[count];
        codigo_cocina = new string[count];
        cocinas = new string[count];


        for (int i = 0; i < count; i++)
        {
            PersonalizacionEntry3 entry = PersonalizacionEntries[i];
            id[i] = entry.id;
            letra_empl[i] = entry.letra_empl;
            col_ppal_empl[i] = entry.col_ppal_empl;
            col_sec_empl[i] = entry.col_sec_empl;
            codigo_cocina[i] = entry.codigo_cocina;
            cocinas[i] = entry.cocinas;
        }
        EditarUnity();

        if (count > 0)
        {
            StartCoroutine(LoadEntitlements(id[0]));
        }
    }

    public IEnumerator LoadEntitlements(string restaurantId)
    {
        string entitlementsUrl = urlBase + "/personalizacion/entitlements_tpv/" + restaurantId;
        UnityWebRequest request = UnityWebRequest.Get(entitlementsUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogWarning("Failed to fetch entitlements: " + request.error);
            yield break;
        }

        var wrapper = JsonConvert.DeserializeObject<EntitlementsResponse>(request.downloadHandler.text);
        Features = wrapper.features ?? new Dictionary<string, bool>();
    }

    public void EditarUnity()
    {
        // CAMBIOS DE LOS OBJETOS DE UNITY CON LOS DATOS DE LA DATABASE PERSONALIZACIÓN

        // 1. Colores
        ChangeImageColor();

        // 2. Tipo letra
        // Construimos la ruta con el nombre de la fuente para poder cargarla (tiene que estar en la dirección Resources/Fonts)
        string rutaFuenteGral = "Fonts/" + letra_empl[0].Replace(" ", "");
        TMP_FontAsset fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral);
        if (fuenteGral == null)
            fuenteGral = Resources.Load<TMP_FontAsset>(rutaFuenteGral + " SDF");
        text1.font = fuenteGral;
        text2.font = fuenteGral;
        text3.font = fuenteGral;
        text4.font = fuenteGral;

        // 3. Cambio el color de la letra de los botones a blanco o negro en función de si el fondo de la barra es oscuro o claro 
        UpdateTextColor(barra_abajo,text1);
        UpdateTextColor(barra_abajo,text2);
        UpdateTextColor(barra_abajo,text3);
        UpdateTextColor(barra_abajo,text4);
    }

    public void ChangeImageColor() // función para cambiar los colores por los de la DataBase Personalización
    {
        Color newColorBarra;

        // Convertimos el string hex a un Color
        if (ColorUtility.TryParseHtmlString(col_sec_empl[0], out newColorBarra)) // Cambiamos color al fondo de la barra de secciones
        {
            // Asignamos el nuevo color al componente Image
            barra_abajo.color = newColorBarra;
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

    public List<PersonalizacionEntry3> ParsePersonalizacion(string PersonalizacionString)
    {
        List<PersonalizacionEntry3> PersonalizacionEntries = new List<PersonalizacionEntry3>();

        // Wrap the JSON array in a root object for JsonUtility
        string wrappedJson = "{ \"items\": " + PersonalizacionString + " }";

        PersonalizacionDataList3 PersonalizacionItems = JsonUtility.FromJson<PersonalizacionDataList3>(wrappedJson);

        foreach (var item in PersonalizacionItems.items)
        {
            if (item.codigo_cocina == codCocinaText.text)
            {
                PersonalizacionEntries.Add(new PersonalizacionEntry3(
                    item.id, item.letra_empl, item.col_ppal_empl, item.col_sec_empl, item.codigo_cocina, item.cocinas
                ));
            }
        }

        return PersonalizacionEntries;
    }
}

[Serializable]
public class PersonalizacionData3
{
    public string id;
    public string letra_empl;
    public string col_ppal_empl;
    public string col_sec_empl;
    public string codigo_cocina;
    public string cocinas;
}

[Serializable]
public class PersonalizacionDataList3
{
    public PersonalizacionData3[] items;
}

public class PersonalizacionEntry3
{
    public string id { get; private set; }
    public string letra_empl { get; private set; }
    public string col_ppal_empl { get; private set; }
    public string col_sec_empl { get; private set; }
    public string codigo_cocina { get; private set; }
    public string cocinas { get; private set; }

    public PersonalizacionEntry3(
        string id, string letra_empl, string col_ppal_empl, string col_sec_empl, string codigo_cocina, string cocinas)
    {
        this.id = id;
        this.letra_empl = letra_empl;
        this.col_ppal_empl = col_ppal_empl;
        this.col_sec_empl = col_sec_empl;
        this.codigo_cocina = codigo_cocina;
        this.cocinas = cocinas;
    }
}
