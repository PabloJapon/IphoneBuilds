using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonAtrasMenu : MonoBehaviour
{
    [SerializeField] string nombreBuscar = "CanvasMenús"; // cambia si hace falta

    public void ActivarCanvasMenús()
    {
        GameObject target = FindInactiveInScene(nombreBuscar);
        if (target != null)
        {
            target.SetActive(true);
        }
        else
        {
            Debug.LogError($"No se encontró '{nombreBuscar}' en la escena (inactivo o activo).");
        }
    }

    GameObject FindInactiveInScene(string name)
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var found = FindRecursive(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    GameObject FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t.gameObject;
        foreach (Transform child in t)
        {
            var r = FindRecursive(child, name);
            if (r != null) return r;
        }
        return null;
    }
}
