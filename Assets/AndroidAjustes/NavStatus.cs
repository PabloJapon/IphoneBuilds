using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavStatus : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        ApplicationChrome.statusBarColor = ApplicationChrome.navigationBarColor = 0xFFFFFFFF; // White
    }
}
