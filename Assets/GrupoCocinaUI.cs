using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class GrupoCocinaUI : MonoBehaviour
{
    [Header("Referencias UI (asignar en el prefab)")]
    public Image borde;
    public TMP_Text headerText;
    public Transform dishesContainer;
    public Toggle toggleListo;
    public TMP_Text toggleLabel;

    [Header("Identificadores (los rellena ConnectMirrorCocina)")]
    [HideInInspector] public int mesaNumber;
    [HideInInspector] public int batchIndex;
    [HideInInspector] public int ordenGrupo;       // 0 = "sin orden", 1/2/3 = 1º/2º/3º...
    [HideInInspector] public int totCocinasGrupo;  // cocinas distintas involucradas en ESTE grupo (todas las cocinas, no solo la local)
    [HideInInspector] public bool desbloqueado;    // true si el grupo anterior ya está 100% completado

    public static readonly Color ROJO = new Color32(0xE1, 0x3D, 0x3D, 0xFF);
    public static readonly Color NARANJA = new Color32(0xFF, 0xC3, 0x68, 0xFF);
    public static readonly Color VERDE = new Color32(0x1F, 0xCB, 0x17, 0xFF);

    // Clave única para identificar este panel al recibir RPCs del servidor
    public string ClaveGrupo => $"Grupo_Mesa{mesaNumber}_Batch{batchIndex}_Orden{ordenGrupo}";

    [Header("Aviso de bloqueo")]
    public Button blockerAvisoButton;
    public Color colorAviso = Color.red;
    public Color colorTextoNormal = Color.black; // pon aquí el color que ya usabas antes para el texto del toggle

    private Coroutine avisoCoroutine;

    public void AsignarNombreUnico()
    {
        gameObject.name = ClaveGrupo;
    }

    [HideInInspector] public int cocinasReadyActual;

    /// <summary>
    /// Repinta el cuadro y el toggle según el estado actual.
    /// cocinasReady solo es relevante si totCocinasGrupo > 1.
    /// </summary>
    public void RefrescarVisual(int cocinasReady)
    {
        cocinasReadyActual = cocinasReady;

        bool esMultiCocina = totCocinasGrupo > 1;

        if (toggleListo != null) toggleListo.gameObject.SetActive(esMultiCocina);
        if (toggleLabel != null) toggleLabel.gameObject.SetActive(esMultiCocina);
        if (blockerAvisoButton != null) blockerAvisoButton.gameObject.SetActive(false); // ya no hace falta interceptar el clic

        if (!desbloqueado)
        {
            if (borde != null) borde.color = ROJO;
            if (toggleListo != null)
            {
                toggleListo.SetIsOnWithoutNotify(false);
                toggleListo.interactable = false;
            }
            if (esMultiCocina && toggleLabel != null)
            {
                toggleLabel.color = colorAviso;
                toggleLabel.text = MensajeBloqueado();
            }
            return;
        }

        if (!esMultiCocina)
        {
            if (borde != null) borde.color = VERDE;
            return;
        }

        // Desbloqueado y multi-cocina
        if (toggleListo != null) toggleListo.interactable = !toggleListo.isOn;

        if (cocinasReady <= 0) { if (borde != null) borde.color = ROJO; }
        else if (cocinasReady < totCocinasGrupo) { if (borde != null) borde.color = NARANJA; }
        else { if (borde != null) borde.color = VERDE; }

        ActualizarLabel(toggleListo != null && toggleListo.isOn);
    }

    private void ActualizarLabel(bool listo)
    {
        if (toggleLabel == null) return;
        toggleLabel.color = colorTextoNormal; // ver más abajo, campo nuevo
        toggleLabel.text = listo ? "Esta cocina está lista" : "Esta cocina aún no está lista";
    }

    /// <summary>
    /// Llamar cuando el usuario intenta pulsar el toggle estando el grupo bloqueado.
    /// Aquí solo dejamos el hook; el aviso (popup/mensaje) lo dispara quien detecte el intento.
    /// </summary>
    public string MensajeBloqueado()
    {
        return "Primero deben completarse todos los platos del grupo anterior.";
    }

    public bool NecesitaConfirmacion()
    {
        if (!desbloqueado) return true; // rojo: bloqueado por secuencia
        if (totCocinasGrupo <= 1) return false; // mono-cocina desbloqueado = siempre verde, nunca pide confirmación
        return cocinasReadyActual < totCocinasGrupo; // rojo O naranja: aún no están listas TODAS las cocinas implicadas
    }
}
