using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class TranslationManager : MonoBehaviour
{
    public string libreTranslateEndpoint = "https://libretranslate.com/translate";
    public string sourceLanguage = "auto"; // auto-detect the source language
    public string targetLanguage = "en";   // target language for translation

    public void TranslateText(string sourceText, Action<string> callback)
    {
        StartCoroutine(SendTranslationRequest(sourceText, callback));
    }

    IEnumerator SendTranslationRequest(string sourceText, Action<string> callback)
    {
        // Construct the translation request body
        TranslationRequest request = new TranslationRequest
        {
            q = sourceText,
            source = sourceLanguage,
            target = targetLanguage,
            format = "text",
            api_key = "" // Optional: You can include an API key if required
        };

        // Convert the request object to JSON
        string jsonRequestBody = JsonUtility.ToJson(request);

        // Log the JSON request body for debugging
        Debug.Log("JSON Request Body: " + jsonRequestBody);

        // Create a UnityWebRequest object for making the translation request
        using (UnityWebRequest webRequest = new UnityWebRequest(libreTranslateEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the translation request and wait for the response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Translation request error: " + webRequest.error);
                callback?.Invoke(null);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("JSON Response Body: " + jsonResponse);
                TranslationResponse response = JsonUtility.FromJson<TranslationResponse>(jsonResponse);

                // Get the translated text from the response
                string translatedText = response.translatedText;

                // Invoke the callback with the translated text
                callback?.Invoke(translatedText);
            }
        }
    }
}

[System.Serializable]
public class TranslationRequest
{
    public string q;
    public string source;
    public string target;
    public string format;
    public string api_key;
}

[System.Serializable]
public class TranslationResponse
{
    public DetectedLanguage detectedLanguage;
    public string translatedText;
}

[System.Serializable]
public class DetectedLanguage
{
    public int confidence;
    public string language;
}
