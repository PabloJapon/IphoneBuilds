using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ButtonColorSync : NetworkBehaviour
{
    public Button syncButton;
    public Color syncedColor;

    private void Start()
    {
        syncButton = GameObject.FindGameObjectWithTag("buttonPrueba").GetComponent<Button>();
    }

    [Command]
    void CmdChangeButtonColor(Color newColor)
    {
        syncedColor = newColor;

        // Invoke an Rpc to sync the color across all clients
        RpcSyncButtonColor(newColor);
    }

    [ClientRpc]
    void RpcSyncButtonColor(Color newColor)
    {
        syncedColor = newColor;
        syncButton.image.color = newColor;
    }

    // This method is called by the client to request a color change
    public void RequestColorChange(Color newColor)
    {
        // Call the Command to change the color on the server
        CmdChangeButtonColor(newColor);
    }
}
