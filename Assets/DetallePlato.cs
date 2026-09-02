using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

public class DetallePlato : MonoBehaviour
{
    public static DetallePlato Instance { get; private set; }

    public GameObject detallePlato;
    public TMP_Text textDetalle;
    public TMP_Text textDetalleDescripcion;
    public TMP_Text textDetallePrecio;
    public Image image;
    public Image imageDetalle;
    public Sprite simpleSprite;
    public GameObject separador;

    // Option groups
    public Transform contentDetallePlatoX;
    public Transform PanelIzq;
    public GameObject aElegirPrefab;
    public GameObject aElegirPrefabBarra;
    public GameObject toggleElegirOptionPrefab;      // radio
    public GameObject toggleCheckboxOptionPrefab;    // checkbox
    public GameObject imageMarginBottomPrefab;
    private GameObject headerGroup;

    public Button añadirButton;

    public static int xPlato;
    public static float yPlato;

    public AspectFill aspectFillImageDetallePlatoX;

    // Alergenos
    public GameObject[] alergs;
    // Veggie/Vegano
    public GameObject[] vegs;

    public int currentQuantity = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        detallePlato.SetActive(false);

        TMP_Text[] textComponents = detallePlato.GetComponentsInChildren<TMP_Text>();

        if (textComponents.Length >= 2)
        {
            textDetalle = textComponents[0];
            textDetalleDescripcion = textComponents[1];
        }
        else
        {
            Debug.LogError("Not enough TMP_Text components found in children of detallePlato.");
        }

