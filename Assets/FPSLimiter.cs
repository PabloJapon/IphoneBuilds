using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0;   // Disable VSync
        Application.targetFrameRate = 60; // Or 30 for weak phones
    }
}