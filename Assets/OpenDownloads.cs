using UnityEngine;
using System.Diagnostics;

public class OpenDownloads : MonoBehaviour
{
    public void OpenDownloadsFolder()
    {
        string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + @"\Downloads";
        Process.Start("explorer.exe", downloadsPath);
    }
}
