using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DataBaseEmpresasDelivery : MonoBehaviour
{
    public string url;

    public List<string> menus = new List<string>();
    public List<string> ids = new List<string>(); // stores empresa_id
    public List<string> nombres = new List<string>();

    private TMP_Text textNombre;
    private TMP_Text textMenu;
    private TMP_Text textID;
    public GameObject prefabEmpresa;
    public GameObject[] prefabsEmpresa;
    public GameObject masEmpresaPrefab;
    public Transform parent;
    public GameObject canvasRellenarEmpresa;
    public bool creatingData = false;
    private EmpresasDeliveryList empresasDeliveryList;
    private string pendingMenuToSelect;
    public Button saveButton;
    private bool isProgrammaticChange = false;

    private GameObject masEmpresaInstance;


    void Awake()
    {
        StartCoroutine(WaitForRestaurantIDResponsable());
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
            yield return null;

        StartCoroutine(LoadEmpresasDeliveryData());
    }

    public IEnumerator LoadEmpresasDeliveryData()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/restaurant/" + LoginManagerResponsable.restaurantID);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch EmpresasDelivery: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        string wrappedJson = "{\"empresasdelivery\":" + json + "}";
        empresasDeliveryList = JsonUtility.FromJson<EmpresasDeliveryList>(wrappedJson);

        menus.Clear();
        ids.Clear();
        nombres.Clear();

        foreach (var person in empresasDeliveryList.empresasdelivery)
        {
            menus.Add(person.menu);
            ids.Add(person.empresa_id);
            nombres.Add(person.nombre);
        }

        CreatePrefabs();
    }

    void CreatePrefabs()
    {
        prefabsEmpresa = new GameObject[nombres.Count];
        for (int i = 0; i < nombres.Count; i++)
            CreatePrefab(i);

        CreateMasEmpresaButton();
    }

    private void CreatePrefab(int index)
    {
        var prefabEmpresaInstance = Instantiate(prefabEmpresa, transform.position, Quaternion.identity, parent);
        prefabEmpresaInstance.GetComponent<RectTransform>().localScale = Vector3.one;

        // 👇 Insert BEFORE the "+" button if it exists
        if (masEmpresaInstance != null)
        {
            int plusIndex = masEmpresaInstance.transform.GetSiblingIndex();
            prefabEmpresaInstance.transform.SetSiblingIndex(plusIndex);
        }

        prefabsEmpresa[index] = prefabEmpresaInstance;

        SetPrefabDetails(prefabEmpresaInstance, index, nombres.ToArray(), ids.ToArray(), menus.ToArray());

        var buttons = prefabEmpresaInstance.GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
            buttons[0].onClick.AddListener(() => OnClickButtonEmpresa(index, false));
        if (buttons.Length > 1)
            buttons[1].onClick.AddListener(() => DeleteEmpresaOnClick(index));
    }


    private void SetPrefabDetails(GameObject prefab, int index, string[] names, string[] ids, string[] menus)
    {
        var textComponents = prefab.GetComponentsInChildren<TMP_Text>();
        textNombre = textComponents[0];
        textMenu = textComponents[1];
        textID = textComponents[2];
        textNombre.text = names[index];
        textMenu.text = "Menú: " + menus[index];
        textID.text = ids[index];
    }

    private void OnClickButtonEmpresa(int index, bool isNew)
    {
        canvasRellenarEmpresa.SetActive(true);
        saveButton.interactable = false;

        var inputFields = canvasRellenarEmpresa.GetComponentsInChildren<TMP_InputField>();
        TMP_Dropdown formDropdown = canvasRellenarEmpresa.GetComponentInChildren<TMP_Dropdown>();
        var TMP_Texts = canvasRellenarEmpresa.GetComponentsInChildren<TMP_Text>();

        inputFields[0].onValueChanged.RemoveAllListeners();
        inputFields[0].onValueChanged.AddListener((text) =>
        {
            saveButton.interactable = !string.IsNullOrEmpty(text);
        });

        if (isNew)
        {
            inputFields[0].text = "";
            creatingData = true;
            pendingMenuToSelect = null;
            StartCoroutine(LoadMenusFromServer(formDropdown));
            return;
        }

        creatingData = false;
        inputFields[0].text = nombres[index];

        if (index >= 0 && index < menus.Count)
            pendingMenuToSelect = menus[index];
        else
            pendingMenuToSelect = null;

        TMP_Texts[5].text = ids[index];

        StartCoroutine(LoadMenusFromServer(formDropdown));
    }

    private void DeleteEmpresaOnClick(int index)
    {
        string jsonData = $"{{\"empresa_id\":\"{ids[index]}\"}}";
        StartCoroutine(DeleteEmpresaData(jsonData, index));
    }

    private IEnumerator DeleteEmpresaData(string jsonData, int index)
    {
        UnityWebRequest request = new UnityWebRequest(url + "/delete", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
            yield break;
        }

        nombres.RemoveAt(index);
        menus.RemoveAt(index);
        ids.RemoveAt(index);

        Destroy(prefabsEmpresa[index]);
        RemoveItemFromArray(ref prefabsEmpresa, index);
        UpdatePrefabIndices();
    }

    private void RemoveItemFromArray<T>(ref T[] array, int index)
    {
        for (int i = index; i < array.Length - 1; i++)
            array[i] = array[i + 1];
        Array.Resize(ref array, array.Length - 1);
    }

    private void UpdatePrefabIndices()
    {
        for (int i = 0; i < prefabsEmpresa.Length; i++)
        {
            var buttons = prefabsEmpresa[i].GetComponentsInChildren<Button>();
            if (buttons.Length > 0)
            {
                int updatedIndex = i;
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(() => OnClickButtonEmpresa(updatedIndex, false));
            }
            if (buttons.Length > 1)
            {
                int updatedIndex = i;
                buttons[1].onClick.RemoveAllListeners();
                buttons[1].onClick.AddListener(() => DeleteEmpresaOnClick(updatedIndex));
            }
        }
    }

    private void CreateMasEmpresaButton()
    {
        masEmpresaInstance = Instantiate(masEmpresaPrefab, transform.position, Quaternion.identity, parent);
        masEmpresaInstance.GetComponent<RectTransform>().localScale = Vector3.one;

        var buttonMas = masEmpresaInstance.GetComponentInChildren<Button>();
        if (buttonMas != null)
            buttonMas.onClick.AddListener(() => OnClickButtonEmpresa(-1, true));

        masEmpresaInstance.transform.SetAsLastSibling();
    }


    public void UpdateEmpresas()
    {
        var inputFields = canvasRellenarEmpresa.GetComponentsInChildren<TMP_InputField>();
        string nombre = inputFields[0].text;
        var dropdown = canvasRellenarEmpresa.GetComponentInChildren<TMP_Dropdown>();
        string selectedMenu = dropdown.options[dropdown.value].text;

        var TMP_Texts = canvasRellenarEmpresa.GetComponentsInChildren<TMP_Text>();
        string empresa_id = TMP_Texts[5].text;

        if (creatingData)
            StartCoroutine(CreateEmpresasData(nombre, selectedMenu));
        else
            StartCoroutine(UpdateEmpresasData(empresa_id, nombre, selectedMenu));
    }

    public IEnumerator UpdateEmpresasData(string empresa_id, string nombre, string selectedMenu)
    {
        string jsonData = $"{{\"empresa_id\":\"{empresa_id}\",\"nombre\":\"{nombre}\",\"selected_Menu\":\"{selectedMenu}\"}}";
        yield return SendRequest("/update", jsonData);

        Debug.Log(empresa_id + nombre + selectedMenu);
        // Update prefab visually
        UpdatePrefabById(empresa_id, nombre, selectedMenu);
        canvasRellenarEmpresa.SetActive(false);
    }

    private void UpdatePrefabById(string empresa_id, string nombre, string selectedMenu)
    {
        int index = ids.IndexOf(empresa_id);
        if (index >= 0 && index < prefabsEmpresa.Length)
        {
            var prefab = prefabsEmpresa[index];
            var texts = prefab.GetComponentsInChildren<TMP_Text>();
            texts[0].text = nombre;
            texts[1].text = "Menú: " + selectedMenu;

            nombres[index] = nombre;
            menus[index] = selectedMenu;
        }
    }

    private IEnumerator CreateEmpresasData(string nombre, string menu)
    {
        var restaurantId = LoginManagerResponsable.restaurantID;

        // ✅ Generate empresa_id in Unity
        string empresaId = Guid.NewGuid().ToString();

        string jsonData =
            $"{{\"id\":\"{restaurantId}\",\"nombre\":\"{nombre}\",\"menu\":\"{menu}\",\"empresa_id\":\"{empresaId}\"}}";

        yield return SendRequest("/add", jsonData);

        Debug.Log("Empresa creada con ID: " + empresaId);

        // Add to local lists
        nombres.Add(nombre);
        menus.Add(menu);
        ids.Add(empresaId);

        int newIndex = nombres.Count - 1;

        // Resize prefab array
        Array.Resize(ref prefabsEmpresa, nombres.Count);

        // Create the prefab using YOUR method
        CreatePrefab(newIndex);

        canvasRellenarEmpresa.SetActive(false);
        creatingData = false;
    }

    private IEnumerator SendRequest(string endpoint, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
            Debug.LogError(request.error);
        else
            UpdatePrefabs();
    }

    void UpdatePrefabs()
    {
        // Optional: refresh visual prefabs if needed
        //for (int i = 0; i < prefabsEmpresa.Length; i++)
            //SetPrefabDetails(prefabsEmpresa[i], i, nombres.ToArray(), menus.ToArray());
    }

    public IEnumerator LoadMenusFromServer(TMP_Dropdown dropdown)
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/menus/" + LoginManagerResponsable.restaurantID);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
            yield break;

        string json = request.downloadHandler.text;
        MenuList list = JsonUtility.FromJson<MenuList>("{\"items\":" + json + "}");

        dropdown.ClearOptions();
        List<string> menuOptions = new List<string>();
        foreach (var menu in list.items)
            menuOptions.Add("Menú " + menu);

        dropdown.AddOptions(menuOptions);

        if (!string.IsNullOrEmpty(pendingMenuToSelect))
        {
            int index = dropdown.options.FindIndex(o => o.text == pendingMenuToSelect);
            if (index >= 0)
            {
                isProgrammaticChange = true;
                dropdown.value = index;
                isProgrammaticChange = false;
            }
            pendingMenuToSelect = null;
        }

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener((v) =>
        {
            if (!isProgrammaticChange)
                saveButton.interactable = true;
        });
    }
}

[System.Serializable]
public class EmpresasDeliveryEntry
{
    public string menu;
    public string empresa_id;
    public string nombre;
}

[System.Serializable]
public class EmpresasDeliveryList
{
    public List<EmpresasDeliveryEntry> empresasdelivery;
}

[Serializable]
public class MenuList
{
    public List<string> items;
}
