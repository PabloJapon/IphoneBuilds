using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RegistrosPedidosToServer : MonoBehaviour
{
    [System.Serializable]
    public class RegistrosPedidos
    {
        public string id;
        public float precio;
        public string mesa;
        public string plato;
        public string n;
        public string empresa_id; // null = mesa/recoger normal
    }

    public void SendDataToServer(string id, float precio, string mesa, string plato, string n, string empresaId = null)
    {
        RegistrosPedidos newRegistro = new RegistrosPedidos
        {
            id = id,
            precio = precio,
            mesa = mesa,
            plato = plato,
            n = n,
            empresa_id = empresaId
        };

        // Runs on a persistent object so this coroutine is NOT killed
        // when the calling dialog's GameObject gets SetActive(false) right after.
        NetworkCoroutineRunner.Instance.StartCoroutine(
            PostRequest("https://gastrali.tail634a78.ts.net/registros_pedidos/add", JsonUtility.ToJson(newRegistro))
        );
    }

    private IEnumerator PostRequest(string url, string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[RegistrosPedidos] Request STARTING for payload: " + jsonData);
            yield return request.SendWebRequest();
            Debug.Log("[RegistrosPedidos] Request FINISHED, code: " + request.responseCode);

            if (request.isNetworkError || request.isHttpError) // Use these properties in Unity 2019
            {
                Debug.LogError($"[RegistrosPedidos] FAILED sending data. Error: {request.error} | Code: {request.responseCode} | Response body: {request.downloadHandler.text} | Payload: {jsonData}");
            }
            else
            {
                Debug.Log($"[RegistrosPedidos] SUCCESS. Code: {request.responseCode} | Response: {request.downloadHandler.text} | Payload: {jsonData}");
            }
        }
    }
}