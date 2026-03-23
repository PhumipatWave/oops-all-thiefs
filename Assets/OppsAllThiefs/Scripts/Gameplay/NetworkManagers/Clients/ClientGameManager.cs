using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    PlayerLobbyInfo playerLobbyInfo;
    private JoinAllocation allocation;
    private NetworkClient networkClient;
    private const string MenuSceneName = "MenuScene";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        networkClient = new NetworkClient(NetworkManager.Singleton);
        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated) 
        { 
            return true;
        }

        return false;
    }

    public void GoToMenuScene()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        string playerName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Unknown");
        Color playerColor = Color.gray;
        string playerColorHex = ColorUtility.ToHtmlStringRGB(playerColor);
        bool playerReady = false;

        playerLobbyInfo = new PlayerLobbyInfo(playerName, playerColorHex, playerReady.ToString());

        try
        {
            await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, new JoinLobbyByCodeOptions
            {
                Player = playerLobbyInfo.GetPlayerLobbyData(),
            });
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId,
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }
}
