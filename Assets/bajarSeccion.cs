using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class bajarSeccion : MonoBehaviour
{
    public void BajarSeccion()
    {
        var parent = transform.parent;
        int childCount = transform.parent.parent.childCount;
        Debug.Log(parent.GetSiblingIndex());
        Debug.Log(childCount);
        if (parent.GetSiblingIndex() < childCount)
        {
            Transform uncle = null;
            if (parent?.parent != null)
            {
                uncle = parent.parent.GetChild(parent.GetSiblingIndex() + 1);
            }

            if (parent?.parent?.childCount > parent.GetSiblingIndex() + 3)
                parent.SetSiblingIndex(parent.parent.GetChild(parent.GetSiblingIndex() + 3).GetSiblingIndex());

            if (uncle?.parent?.childCount > uncle.GetSiblingIndex() + 3)
                uncle.SetSiblingIndex(uncle.parent.GetChild(uncle.GetSiblingIndex() + 3).GetSiblingIndex());
        }
        
    }

    public void SubirSeccion()
    {
        var parent = transform.parent;
        Debug.Log(parent.GetSiblingIndex());
        if (parent.GetSiblingIndex() > 2)
        {
            Transform uncle = null;
            if (parent?.parent != null)
            {
                uncle = parent.parent.GetChild(parent.GetSiblingIndex() + 1);
            }

            if (parent?.parent?.childCount > parent.GetSiblingIndex() - 2)
                parent.SetSiblingIndex(parent.parent.GetChild(parent.GetSiblingIndex() - 2).GetSiblingIndex());

            if (uncle?.parent?.childCount > uncle.GetSiblingIndex() - 2)
                uncle.SetSiblingIndex(uncle.parent.GetChild(uncle.GetSiblingIndex() - 2).GetSiblingIndex());
        }
    }
}