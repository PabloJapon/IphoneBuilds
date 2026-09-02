using UnityEngine;
using Mirror;

public class DisconnectNotifier : MonoBehaviour
{
    public GameObject canvasDesconectado; // Panel con mensaje "Conexión perdida"

    void OnEnable()
    {
        NetworkClient.OnDisconnectedEvent += MostrarDesconexion;
    }

    void OnDisable()
    {
        NetworkClient.OnDisconnectedEvent -= MostrarDesconexion;
    }

    private void MostrarDesconexion()
    {
        if (canvasDesconectado != null)
            canvasDesconectado.SetActive(true);
    }
}