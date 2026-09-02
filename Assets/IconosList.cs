using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IconosList", menuName = "Font/IconoList", order = 1)]
public class IconosList : ScriptableObject
{
    // Lista de nombres de iconos (numeros)
    public List<string> iconoNames;

    // Lista de sprites asociados a esos iconos
    public List<Sprite> iconoImages;
}
