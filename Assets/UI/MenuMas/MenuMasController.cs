using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MenuMasController : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private NavigationBarraTPV navBar; // referencia directa (Opción A)
    [SerializeField] private GameObject pedidosAnteriores;
    [SerializeField] private GameObject reservas;

    private Button btnMas;
    private VisualElement panelDesplegable;
    private VisualElement bloqueadorClickFuera;

    private Button btnFacturas;
    private Button btnGestionCarta;
    private Button btnPanelTurnos;
    private Button btnFichajes;

    [System.Serializable]
    public class BotonPermisoUIT
    {
        public string permisoId;
        public string buttonName; // nombre del Button en el UXML (Q<Button>(name))
    }

    [SerializeField] private List<BotonPermisoUIT> botonesPermisoUIT;

    private bool seleccionado = false;

    void OnEnable()
    {
        var root = document.rootVisualElement;

        root.pickingMode = PickingMode.Ignore;               // ← añadir: el contenedor raíz no bloquea clics
        root.Q<VisualElement>("root-menu-mas").pickingMode = PickingMode.Ignore; // ← añadir también

        btnMas = root.Q<Button>("btn-mas");
        panelDesplegable = root.Q<VisualElement>("panel-desplegable-mas");
        bloqueadorClickFuera = root.Q<VisualElement>("bloqueador-click-fuera");

        btnFacturas = root.Q<Button>("btn-facturas");
        btnGestionCarta = root.Q<Button>("btn-gestion-carta");
        btnPanelTurnos = root.Q<Button>("btn-panel-turnos");
        btnFichajes = root.Q<Button>("btn-fichajes");

        btnMas.clicked += ToggleMenu;
        bloqueadorClickFuera.RegisterCallback<ClickEvent>(evt => CerrarMenu());

        btnFacturas.clicked += () => { navBar.ActivateFacturas(); DesactivarPaneles(); CerrarMenu(); };
        btnGestionCarta.clicked += () => { navBar.ActivateGestionCarta(); DesactivarPaneles(); CerrarMenu(); };
        btnPanelTurnos.clicked += () => { navBar.ActivatePanelTurnos(); DesactivarPaneles(); CerrarMenu(); };
        btnFichajes.clicked += () => { navBar.ActivateFichajes(); DesactivarPaneles(); CerrarMenu(); };

        AplicarPermisosUIT(root);

        // Panel oculto por defecto
        panelDesplegable.style.display = DisplayStyle.None;
        bloqueadorClickFuera.style.display = DisplayStyle.None;

        ResetEstilo();
        AplicarBordePanel();
    }

    void AplicarBordePanel()
    {
        if (panelDesplegable == null) return;   // ← añadir

        Color colorPrincipal = navBar.ColorPrincipalPersonalizacion;

        panelDesplegable.style.borderTopColor = colorPrincipal;
        panelDesplegable.style.borderBottomColor = colorPrincipal;
        panelDesplegable.style.borderLeftColor = colorPrincipal;
        panelDesplegable.style.borderRightColor = colorPrincipal;
    }

    void AplicarPermisosUIT(VisualElement root)
    {
        foreach (var bp in botonesPermisoUIT)
        {
            var btn = root.Q<Button>(bp.buttonName);
            if (btn != null)
                btn.SetEnabled(SesionEmpleado.Permisos.Contains(bp.permisoId));
        }
    }

    void ToggleMenu()
    {
        bool activo = panelDesplegable.style.display == DisplayStyle.None;

        panelDesplegable.style.display = activo ? DisplayStyle.Flex : DisplayStyle.None;
        bloqueadorClickFuera.style.display = activo ? DisplayStyle.Flex : DisplayStyle.None;

        if (activo)
        {
            // aseguramos que el panel esté por encima del bloqueador en el árbol
            bloqueadorClickFuera.BringToFront();
            panelDesplegable.BringToFront();
        }
    }

    void CerrarMenu()
    {
        panelDesplegable.style.display = DisplayStyle.None;
        bloqueadorClickFuera.style.display = DisplayStyle.None;
    }

    void DesactivarPaneles()
    {
        if (pedidosAnteriores != null) pedidosAnteriores.SetActive(false);
        if (reservas != null) reservas.SetActive(false);
    }

    // Llamado desde NavigationBarraTPV cuando el usuario navega a Facturas / Gestión de carta / etc.
    public void SetSeleccionado(bool activo)
    {
        seleccionado = activo;
        AplicarEstilo();
        AplicarBordePanel();
    }

    void ResetEstilo()
    {
        seleccionado = false;
        AplicarEstilo();
    }

    void AplicarEstilo()
    {
        if (btnMas == null) return;   // ← añadir: aún no se ha hecho OnEnable

        Color colorFondoNormal = navBar.ColorPrincipalPersonalizacion;

        Color fondo = seleccionado ? CalcularColorContraste(colorFondoNormal) : colorFondoNormal;
        Color texto = seleccionado ? colorFondoNormal : CalcularColorContraste(colorFondoNormal);

        btnMas.style.backgroundColor = fondo;
        btnMas.style.color = texto;

        btnMas.style.borderTopColor = texto;
        btnMas.style.borderBottomColor = texto;
        btnMas.style.borderLeftColor = texto;
        btnMas.style.borderRightColor = texto;
    }

    // misma fórmula de luminancia que usas en NavigationBarraTPV
    Color CalcularColorContraste(Color fondo)
    {
        float luminance = 0.299f * fondo.r + 0.587f * fondo.g + 0.114f * fondo.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }
}