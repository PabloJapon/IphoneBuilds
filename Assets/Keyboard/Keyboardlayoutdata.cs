using UnityEngine;

// Un asset de este tipo = un idioma/layout de teclado.
// Crear desde: click derecho en Project > Create > UI > Keyboard Layout
[CreateAssetMenu(fileName = "NewKeyboardLayout", menuName = "UI/Keyboard Layout")]
public class KeyboardLayoutData : ScriptableObject
{
    public string layoutName = "Español (QWERTY)";

    [System.Serializable]
    public class KeyRow
    {
        // Ambos arrays deben tener la misma longitud y corresponderse índice a índice.
        // Ej: normalKeys = ["q","w","e"]  shiftKeys = ["Q","W","E"]
        public string[] normalKeys;
        public string[] shiftKeys;
    }

    public KeyRow[] rows;
}