using System;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [Header("Network Components")]
    [SerializeField] private ClientHandler clientPrefab;
    [SerializeField] private HostHandler hostPrefab;

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (!isDedicatedServer)
        {
            HostHandler hostHandler = Instantiate(hostPrefab);
            hostHandler.CreateHost();

            ClientHandler clientHandler = Instantiate(clientPrefab);
            bool authenticated = await clientHandler.CreateClient();

            if (authenticated)
            {
                clientHandler.GameManager.GoToMenuScene();
            }
        }
    }
}
