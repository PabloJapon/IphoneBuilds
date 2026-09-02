using UnityEngine;

public class NetworkCoroutineRunner : MonoBehaviour
{
    private static NetworkCoroutineRunner _instance;
    public static NetworkCoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("NetworkCoroutineRunner");
                _instance = go.AddComponent<NetworkCoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}