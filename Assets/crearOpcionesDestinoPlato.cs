using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

public class crearOpcionesDestinoPlato : MonoBehaviour
{
    public GameObject canvasRellenarPlato;
    public GameObject contenedorDestinos;
    public GameObject prefabOpcionDestino;
    public int nCocinas; // Número de instancias a crear
    private List<string> cocinasLista;
    private List<GameObject> instanciasCocinas = new List<GameObject>(); // Para almacenar las instancias de los prefabs
    [SerializeField] private ToggleGroup grupoDestinos;

    public DataBasePersonalizacionRespScene DB;

    void Start()
    {
        DB.OnDataLoaded += CrearOpcionesDestino;
    }
    
    void OnDestroy()
    {
        DB.OnDataLoaded -= CrearOpcionesDestino;
    }

    public void CrearOpcionesDestino()
    {
        // Sacamos el campo cocinas de la base de datos
        string cocinasDB = DataBasePersonalizacionRespScene.cocinas[0]; // creo que solo hay un valor?
        
        // Separamos las cocinas
        cocinasLista = new List<string>(cocinasDB.Split(';'));
        nCocinas = cocinasLista.Count; // número de cocinas

        if (nCocinas != null && nCocinas > 1) // o usa .Length si es un array
        {
            // Instanciamos camarero y las cocinas del array
            GameObject instancia0 = Instantiate(prefabOpcionDestino, contenedorDestinos.transform);
            // Asignar el ToggleGroup
            instancia0.GetComponent<Toggle>().group = grupoDestinos;
            instancia0.GetComponent<Toggle>().isOn = false;

            // Obtener el texto de la opción
            TMP_Text texto0 = instancia0.GetComponentInChildren<TMP_Text>();

            // Asignar el texto de la lista al InputField
            texto0.text = "Camarero";
            
            // Añadir la instancia a la lista para poder referenciarla luego
            instanciasCocinas.Add(instancia0);

            for (int i = 0; i < nCocinas; i++)
            {
                // Instanciar el prefab dentro del contenedor
                GameObject instancia = Instantiate(prefabOpcionDestino, contenedorDestinos.transform);
                instancia.GetComponent<Toggle>().group = grupoDestinos;
                instancia.GetComponent<Toggle>().isOn = false;

                // Obtener el texto de la opción
                TMP_Text texto = instancia.GetComponentInChildren<TMP_Text>();

                // Asignar el texto de la lista al InputField
                texto.text = cocinasLista[i]+" (Cocina)";
                
                // Añadir la instancia a la lista para poder referenciarla luego
                instanciasCocinas.Add(instancia);
            }
        }
        else
        {
            // Instanciamos solo camarero y cocina (si no hay varias cocinas)
            GameObject instancia = Instantiate(prefabOpcionDestino, contenedorDestinos.transform);
            instancia.GetComponent<Toggle>().group = grupoDestinos;

            // Obtener el texto de la opción
            TMP_Text texto = instancia.GetComponentInChildren<TMP_Text>();
            texto.text = "Camarero";
            // Añadir la instancia a la lista para poder referenciarla luego
            instanciasCocinas.Add(instancia);

            GameObject instancia1 = Instantiate(prefabOpcionDestino, contenedorDestinos.transform);
            instancia1.GetComponent<Toggle>().group = grupoDestinos;
            instancia.GetComponent<Toggle>().isOn = false;
            instancia1.GetComponent<Toggle>().isOn = false;

            // Obtener el texto de la opción
            TMP_Text texto1 = instancia1.GetComponentInChildren<TMP_Text>();
            texto1.text = "Cocina";
            // Añadir la instancia a la lista para poder referenciarla luego
            instanciasCocinas.Add(instancia1);
        }

    }
    
}
