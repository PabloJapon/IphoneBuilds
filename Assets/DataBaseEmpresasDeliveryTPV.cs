using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class DataBaseEmpresasDeliveryTPV : MonoBehaviour
{
    public string url;
    public TMP_Text textId;

    private List<string> menus = new List<string>();
    private List<string> ids = new List<string>();
    private List<string> nombres = new List<string>();

    private TMP_Text textNombre;
    private GameObject[] prefabsEmpresa;
    private EmpresasDeliveryList empresasDeliveryList;



    public GameObject chooseDelivery;
    public GameObject contentChooseDelivery;
    public GameObject prefabButtonChooseDelivery;

    public GameObject detalleCliente;

    public static string nameEmpresa;
    public static string menuEmpresa;
    public static string idEmpresa;

    void Awake()
    {
        StartCoroutine(WaitForRestaurantIDResponsable());
    }


    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(textId.text))
        {
            yield return null; // Wait for the next frame
        }

        // Once we have a valid restaurant ID, start loading data
        StartCoroutine(LoadEmpresasDeliveryData());
    }

    public IEnumerator LoadEmpresasDeliveryData()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/restaurant/" + textId.text);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch EmpresasDelivery: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        //Debug.Log("Received JSON data empresasdelivery: " + json);

        // Arreglamos el JSON para que sea compatible con JsonUtility
        string wrappedJson = "{\"empresasdelivery\":" + json + "}";

        // Deserializamos
        empresasDeliveryList = JsonUtility.FromJson<EmpresasDeliveryList>(wrappedJson);

        foreach (var person in empresasDeliveryList.empresasdelivery)
        {
            menus.Add(person.menu);
            ids.Add(person.empresa_id);
            nombres.Add(person.nombre);
        }

        CreatePrefabButtons();
    }

    void CreatePrefabButtons()
    {
        prefabsEmpresa = new GameObject[nombres.Count];

        for (int i = 0; i < nombres.Count; i++)
        {
            CreatePrefab(i);
        }
    }
    private void CreatePrefab(int index)
    {
        // Instancia el prefab del empresa
        var prefabEmpresaInstance = Instantiate(prefabButtonChooseDelivery, transform.position, Quaternion.identity);
        
        // Lo hace hijo del contenedor general
        prefabEmpresaInstance.transform.SetParent(contentChooseDelivery.transform, false);
        //prefabEmpresaInstance.GetComponent<RectTransform>().localScale = Vector3.one;

        // Guarda la referencia
        prefabsEmpresa[index] = prefabEmpresaInstance;

        // Asigna los datos al prefab
        SetPrefabDetails(prefabEmpresaInstance, index, nombres.ToArray(), menus.ToArray());

        // Asigna los botones si existen
        var button = prefabEmpresaInstance.GetComponent<Button>();
        button.onClick.AddListener(() => OnClickButtonEmpresa(index));
    }

    private void SetPrefabDetails(GameObject prefab, int index, string[] names, string[] menus)
    {
        var textComponents = prefab.GetComponentsInChildren<TMP_Text>();

        textNombre = textComponents[0];
        textNombre.text = names[index];
    }

    private void OnClickButtonEmpresa(int index)
    {
        //Debug.Log(nombres[index]);
        nameEmpresa = nombres[index];
        menuEmpresa = menus[index];
        idEmpresa = ids[index];

        if (!string.IsNullOrEmpty(IncomingCallOrderRouter.pendingNumero))
        {
            TPV_DataManager.instance.PrefillFromPhoneNumber(IncomingCallOrderRouter.pendingNumero);
            IncomingCallOrderRouter.pendingNumero = null;
        }

        detalleCliente.SetActive(true);
        chooseDelivery.SetActive(false);
        //StartCoroutine(LoadMenusFromServer(formDropdown));
    }
    

    public IEnumerator LoadMenusFromServer(TMP_Dropdown dropdown)
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/menus/" + textId.text);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to load menu list: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        MenuList list = JsonUtility.FromJson<MenuList>("{\"items\":" + json + "}");

        dropdown.ClearOptions();

        List<string> menuOptions = new List<string>();
        foreach (var menu in list.items)
            menuOptions.Add("Menú " + menu);

        dropdown.AddOptions(menuOptions);

        //Debug.Log("Loaded menus: " + string.Join(", ", list.items));

    }
}
