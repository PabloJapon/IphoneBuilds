using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Add this if you use TextMeshPro

public class CustomScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect scrollRect; // Assign your ScrollRect here in the Inspector
    public TMP_Text fpsText; // Assign your TMP_Text here in the Inspector to show FPS
    private bool isDragging = false;
    private Vector2 lastTouchPosition;
    private Vector2 velocity;
    private Vector2 velocity2; // Mantener velocity2 como estaba
    private float decelerationRate = 0.001f; // Adjust to control the inertia
    private float deltaTime = 0.0f;

    void Update()
    {
        // FPS Calculation
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";

        if (isDragging)
        {
            // Handle dragging and update scroll position manually, only in the Y direction
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = touch.deltaPosition;
                    // Only move the content along the Y axis (ignore X axis)
                    scrollRect.content.anchoredPosition += new Vector2(0, deltaPosition.y);
                }
            }
        }
        else
        {
            // Apply inertia if the user has stopped dragging
            if (velocity.magnitude > 0.1f)
            {
                velocity2 = velocity; // Usar velocity2 como estaba
                scrollRect.content.anchoredPosition += velocity2 * Time.deltaTime;

                // Apply deceleration in a more controlled manner
                velocity = Vector2.Lerp(velocity, Vector2.zero, decelerationRate * Time.deltaTime);

                // Stop the movement if it's slow enough
                if (velocity.magnitude < 0.1f)
                {
                    velocity = Vector2.zero;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Enable dragging mode
        isDragging = true;

        if (Input.touchCount > 0)
        {
            lastTouchPosition = Input.GetTouch(0).position;
            velocity = Vector2.zero; // Reset velocity at the start of dragging
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Disable dragging mode and calculate the final velocity for inertia, only in Y direction
        isDragging = false;

        if (Input.touchCount > 0)
        {
            Vector2 currentTouchPosition = Input.GetTouch(0).position;
            velocity = new Vector2(0, (currentTouchPosition.y - lastTouchPosition.y) / Time.deltaTime);
        }
    }
}
