using UnityEngine;
using UnityEngine.UI;

public class MesaButtonSelector : MonoBehaviour
{
    private static MesaButtonSelector currentSelected;
    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = Color.grey; // your border color
        outline.effectDistance = new Vector2(2, 2); // border thickness
        outline.enabled = false;

        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (currentSelected != null && currentSelected != this)
            currentSelected.SetBorder(false);

        currentSelected = this;
        SetBorder(true);
    }

    public void SetBorder(bool visible)
    {
        if (outline != null)
            outline.enabled = visible;
    }

    public static void ClearSelection()
    {
        if (currentSelected != null)
        {
            currentSelected.SetBorder(false);
            currentSelected = null;
        }
    }
}