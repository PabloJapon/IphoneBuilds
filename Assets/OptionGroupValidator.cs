using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class OptionGroupValidator : MonoBehaviour
{
    private Button confirmButton;

    void Start()
    {
        // Find the confirm button once
        Transform buttonParent = transform.parent?.parent?.parent;
        if (buttonParent == null)
        {
            Debug.LogWarning("[OptionGroupValidator] could not find buttonContainer (3 levels up).");
            return;
        }

        Transform lastSibling = buttonParent.GetChild(buttonParent.childCount - 1);
        confirmButton = lastSibling.GetComponentInChildren<Button>(true);

        if (confirmButton != null)
        {
            confirmButton.interactable = false;
        }
        else
        {
            Debug.LogWarning("[OptionGroupValidator] Confirm button NOT found in last sibling of container.");
        }
    }

    void Update()
    {
        if (confirmButton == null) return;

        var groups = GetComponentsInChildren<ToggleGroup>(true);

        if (groups.Length == 0)
        {
            confirmButton.interactable = true;
            return;
        }

        bool allSelected = true;
        for (int i = 0; i < groups.Length; i++)
        {
            var group = groups[i];

            // Only enforce mandatory groups
            OptionGroupMeta meta = group.GetComponentInParent<OptionGroupMeta>();
            if (meta != null && !meta.obligatorio) continue;

            if (!group.AnyTogglesOn())
            {
                allSelected = false;
                break;
            }
        }

        if (allSelected && !confirmButton.interactable)
            confirmButton.interactable = true;
        else if (!allSelected && confirmButton.interactable)
            confirmButton.interactable = false;
    }
}
