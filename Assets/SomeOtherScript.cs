using UnityEngine;
using UnityEngine.UI;

public class SomeOtherScript : MonoBehaviour
{
    public ButtonColorSync buttonColorSync;

    void Start()
    {
        // Make sure you've assigned the ButtonColorSync script in the Inspector
        // or you can find it dynamically using GetComponent if it's on the same GameObject.
    }

    void Update()
    {
        // For demonstration purposes, let's change the color when the user presses the space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Call the RequestColorChange method with a new color (e.g., Color.red)
            buttonColorSync.RequestColorChange(Color.red);
        }
    }
}
