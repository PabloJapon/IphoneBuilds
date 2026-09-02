using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json; // Ensure you import Newtonsoft.Json for better JSON parsing

public class LoginManagerResponsable : MonoBehaviour
{
    private string functionURL = "https://gastrali.netlify.app/.netlify/functions/verificar-sesion";
    
    public TMP_Text userText;
    public TMP_Text planText;
    public TMP_Text letterText;
    public TMP_Text userText2;
    public TMP_Text planText2;
    public static string restaurantID;
    public GameObject canvasInicioSesion;
    public GameObject panelErrorConexion;

    void Start()
    {
        canvasInicioSesion.SetActive(true);
    }

    public void OpenVerifyAccountURL()
    {
        Application.OpenURL("https://gastrali.com/verifyaccount/");
        StartCoroutine(CheckLoginCoroutine());
    }

    IEnumerator CheckLoginCoroutine()
    {
        // Polling configuration
        float pollingInterval = 1f; // Wait for 1 seconds between requests
        int maxAttempts = 10;       // Set a max limit for polling attempts (e.g., 10 attempts)
        int attempts = 0;           // Counter for how many attempts we've made

        while (attempts < maxAttempts)
        {
            UnityWebRequest request = UnityWebRequest.Get(functionURL);
            yield return request.SendWebRequest();

            // Use isNetworkError and isHttpError for versions of Unity older than 2020.1
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Error: " + request.error);
                yield break; // Exit the coroutine if there is an error
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                //Debug.Log("Raw JSON response: " + responseJson);

                // Parse the JSON response using Newtonsoft.Json
                var responseData = JsonConvert.DeserializeObject<LoginResponse>(responseJson);

                // Check if we have valid data in the response
                if (responseData != null && responseData.inputData != null && !string.IsNullOrEmpty(responseData.inputData.message))
                {
                    // Extract the username and plan from the response
                    string username = responseData.inputData.message;
                    string plan = responseData.inputData.subscription_plan;
                    restaurantID = responseData.inputData.id;

                    // Update the UI text
                    userText.text = username;
                    planText.text = "Plan " + plan;
                    letterText.text = username.Substring(0, 1); // Gets the first letter of the username

                    // Lo mismo pero para el Area Personal
                    userText2.text = "Hola " + username + "!";
                    planText2.text = "Plan " + plan;

                    // Response is valid, exit the coroutine
                    break;
                }
                else
                {
                    Debug.Log("Response not ready, retrying...");
                }
            }

            // Increment attempt counter and wait before next request
            attempts++;
            yield return new WaitForSeconds(pollingInterval);
        }
        
        Debug.Log("Response ..." + restaurantID);

        // Hide the login canvas after polling is done (whether success or timeout)
        canvasInicioSesion.SetActive(false);

        // Optional: Handle timeout case
        if (attempts >= maxAttempts)
        {
            Debug.LogError("Max polling attempts reached. Failed to get valid data.");
            panelErrorConexion.SetActive(true);
        }
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string message;
        public UserData inputData;
    }

    [System.Serializable]
    public class UserData
    {
        public string message;
        public string subscription_plan;
        public string id; // Ensure this field is added to capture the restaurant ID
    }
}
