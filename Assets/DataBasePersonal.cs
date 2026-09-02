using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

public class DataBasePersonal : MonoBehaviour
{
    public string url;

    private List<string> codigos = new List<string>();
    private List<int> idsEmpleado = new List<int>();
    private List<string> nombres = new List<string>();
    private List<List<string>> permisosEmpleados = new List<List<string>>();

    public GameObject prefabEmpleado;
    private List<GameObject> prefabsEmpleado = new List<GameObject>();
    public GameObject masEmpleadoPrefab;
    private GameObject masEmpleadoInstancia;
    public Transform parent;
    public GameObject canvasRellenarEmpleado;
    public bool creatingData = false;
    private int editingIndex = -1;
    private PersonalList personalList;

    // Un toggle por cada permiso del catálogo — se asigna a mano en el Inspector
    [System.Serializable]
    public class TogglePermiso
    {
        public string permisoId;
        public Toggle toggle;
    }
    public List<TogglePermiso> togglesPermiso;

    void Awake()
    {
        StartCoroutine(WaitForRestaurantIDResponsable());
    }

    private IEnumerator WaitForRestaurantIDResponsable()
    {
        while (string.IsNullOrEmpty(LoginManagerResponsable.restaurantID))
        {
            yield return null;
        }

        StartCoroutine(LoadPersonalData());
    }

