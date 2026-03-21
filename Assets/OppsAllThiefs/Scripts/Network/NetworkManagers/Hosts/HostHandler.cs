using Unity.Netcode;
using UnityEngine;

public class HostHandler : MonoBehaviour
{
    private static HostHandler instance;
    public static HostHandler Instance
    {
        get 
        { 
            if (instance != null) return instance;

            instance = FindFirstObjectByType<HostHandler>();
            if (instance == null)
            {
                Debug.LogError("No HostHandler in scene");
                return null;
            }

            return instance; 
        }
    }

    public HostGameManager GameManager { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost(NetworkObject playerPrefab)
    {
        GameManager = new HostGameManager(playerPrefab);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
