using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorQR : MonoBehaviour
{
    public GameObject ImagenQRblanco;

    void Start()
    {
        ImagenQRblanco.GetComponent<Image>().color = new Color(255,0,0,1f);
    }

   
}