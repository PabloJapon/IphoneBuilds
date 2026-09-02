using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScroll : MonoBehaviour
{
    public CustomScrollRect customScroll;  // Reference to the custom scroll script

    void Start()
    {
        // By default, enable the built-in ScrollRect and disable custom
        EnableBuiltInScroll();
    }

    // Method to enable the built-in ScrollRect and disable the custom scroll
    public void EnableBuiltInScroll()
    {
        customScroll.enabled = false;        // Disable custom scroll script
    }

    // Method to enable the custom scroll and disable the built-in ScrollRect
    public void EnableCustomScroll()
    {
        customScroll.enabled = true;         // Enable custom scroll script
    }

    // Example of a method to toggle between the two
    public void ToggleScroll()
    {
        if (customScroll.enabled)
        {
            EnableBuiltInScroll();
        }
        else
        {
            EnableCustomScroll();
        }
    }
}
