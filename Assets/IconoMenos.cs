using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IconoMenos : MonoBehaviour
{
    private TMP_Text textParent;
    private GameObject childGameObject;
    private TMP_Text textChild;

    // Start is called before the first frame update
    void Start()
    {
        var parentGameObject = this.transform.parent.gameObject;
        textParent = parentGameObject.GetComponent<TMP_Text>();

        childGameObject = this.transform.GetChild(0).gameObject;
        textChild = childGameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (textParent.text == "1")
        {
            textChild.text = "";
        }
        else
        {
            textChild.text = "-";
        }
    }
}
