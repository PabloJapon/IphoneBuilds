using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeTextTipoComidaButton : MonoBehaviour, IPointerClickHandler
{
    private Button button;
    private Text buttonText;

    private float lastClickTime = 0f;
    private float doubleClickDelay = 0.3f; // Adjust this value to set the double click delay

    void Start()
    {
        // Get the Button component
        button = GetComponent<Button>();

        // Get the Text component of the button
        buttonText = button.GetComponentInChildren<Text>();
    }

    // Handle double-click event
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
         Debug.Log("Clicked");
            float currentTime = Time.time;
            if (currentTime - lastClickTime < doubleClickDelay)
            {
                // Double click detected, change the button text
                ChangeButtonText();
            }
            lastClickTime = currentTime;
        }
    }

    // Change the button text
    private void ChangeButtonText()
    {
        // Here you can implement the logic to change the button text
        // For demonstration, let's set it to a random string
        buttonText.text = Random.Range(0, 100).ToString();
    }
}
