using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

// Lee la conexión SSE en crudo, trozo a trozo, y separa los mensajes "data: ...\n\n"
public class SSEDownloadHandler : DownloadHandlerScript
{
    private System.Text.StringBuilder buffer = new System.Text.StringBuilder();
    public Action<string> OnEventData;

    public SSEDownloadHandler() : base(new byte[4096]) { }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0) return false;

        buffer.Append(System.Text.Encoding.UTF8.GetString(data, 0, dataLength));

        string content = buffer.ToString();
        int sepIndex;
        while ((sepIndex = content.IndexOf("\n\n")) >= 0)
        {
            string rawEvent = content.Substring(0, sepIndex);
            content = content.Substring(sepIndex + 2);

            foreach (string line in rawEvent.Split('\n'))
            {
                if (line.StartsWith("data:"))
                    OnEventData?.Invoke(line.Substring(5).Trim());
            }
        }

        buffer.Clear();
        buffer.Append(content);
        return true;
    }
}

public class MenuStreamListener : MonoBehaviour
{
    public DataBase DB;
    private UnityWebRequest currentRequest;
    private bool connectedBefore = false;

    void OnEnable()
    {
        StartCoroutine(ConnectLoop());
    }

    void OnDisable()
    {
        currentRequest?.Abort();
        StopAllCoroutines();
    }

    private IEnumerator ConnectLoop()
    {
        while (DB == null || string.IsNullOrEmpty(DB.restaurantId) || DataBase.itemIds == null)
            yield return null; // esperar a que la DB ya tenga restaurantId y platos cargados

        while (enabled)
        {
            yield return StartCoroutine(ListenStream());
            yield return new WaitForSeconds(2f); // pausa antes de reintentar si se cae
        }
    }

    private IEnumerator ListenStream()
    {
        string streamUrl = $"{DB.url}/menu/stream/{DB.restaurantId}";

        var handler = new SSEDownloadHandler();
        handler.OnEventData = OnStreamMessage;

        currentRequest = new UnityWebRequest(streamUrl, "GET");
        currentRequest.downloadHandler = handler;
        currentRequest.timeout = 0; // conexión larga, sin timeout

        if (connectedBefore)
            StartCoroutine(ResyncDisponibilidad()); // por si se perdió algún aviso mientras estaba desconectado
        connectedBefore = true;

        yield return currentRequest.SendWebRequest();
        // si llega aquí, la conexión se cerró o falló -> ConnectLoop reintentará
    }

    private void OnStreamMessage(string json)
    {
        try
        {
            var update = JsonConvert.DeserializeObject<MenuUpdateMessage>(json);
            AplicarCambio(update.itemId, update.disponible == 1);
        }
        catch
        {
            // mensajes de ping u otros formatos, se ignoran
        }
    }

    private void AplicarCambio(int itemId, bool nuevoValor)
    {
        if (DataBase.itemIds == null) return;

        for (int i = 0; i < DataBase.itemIds.Length; i++)
        {
            if (DataBase.itemIds[i] == itemId)
            {
                DataBase.NotificarCambioDisponibilidad(i, nuevoValor);
                break;
            }
        }
    }

    // Red de seguridad: al reconectar, comprobamos si algo cambió mientras no escuchábamos
    private IEnumerator ResyncDisponibilidad()
    {
        string itemUrl = $"{DB.url}/menu/restaurant/{DB.restaurantId}";
        UnityWebRequest request = UnityWebRequest.Get(itemUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError) yield break;

        List<MenuEntry> entries = DB.ParseMenu(request.downloadHandler.text);
        if (entries == null) yield break;

        foreach (var entry in entries)
        {
            for (int i = 0; i < DataBase.itemIds.Length; i++)
            {
                if (DataBase.itemIds[i] == entry.ItemId)
                {
                    bool nuevoValor = entry.Disponible == 1;
                    if ((DataBase.disponible[i] == 1) != nuevoValor)
                        DataBase.NotificarCambioDisponibilidad(i, nuevoValor);
                    break;
                }
            }
        }
    }
}

[Serializable]
public class MenuUpdateMessage
{
    public int itemId;
    public int disponible;
}