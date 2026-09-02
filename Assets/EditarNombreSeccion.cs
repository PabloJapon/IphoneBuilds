using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Globalization;
using System;
using System.Collections.Generic;

public class EditarNombreSeccion : MonoBehaviour
{
    private GameObject CanvasRellenarSeccion;
    public EditarMenu editarMenu;
    private Button saveButton;
    private Button deleteButton;

    private const string ImageUrl = "https://drive.google.com/uc?id=19iw_RXpnG_dS7gM-eZYWiOuKA66XZfc9";

    void Start()
    {
        // Get the parent GameObject three levels up in the hierarchy
        Transform canvasMenuTransform = transform.parent.parent.parent.parent.parent;

        // Ensure we are getting the last child of the parent canvas correctly
        if (canvasMenuTransform != null)
        {
            // Access CanvasRellenarSeccion
            Transform lastChild = canvasMenuTransform.GetChild(canvasMenuTransform.childCount - 2);
            CanvasRellenarSeccion = lastChild.gameObject;

            saveButton = CanvasRellenarSeccion.GetComponentsInChildren<Button>()[0];
            deleteButton = CanvasRellenarSeccion.GetComponentsInChildren<Button>()[1];
        }
        else
        {
            Debug.LogError("Canvas Menu Transform is null. Check the hierarchy.");
        }

        editarMenu = GameObject.FindGameObjectWithTag("dataBase").GetComponent<EditarMenu>();
    }

    public void ChangeSeccion()
    {
        // Activate the canvas
        CanvasRellenarSeccion.SetActive(true);
        
        // Set the input field's text to the current text of the parent TMP_Text
        TMP_InputField inputField = CanvasRellenarSeccion.GetComponentInChildren<TMP_InputField>();
        TMP_Text parentText = gameObject.GetComponentInParent<TMP_Text>();
        
        inputField.text = parentText.text;

        // Clear previous listeners
        saveButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        // Add listeners to the buttons
        saveButton.onClick.AddListener(SaveSeccion);
        deleteButton.onClick.AddListener(DeleteSeccion);
    }

    public void SaveSeccion()
    {
        TMP_InputField inputField = CanvasRellenarSeccion.GetComponentInChildren<TMP_InputField>();
        TMP_Text parentText = gameObject.GetComponentInParent<TMP_Text>();

        // Save the input field text back to the parent TMP_Text
        parentText.text = inputField.text;

        // change sections in platos
        GetSiblingChildren(parentText.text);

        // Deactivate the canvas
        CanvasRellenarSeccion.SetActive(false);
    }

    public void GetSiblingChildren(string parentText)
    {
        // Get the parent of the current GameObject
        Transform parent = transform.parent;

        // Ensure the parent exists
        if (parent != null)
        {
            // Get the index of the parent in its parent's hierarchy
            int currentParentIndex = parent.GetSiblingIndex();

            // Get the grandparent of the current GameObject
            Transform grandparent = parent.parent;

            // Ensure the grandparent exists
            if (grandparent != null)
            {
                // Calculate the index of the parent's sibling
                int siblingIndex = (currentParentIndex + 1) % grandparent.childCount;

                // Get the sibling Transform
                Transform sibling = grandparent.GetChild(siblingIndex);

                // Access the children of the sibling, excluding the last child
                int childCount = sibling.childCount;

                // Loop through all but the last child
                for (int i = 0; i < childCount - 1; i++)
                {
                    Transform child = sibling.GetChild(i);
                    Debug.Log("Sibling Child (excluding last): " + child.name);

                    var texts = child.GetComponentsInChildren<TMP_Text>();
                    texts[4].text = parentText;
                    Debug.Log(texts[4].text);

                    var toggles = child.GetComponentsInChildren<Toggle>();
                    int toggle;
                    if (toggles[0].isOn = true)
                    {
                        toggle=1;
                    }
                    else
                    {
                        toggle=0;
                    }

                    // Trim whitespace and replace currency symbol
                    string rawValue = texts[2].text.Replace("€", "").Trim();

                    // Check for empty strings
                    if (string.IsNullOrEmpty(rawValue))
                    {
                        Debug.LogWarning("Input string is empty or null.");
                        continue; // Skip to the next iteration if it's empty
                    }

                    // Creo variable para alergenos

                    int[] alergenos = new int[14];
                    for (int j=0; j<13; j++)
                    {
                        if (!int.TryParse(texts[j+6].text, out alergenos[j]))
                        {
                            alergenos[j] = 0; // Valor por defecto si la conversión falla
                        }
                    }

                    int veg = int.Parse(texts[20].text);
                    string optiongroups = texts[21].text;
                    int destino = int.Parse(texts[22].text);

                    // Use float.TryParse to safely parse the float
                    if (float.TryParse(rawValue, NumberStyles.Currency, CultureInfo.CurrentCulture, out float price))
                    {
                        // aqui antes mandaba toggle, pero creo que no tocaba y lo quité LO HE COMENTADO
                        //StartCoroutine(editarMenu.UpdateMenuData(texts[0].text, texts[0].text, texts[1].text, price, ImageUrl, texts[4].text, alergenos[0], alergenos[1], alergenos[2], alergenos[3], alergenos[4], alergenos[5], alergenos[6], alergenos[7], alergenos[8], alergenos[9], alergenos[10], alergenos[11], alergenos[12], alergenos[13], veg, optiongroups, destino));
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to parse float from: {rawValue}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Grandparent is not found.");
            }
        }
        else
        {
            Debug.LogWarning("Parent is not found.");
        }
    }


    public void DeleteSeccion()
    {
        // Clear save button listeners
        saveButton.onClick.RemoveAllListeners();

        // Deactivate the canvas
        CanvasRellenarSeccion.SetActive(false);
    }

}
