using UnityEngine;
using UnityEngine.UI;

public class DropdownFontChanger : MonoBehaviour
{
    public Dropdown dropdown;
    public Font fontA; // Asignar la fuente para Option A desde el inspector
    public Font fontB; // Asignar la fuente para Option B desde el inspector
    public Font fontC; // Asignar la fuente para Option C desde el inspector

    void Start()
    {
        ChangeDropdownFont();
        dropdown.onValueChanged.AddListener(delegate { ChangeDropdownFont(); });
    }

    void ChangeDropdownFont()
    {
        // Asegurarse de que las opciones estén desplegadas para cambiar las fuentes
        dropdown.Hide();
        dropdown.Show();

        // Iterar sobre las opciones del Dropdown
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            var optionText = dropdown.options[i].text;
            var item = dropdown.transform.GetChild(2).GetChild(i + 1).GetComponent<Text>();

            // Cambiar la tipografía según la opción
            if (optionText == "Option A")
            {
                item.font = fontA;
            }
            else if (optionText == "Option B")
            {
                item.font = fontB;
            }
            else if (optionText == "Option C")
            {
                item.font = fontC;
            }
        }

        // Cambiar la fuente del item seleccionado actualmente
        var selectedItem = dropdown.captionText;
        if (dropdown.value == 0)
        {
            selectedItem.font = fontA;
        }
        else if (dropdown.value == 1)
        {
            selectedItem.font = fontB;
        }
        else if (dropdown.value == 2)
        {
            selectedItem.font = fontC;
        }
    }
}
