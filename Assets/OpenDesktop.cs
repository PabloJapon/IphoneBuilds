using UnityEngine;
using System.Diagnostics;

public class OpenDesktop : MonoBehaviour
{
    public void OpenDesktopFolder()
    {
        string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + @"\Desktop";
        Process.Start("explorer.exe", downloadsPath);
    }
}
