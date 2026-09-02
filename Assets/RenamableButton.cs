using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class RenamableButton : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text buttonText;
    public TMP_InputField inputFieldPrefab;
    private TMP_InputField currentInputField;

    public void Start()
    {
        inputFieldPrefab.gameObject.SetActive(false);

    }

    // Called when the button is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Button right-clicked, renaming...");
            RenameButton();
        }
    }

    // Method to rename the button
    private void RenameButton()
    {
        // Destroy any existing input field
        DestroyInputField();

        // Create a new input field for the user to enter the new name
        currentInputField = Instantiate(inputFieldPrefab, transform);
        currentInputField.transform.SetAsLastSibling(); // Ensure input field is on top
        currentInputField.text = buttonText.text; // Set initial text to current button text

        // Subscribe to the input field's end edit event
        currentInputField.onEndEdit.AddListener(OnEndEdit);

        // Focus on the input field for immediate editing
        currentInputField.Select();
        currentInputField.ActivateInputField();
    }


    // Method to handle end edit event
    private void OnEndEdit(string value)
    {
        // Change the button text to the entered text
        buttonText.text = value;

        //Detect when the Return key is pressed down
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Return key was pressed.");

            // Deactivate the input field
            currentInputField.gameObject.SetActive(false);
            inputFieldPrefab.gameObject.SetActive(false);
        }
    }

    // Method to destroy the current input field
    private void DestroyInputField()
    {
        if (currentInputField != null)
        {
            Destroy(currentInputField.gameObject);
            currentInputField = null;
        }
    }



    // Method to activate the input field
    public void HabilitarInput()
    {
        inputFieldPrefab.gameObject.SetActive(true);
        RenameButton();
    }
}