        deactivateAlergenos();
    }

    public void deactivateAlergenos()
    {
        foreach (GameObject alerg in alergs)
            alerg.SetActive(false);
        foreach (GameObject alerg in vegs)
            alerg.SetActive(false);
    }

    public void click()
    {
        detallePlato.SetActive(true);
    }

    public void clickClose()
    {
        deactivateAlergenos();
        currentQuantity = 1;
        detallePlato.SetActive(false);
    }

    public void seleccionPlato(int numeroPlato)
    {
        ClearOptionGroups();
        deactivateAlergenos();

        if (añadirButton != null)
            añadirButton.interactable = false;

        string[] nombres = DataBase.nombrePlatos;
        textDetalle.text = nombres[numeroPlato - 1];

        string[] descripcion = DataBase.descripcionPlatos;
        textDetalleDescripcion.text = descripcion[numeroPlato - 1];

        Sprite[] sprites = DataBase.spritePlatos;
        imageDetalle.sprite = sprites[numeroPlato - 1];

        if (imageDetalle.sprite == null)
        {
            imageDetalle.sprite = simpleSprite;
            imageDetalle.color = image.color;
            if (SceneManager.GetActiveScene().name == "MobileScene")
                separador.SetActive(false);
        }
        else
        {
            imageDetalle.color = Color.white;
            if (SceneManager.GetActiveScene().name == "MobileScene")
                separador.SetActive(true);
        }

        xPlato = numeroPlato;
        StartCoroutine(AdjustCoverNextFrame());

        int[][] allAlergs = new int[][]
        {
            DataBase.alergs1, DataBase.alergs2, DataBase.alergs3, DataBase.alergs4,
            DataBase.alergs5, DataBase.alergs6, DataBase.alergs7, DataBase.alergs8,
            DataBase.alergs9, DataBase.alergs10, DataBase.alergs11, DataBase.alergs12,
            DataBase.alergs13, DataBase.alergs14
        };

        for (int i = 0; i < allAlergs.Length; i++)
        {
            if (allAlergs[i][numeroPlato - 1] == 1)
                alergs[i].SetActive(true);
        }

        if (DataBase.vegs[numeroPlato - 1] == 1)
            vegs[0].SetActive(true);
        else if (DataBase.vegs[numeroPlato - 1] == 2)
            vegs[1].SetActive(true);

        var groups = DataBase.optionGroups[numeroPlato - 1];

        if (!string.IsNullOrWhiteSpace(groups))
        {
            var jsonGroups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OptionGroupData>>(groups);

            foreach (var group in jsonGroups)
                CreateOptionGroup(group.titulo, group.opciones, group.tipo, group.obligatorio);

            RefreshMarginBottomVisibility();
        }
        else
        {
            if (añadirButton != null)
                añadirButton.interactable = true;
        }

        ValidateAllGroupsSelected();

        MenuPedir menuPedir = FindObjectOfType<MenuPedir>();
        if (menuPedir != null)
            menuPedir.platoCount[numeroPlato] = 1;

        precioPlato();
    }

    private IEnumerator AdjustCoverNextFrame()
    {
        yield return null;
        aspectFillImageDetallePlatoX.AdjustToCover();
    }

    public void CreateOptionGroup(string headerText, List<string> options, string tipo = "radio", bool obligatorio = true)
    {
        if (SceneManager.GetActiveScene().name == "MobileScene")
            headerGroup = Instantiate(aElegirPrefab, contentDetallePlatoX.transform);
        else
            headerGroup = Instantiate(aElegirPrefabBarra, PanelIzq.transform);

        TMP_Text[] headerTexts = headerGroup.GetComponentsInChildren<TMP_Text>();
        if (headerTexts.Length >= 1)
            headerTexts[0].text = headerText;

        if (headerTexts.Length >= 2)
        {
            headerTexts[1].text = obligatorio ? "Obligatorio" : "Opcional";
            headerTexts[1].color = obligatorio ? new Color(0.85f, 0.25f, 0.25f) : new Color(0.55f, 0.55f, 0.55f);
        }

        ToggleGroup toggleGroup = headerGroup.GetComponentInChildren<ToggleGroup>();

        OptionGroupMeta meta = headerGroup.AddComponent<OptionGroupMeta>();
        meta.obligatorio = obligatorio;
        meta.tipo = tipo;

        bool isCheckbox = tipo == "checkbox";
        GameObject togglePrefabToUse = isCheckbox ? toggleCheckboxOptionPrefab : toggleElegirOptionPrefab;

        foreach (string option in options)
        {
            GameObject toggleGO = Instantiate(togglePrefabToUse, toggleGroup.transform);
            Toggle toggle = toggleGO.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggle.group = isCheckbox ? null : toggleGroup;
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    ValidateAllGroupsSelected();
                });
            }

            TMP_Text label = toggleGO.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = FormatOptionLabel(option);

            Image[] allImages = toggleGO.GetComponentsInChildren<Image>();
            if (allImages.Length >= 3)
            {
                Image Tick = allImages[2];
                Color newColorBotonSec;
                if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_botones[0], out newColorBotonSec))
                    Tick.color = newColorBotonSec;
            }
        }

        if (SceneManager.GetActiveScene().name == "MobileScene")
            Instantiate(imageMarginBottomPrefab, toggleGroup.transform);
    }

    public void ClearOptionGroups()
    {
        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            foreach (Transform child in contentDetallePlatoX.transform)
            {
                if (child.name == "AElegir(Clone)")
                    Destroy(child.gameObject);
            }
        }
        else
        {
            foreach (Transform child in PanelIzq.transform)
            {
                if (child.name == "AElegirBarra(Clone)")
                    Destroy(child.gameObject);
            }
        }
    }

    private void RefreshMarginBottomVisibility()
    {
        List<Transform> groups = new List<Transform>();

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            foreach (Transform child in contentDetallePlatoX.transform)
            {
                if (child.name == "AElegir(Clone)")
                    groups.Add(child);
            }
        }
        else
        {
            foreach (Transform child in PanelIzq.transform)
            {
                if (child.name == "AElegirBarra(Clone)")
                    groups.Add(child);
            }
        }

        for (int i = 0; i < groups.Count; i++)
        {
            ToggleGroup tg = groups[i].GetComponentInChildren<ToggleGroup>();
            if (tg == null) continue;

            Transform marginBottom = tg.transform.Find("ImageMarginBottom(Clone)");
            if (marginBottom != null)
                marginBottom.gameObject.SetActive(i == groups.Count - 1);
        }
    }

    public void ValidateAllGroupsSelected()
    {
        bool hasMandatory = false;
        bool allSatisfied = true;

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            foreach (Transform group in contentDetallePlatoX)
            {
                OptionGroupMeta meta = group.GetComponent<OptionGroupMeta>();
                if (meta == null || !meta.obligatorio) continue;

                hasMandatory = true;

                bool anySelected = false;
                foreach (Toggle t in group.GetComponentsInChildren<Toggle>())
                {
                    if (t.isOn) { anySelected = true; break; }
                }

                if (!anySelected)
                {
                    allSatisfied = false;
                    break;
                }
            }
        }
        else
        {
            foreach (Transform group in PanelIzq)
            {
                OptionGroupMeta meta = group.GetComponent<OptionGroupMeta>();
                if (meta == null || !meta.obligatorio) continue;

                hasMandatory = true;

                bool anySelected = false;
                foreach (Toggle t in group.GetComponentsInChildren<Toggle>())
                {
                    if (t.isOn) { anySelected = true; break; }
                }

                if (!anySelected)
                {
                    allSatisfied = false;
                    break;
                }
            }
        }

        // If there are no mandatory groups at all, always enable the button
        if (añadirButton != null)
            añadirButton.interactable = !hasMandatory || allSatisfied;

        UpdatePrecioConOpciones();

        if (!hasMandatory || allSatisfied)
            CambiarCantidad.Instance.cantidadDetallePlatoX.text = "1";
    }

    public Dictionary<string, string> GetOptionSelections()
    {
        Dictionary<string, string> selections = new Dictionary<string, string>();

        Transform parentTransform;

        if (SceneManager.GetActiveScene().name == "MobileScene")
        {
            parentTransform = contentDetallePlatoX;
        }
        else
        {
            parentTransform = PanelIzq;
        }

        foreach (Transform group in parentTransform)
        {
            OptionGroupMeta meta = group.GetComponent<OptionGroupMeta>();
            TMP_Text header = group.GetComponentInChildren<TMP_Text>();
            if (header == null || meta == null) continue;

            string groupName = header.text.Replace(":", "").Trim();

            if (meta.tipo == "checkbox")
            {
                List<string> chosen = new List<string>();
                foreach (Toggle t in group.GetComponentsInChildren<Toggle>())
                {
                    if (t.isOn)
                    {
                        TMP_Text lbl = t.GetComponentInChildren<TMP_Text>();
                        if (lbl != null) chosen.Add(lbl.text);
                    }
                }
                for (int idx = 0; idx < chosen.Count; idx++)
                    selections[groupName + "_" + idx] = chosen[idx];
            }
            else
            {
                ToggleGroup toggleGroup = group.GetComponentInChildren<ToggleGroup>();
                if (toggleGroup == null) continue;

                Toggle selected = toggleGroup.ActiveToggles().FirstOrDefault();
                if (selected != null)
                {
                    TMP_Text label = selected.GetComponentInChildren<TMP_Text>();
                    if (label != null)
                        selections[groupName] = label.text;
                }
            }
        }

        return selections;
    }

    public void precioPlato()
    {
        float[] precios = DataBase.precioPlatos;
        float unitPrice = precios[xPlato - 1];
        float finalPrice = unitPrice * currentQuantity;
        textDetallePrecio.text = "Añadir   " + finalPrice.ToString("0.00") + " €";
        yPlato = finalPrice;
    }

    public void UpdatePrecioConOpciones()
    {
        float basePrice = DataBase.precioPlatos[xPlato - 1];
        Dictionary<string, string> selectedOptions = GetOptionSelections();

        float extraTotal = 0f;
        foreach (var pair in selectedOptions)
            extraTotal += ExtractOptionExtraPrice(pair.Value);

        float unitPrice = basePrice + extraTotal;
        float finalPrice = unitPrice * currentQuantity;

        textDetallePrecio.text = "Añadir   " + finalPrice.ToString("0.00") + " €";
        yPlato = finalPrice;
    }

    public float ExtractOptionExtraPrice(string optionValue)
    {
        float total = 0f;
        foreach (Match match in Regex.Matches(optionValue, @"\+(\d+[.,]\d+)"))
        {
            string val = match.Groups[1].Value.Replace(',', '.');
            if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float extra))
                total += extra;
        }
        return total;
    }

    private string FormatOptionLabel(string option)
    {
        Match match = Regex.Match(option, @"^(.+?),\s*(\d+[.,]\d+)$");
        if (match.Success)
        {
            string name = match.Groups[1].Value.Trim();
            string priceStr = match.Groups[2].Value.Replace('.', ',');
            return $"{name} +{priceStr}€";
        }
        return option;
    }
}

[System.Serializable]
public class OptionGroupData
{
    public string titulo;
    public string tipo;
    public bool obligatorio;
    public List<string> opciones;
}

public class OptionGroupMeta : MonoBehaviour
{
    public bool obligatorio = true;
    public string tipo = "radio";
}