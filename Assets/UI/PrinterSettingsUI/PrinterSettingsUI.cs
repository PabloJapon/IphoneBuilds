using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class PrinterSettingsUI : MonoBehaviour
{
    public static PrinterSettingsUI Instance { get; private set; }

    private VisualElement overlay;
    private DropdownField dropdownPuerto;
    private VisualElement statusDot;
    private Label labelEstado;
    private Button btnRefrescar;
    private Button btnGuardar;
    private Button btnClose;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        overlay = root.Q<VisualElement>("overlay");
        dropdownPuerto = root.Q<DropdownField>("dropdown-puerto");
        statusDot = root.Q<VisualElement>("status-dot");
        labelEstado = root.Q<Label>("label-estado");
        btnRefrescar = root.Q<Button>("btn-refrescar");
        btnGuardar = root.Q<Button>("btn-guardar");
        btnClose = root.Q<Button>("btn-close");

        btnRefrescar.clicked += RefrescarPuertos;
        btnGuardar.clicked += GuardarPuerto;
        btnClose.clicked += Close;
        overlay.RegisterCallback<PointerDownEvent>(OnOverlayPointerDown);

        overlay.style.display = DisplayStyle.Flex;
        RefrescarPuertos();
    }

    private void OnDisable()
    {
        btnRefrescar.clicked -= RefrescarPuertos;
        btnGuardar.clicked -= GuardarPuerto;
        btnClose.clicked -= Close;
        overlay.UnregisterCallback<PointerDownEvent>(OnOverlayPointerDown);
    }

    private void OnOverlayPointerDown(PointerDownEvent evt)
    {
        if (evt.target == overlay) Close();
    }

    public void Open()
    {
        overlay.style.display = DisplayStyle.Flex;
        RefrescarPuertos();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void RefrescarPuertos()
    {
        if (POSPrinterManager.instance == null)
        {
            SetStatus("POSPrinterManager no disponible.", false);
            return;
        }

        List<string> puertos = POSPrinterManager.instance.GetAvailableSerialPorts();
        dropdownPuerto.choices = puertos;

        if (puertos.Count == 0)
        {
            SetStatus("No se detectó ningún puerto serie.", false);
            return;
        }

        string actual = POSPrinterManager.instance.serialPortName;
        bool configured = !string.IsNullOrEmpty(actual) && puertos.Contains(actual);
        dropdownPuerto.value = configured ? actual : puertos[0];

        SetStatus(configured ? "Impresora configurada en " + actual
                              : puertos.Count + " puerto(s) encontrado(s). Selecciona uno.", configured);
    }

    private void GuardarPuerto()
    {
        if (POSPrinterManager.instance == null || string.IsNullOrEmpty(dropdownPuerto.value))
            return;

        POSPrinterManager.instance.serialPortName = dropdownPuerto.value;
        SetStatus("Guardado: " + dropdownPuerto.value, true);
    }

    private void SetStatus(string text, bool ok)
    {
        labelEstado.text = text;
        statusDot.RemoveFromClassList("status-dot-ok");
        statusDot.RemoveFromClassList("status-dot-warn");
        statusDot.AddToClassList(ok ? "status-dot-ok" : "status-dot-warn");
    }
}