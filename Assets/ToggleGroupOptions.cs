using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupOptions : MonoBehaviour
{
    public ToggleGroup group;
    public bool hasSelection = false;

    void Update()
    {
        // Check if any toggle is on
        bool currentSelection = group.AnyTogglesOn();

        // If a toggle has been selected, lock the group (can't unselect all)
        if (!hasSelection && currentSelection)
        {
            hasSelection = true;
            group.allowSwitchOff = false;
        }
    }
}
