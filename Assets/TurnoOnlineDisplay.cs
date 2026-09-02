using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[Serializable] public class TurnoEmpleado { public string nombre; public string fecha_hora; }
[Serializable] public class TurnoActivosResponse { public TurnoEmpleado[] activos; }

public class TurnoOnlineDisplay : MonoBehaviour
{
    [SerializeField] private string apiBase = "https://gastrali.tail634a78.ts.net";
    [SerializeField] private Transform avatarContainer;
    [SerializeField] private GameObject avatarChipPrefab;
    [SerializeField] private RectTransform logoRect;
    [SerializeField] private CanvasGroup turnoCanvasGroup;
    [SerializeField] private float animDuration = 0.45f;
    [SerializeField] private float chipStaggerDelay = 0.06f;
    [SerializeField] private GameObject canvasIntro;

    private string restId;
    private bool turnoVisible = false;
    private bool introHandled = false;
    private Vector2 logoCenteredPos;
    private Vector2 logoUpPos;
    private Coroutine animCoroutine;

    void Awake()
    {
        logoCenteredPos = logoRect.anchoredPosition;
        logoUpPos = logoCenteredPos + new Vector2(0f, 100f);
    }

    void OnEnable()
    {
        restId = ObtenerRestaurantId();
        ResetToHidden();
        FichajeEvents.OnFichajeRegistrado += HandleFichaje;
        StartCoroutine(FirstEnableOrFetch());
    }

    void OnDisable()
    {
        FichajeEvents.OnFichajeRegistrado -= HandleFichaje;
    }

    void ResetToHidden()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        logoRect.anchoredPosition = logoCenteredPos;
        turnoCanvasGroup.alpha = 0f;
        turnoCanvasGroup.interactable = false;
        turnoCanvasGroup.blocksRaycasts = false;
        turnoVisible = false;
    }

    IEnumerator FirstEnableOrFetch()
    {
        if (!introHandled)
        {
            introHandled = true;
            if (canvasIntro != null)
            {
                yield return new WaitUntil(() => canvasIntro.activeInHierarchy);
                yield return new WaitUntil(() => !canvasIntro.activeInHierarchy);
            }
        }
        yield return FetchTurnoActivos();
    }

    void HandleFichaje()
    {
        StartCoroutine(FetchTurnoActivos());
    }

    string ObtenerRestaurantId()
    {
        GameObject go = GameObject.FindGameObjectWithTag("textID");
        return go != null ? go.GetComponent<TMP_Text>()?.text : null;
    }

    IEnumerator FetchTurnoActivos()
    {
        if (string.IsNullOrEmpty(restId)) yield break;

        using var req = UnityWebRequest.Get($"{apiBase}/turno/activos/{restId}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) yield break;

        var data = JsonUtility.FromJson<TurnoActivosResponse>(req.downloadHandler.text);
        Render(data.activos);
    }

    void Render(TurnoEmpleado[] activos)
    {
        foreach (Transform child in avatarContainer) Destroy(child.gameObject);

        bool hasActivos = activos != null && activos.Length > 0;

        if (activos != null)
        {
            for (int i = 0; i < activos.Length; i++)
            {
                var emp = activos[i];
                var chip = Instantiate(avatarChipPrefab, avatarContainer);
                chip.transform.Find("Circle/Initials").GetComponent<TMP_Text>().text = Iniciales(emp.nombre);
                chip.transform.Find("NameLabel").GetComponent<TMP_Text>().text = emp.nombre.Trim().Split(' ')[0];
                StartCoroutine(AnimateChipIn(chip, i * chipStaggerDelay));
            }
        }

        if (hasActivos != turnoVisible)
        {
            turnoVisible = hasActivos;
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(AnimateTransition(hasActivos));
        }
    }

    IEnumerator AnimateChipIn(GameObject chip, float delay)
    {
        var cg = chip.GetComponent<CanvasGroup>();
        if (cg == null) cg = chip.AddComponent<CanvasGroup>();
        var rt = chip.GetComponent<RectTransform>();

        cg.alpha = 0f;
        if (rt != null) rt.localScale = Vector3.one * 0.85f;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        const float chipDuration = 0.25f;
        while (t < chipDuration)
        {
            t += Time.deltaTime;
            float p = EaseOutBack(Mathf.Clamp01(t / chipDuration));
            cg.alpha = Mathf.Clamp01(p);
            if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, p);
            yield return null;
        }

        cg.alpha = 1f;
        if (rt != null) rt.localScale = Vector3.one;
    }

    IEnumerator AnimateTransition(bool show)
    {
        Vector2 logoFrom = logoRect.anchoredPosition;
        Vector2 logoTo = show ? logoUpPos : logoCenteredPos;
        float alphaFrom = turnoCanvasGroup.alpha;
        float alphaTo = show ? 1f : 0f;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float p = EaseOutBack(Mathf.Clamp01(t / animDuration));
            logoRect.anchoredPosition = Vector2.LerpUnclamped(logoFrom, logoTo, p);
            turnoCanvasGroup.alpha = Mathf.Clamp01(Mathf.LerpUnclamped(alphaFrom, alphaTo, p));
            yield return null;
        }

        logoRect.anchoredPosition = logoTo;
        turnoCanvasGroup.alpha = alphaTo;
        turnoCanvasGroup.interactable = show;
        turnoCanvasGroup.blocksRaycasts = show;
    }

    float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    string Iniciales(string nombre)
    {
        var partes = nombre.Trim().Split(' ');
        string ini = partes[0].Substring(0, 1);
        if (partes.Length > 1) ini += partes[1].Substring(0, 1);
        return ini.ToUpper();
    }
}