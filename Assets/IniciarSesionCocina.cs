using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class IniciarSesionCocina : MonoBehaviour
{
    public string url;      // URL base del servidor (endpoint /personalizacion completo)
    public string urlBase;  // Host del servidor, ej https://gastrali.com (para entitlements)

    // campo de entrada para la informacion que hay que llevar a la base de datos
    public TMP_InputField codCocina;
    public OnScreenKeyboardController keyboard;
    public TMP_Text codCocinaText;
    public TMP_Text idOk;
    public static string[] codigo_cocina;
    public static string[] id;
    public static string[] cocinas;
    public GameObject canvasInicioSesion;
    public GameObject canvasError;
    public GameObject canvasTipoCocina;
    public GameObject contenedorCocinas;
    public GameObject prefabTipoCocina;
    public int nCocinas; // Número de instancias a crear
    private List<string> cocinasLista;
    public TMP_Text tipoCocina;
    public TMP_Text nCocina;
    public GameObject canvasKeyboard;
    public GameObject canvasLoading; // Panel "Cargando..." — asígnalo en el Inspector
    private bool isProcessing = false;

    public ConnectMirrorCocina CMC;

    void Start()
    {
        canvasError.SetActive(false);
        canvasInicioSesion.SetActive(true);
        canvasTipoCocina.SetActive(false);

        keyboard.RegisterInputField(codCocina); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isProcessing) // Enter o Return
        {
            OnButtonClick2();
        }
    }

    public void OnButtonClick2()
    {
        if (isProcessing) return; // bloquea clicks repetidos

        isProcessing = true;
        if (canvasLoading != null) canvasLoading.SetActive(true);

        codCocinaText.text = codCocina.text;
        StartCoroutine(LoadPersonalizacionData4());
    }

    public IEnumerator LoadPersonalizacionData4()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personalizacion: " + request.error);
            canvasError.SetActive(true);
            if (canvasLoading != null) canvasLoading.SetActive(false);
            isProcessing = false;
            yield break;
        }

        string PersonalizacionString = request.downloadHandler.text;
        Debug.Log("Received JSON data: " + PersonalizacionString);

        List<PersonalizacionEntry4> PersonalizacionEntries = ParsePersonalizacion(PersonalizacionString);

        int count = PersonalizacionEntries.Count;
        id = new string[count];
        codigo_cocina = new string[count];
        cocinas = new string[count];

        for (int i = 0; i < count; i++)
        {
            PersonalizacionEntry4 entry = PersonalizacionEntries[i];
            id[i] = entry.id;
            codigo_cocina[i] = entry.codigo_cocina;
            cocinas[i] = entry.cocinas;
        }
    }


    public List<PersonalizacionEntry4> ParsePersonalizacion(string PersonalizacionString)
    {
        List<PersonalizacionEntry4> PersonalizacionEntries = new List<PersonalizacionEntry4>();

        string wrappedJson = "{ \"items\": " + PersonalizacionString + " }";
        PersonalizacionDataList4 PersonalizacionItems = JsonUtility.FromJson<PersonalizacionDataList4>(wrappedJson);

        bool encontrado = false;

        foreach (var item in PersonalizacionItems.items)
        {
            if (item.codigo_cocina == codCocinaText.text)
            {
                encontrado = true;
                PersonalizacionEntries.Add(new PersonalizacionEntry4(
                    item.id, item.codigo_cocina, item.cocinas
                ));

                canvasInicioSesion.SetActive(false);
                idOk.text = item.id;
                canvasKeyboard.SetActive(false);
                StartCoroutine(LoadEntitlementsThenDecide(item.id, item.cocinas));
                break;
            }
        }

        if (!encontrado)
        {
            canvasError.SetActive(true);
            codCocina.text = "";
            if (canvasLoading != null) canvasLoading.SetActive(false);
            isProcessing = false;
        }

        return PersonalizacionEntries;
    }

    public IEnumerator LoadEntitlementsThenDecide(string restaurantId, string cocinas)
    {
        string entitlementsUrl = urlBase + "/personalizacion/entitlements_tpv/" + restaurantId;
        UnityWebRequest request = UnityWebRequest.Get(entitlementsUrl);
        yield return request.SendWebRequest();

        if (!request.isNetworkError && !request.isHttpError)
        {
            var wrapper = JsonConvert.DeserializeObject<EntitlementsResponse>(request.downloadHandler.text);
            DataBasePersonalizacionCocinaScene.Features = wrapper.features ?? new Dictionary<string, bool>();
        }
        else
        {
            Debug.LogWarning("Failed to fetch entitlements: " + request.error);
        }

        variasCocinas(cocinas);
    }

    private void variasCocinas(string cocinas)
    {
        string cocinasDB = cocinas;
        cocinasLista = new List<string>(cocinasDB.Split(';'));
        nCocinas = cocinasLista.Count;

        bool tieneVariasCocinas = DataBasePersonalizacionCocinaScene.HasFeature("cocinas_multiples");

        if (nCocinas > 1 && tieneVariasCocinas)
        {
            instanciarCocinas();
        }
        else if (nCocinas > 0)
        {
            tipoCocina.text = "";
            nCocina.text = "1";
            CMC.LoginStart();
        }
        else
        {
            CMC.LoginStart();
        }

        if (canvasLoading != null) canvasLoading.SetActive(false);
        isProcessing = false;
    }

    private void instanciarCocinas()
    {
        canvasTipoCocina.SetActive(true);
        if (nCocinas > 0)
        {
            for (int i = 0; i < nCocinas; i++)
            {
                int index = i;
                // Instanciar el prefab dentro del contenedor
                GameObject instancia = Instantiate(prefabTipoCocina, contenedorCocinas.transform);

                // Obtener el text dentro del prefab instanciado
                TMP_Text textCocina = instancia.GetComponentInChildren<TMP_Text>();

                // Asignar el nombre de la cocina
                textCocina.text = cocinasLista[i];

                // Obtener el botón de eliminar
                Button boton = instancia.GetComponentInChildren<Button>();
                if (boton != null)
                {
                    boton.onClick.AddListener(() => elegirCocina(textCocina.text, index));
                }
            }
        }
    }

    private void elegirCocina(string textCocina, int numCocina)
    {
        tipoCocina.text = textCocina;
        nCocina.text = (numCocina + 1).ToString(); // sumamos la del camarero
        canvasTipoCocina.SetActive(false);
        CMC.LoginStart();
    }
}


[Serializable]
public class PersonalizacionData4
{
    public string id;
    public string codigo_cocina;
    public string cocinas;
}

[Serializable]
public class PersonalizacionDataList4
{
    public PersonalizacionData4[] items;
}

public class PersonalizacionEntry4
{
    public string id { get; private set; }
    public string codigo_cocina { get; private set; }
    public string cocinas { get; private set; }

    public PersonalizacionEntry4(
        string id, string codigo_cocina, string cocinas)
    {
        this.id = id;
        this.codigo_cocina = codigo_cocina;
        this.cocinas = cocinas;
    }
}