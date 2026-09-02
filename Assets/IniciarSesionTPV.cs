using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class IniciarSesionTPV : MonoBehaviour
{
    public string urlPersonalizacion;
    public string urlMenu;

    public TMP_InputField codTPV;
    public TMP_Text codTPVText;
    public TMP_Text idOk;
    public static string[] codigo_TPV;
    public static string[] id;
    public GameObject canvasInicioSesion;
    public GameObject canvasInicioSesionPersonal;
    public GameObject canvasError;

    public ConnectMirrorTPV CMTPV;
    public GameObject canvasCargando; // opcional: un "cargando..." mientras espera respuesta
    private bool isProcessing = false;



    void Start()
    {
        canvasError.SetActive(false);
        canvasInicioSesion.SetActive(true);
    }

    void Update()
    {
        if (canvasInicioSesion.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnButtonClick2();
            }
        }
    }

    public void OnButtonClick2()
    {
        if (isProcessing) return;
        if (string.IsNullOrWhiteSpace(codTPV.text)) return;

        isProcessing = true;
        canvasError.SetActive(false);
        if (canvasCargando != null) canvasCargando.SetActive(true);

        codTPVText.text = codTPV.text;
        StartCoroutine(LoadPersonalizacionData());
    }
    public IEnumerator LoadPersonalizacionData()
    {
        UnityWebRequest request = UnityWebRequest.Get(urlPersonalizacion);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personalizacion: " + request.error);
            canvasError.SetActive(true);
            if (canvasCargando != null) canvasCargando.SetActive(false);
            isProcessing = false;
            yield break;
        }

        string PersonalizacionString = request.downloadHandler.text;
        //Debug.Log("Received JSON data: " + PersonalizacionString);

        List<PersonalizacionEntry5> PersonalizacionEntries = ParsePersonalizacion(PersonalizacionString);

        if (canvasCargando != null) canvasCargando.SetActive(false);
        isProcessing = false;

        // Initialize arrays with the size of the PersonalizacionEntries list
        int count = PersonalizacionEntries.Count;
        id = new string[count];
        //letra_empl = new string[count];
        //col_ppal_empl = new string[count];
        //col_sec_empl = new string[count];
        codigo_TPV = new string[count];

        for (int i = 0; i < count; i++)
        {
            PersonalizacionEntry5 entry = PersonalizacionEntries[i];
            id[i] = entry.id;
            codigo_TPV[i] = entry.codigo_TPV;
        }
    }

    


    public List<PersonalizacionEntry5> ParsePersonalizacion(string PersonalizacionString)
    {
        List<PersonalizacionEntry5> PersonalizacionEntries = new List<PersonalizacionEntry5>();

        // Wrap the JSON array in a root object for JsonUtility
        string wrappedJson = "{ \"items\": " + PersonalizacionString + " }";

        PersonalizacionDataList5 PersonalizacionItems = JsonUtility.FromJson<PersonalizacionDataList5>(wrappedJson);

        bool matched = false;

        foreach (var item in PersonalizacionItems.items)
        {
            if (item.codigo_TPV == codTPVText.text)
            {
                matched = true;
                PersonalizacionEntries.Add(new PersonalizacionEntry5(
                    item.id, item.codigo_TPV
                ));
                // si el codigo coincide con alguno de la db, se desactiva el canva de inicio
                canvasInicioSesion.SetActive(false);
                idOk.text = item.id;
                CMTPV.LoginStart();

                // active iniciar sesionTPV personal
                canvasInicioSesionPersonal.SetActive(true);
                break; // ya encontrado, no sigas comprobando el resto
            }
        }

        if (!matched)
        {
            canvasError.SetActive(true);
            codTPV.text = "";
        }

        return PersonalizacionEntries;
    }
}


[Serializable]
public class PersonalizacionData5
{
    public string id;
    //public string letra_empl;
    //public string col_ppal_empl;
    //public string col_sec_empl;
    public string codigo_TPV;
}

[Serializable]
public class PersonalizacionDataList5
{
    public PersonalizacionData5[] items;
}

public class PersonalizacionEntry5
{
    public string id { get; private set; }
    //public string letra_empl { get; private set; }
    //public string col_ppal_empl { get; private set; }
    //public string col_sec_empl { get; private set; }
    public string codigo_TPV { get; private set; }

    public PersonalizacionEntry5(
        string id, string codigo_TPV)
    {
        this.id = id;
        //this.letra_empl = letra_empl;
        //this.col_ppal_empl = col_ppal_empl;
        //this.col_sec_empl = col_sec_empl;
        this.codigo_TPV = codigo_TPV;
    }
}
