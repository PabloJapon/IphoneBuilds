using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ReservasBadgeManager : MonoBehaviour
{
    public static ReservasBadgeManager instance;

    [SerializeField] private string apiBase = "https://tu-api.com";
    private const float IntervaloRefresco = 20f;

    public static event Action<int> OnPendientesCountChanged;

    private int ultimoConteo = -1;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(PollLoop());
    }

    IEnumerator PollLoop()
    {
        while (true)
        {
            _ = RefrescarConteo();
            yield return new WaitForSeconds(IntervaloRefresco);
        }
    }

    public void ForzarRefresco()
    {
        _ = RefrescarConteo();
    }

    public int ObtenerUltimoConteo() => ultimoConteo;

    private string ObtenerRestaurantId()
    {
        GameObject go = GameObject.FindGameObjectWithTag("textID");
        if (go == null)
        {
            return null;
        }
        var text = go.GetComponent<TMPro.TMP_Text>();
        string val = text != null ? text.text : null;
        return val;
    }

    private async Task RefrescarConteo()
    {
        string restId = ObtenerRestaurantId();
        if (string.IsNullOrEmpty(restId))
        {
            return;
        }

        string url = $"{apiBase}/reservas/pendientes?restaurant_id={restId}";

        using var req = UnityWebRequest.Get(url);
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            return;
        }

        var data = JsonUtility.FromJson<ReservasResponse>(req.downloadHandler.text);
        int conteo = data.reservas != null ? data.reservas.Length : 0;

        ultimoConteo = conteo;
        OnPendientesCountChanged?.Invoke(conteo);
    }
}