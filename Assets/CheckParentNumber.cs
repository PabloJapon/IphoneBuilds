using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.SceneManagement;

public class CheckParentNumber : MonoBehaviour
{
    private string spritePath = "Sprites/Sprite";

    private int dishNumber;

    void Start()
    {
        // Add a listener to the button's onClick event
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickButton);
        }
        else
        {
            Debug.LogWarning("Button component not found on the GameObject.");
        }
    }

    // Function to be called when the button is clicked
    void OnClickButton()
    {
        bool esTPV = SceneManager.GetActiveScene().name == "TPVScene";
        if (esTPV) DetallePlatoUI.Instance.click();
        else DetallePlato.Instance.click();

        // Extract the number from the text of the TMP_Text component of the clicked GameObject
        TMP_Text[] textComponents = transform.GetComponentsInChildren<TMP_Text>();
        if (textComponents.Length > 3)
        {
            if (int.TryParse(textComponents[3].text, out dishNumber))
            {
                if (esTPV) DetallePlatoUI.Instance.seleccionPlato(dishNumber + 1);
                else DetallePlato.Instance.seleccionPlato(dishNumber + 1);
            }
            else
            {
                Debug.LogWarning("Could not extract dish number from TMP_Text component.");
            }
        }
        else
        {
            Debug.LogWarning("TMP_Text component not found or not enough TMP_Text components.");
        }

        CambiarCantidad.Instance.OnClickDetalle();
        if (esTPV) DetallePlatoUI.Instance.precioPlato();
        else DetallePlato.Instance.precioPlato();
    }
}
