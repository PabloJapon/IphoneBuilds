using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Prueba : MonoBehaviour
{
    public bool plato1;
    public bool plato2;
    public bool plato3;
    public bool plato4;
    public bool plato5;

    public GameObject[] espacios;
    public TMP_Text[] textEspacios;

    private void Start()
    {
        foreach (var espacio in espacios)
        {
            espacio.SetActive(false);
        }
    }

    private void SetEspacio(int index, string dishName)
    {
        if (!espacios[index].activeSelf)
        {
            espacios[index].SetActive(true);
            textEspacios[index].text = dishName;
        }
        else
        {
            Debug.LogError("Space " + (index + 1) + " is already occupied");
        }
    }

    private bool IsPlatoSelected(int platoNumber)
    {
        switch (platoNumber)
        {
            case 1:
                return plato1;
            case 2:
                return plato2;
            case 3:
                return plato3;
            case 4:
                return plato4;
            case 5:
                return plato5;
            default:
                Debug.LogError("Invalid plato number");
                return false;
        }
    }

    public void SelectPlato(int platoNumber)
    {
        if (!IsPlatoSelected(platoNumber))
        {
            // Find the first available space for the selected dish
            for (int i = 0; i < espacios.Length; i++)
            {
                if (!espacios[i].activeSelf)
                {
                    SetEspacio(i, "PLATO " + platoNumber);
                    // Set the corresponding plato variable to true
                    switch (platoNumber)
                    {
                        case 1:
                            plato1 = true;
                            break;
                        case 2:
                            plato2 = true;
                            break;
                        case 3:
                            plato3 = true;
                            break;
                        case 4:
                            plato4 = true;
                            break;
                        case 5:
                            plato5 = true;
                            break;
                    }
                    return; // Exit the loop if a space is found
                }
            }

            Debug.LogError("All spaces are occupied");
        }
        else
        {
            Debug.LogError("PLATO " + platoNumber + " is already selected");
        }
    }
}
