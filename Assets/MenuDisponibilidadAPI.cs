using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class MenuDisponibilidadAPI
{
    public static async Task ActualizarDisponibilidad(string baseUrl, int platoIndex, bool nuevoValor)
    {
        // Construimos el payload completo, reutilizando los datos ya cargados en DataBase
        var body = new MenuUpdatePayload
        {
            id = DataBase.id[platoIndex],
            menuNumber = DataBase.numeroMenu[platoIndex],
            name = DataBase.nombrePlatos[platoIndex],      // usado por el backend para el WHERE
            new_name = DataBase.nombrePlatos[platoIndex],   // no cambia el nombre
            description = DataBase.descripcionPlatos[platoIndex],
            price = DataBase.precioPlatos[platoIndex],
            imageUrl = DataBase.imageUrls[platoIndex],
            seccion = DataBase.seccion[platoIndex],
            alerg1 = DataBase.alergs1[platoIndex],
            alerg2 = DataBase.alergs2[platoIndex],
            alerg3 = DataBase.alergs3[platoIndex],
            alerg4 = DataBase.alergs4[platoIndex],
            alerg5 = DataBase.alergs5[platoIndex],
            alerg6 = DataBase.alergs6[platoIndex],
            alerg7 = DataBase.alergs7[platoIndex],
            alerg8 = DataBase.alergs8[platoIndex],
            alerg9 = DataBase.alergs9[platoIndex],
            alerg10 = DataBase.alergs10[platoIndex],
            alerg11 = DataBase.alergs11[platoIndex],
            alerg12 = DataBase.alergs12[platoIndex],
            alerg13 = DataBase.alergs13[platoIndex],
            alerg14 = DataBase.alergs14[platoIndex],
            veg = DataBase.vegs[platoIndex],
            optionGroups = DataBase.optionGroups[platoIndex],
            toggle = DataBase.toggle[platoIndex],
            disponible = nuevoValor ? 1 : 0
        };

        string json = JsonConvert.SerializeObject(body);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        string itemUrl = $"{baseUrl}/menu/update";
        UnityWebRequest request = new UnityWebRequest(itemUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Error actualizando disponibilidad de {body.name}: {request.error}");
        }
        else
        {
            // Éxito: actualizamos también el array local para que quede consistente
            DataBase.NotificarCambioDisponibilidad(platoIndex, nuevoValor);
        }
    }
}

[Serializable]
public class MenuUpdatePayload
{
    public string id;
    public int menuNumber;
    public string name;
    public string new_name;
    public string description;
    public float price;
    public string imageUrl;
    public string seccion;
    public int alerg1, alerg2, alerg3, alerg4, alerg5, alerg6, alerg7;
    public int alerg8, alerg9, alerg10, alerg11, alerg12, alerg13, alerg14;
    public int veg;
    public string optionGroups;
    public int toggle;
    public int disponible;
}