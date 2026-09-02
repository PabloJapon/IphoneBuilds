using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TranslationScript : MonoBehaviour
{
    void Start()
    {
        // Translate the word "Hola" when the script starts
        StartCoroutine(TranslateHello());
    }

    IEnumerator TranslateHello()
    {
        // Construct the translation request body
        string jsonRequestBody = JsonUtility.ToJson(new
        {
            q = "Hola",
            source = "es", // Spanish
            target = "en", // English
            format = "text",
            api_key = "" // Optional: You can include an API key if required
        });

        // Log the JSON request body for debugging
        Debug.Log("JSON Request Body: " + jsonRequestBody);

        // Create a UnityWebRequest object for making the translation request
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm("https://libretranslate.com/translate", jsonRequestBody))
        {
            // Set the content type header
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the translation request and wait for the response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                // Log the error details
                Debug.LogError("Translation request error: " + webRequest.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("JSON Response Body: " + jsonResponse);

                // Log the translated text
                TranslationResponse response = JsonUtility.FromJson<TranslationResponse>(jsonResponse);
                Debug.Log("Translated Text: " + response.translatedText);
            }
        }
    }

    [System.Serializable]
    public class TranslationResponse
    {
        public string translatedText;
    }
}
