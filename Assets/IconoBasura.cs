using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IconoBasura : MonoBehaviour
{
    private TMP_Text textParent;
    private Image img;

    // Start is called before the first frame update
    void Start()
    {
        img = gameObject.GetComponent<Image>();
        img.enabled = false;

        var parentGameObject = this.transform.parent.gameObject;
        textParent = parentGameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (textParent.text == "1")
        {
            img.enabled = true;
        }
        else if (img.enabled == true)
        {
            img.enabled = false;
        }

    }
}
