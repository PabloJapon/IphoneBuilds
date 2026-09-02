using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class IniciarSesionTPVPersonal : MonoBehaviour
{
    public string url;  // URL base del servidor

    // campo de entrada para la informacion que hay que llevar a la base de datos
    public TMP_InputField codCamarero;
    public TMP_Text codCamareroText;
    public TMP_Text idOk;
    public static string[] codigo;
    public static string[] nombre;
    public static List<string>[] permisos;
    public static string[] id;
    public GameObject canvasInicioSesion;
    public GameObject canvasError;
    public TMP_Text textoError;
    public TMP_Text nombreUsuario;
    public FichajeController fichajeController;
    public GameObject menuMas;
    private string codigoActual;

    public Navigation Nv;

    [System.Serializable]
    public class BotonPermiso
    {
        public string permisoId;
        public Button boton;
    }

    // Un botón por cada permiso del catálogo — se asigna a mano en el Inspector
    public List<BotonPermiso> botonesPermiso;

    // Resetear a mesas al entrar
    public Button buttonMesas;

    // public ConnectMirrorCamarero CMC;

    void Start()
    {
        codCamarero.contentType = TMP_InputField.ContentType.Password;
        codCamarero.ForceLabelUpdate();
        canvasError.SetActive(false);
        canvasInicioSesion.SetActive(false);  // ← No se activa hasta que idOk tenga valor

    }

    void OnEnable()
    {
        FichajeEvents.OnFichajeCodigoInvalido += MostrarErrorFichaje;
    }

    void OnDisable()
    {
        FichajeEvents.OnFichajeCodigoInvalido -= MostrarErrorFichaje;
    }

    void MostrarErrorFichaje()
    {
        if (textoError != null) textoError.text = "El código de empleado introducido no existe";
        canvasError.SetActive(true);
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
        canvasError.SetActive(false);
        codCamareroText.text = codCamarero.text;
        codCamarero.text = "";
        StartCoroutine(LoadPersonalData6());
    }

    public void OnFicharClick()
    {
        canvasError.SetActive(false);
        string codigoTecleado = codCamarero.text;
        codCamarero.text = "";

        if (string.IsNullOrEmpty(codigoTecleado))
        {
            if (textoError != null) textoError.text = "El código de empleado introducido no existe";
            canvasError.SetActive(true);
            return;
        }

        fichajeController.FicharConCodigo(idOk.text, codigoTecleado);
    }

    public IEnumerator LoadPersonalData6()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/restaurant/" + idOk.text);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personal: " + request.error);
            yield break;
        }

        string PersonalString = request.downloadHandler.text;
        //Debug.Log("Received JSON data: " + PersonalString);

        List<PersonalEntry6> PersonalEntries = ParsePersonal(PersonalString);

        // Initialize arrays with the size of the PersonalEntries list
        int count = PersonalEntries.Count;
        id = new string[count];
        codigo = new string[count];
        nombre = new string[count];
        permisos = new List<string>[count];

        for (int i = 0; i < count; i++)
        {
            PersonalEntry6 entry = PersonalEntries[i];
            id[i] = entry.id;
            codigo[i] = entry.codigo;
            nombre[i] = entry.nombre;
            permisos[i] = entry.permisos;
        }

        // call initializing camarero in navigation
        Nv.ZonaCamarero();
    }


    public List<PersonalEntry6> ParsePersonal(string PersonalString)
    {
        List<PersonalEntry6> PersonalEntries = new List<PersonalEntry6>();

        string wrappedJson = "{ \"items\": " + PersonalString + " }";
        PersonalDataList6 PersonalizacionItems = JsonUtility.FromJson<PersonalDataList6>(wrappedJson);

        bool encontrado = false;

        foreach (var item in PersonalizacionItems.items)
        {
            if (item.codigo == codCamareroText.text && item.id == idOk.text)
            {
                encontrado = true;

                if (!item.fichado)
                {
                    if (textoError != null) textoError.text = "No puedes iniciar sesión sin haber fichado";
                    canvasError.SetActive(true);
                    codCamarero.text = "";
                    codCamareroText.text = "";
                    break;
                }

                PersonalEntries.Add(new PersonalEntry6(item.id, item.codigo, item.nombre, item.permisos));

                codigoActual = item.codigo; // guardamos el código verificado para usarlo luego en Fichar
                codCamareroText.text = ""; // no longer needed, don't leave the code in a visible field

                SesionEmpleado.RestaurantId = item.id;
                SesionEmpleado.Codigo = item.codigo;
                SesionEmpleado.Permisos = item.permisos ?? new List<string>();

                canvasInicioSesion.SetActive(false);
                canvasError.SetActive(false);

                nombreUsuario.text = "Hola, " + item.nombre;

                // Activar/desactivar cada botón (uGUI) según sus permisos
                foreach (var bp in botonesPermiso)
                {
                    if (bp.boton != null)
                        bp.boton.interactable = SesionEmpleado.Permisos.Contains(bp.permisoId);
                }

                // Canvas Menu al iniciar sesion
                buttonMesas.onClick.Invoke();
                if (menuMas != null) menuMas.SetActive(true); 

                break; // ya no seguimos buscando
            }
        }

        if (!encontrado)
        {
            if (textoError != null) textoError.text = "El código de empleado introducido no existe";
            canvasError.SetActive(true);
            codCamarero.text = "";
            codCamareroText.text = "";
        }

        return PersonalEntries;
    }
}


[Serializable]
public class PersonalData6
{
    public string id;
    public string codigo;
    public string nombre;
    public List<string> permisos;
    public bool fichado;
}

[Serializable]
public class PersonalDataList6
{
    public PersonalData6[] items;
}

public class PersonalEntry6
{
    public string id { get; private set; }
    public string codigo { get; private set; }
    public string nombre { get; private set; }
    public List<string> permisos { get; private set; }

    public PersonalEntry6(
        string id, string codigo, string nombre, List<string> permisos)
    {
        this.id = id;
        this.codigo = codigo;
        this.nombre = nombre;
        this.permisos = permisos ?? new List<string>();
    }
}
