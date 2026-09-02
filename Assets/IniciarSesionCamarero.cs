using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class IniciarSesionCamarero : MonoBehaviour
{
    public string url;  // URL base del servidor
    public Navigation NAV;

    // campo de entrada para la informacion que hay que llevar a la base de datos
    public TMP_InputField codCamarero;
    public TMP_Text codCamareroText;
    public TMP_Text idOk;
    public static string[] codigo;
    public static string[] nombre;
    public static string[] id;
    public static List<string>[] permisos;
    public GameObject canvasInicioSesion;
    public GameObject canvasError;
    public GameObject canvasTemporal;
    public TMP_Text textoError;
    public TMP_Text holaText;
    public TMP_Text tituloInicioSesion; // Arrastra aquí el texto título del canvas (el que dice "Iniciar sesión")

    [System.Serializable]
    public class BotonPermiso
    {
        public string permisoId;
        public Button boton;
    }

    // Un botón por cada permiso del catálogo — se asigna a mano en el Inspector
    public List<BotonPermiso> botonesPermiso;

    void Start()
    {
        canvasError.SetActive(false);
        canvasInicioSesion.SetActive(false);  // ← No se activa hasta que idOk tenga valor
    }

    private bool canvasInicioSesionActivoAnterior = false;

    void Update()
    {
        if (canvasInicioSesion != null && canvasInicioSesion.activeSelf != canvasInicioSesionActivoAnterior)
        {
            canvasInicioSesionActivoAnterior = canvasInicioSesion.activeSelf;
            if (canvasInicioSesionActivoAnterior)
            {
                ActualizarTituloInicioSesion();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return)) // Enter o Return
        {
            OnButtonClick2();
        }
    }

    private void ActualizarTituloInicioSesion()
    {
        if (tituloInicioSesion == null) return;

        switch (Navigation.destinoPendiente)
        {
            case Navigation.DestinoCamarero.Turnos:
                tituloInicioSesion.text = "Ver mis turnos";
                break;
            case Navigation.DestinoCamarero.Fichajes:
                tituloInicioSesion.text = "Ver mis fichajes";
                break;
            default:
                tituloInicioSesion.text = "Iniciar sesión";
                break;
        }
    }

    public void OnButtonClick2()
    {
        codCamareroText.text = codCamarero.text;
        codCamarero.text = "";
        StartCoroutine(LoadPersonalData4());
    }

    public IEnumerator LoadPersonalData4()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/restaurant/" + idOk.text);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Personal: " + request.error);
            if (textoError != null) textoError.text = "Sin conexión. Compruebe la conexión red";
            canvasError.SetActive(true);
            codCamarero.text = "";
            yield break;
        }

        string PersonalString = request.downloadHandler.text;
        List<PersonalEntry4> PersonalEntries = ParsePersonal(PersonalString);

        int count = PersonalEntries.Count;
        id = new string[count];
        codigo = new string[count];
        nombre = new string[count];
        permisos = new List<string>[count];

        for (int i = 0; i < count; i++)
        {
            id[i] = PersonalEntries[i].id;
            codigo[i] = PersonalEntries[i].codigo;
            nombre[i] = PersonalEntries[i].nombre;
            permisos[i] = PersonalEntries[i].permisos;
        }
    }

    public List<PersonalEntry4> ParsePersonal(string PersonalString)
    {
        List<PersonalEntry4> PersonalEntries = new List<PersonalEntry4>();

        string wrappedJson = "{ \"items\": " + PersonalString + " }";
        PersonalDataList4 PersonalizacionItems = JsonUtility.FromJson<PersonalDataList4>(wrappedJson);

        // Buscamos únicamente el empleado que coincide con el código introducido.
        // Antes se iteraba y se disparaba canvasError por cada NO coincidencia,
        // aunque hubiera una coincidencia real en la lista. Ahora se busca primero.
        PersonalData4 empleadoEncontrado = null;
        foreach (var item in PersonalizacionItems.items)
        {
            if (item.codigo == codCamareroText.text && item.id == idOk.text)
            {
                empleadoEncontrado = item;
                break;
            }
        }

        bool requiereFichaje = Navigation.destinoPendiente == Navigation.DestinoCamarero.Ninguno;

        if (requiereFichaje && empleadoEncontrado != null && !empleadoEncontrado.fichado)
        {
            if (textoError != null) textoError.text = "No puedes iniciar sesión sin haber fichado";
            canvasError.SetActive(true);
            codCamarero.text = "";
            return PersonalEntries;
        }

        if (empleadoEncontrado != null)
        {
            PersonalEntries.Add(new PersonalEntry4(
                empleadoEncontrado.id,
                empleadoEncontrado.id_empleado,
                empleadoEncontrado.codigo,
                empleadoEncontrado.nombre,
                empleadoEncontrado.permisos));

            holaText.text = "Hola, " + empleadoEncontrado.nombre;

            SesionEmpleado.RestaurantId = empleadoEncontrado.id;
            SesionEmpleado.IdEmpleado = empleadoEncontrado.id_empleado;
            SesionEmpleado.Codigo = empleadoEncontrado.codigo;
            SesionEmpleado.Permisos = empleadoEncontrado.permisos ?? new List<string>();

            // Activar/desactivar cada botón según los permisos del empleado
            foreach (var bp in botonesPermiso)
            {
                if (bp.boton != null)
                    bp.boton.interactable = SesionEmpleado.Permisos.Contains(bp.permisoId);
            }

            canvasInicioSesion.SetActive(false);
            canvasError.SetActive(false);
            StartCoroutine(MostrarCanvasPorTresSegundosYDestino());
        }
        else
        {
            if (textoError != null) textoError.text = "El código de empleado introducido no existe";
            canvasError.SetActive(true);
            codCamarero.text = "";
        }

        return PersonalEntries;
    }

    private IEnumerator MostrarCanvasPorTresSegundosYDestino()
    {
        canvasTemporal.SetActive(true);
        yield return new WaitForSeconds(3f);
        canvasTemporal.SetActive(false);
        NAV.MostrarDestinoTrasLogin();
    }
}

[Serializable]
public class PersonalData4
{
    public string id;
    public int id_empleado;
    public string codigo;
    public string nombre;
    public bool fichado;
    public List<string> permisos;
}

[Serializable]
public class PersonalDataList4
{
    public PersonalData4[] items;
}

public class PersonalEntry4
{
    public string id { get; private set; }
    public int id_empleado { get; private set; }
    public string codigo { get; private set; }
    public string nombre { get; private set; }
    public List<string> permisos { get; private set; }

    public PersonalEntry4(string id, int id_empleado, string codigo, string nombre, List<string> permisos)
    {
        this.id = id;
        this.id_empleado = id_empleado;
        this.codigo = codigo;
        this.nombre = nombre;
        this.permisos = permisos ?? new List<string>();
    }
}