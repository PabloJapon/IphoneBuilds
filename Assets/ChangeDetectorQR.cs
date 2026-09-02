using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChangeDetectorQR : MonoBehaviour
{
    public RespDataBaseQrs DB;
    public TMP_InputField inputField;
    public TMP_Dropdown dropdown1;
    public TMP_Dropdown dropdown2;
    public TMP_Dropdown dropdown3;
    public Button guardarButtonQR;

    public Button button1;
    public Button button2;

    public Toggle toggle;  // New Toggle reference

    private bool isUserEditing = false;

    void Start()
    {
        // Subscribe to OnDataLoaded event
        DB.OnDataLoaded += OnDataLoadedHandler;

        // Add listeners to the buttons for color change detection
        button1.onClick.AddListener(OnButtonColorChanged);
        button2.onClick.AddListener(OnButtonColorChanged);

        // Add listener for toggle change detection
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDataLoadedHandler()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0f);
        OnDatabaseLoaded();
        guardarButtonQR.interactable = false;
    }

    void OnDatabaseLoaded()
    {
        // Attach listeners for input field and dropdowns
        inputField.onSelect.AddListener(delegate { isUserEditing = true; });
        inputField.onEndEdit.AddListener(OnUserEndsEditing);
        dropdown1.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown2.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown3.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    // Detect when user finishes editing input field
    void OnUserEndsEditing(string newText)
    {
        if (isUserEditing)
        {
            guardarButtonQR.interactable = true;
        }
        isUserEditing = false;
    }

    // Detect when user selects a new dropdown option (for any dropdown)
    void OnDropdownValueChanged(int newValue)
    {
        guardarButtonQR.interactable = true;
    }

    // Detect if any button changes color (clicked)
    void OnButtonColorChanged()
    {
        guardarButtonQR.interactable = true;
    }

    // Detect toggle value change
    void OnToggleValueChanged(bool isOn)
    {
        // If toggle changes its state, enable the save button
        guardarButtonQR.interactable = true;
    }
}
