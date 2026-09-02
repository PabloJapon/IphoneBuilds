using Mirror;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HostClient : MonoBehaviour
{
    public static HostClient instance;

    [SerializeField] private string serverAddress = "localhost";
    [SerializeField] private bool isHost = true;
    public static bool isHostMode = false;
    public Navigation N;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Full Mirror reset for Editor sessions
        if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
        }
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }

        NetworkManager networkManager = GetComponent<NetworkManager>();
        networkManager.networkAddress = serverAddress;
        /* Debug.Log($"Connecting to: {networkManager.networkAddress} port: {GetComponent<kcp2k.KcpTransport>().port}"); */

        var kcp = GetComponent<kcp2k.KcpTransport>();

        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            kcp.port = 7778;
            Debug.Log("Starting as dedicated server...");
            networkManager.StartServer();
        }
        else if (isHost)
        {
            isHostMode = true;
            kcp.port = 13434;
            Debug.Log("Starting as host...");
            networkManager.StartHost();
        }
        else
        {
            kcp.port = 13434;
        }
    }
}