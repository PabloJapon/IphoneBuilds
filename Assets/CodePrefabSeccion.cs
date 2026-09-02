using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodePrefabSeccion : MonoBehaviour
{
    // prefab para el botoncito
    public GameObject tabPrefab; // Prefab de la pestaña
    public Transform tabsParent; // Objeto padre donde se colocarán las pestañas
    public Vector3 tabOffset = new Vector3(0f, -425, 0f); // Desplazamiento para la nueva pestaña
    // prefab para el cuadro para meter nuevos platos
    public GameObject tabPrefab2; 
    public Transform tabsParent2; 

    private List<GameObject> tabs = new List<GameObject>(); // Lista para almacenar las pestañas


    public void AddTab()
{
    // Instanciar una nueva pestaña desde el prefab con offset
    GameObject newTab = Instantiate(tabPrefab, tabsParent);

    // Calcular la posición de la nueva pestaña basada en la última pestaña agregada o en la posición inicial si no hay pestañas aún
    Vector3 newPosition;
    if (tabs.Count == 0)
    {
        newPosition = tabPrefab.transform.position + tabOffset;
    }
    else
    {
        Vector3 lastTabPosition = tabs[tabs.Count - 1].transform.position;
        newPosition = lastTabPosition + tabOffset;
    }

    // Asignar la posición calculada a la nueva pestaña
    newTab.transform.position = newPosition;

    // Agregar la pestaña a la lista de pestañas
    tabs.Add(newTab);

    // Instanciar otro prefab sin offset
    GameObject newTab2 = Instantiate(tabPrefab2, tabsParent2);

    // Asignar la posición inicial al segundo prefab
    newTab2.transform.position = tabPrefab2.transform.position;

    // Agregar el segundo prefab a la lista de pestañas
    tabs.Add(newTab2);
}
}