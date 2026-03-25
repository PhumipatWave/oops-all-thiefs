using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
    private const string LobbySceneName = "LobbyScene";

    private string lobbyId;

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
        string playerTeamIndex = (-1).ToString();
        bool playerReady = false;

        playerLobbyInfo = new PlayerLobbyInfo(playerName, playerTeamIndex, playerReady.ToString());

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

        await LobbyService.Instance.UpdatePlayerAsync(
            LobbyDataManager.Instance.LobbyID,
            AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = playerLobbyInfo.GetPlayerLobbyData()
            }
        );

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
