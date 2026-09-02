using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FontImageList", menuName = "Font/FontImageList", order = 1)]
public class FontImageList : ScriptableObject
{
    // Lista de nombres de fuentes
    public List<string> fontNames;

    // Lista de sprites asociados a esas fuentes
    public List<Sprite> fontImages;
}