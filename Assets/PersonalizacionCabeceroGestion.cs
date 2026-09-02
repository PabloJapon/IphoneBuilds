using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonalizacionCabeceroGestion : MonoBehaviour
{
    // Referencia a la base de datos de personalización
    public DataBasePersonalizacion DBP;
    private bool isDBLoaded = false;

    // --- Nombre del restaurante, en las dos barras ---
    public TMP_Text nombreRest1;
    public TMP_Text nombreRest2;

    // --- Imagen del cabecero (logo del restaurante) ---
    public Image imageRest;
    public AspectFill aspectFillImageRestaurante; // para que recorte en vez de dejar huecos

    // --- Color de fondo de la barra de secciones (etiquetas), en las dos barras ---
    public Image fondoBarraSecciones1;
    public Image fondoBarraSecciones2;

    // --- Color de fondo de la barra del título (nombre del restaurante), en las dos barras ---
    public Image fondoBarraTitulo1;
    public Image fondoBarraTitulo2;

    void Start()
    {
        DBP.OnDataLoaded += OnDBLoaded;
    }

    private void OnDestroy()
    {
        DBP.OnDataLoaded -= OnDBLoaded;
    }

    private void OnDBLoaded()
    {
        isDBLoaded = true;
        EditarCabecero();
    }

    private void EditarCabecero()
    {
        // 1. Nombre del restaurante
        nombreRest1.text = DataBasePersonalizacion.nombre_rest[0];
        nombreRest2.text = DataBasePersonalizacion.nombre_rest[0];

        // 2. Colores
        ChangeImageColor();

        // 3. Tipo de letra del nombre
        string rutaFuenteNombreRest = "Fonts/" + DataBasePersonalizacion.letra_titulo[0].Replace(" ", "");
        TMP_FontAsset fuenteNombreRest = Resources.Load<TMP_FontAsset>(rutaFuenteNombreRest);
        if (fuenteNombreRest == null)
            fuenteNombreRest = Resources.Load<TMP_FontAsset>(rutaFuenteNombreRest + " SDF");
        nombreRest1.font = fuenteNombreRest;
        nombreRest2.font = fuenteNombreRest;
    }

    private void ChangeImageColor()
    {
        Color newColorBarsecc;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo[0], out newColorBarsecc))
        {
            fondoBarraSecciones1.color = newColorBarsecc;
            fondoBarraSecciones2.color = newColorBarsecc;
        }

        Color newColorFondoTitulo;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_fondo_titulo[0], out newColorFondoTitulo))
        {
            fondoBarraTitulo1.color = newColorFondoTitulo;
            fondoBarraTitulo2.color = newColorFondoTitulo;
        }
        else
        {
            Debug.LogWarning("No se pudo parsear col_fondo_titulo: '" + DataBasePersonalizacion.col_fondo_titulo[0] + "'");
        }

        Color textColorTitulo;
        if (ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_letra_titulo[0], out textColorTitulo))
        {
            nombreRest1.color = textColorTitulo;
            nombreRest2.color = textColorTitulo;
        }

        CreateImage();
    }

    private void CreateImage()
    {
        Sprite[] sprites = DataBasePersonalizacion.spriteRest;
        imageRest.sprite = sprites[0];
        imageRest.preserveAspect = false; // dejamos que AspectFill controle el recorte

        if (aspectFillImageRestaurante != null)
            aspectFillImageRestaurante.AdjustToCover();
        else
            Debug.LogWarning("PersonalizacionCabeceroGestion: falta asignar 'Aspect Fill Image Restaurante' en el Inspector.");
    }
}