using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasCarousel : MonoBehaviour
{
    public List<GameObject> canvases; // Arrastra los 5 GameObjects en el Inspector
    public Button leftButton, rightButton; // Botones de navegación
    public Vector3[] positions; // Posiciones de cada objeto
    public Vector3[] scales; // Tamaños de cada objeto
    public int[] sortingOrders = { 3, 2, 1, 1, 2 }; // Orden de dibujo
    public float transitionDuration = 0.5f; // Duración de la animación

    private bool isAnimating = false; // Para evitar doble clic durante la animación
    private float[] alphas = { 0f, 0.3f, 0.6f, 0.6f, 0.3f }; // Transparencias según la posición

    void Start()
    {
        leftButton.onClick.AddListener(() => RotateLeft());
        rightButton.onClick.AddListener(() => RotateRight());
        UpdateCanvasPositions(true); // Posiciona correctamente al inicio sin animación
    }

    void RotateRight()
    {
        if (isAnimating) return; // No permitir cambios si está animando
        StartCoroutine(AnimateRotation(true));
    }

    void RotateLeft()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateRotation(false));
    }

    IEnumerator AnimateRotation(bool toRight)
    {
        isAnimating = true;

        if (toRight)
        {
            // Rotación a la derecha: mueve el primer elemento al final...
            GameObject first = canvases[0];
            canvases.RemoveAt(0);
            canvases.Add(first);
            // ...y actualiza el orden de renderizado de inmediato para que el canvas entrante quede detrás.
            UpdateCanvasRenderOrder();
        }
        else
        {
            // Rotación a la izquierda: mueve el último elemento al principio.
            // En este caso NO actualizamos el orden de renderizado inmediatamente,
            // dejando que se conserve el comportamiento original.
            GameObject last = canvases[canvases.Count - 1];
            canvases.RemoveAt(canvases.Count - 1);
            canvases.Insert(0, last);
        }

        // Guardamos las posiciones, escalas y alphas iniciales de cada canvas
        float elapsedTime = 0;
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> startScales = new List<Vector3>();
        List<float> startAlphas = new List<float>();

        for (int i = 0; i < canvases.Count; i++)
        {
            startPositions.Add(canvases[i].transform.localPosition);
            startScales.Add(canvases[i].transform.localScale);
            Image overlay = canvases[i].transform.Find("Overlay")?.GetComponent<Image>();
            startAlphas.Add(overlay != null ? overlay.color.a : 0f);
        }

        // Interpolamos la animación hasta alcanzar las posiciones y escalas destino
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            for (int i = 0; i < canvases.Count; i++)
            {
                canvases[i].transform.localPosition = Vector3.Lerp(startPositions[i], positions[i], t);
                canvases[i].transform.localScale = Vector3.Lerp(startScales[i], scales[i], t);
                SetOverlayAlpha(canvases[i], Mathf.Lerp(startAlphas[i], alphas[i], t));
            }
            yield return null;
        }

        // Al finalizar la animación, aseguramos que todo quede en su sitio y se actualice el orden.
        UpdateCanvasPositions(false);

        isAnimating = false;
    }

    // Actualiza únicamente el orden de renderizado (sibling indices y sortingOrder) sin modificar posiciones o escalas
    void UpdateCanvasRenderOrder()
    {
        for (int i = 0; i < canvases.Count; i++)
        {
            var renderer = canvases[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrders[i];
            }
            canvases[i].transform.SetSiblingIndex(sortingOrders[i]);
        }
    }

    // Actualiza posiciones, escalas, orden de renderizado y transparencias.
    // Si "instant" es true, se asignan las posiciones y escalas directamente.
    void UpdateCanvasPositions(bool instant)
    {
        for (int i = 0; i < canvases.Count; i++)
        {
            if (instant)
            {
                canvases[i].transform.localPosition = positions[i];
                canvases[i].transform.localScale = scales[i];
            }

            var renderer = canvases[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrders[i];
            }
            canvases[i].transform.SetSiblingIndex(sortingOrders[i]);
            SetOverlayAlpha(canvases[i], alphas[i]);
        }
    }

    void SetOverlayAlpha(GameObject obj, float alpha)
    {
        Image overlay = obj.transform.Find("Overlay")?.GetComponent<Image>();
        if (overlay != null)
        {
            Color c = overlay.color;
            c.a = alpha;
            overlay.color = c;
        }
    }
}