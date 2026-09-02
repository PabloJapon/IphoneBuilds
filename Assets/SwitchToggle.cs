using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using DG.Tweening;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] RectTransform uiHandleRectTransform;
    [SerializeField] Color backgroundActiveColor;
    [SerializeField] Color handleActiveColor;

    Image backgroundImage, handleImage;

    Color backgroundDefaultColor, handleDefaultColor;

    Toggle toggle;

    Vector2 handlePosition;

    public bool switchQR;
    public GameObject hadleCocina;
    public GameObject hadleCamarero;

    public string url;

    private bool isInitialized = false;  // Track if the toggle has been initialized
    private bool isUpdateActive = true;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        handlePosition = uiHandleRectTransform.anchoredPosition;

        backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
        handleImage = uiHandleRectTransform.GetComponent<Image>();

        backgroundDefaultColor = backgroundImage.color;
        handleDefaultColor = handleImage.color;

        toggle.onValueChanged.AddListener(OnSwitch);
    }

    private void Update()
    {
        if (isUpdateActive && EditarMenu.menuReady)
        {
            isInitialized = true;
            // Disable further updates
            isUpdateActive = false;
        }
    }

    void OnSwitch(bool on)
    {
        Color newColorToggle;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_botones[0], out newColorToggle))
        {
            backgroundActiveColor = newColorToggle;

            // Derive handle color: darker and more saturated than the background color
            Color.RGBToHSV(newColorToggle, out float h, out float s, out float v);
            s = Mathf.Clamp01(s * 1.25f);
            v = Mathf.Clamp01(v * 0.75f);
            handleActiveColor = Color.HSVToRGB(h, s, v);
            handleActiveColor.a = newColorToggle.a;
        }

        // Animate handle position
        uiHandleRectTransform.DOAnchorPos(on ? handlePosition * -1 : handlePosition, .4f).SetEase(Ease.InOutBack);

        // Animate background color
        backgroundImage.DOColor(on ? backgroundActiveColor : backgroundDefaultColor, .6f);

        // Animate handle color
        handleImage.DOColor(on ? handleActiveColor : handleDefaultColor, .4f);

        // Toggle game objects based on the state
        if (switchQR) // QR mensaje
        {
            // QR code logic goes here if needed
        }
        else // Espacio menu
        {
            hadleCamarero.SetActive(on);
            hadleCocina.SetActive(!on);

            // Send data to the database, always, but only after initialization
            if (isInitialized)
            {
                // Find the correct sibling text component
                GameObject goParent = transform.parent.gameObject;
                TMP_Text textComponent = goParent.GetComponentInChildren<TMP_Text>();
                string name = textComponent ? textComponent.text : "Unknown";  // Avoid null reference

                Debug.Log($"Sending data to server. Name: {name}, Toggle state: {on}");

                // Send the data to the server
                StartCoroutine(UpdateToggleState(on, name));
            }
            else
            {
            }
        }
    }

    public IEnumerator UpdateToggleState(bool state, string name)
    {
        var id = LoginManagerResponsable.restaurantID;
        // int toggleState;
        // if (!int.TryParse(state.ToString().ToLower(), out toggleState))
        // {
        //     toggleState = 0; // Valor por defecto si la conversión falla
        // }

        string jsonData = $"{{\"id\":\"{id}\",\"toggleState\":{state.ToString().ToLower()}, \"name\":\"{name}\"}}";
        //string jsonData = $"{{\"id\":\"{id}\",\"toggleState\":{toggleState}, \"name\":\"{name}\"}}";

        Debug.Log($"UpdateToggleState called. ID: {id}, ToggleState: {state}, Name: {name}, JSON: {jsonData}");

        yield return SendRequest("/updateToggle", jsonData);
    }

    private IEnumerator SendRequest(string endpoint, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Sending request to: {url + endpoint}, JSON: {jsonData}");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Request failed: {request.error}");
        }
        else
        {
            Debug.Log($"Response: {request.downloadHandler.text}");
        }
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
        /*Debug.Log("SwitchToggle listener removed.");*/
    }
}