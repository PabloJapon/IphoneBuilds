using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CallPopupController : MonoBehaviour
{
    public static CallPopupController instance;
    private static readonly Queue<string> earlyCalls = new Queue<string>();

    public static void NotifyIncomingCall(string numero)
    {
        if (instance != null)
            instance.ShowIncomingCall(numero);
        else
            earlyCalls.Enqueue(numero);
    }

    [Tooltip("Tiempo maximo de seguridad si nunca llega la confirmacion de colgado (fallback)")]
    public float autoDismissSeconds = 60f;

    private UIDocument uiDocument;
    private VisualElement popupRoot;
    private VisualElement panel;
    private VisualElement icon;
    private VisualElement progressFill;
    private Label numberLabel;
    private Label customerNameLabel;
    private Label queueBadge;
    private Button dismissButton;

    private readonly Queue<string> pendingCalls = new Queue<string>();
    private bool isShowing = false;
    private Coroutine autoDismissRoutine;
    private string currentNumber = null;

    // Fallback si aun no hay color de marca cargado (mismo patron que DetallePlatoUI)
    private Color brandColor = new Color(0.20f, 0.78f, 0.35f);

    void Awake()
    {
        instance = this;
        uiDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        while (earlyCalls.Count > 0)
            ShowIncomingCall(earlyCalls.Dequeue());
        popupRoot = root.Q<VisualElement>("call-popup-root");
        panel = root.Q<VisualElement>("call-popup-panel");
        icon = root.Q<VisualElement>("call-popup-icon");
        numberLabel = root.Q<Label>("call-popup-number");
        customerNameLabel = root.Q<Label>("call-popup-customer-name");
        queueBadge = root.Q<Label>("call-popup-queue-badge");
        dismissButton = root.Q<Button>("call-popup-dismiss");
        progressFill = root.Q<VisualElement>("call-popup-progress-fill");

        dismissButton.clicked += OnDismissClicked;

        popupRoot.style.display = DisplayStyle.None;
        popupRoot.RemoveFromClassList("visible");
        queueBadge.style.display = DisplayStyle.None;

        ApplyBrandColor();
    }

    void OnDisable()
    {
        if (dismissButton != null)
            dismissButton.clicked -= OnDismissClicked;

        if (autoDismissRoutine != null)
        {
            StopCoroutine(autoDismissRoutine);
            autoDismissRoutine = null;
        }
    }

    private void ApplyBrandColor()
    {
        Color parsed;
        if (DataBasePersonalizacion.col_ppal_empl != null && DataBasePersonalizacion.col_ppal_empl.Length > 0 &&
            ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_ppal_empl[0], out parsed))
        {
            brandColor = parsed;
        }

        if (panel != null) panel.style.borderLeftColor = brandColor;
        if (icon != null) icon.style.backgroundColor = brandColor;
        if (dismissButton != null) dismissButton.style.backgroundColor = brandColor;
        if (progressFill != null) progressFill.style.backgroundColor = brandColor;
    }

    // Llamar a esto desde WebOrderBridge cuando llegue una llamada nueva
    public void ShowIncomingCall(string numero)
    {
        pendingCalls.Enqueue(numero);
        UpdateQueueBadge();

        if (!isShowing)
            DisplayNext();
    }

    // Llamar a esto cuando el AMI confirme que la llamada dejo de sonar
    public static void NotifyCallEnded(string numero)
    {
        if (instance != null)
            instance.HideCall(numero);
    }

    private void HideCall(string numero)
    {
        if (isShowing && currentNumber == numero)
        {
            OnDismissClicked();
            return;
        }

        var remaining = new Queue<string>();
        bool removed = false;
        foreach (var n in pendingCalls)
        {
            if (!removed && n == numero) { removed = true; continue; }
            remaining.Enqueue(n);
        }
        pendingCalls.Clear();
        foreach (var n in remaining) pendingCalls.Enqueue(n);
        UpdateQueueBadge();
    }

    void DisplayNext()
    {
        if (pendingCalls.Count == 0)
        {
            isShowing = false;
            currentNumber = null;
            popupRoot.RemoveFromClassList("visible");
            return;
        }

        isShowing = true;
        currentNumber = pendingCalls.Dequeue();
        numberLabel.text = currentNumber;

        Debug.Log($"[CallPopupController] numero='{currentNumber}' | instance null? {TPV_DataManager.instance == null} | label null? {customerNameLabel == null} | customers count: {TPV_DataManager.instance?.tpvData.customers.Count ?? -1}");
        var customer = TPV_DataManager.instance?.tpvData.customers.Find(c => c.phoneNumber == currentNumber);
        Debug.Log($"[CallPopupController] match found? {customer != null}");
        if (customerNameLabel != null)
            customerNameLabel.text = customer != null ? customer.name : "Número desconocido";

        UpdateQueueBadge();

        popupRoot.style.display = DisplayStyle.Flex;

        // Se quita "visible" y se vuelve a anadir en el siguiente frame para que
        // la transicion de opacity/translate del USS se dispare siempre, incluso
        // cuando se encadenan llamadas una detras de otra.
        popupRoot.RemoveFromClassList("visible");
        popupRoot.schedule.Execute(() => popupRoot.AddToClassList("visible")).StartingIn(0);

        if (autoDismissRoutine != null)
            StopCoroutine(autoDismissRoutine);
        autoDismissRoutine = StartCoroutine(AutoDismissWithProgress());
    }

    void UpdateQueueBadge()
    {
        if (queueBadge == null) return;

        int extra = pendingCalls.Count;
        if (isShowing && extra > 0)
        {
            queueBadge.text = "+" + extra;
            queueBadge.style.display = DisplayStyle.Flex;
        }
        else
        {
            queueBadge.style.display = DisplayStyle.None;
        }
    }

    IEnumerator AutoDismissWithProgress()
    {
        float elapsed = 0f;
        if (progressFill != null)
            progressFill.style.width = new StyleLength(new Length(100, LengthUnit.Percent));

        while (elapsed < autoDismissSeconds)
        {
            elapsed += Time.deltaTime;
            if (progressFill != null)
            {
                float pct = Mathf.Clamp01(1f - elapsed / autoDismissSeconds) * 100f;
                progressFill.style.width = new StyleLength(new Length(pct, LengthUnit.Percent));
            }
            yield return null;
        }

        OnDismissClicked();
    }

    void OnDismissClicked()
    {
        if (autoDismissRoutine != null)
        {
            StopCoroutine(autoDismissRoutine);
            autoDismissRoutine = null;
        }

        popupRoot.RemoveFromClassList("visible");

        // Espera a que termine la transicion de salida antes de ocultar y
        // encadenar la siguiente llamada pendiente, si la hay.
        popupRoot.schedule.Execute(() =>
        {
            popupRoot.style.display = DisplayStyle.None;
            DisplayNext();
        }).StartingIn(200);
    }
}