    public IEnumerator LoadPersonalData()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/restaurant/" + LoginManagerResponsable.restaurantID);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personal: " + request.error);
            yield break;
        }

        string json = request.downloadHandler.text;
        string wrappedJson = "{\"personal\":" + json + "}";
        personalList = JsonUtility.FromJson<PersonalList>(wrappedJson);

        codigos.Clear();
        idsEmpleado.Clear();
        nombres.Clear();
        permisosEmpleados.Clear();

        foreach (var person in personalList.personal)
        {
            codigos.Add(person.codigo);
            idsEmpleado.Add(person.id_empleado);
            nombres.Add(person.nombre);
            permisosEmpleados.Add(person.permisos ?? new List<string>());
        }

        RepintarPrefabs();
    }

    void RepintarPrefabs()
    {
        foreach (var p in prefabsEmpleado)
        {
            if (p != null) Destroy(p);
        }
        prefabsEmpleado.Clear();

        if (masEmpleadoInstancia != null)
        {
            Destroy(masEmpleadoInstancia);
            masEmpleadoInstancia = null;
        }

        for (int i = 0; i < nombres.Count; i++)
        {
            CreatePrefab(i);
        }

        CreateMasEmpleadoButton();
    }

    private void CreatePrefab(int index)
    {
        var prefabEmpleadoInstance = Instantiate(prefabEmpleado, transform.position, Quaternion.identity);
        prefabEmpleadoInstance.transform.SetParent(parent.transform, false);
        prefabEmpleadoInstance.GetComponent<RectTransform>().localScale = Vector3.one;

        prefabsEmpleado.Add(prefabEmpleadoInstance);

        SetPrefabDetails(prefabEmpleadoInstance, index);

        var button = prefabEmpleadoInstance.GetComponentsInChildren<Button>();
        if (button.Length > 0 && button[0] != null)
        {
            int capturedIndex = index;
            button[0].onClick.AddListener(() => OnClickButtonEmpleado(capturedIndex, false));
        }

        if (button.Length > 1 && button[1] != null)
        {
            int capturedIndex = index;
            button[1].onClick.AddListener(() => DeleteEmpleadoOnClick(capturedIndex));
        }
    }

    private void SetPrefabDetails(GameObject prefab, int index)
    {
        var textComponents = prefab.GetComponentsInChildren<TMP_Text>();

        textComponents[0].text = nombres[index];
        textComponents[1].text = "Código: " + codigos[index];
        textComponents[2].text = permisosEmpleados[index].Count + " permisos habilitados";
    }

    private void OnClickButtonEmpleado(int index, bool isNew)
    {
        canvasRellenarEmpleado.SetActive(true);

        var inputFields = canvasRellenarEmpleado.GetComponentsInChildren<TMP_InputField>();

        if (isNew)
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i].text = string.Empty;
            }

            creatingData = true;
            editingIndex = -1;

            foreach (var tp in togglesPermiso)
            {
                if (tp.toggle != null) tp.toggle.isOn = false;
            }
        }
        else
        {
            inputFields[0].text = nombres[index];
            inputFields[1].text = codigos[index];

            List<string> permisosActuales = permisosEmpleados[index];
            foreach (var tp in togglesPermiso)
            {
                if (tp.toggle != null) tp.toggle.isOn = permisosActuales.Contains(tp.permisoId);
            }

            creatingData = false;
            editingIndex = index;
        }
    }

    private void DeleteEmpleadoOnClick(int index)
    {
        string jsonData = $"{{\"id_empleado\":{idsEmpleado[index]}}}";
        StartCoroutine(DeleteEmpleadoData(jsonData));
    }

    private IEnumerator DeleteEmpleadoData(string jsonData)
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
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            yield return LoadPersonalData();
        }
    }

    private void CreateMasEmpleadoButton()
    {
        masEmpleadoInstancia = Instantiate(masEmpleadoPrefab, transform.position, Quaternion.identity);
        masEmpleadoInstancia.transform.SetParent(parent.transform, false);
        masEmpleadoInstancia.GetComponent<RectTransform>().localScale = Vector3.one;

        var buttonMas = masEmpleadoInstancia.GetComponentInChildren<Button>();
        if (buttonMas != null)
        {
            buttonMas.onClick.AddListener(() => OnClickButtonEmpleado(-1, true));
        }

        masEmpleadoInstancia.transform.SetAsLastSibling();
    }

    public void UpdateEmpleados()
    {
        var inputFields = canvasRellenarEmpleado.GetComponentsInChildren<TMP_InputField>();

        List<string> permisosSeleccionados = new List<string>();
        foreach (var tp in togglesPermiso)
        {
            if (tp.toggle != null && tp.toggle.isOn)
                permisosSeleccionados.Add(tp.permisoId);
        }

        if (creatingData)
        {
            StartCoroutine(CreateEmpleadosData(inputFields[0].text, inputFields[1].text, permisosSeleccionados));
        }
        else
        {
            StartCoroutine(UpdateEmpleadosData(editingIndex, inputFields[0].text, inputFields[1].text, permisosSeleccionados));
        }
    }

    private string PermisosToJson(List<string> permisos)
    {
        if (permisos == null || permisos.Count == 0) return "[]";
        return "[" + string.Join(",", permisos.Select(p => "\"" + p + "\"")) + "]";
    }

    public IEnumerator UpdateEmpleadosData(int index, string newName, string codigo, List<string> permisos)
    {
        if (index < 0 || index >= idsEmpleado.Count)
        {
            Debug.LogError("Índice de empleado no válido al actualizar.");
            yield break;
        }

        int idEmpleado = idsEmpleado[index];
        string jsonData = $"{{\"id_empleado\":{idEmpleado},\"nombre\":\"{newName}\",\"codigo\":\"{codigo}\",\"permisos\":{PermisosToJson(permisos)}}}";

        UnityWebRequest request = new UnityWebRequest(url + "/update", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("info actualizada: " + jsonData);
            yield return LoadPersonalData();
        }

        canvasRellenarEmpleado.SetActive(false);
    }

    private IEnumerator CreateEmpleadosData(string nombre, string codigo, List<string> permisos)
    {
        var id = LoginManagerResponsable.restaurantID;
        string jsonData = $"{{\"id\":\"{id}\",\"nombre\":\"{nombre}\",\"codigo\":\"{codigo}\",\"permisos\":{PermisosToJson(permisos)}}}";

        UnityWebRequest request = new UnityWebRequest(url + "/add", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("info creada: " + jsonData);
            yield return LoadPersonalData();
        }

        creatingData = false;
        canvasRellenarEmpleado.SetActive(false);
    }
}

[System.Serializable]
public class PersonalEntry
{
    public string id;
    public string nombre;
    public string codigo;
    public int id_empleado;
    public List<string> permisos;
}

[System.Serializable]
public class PersonalList
{
    public List<PersonalEntry> personal;
}