using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the client manager with singleton pattern.
/// </summary>
/// <remark>
/// Use to command the client manager.
/// </remark>
public class ClientHandler : MonoBehaviour
{
    private static ClientHandler instance;
    public static ClientHandler Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ClientHandler>();
            if (instance == null)
            {
                Debug.LogError("No ClientHandler in scene");
                return null;
            }
            return instance; 
        }
    }

    public ClientGameManager GameManager { get; private set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();
        return await GameManager.InitAsync();
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
