using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InvitacionPersonal : MonoBehaviour
{
    public string url; // URL base del servidor, igual que en los demás scripts de login

    public GameObject canvasInvitacion; // Canvas nuevo, Sort Order alto (ej. 100) para quedar por encima de todo
    public TMP_Text textoSaludo;
    public TMP_InputField campoCodigo;
    public Button botonConfirmar;
    public TMP_Text textoError;
    public Navigation NAV;

    private string restaurantIdPendiente;

    void Awake()
    {
        canvasInvitacion.SetActive(false);
        if (textoError != null) textoError.text = "";

        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLink(Application.absoluteURL);
        }
        else
        {
            Application.deepLinkActivated += OnDeepLink;
        }

        botonConfirmar.onClick.AddListener(OnConfirmarClick);
    }

    void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLink;
    }

    private void OnDeepLink(string urlRecibida)
    {
        string codigo = ExtraerCodigoDeUrl(urlRecibida);
        canvasInvitacion.SetActive(true);
        campoCodigo.text = codigo ?? "";
        if (textoError != null) textoError.text = "";
        textoSaludo.text = "Comprobando invitación...";

        if (!string.IsNullOrEmpty(codigo))
        {
            StartCoroutine(ValidarCodigo(codigo));
        }
        else
        {
            textoSaludo.text = "Introduce tu código de invitación";
        }
    }

    private string ExtraerCodigoDeUrl(string urlRecibida)
    {
        if (string.IsNullOrEmpty(urlRecibida)) return null;
        int idx = urlRecibida.IndexOf("code=");
        if (idx < 0) return null;
        string resto = urlRecibida.Substring(idx + 5);
        int amp = resto.IndexOf('&');
        return amp >= 0 ? resto.Substring(0, amp) : resto;
    }

    private IEnumerator ValidarCodigo(string codigo)
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/personal/invitar/validar/" + codigo);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            textoSaludo.text = "Introduce tu código de invitación";
            yield break;
        }

        InvitacionResponse resp = JsonUtility.FromJson<InvitacionResponse>(request.downloadHandler.text);
        if (resp.ok)
        {
            restaurantIdPendiente = resp.restaurant_id;
            textoSaludo.text = string.IsNullOrEmpty(resp.nombre_empleado)
                ? "¿Confirmar registro de este dispositivo?"
                : "Hola, " + resp.nombre_empleado + ". ¿Confirmar registro de este dispositivo?";
        }
        else
        {
            textoSaludo.text = "Introduce tu código de invitación";
            if (textoError != null) textoError.text = MensajeError(resp.error);
        }
    }

    public void OnConfirmarClick()
    {
        string codigo = campoCodigo.text.Trim();
        if (string.IsNullOrEmpty(codigo)) return;
        if (textoError != null) textoError.text = "";
        StartCoroutine(ConfirmarCodigo(codigo));
    }

    private IEnumerator ConfirmarCodigo(string codigo)
    {
        string jsonBody = JsonUtility.ToJson(new ConfirmarInvitacionRequest { codigo = codigo });
        UnityWebRequest request = new UnityWebRequest(url + "/personal/invitar/confirmar", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            if (textoError != null) textoError.text = "Sin conexión. Inténtalo de nuevo.";
            yield break;
        }

        InvitacionResponse resp = JsonUtility.FromJson<InvitacionResponse>(request.downloadHandler.text);
        if (resp.ok)
        {
            canvasInvitacion.SetActive(false);
            NAV.RegistrarDispositivoComoPersonal(resp.restaurant_id);
        }
        else
        {
            if (textoError != null) textoError.text = MensajeError(resp.error);
        }
    }

    private string MensajeError(string error)
    {
        switch (error)
        {
            case "código ya usado": return "Este código ya se ha usado. Pide uno nuevo a tu encargado.";
            case "código caducado": return "Este código ha caducado. Pide uno nuevo a tu encargado.";
            default: return "Código no válido.";
        }
    }
}

[Serializable]
public class InvitacionResponse
{
    public bool ok;
    public string restaurant_id;
    public string nombre_empleado;
    public string error;
}

[Serializable]
public class ConfirmarInvitacionRequest
{
    public string codigo;
}