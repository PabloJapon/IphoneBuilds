using UnityEngine;
using UnityEngine.UI; // Para manejar imágenes
using System.Collections.Generic; // Esto es necesario para usar Dictionary

public class CombinacionesIconos : MonoBehaviour
{
    // Aquí asignamos los iconos en el inspector de Unity
    public Sprite a1, a2, a3, a4;
    public Sprite b1, b2, b3, b4;
    public Sprite c1, c2, c3, c4;
    public Sprite d1, d2, d3, d4;

    // Asignamos los iconos a los objetos de UI (como Image en un Canvas)
    public Image icon1, icon2, icon3, icon4;

    // Diccionario que almacenará las combinaciones de sprites
    private Dictionary<int, Sprite[]> spriteCombinations;

    void Start()
    {
        // Inicializamos el diccionario con combinaciones
        spriteCombinations = new Dictionary<int, Sprite[]>();

        // Añadimos las combinaciones de sprites con un número
        spriteCombinations.Add(1, new Sprite[] { a1, a2, a3, a4 });
        spriteCombinations.Add(2, new Sprite[] { b1, b2, b3, b4 });
        spriteCombinations.Add(3, new Sprite[] { c1, c2, c3, c4 });
        spriteCombinations.Add(4, new Sprite[] { d1, d2, d3, d4 });

        // Muestra la combinación inicial (por ejemplo la número 1)
        SetIcons(1);
    }

    // Función para cambiar la combinación de iconos en función de un número
    public void SetIcons(int number)
    {
        if (spriteCombinations.ContainsKey(number))
        {
            // Accedemos a la combinación de iconos según el número
            Sprite[] selectedIcons = spriteCombinations[number];

            // Asignamos los sprites a los objetos de la UI
            icon1.sprite = selectedIcons[0];
            icon2.sprite = selectedIcons[1];
            icon3.sprite = selectedIcons[2];
            icon4.sprite = selectedIcons[3];
        }
        else
        {
            Debug.LogWarning("Número fuera de rango. No hay combinación asociada.");
        }
    }
}
