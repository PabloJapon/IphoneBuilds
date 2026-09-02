using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cabezera : MonoBehaviour
{
    public GameObject cabecera;
    public RectTransform content;

    public float thresholdY = 5.9f; // Adjustable threshold
    private bool isCabeceraActive = false; // Track the current state

    // Start is called before the first frame update
    void Start()
    {
        cabecera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check the y position of content
        float contentY = content.anchoredPosition.y;
        //Debug.Log("Content Y Position: " + contentY); // Log the y position
        //Debug.Log($"World Position: {content.position.y}, Local Position: {content.localPosition.y}, Anchored Position: {content.anchoredPosition.y}");

        // Show cabecera if content's y position exceeds threshold
        if (contentY > thresholdY && !isCabeceraActive)
        {
            cabecera.SetActive(true);
            isCabeceraActive = true; // Update state
        }
        // Hide cabecera if content's y position is below threshold
        else if (contentY < thresholdY && isCabeceraActive)
        {
            cabecera.SetActive(false);
            isCabeceraActive = false; // Update state
        }
    }
}
