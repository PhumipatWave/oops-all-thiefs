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

    public void CreateHost()
    {
        GameManager = new HostGameManager();
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
