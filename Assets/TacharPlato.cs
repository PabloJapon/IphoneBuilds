using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TacharPlato : MonoBehaviour
{
    private TMP_Text textLabel;
    private TMP_Text textTiempo;
    private TMP_Text textCantidad;
    private Image blurImage;

    public void Start()
    {
        if (SceneManager.GetActiveScene().name == "CocinaScene")
        {
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
            textLabel = texts[0];
            textTiempo = texts[1];
            blurImage = gameObject.GetComponentInChildren<Image>();
        }
        else
        {
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
            textLabel = texts[0];
            textCantidad = texts[1];
        }
    }

    public void OnToggleChange(Toggle toggle)
    {
        if (SceneManager.GetActiveScene().name == "CocinaScene")
        {
            if (toggle.isOn)
            {
                textLabel.text = "<s>" + textLabel.text + "</s>";
                textTiempo.enabled = false;

                var tempColor = blurImage.color;
                tempColor.a = 0.5f;
                blurImage.color = tempColor;
            }
            else
            {
                textLabel.text = textLabel.text.Replace("<s>", "").Replace("</s>", "");
                textTiempo.enabled = true;

                var tempColor = blurImage.color;
                tempColor.a = 0f;
                blurImage.color = tempColor;
            }
        }
        else
        {
            if (toggle.isOn)
            {
                textLabel.text = "<s>" + textLabel.text + "</s>";
            }
            else
            {
                textLabel.text = textLabel.text.Replace("<s>", "").Replace("</s>", "");
            }
        }
    }
}