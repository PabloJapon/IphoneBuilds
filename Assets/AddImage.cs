using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AddImage : MonoBehaviour, IDropHandler
{
    public Image imagePreview; // Reference to an Image UI element where the dropped image will be displayed

    public void OnDrop(PointerEventData eventData)
    {
        // Check if the dropped content is a file
        if (eventData.pointerDrag != null)
        {
            // Get the dragged object
            GameObject draggedObject = eventData.pointerDrag;

            // Check if the dragged object has an Image component
            Image draggedImage = draggedObject.GetComponent<Image>();
            if (draggedImage != null)
            {
                // Set the dropped image as the sprite of the image preview UI element
                imagePreview.sprite = draggedImage.sprite;
            }
        }
    }
}
