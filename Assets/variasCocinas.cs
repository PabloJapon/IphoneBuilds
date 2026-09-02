using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class variasCocinas : MonoBehaviour
{
    public GameObject prefabNombreCocina;
    public GameObject prefabMasCocina;
    public Toggle toggleVariasCocinas;
    public GameObject contenedorCocinas;
    public GameObject contenedorPrefab;
    private GameObject instanciaPrefab;
    public int nCocinas; // Número de instancias a crear
    private List<string> cocinasLista;
    private List<GameObject> instanciasCocinas = new List<GameObject>(); // Para almacenar las instancias de los prefabs

    // Start is called before the first frame update
    
    public DataBasePersonalizacionRespScene DB;

    void Start()
    {
        DB.OnDataLoaded += RellenarCocinas;
    }
    
    void OnDestroy()
    {
        DB.OnDataLoaded -= RellenarCocinas;
    }

    public void RellenarCocinas()
    {
        contenedorPrefab.SetActive(false); // Asegúrate de que arranque oculto

        // Eliminar todos los hijos del contenedorPrefab
        foreach (Transform child in contenedorPrefab.transform)
        {
            Destroy(child.gameObject);
        }

        // Sacamos el campo cocinas de la base de datos
        string cocinasDB = DataBasePersonalizacionRespScene.cocinas[0]; // creo que solo hay un valor?
        
        // Separamos las cocinas
        cocinasLista = new List<string>(cocinasDB.Split(';'));
        nCocinas = cocinasLista.Count; // número de cocinas

        if (nCocinas != null && nCocinas > 1) // o usa .Length si es un array
        {
            // Activamos el Toggle si nCocinas no es null y tiene elementos
            toggleVariasCocinas.isOn = true;
            bool activo = true;
            OnToggleCambiado(activo);
        }
        else
        {
            // Opcionalmente, puedes desactivar el Toggle si no cumple la condición
            toggleVariasCocinas.isOn = false;
        }

        // optiongroups()
        toggleVariasCocinas.onValueChanged.AddListener(OnToggleCambiado);
    }

    void OnToggleCambiado(bool isOn)
    {
        if (isOn)
        {
            contenedorPrefab.SetActive(isOn);

            if(nCocinas != null && nCocinas > 1) // si ya hay cocinas en la base de datos
            {
                for (int i = 0; i < nCocinas; i++)
                {
                    // Instanciar el prefab dentro del contenedor
                    GameObject instancia = Instantiate(prefabNombreCocina, contenedorPrefab.transform);

                    // Obtener el InputField dentro del prefab instanciado
                    TMP_InputField inputField = instancia.GetComponentInChildren<TMP_InputField>();
                    TMP_Text texto = instancia.GetComponentInChildren<TMP_Text>();

                    // Asignar el texto de la lista al InputField
                    inputField.text = cocinasLista[i];
                    int j=i+1; // para el número de cocina
                    texto.text = "Nombre cocina "+ j + ":";

                    // Obtener el botón de eliminar
                    Button botonEliminar = instancia.GetComponentInChildren<Button>();
                    if (botonEliminar != null)
                    {
                        // IMPORTANTE: Capturar la referencia actual a 'instancia' usando una variable local
                        GameObject instanciaActual = instancia;
                        botonEliminar.onClick.AddListener(() => EliminarCocina(instanciaActual));
                    }

                    
                    // Añadir la instancia a la lista para poder referenciarla luego
                    instanciasCocinas.Add(instancia);
                }
            }
            else if (nCocinas == null || nCocinas == 0 || nCocinas == 1) // si no hay cocinas
            {
                // Instanciar el prefab dentro del contenedor
                GameObject instancia = Instantiate(prefabNombreCocina, contenedorPrefab.transform);

                // Obtener el text dentro del prefab instanciado
                TMP_Text texto = instancia.GetComponentInChildren<TMP_Text>();
                texto.text = "Nombre cocina 1:";

                // Obtener el botón de eliminar
                Button botonEliminar = instancia.GetComponentInChildren<Button>();
                if (botonEliminar != null)
                {
                    // IMPORTANTE: Capturar la referencia actual a 'instancia' usando una variable local
                    GameObject instanciaActual = instancia;
                    botonEliminar.onClick.AddListener(() => EliminarCocina(instanciaActual));
                }
                
                // Añadir la instancia a la lista para poder referenciarla luego
                instanciasCocinas.Add(instancia);
            }
            // Instanciar el prefab adicional debajo de los prefabs de cocina
            GameObject instanciaMasCocina = Instantiate(prefabMasCocina, contenedorPrefab.transform);

            // Obtener el botón dentro de prefabMasCocina y asignar la función al evento onClick
            Button buttonMasCocina = instanciaMasCocina.GetComponentInChildren<Button>();
            if (buttonMasCocina != null)
            {
                // Asignar la función OnMasCocinaClick para que se ejecute cuando se haga clic en el botón
                buttonMasCocina.onClick.AddListener(() => OnMasCocinaClick(instanciaMasCocina));
            }

            // Puedes añadir el prefab de "Añadir más cocinas" a la lista de instancias también si lo necesitas
            instanciasCocinas.Add(instanciaMasCocina);
        }
        else
        {
            // Cuando el Toggle se apaga, desactivamos o destruimos las instancias
            foreach (var instancia in instanciasCocinas)
            {
                // Desactivamos las instancias
                instancia.SetActive(false);
                // Si quieres destruirlas en lugar de desactivarlas, usa:
                // Destroy(instancia);
            }

            // Opcionalmente, puedes vaciar la lista si ya no necesitas hacer nada con las instancias
            instanciasCocinas.Clear();
        }
    }

    void OnMasCocinaClick(GameObject instanciaMasCocina)
    {
        // Instanciar un nuevo prefab de cocina debajo del último
        GameObject nuevaCocina = Instantiate(prefabNombreCocina, contenedorPrefab.transform);

        // Cambiamos el texto
        TMP_Text texto = nuevaCocina.GetComponentInChildren<TMP_Text>(); 
        int nuevaCocinaIndex = instanciasCocinas.Count + 1; // índice para la nueva cocina
        texto.text = "Nombre cocina " + nuevaCocinaIndex + ":";

        // Asignar función al botón de eliminar
        Button botonEliminar = nuevaCocina.GetComponentInChildren<Button>();
        if (botonEliminar != null)
        {
            GameObject instanciaActual = nuevaCocina;
            botonEliminar.onClick.AddListener(() => EliminarCocina(instanciaActual));
        }

        // Añadir la nueva instancia a la lista de cocinas
        instanciasCocinas.Add(nuevaCocina);

        // Mover prefabMasCocina al final de la jerarquía (debajo de todos los otros prefabs)
        instanciaMasCocina.transform.SetAsLastSibling();
    }

    void EliminarCocina(GameObject cocinaGO)
    {
        instanciasCocinas.Remove(cocinaGO);  // Quitar de la lista si estás haciendo seguimiento
        Destroy(cocinaGO);                   // Destruir el GameObject del prefab
    }
}
