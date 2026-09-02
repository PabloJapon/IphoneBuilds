using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AspectFill : MonoBehaviour
{
    public RectTransform parentRect; // Optionally assign this manually

    public bool menuItem = false;

    void Start()
    {
        if (parentRect == null && transform.parent != null)
        {
            parentRect = transform.parent.GetComponent<RectTransform>();
        }
        if (menuItem)
        {
            AdjustToCover();
        }
    }

    // Call this method manually after setting the sprite
    public void AdjustToCover()
    {
        Image img = GetComponent<Image>();
        if (img.sprite == null || parentRect == null)
            return;

        RectTransform rt = GetComponent<RectTransform>();

        // Ensure anchors and pivot are centered for proper positioning.
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Get parent's dimensions (for example, 700 x 600)
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        // Use the sprite's native dimensions in UI units
        float spriteWidth = img.sprite.rect.width / img.sprite.pixelsPerUnit;
        float spriteHeight = img.sprite.rect.height / img.sprite.pixelsPerUnit;

        // Calculate the scale factor so that the image covers the entire parent container
        float scale = Mathf.Max(parentWidth / spriteWidth, parentHeight / spriteHeight);

        // Set the new size of the image
        rt.sizeDelta = new Vector2(spriteWidth * scale, spriteHeight * scale);
    }
}
