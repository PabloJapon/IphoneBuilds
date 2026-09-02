using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChangeDetectorPA : MonoBehaviour
{
    public RespDataBasePersonalizacion DB;

    public TMP_InputField inputField1;
    public TMP_InputField inputField2;

    public TMP_Dropdown dropdown1;
    public TMP_Dropdown dropdown2;
    public TMP_Dropdown dropdown3;
    public TMP_Dropdown dropdown4;
    public TMP_Dropdown dropdown5;
    public TMP_Dropdown dropdown6;
    public TMP_Dropdown dropdown7;
    public TMP_Dropdown dropdown8;

    public Button guardarButtonPA;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public Button button7;
    public Button button8;
    public Button button9;
    public Button button10;
    public Button button11;
    public Button button12;
    public Button button13;
    public GameObject panelCocinas;
    public Toggle toggle1; // toggle cocinas

    private bool isUserEditing = false;

    void Start()
    {
        // Subscribe to OnDataLoaded event
        DB.OnDataLoaded += OnDataLoadedHandler;

        // Initially set guardarButtonPA as inactive
        guardarButtonPA.interactable = false;

        // Add listeners for input fields
        inputField1.onSelect.AddListener(delegate { isUserEditing = true; });
        inputField1.onEndEdit.AddListener(OnUserEndsEditing);

        inputField2.onSelect.AddListener(delegate { isUserEditing = true; });
        inputField2.onEndEdit.AddListener(OnUserEndsEditing);

        // Add listeners for dropdowns
        dropdown1.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown2.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown3.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown4.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown5.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown6.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown7.onValueChanged.AddListener(OnDropdownValueChanged);
        dropdown8.onValueChanged.AddListener(OnDropdownValueChanged);

        // Add listeners for buttons
        button1.onClick.AddListener(OnButtonChanged);
        button2.onClick.AddListener(OnButtonChanged);
        button3.onClick.AddListener(OnButtonChanged);
        button4.onClick.AddListener(OnButtonChanged);
        button5.onClick.AddListener(OnButtonChanged);
        button6.onClick.AddListener(OnButtonChanged);
        button7.onClick.AddListener(OnButtonChanged);
        button8.onClick.AddListener(OnButtonChanged);
        button9.onClick.AddListener(OnButtonChanged);
        button10.onClick.AddListener(OnButtonChanged);
        button11.onClick.AddListener(OnButtonChanged);
        button12.onClick.AddListener(OnButtonChanged);
        button13.onClick.AddListener(OnButtonChanged);

        // toggle cocinas
        toggle1.onValueChanged.AddListener(OnToggleChanged);
    
        // Detect changes in panelCocinas
        DB.OnDataLoaded += OnDatabaseLoaded;
        //DetectarConRetraso();
    }
    void OnDestroy()
    {
        DB.OnDataLoaded -= OnDatabaseLoaded;
    }
    private void OnDataLoadedHandler()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0f);
        OnDatabaseLoaded2();
    }

    void OnDatabaseLoaded2()
    {
        // The button starts inactive
        guardarButtonPA.interactable = false;
    }

    // Detect when user finishes editing an input field
    void OnUserEndsEditing(string newText)
    {
        if (isUserEditing)
        {
            guardarButtonPA.interactable = true;
        }
        isUserEditing = false;
    }

    // Detect when user selects a new dropdown option
    void OnDropdownValueChanged(int newValue)
    {
        guardarButtonPA.interactable = true;
    }

    // Detect if any button changes (clicked)
    void OnButtonChanged()
    {
        guardarButtonPA.interactable = true;
    }
    // Coroutine that waits for 5 seconds before detecting changes in panelCocinas
    // IEnumerator DetectarConRetraso()
    // {
    //     // Wait for 5 seconds before proceeding
    //     yield return new WaitForSeconds(5f);
    //     Debug.Log(" ya va a mirar los hijos");

    //     // Once 5 seconds have passed, detect the changes in panelCocinas
    //     DetectarCambiosEnPanelCocinas();
    // }
    private void OnDatabaseLoaded()
    {
    //     Debug.Log(" ya va a mirar los hijos");
        DetectarCambiosEnPanelCocinas();
    }

    void OnToggleChanged(bool isOn)
    {
        guardarButtonPA.interactable = true;
    }

    // Detectar cambios en los hijos del panelCocinas
    void DetectarCambiosEnPanelCocinas()
    {
        // Obtenemos todos los hijos del panelCocinas
        Transform[] hijos = panelCocinas.GetComponentsInChildren<Transform>(true);

        foreach (Transform hijo in hijos)
        {
            // Verificamos si el hijo tiene un TMP_InputField
            TMP_InputField input = hijo.GetComponent<TMP_InputField>();
            if (input != null)
            {
                // Añadimos listeners a cada input field
                input.onValueChanged.AddListener(OnInputFieldChanged);
                input.onEndEdit.AddListener(OnInputFieldChanged);
            }

            // Verificamos si el hijo tiene un Button
            Button button = hijo.GetComponent<Button>();
            if (button != null)
            {
                // Añadimos listeners a cada button
                button.onClick.AddListener(OnButtonChanged);
            }
        }
    }

    // Método que detecta los cambios en cualquier input field
    void OnInputFieldChanged(string newText)
    {
        guardarButtonPA.interactable = true;
    }
}
