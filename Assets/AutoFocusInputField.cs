using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AutoFocusInputField : MonoBehaviour
{
    public TMP_InputField inputField;

    void OnEnable()
    {
        // Delay the selection so Unity UI system can catch up
        StartCoroutine(SetFocus());
    }

    System.Collections.IEnumerator SetFocus()
    {
        // Wait for end of frame to make sure the UI is ready
        yield return new WaitForEndOfFrame();

        // Set this input field as the selected UI element
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);

        // Activate the input field so it receives input immediately
        inputField.ActivateInputField();
    }
}
