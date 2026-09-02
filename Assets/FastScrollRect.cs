using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class FastScrollRect : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    public RectTransform content;
    public RectTransform viewport;

    [Header("Behavior")]
    public bool vertical = true;
    public bool horizontal = false;
    public bool inertia = true;

    [Header("Feel tuning")]
    [Tooltip("Content follows finger exactly (1 = 1:1)")]
    public float dragSensitivity = 1f;
    [Tooltip("Velocity multiplier for flicks / inertia")]
    public float flingSensitivity = 1.8f;
    [Tooltip("0..1, lower = longer glide")]
    public float decelerationRate = 0.135f;
    [Tooltip("Velocity threshold to snap to zero")]
    public float snapStopThreshold = 5f;
    [Tooltip("0..1 smoothing for velocity during drag (0 = none)")]
    public float velocitySmoothing = 0.5f;

    // Internal
    RectTransform m_Rect;
    Canvas m_Canvas;
    Vector2 m_Velocity;
    bool m_Dragging;
    bool m_Pressed;
    Vector2 m_PrevDragDelta;

    void Awake()
    {
        m_Rect = GetComponent<RectTransform>();
        m_Canvas = GetComponentInParent<Canvas>();
        if (viewport == null) Debug.LogWarning("FastScrollRect: viewport not assigned.");
        if (content == null) Debug.LogWarning("FastScrollRect: content not assigned.");
    }

    void Update()
    {
        if (!m_Dragging && !m_Pressed && inertia)
        {
            if (m_Velocity.sqrMagnitude > 0.01f)
            {
                Vector2 delta = m_Velocity * Time.unscaledDeltaTime;
                MoveContent(delta);

                float fpsNormalized = 60f * Time.unscaledDeltaTime;
                float decay = Mathf.Pow(decelerationRate, fpsNormalized);
                m_Velocity *= decay;

                // Hard snap to zero if below threshold
                if (m_Velocity.magnitude < snapStopThreshold)
                    m_Velocity = Vector2.zero;
            }
        }

        // Extra safety: if finger is pressed, zero velocity immediately
        if (m_Pressed && m_Velocity.sqrMagnitude > 0f)
        {
            m_Velocity = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_Pressed = true;
        m_Velocity = Vector2.zero; // stop inertia immediately
        m_PrevDragDelta = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_Pressed = false;
        // OnEndDrag will handle inertia after release
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_Dragging = true;
        m_Velocity = Vector2.zero;
        m_PrevDragDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;

        float scale = (m_Canvas != null) ? m_Canvas.scaleFactor : 1f;
        if (scale <= 0f) scale = 1f;
        delta /= scale;

        // Apply drag sensitivity (finger movement)
        Vector2 move = Vector2.zero;
        if (horizontal) move.x = delta.x * dragSensitivity;
        if (vertical) move.y = delta.y * dragSensitivity;

        MoveContent(move);

        // Update velocity for flicks
        Vector2 newVel = move / Mathf.Max(0.0001f, Time.unscaledDeltaTime);
        if (velocitySmoothing > 0f)
            m_Velocity = Vector2.Lerp(m_Velocity, newVel, Mathf.Clamp01(velocitySmoothing));
        else
            m_Velocity = newVel;

        m_PrevDragDelta = delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_Dragging = false;

        // Apply fling multiplier to velocity only after release
        m_Velocity *= flingSensitivity;
    }

    void MoveContent(Vector2 delta)
    {
        if (content == null) return;

        Vector2 anchored = content.anchoredPosition + delta;

        if (viewport != null)
        {
            Vector2 min = GetMinAnchoredPosition();
            Vector2 max = GetMaxAnchoredPosition();
            anchored.x = Mathf.Clamp(anchored.x, min.x, max.x);
            anchored.y = Mathf.Clamp(anchored.y, min.y, max.y);
        }

        content.anchoredPosition = anchored;
    }

    Vector2 GetMinAnchoredPosition()
    {
        Vector2 viewSize = GetSize(viewport);
        Vector2 contentSize = GetSize(content);

        Vector2 min = new Vector2(
            contentSize.x > viewSize.x ? viewSize.x - contentSize.x : 0f,
            contentSize.y > viewSize.y ? 0f : 0f
        );
        return min;
    }

    Vector2 GetMaxAnchoredPosition()
    {
        Vector2 viewSize = GetSize(viewport);
        Vector2 contentSize = GetSize(content);

        Vector2 max = new Vector2(
            0f,
            contentSize.y > viewSize.y ? contentSize.y - viewSize.y : 0f
        );
        return max;
    }

    Vector2 GetSize(RectTransform rt)
    {
        if (rt == null) return Vector2.zero;
        return new Vector2(rt.rect.width, rt.rect.height);
    }
}