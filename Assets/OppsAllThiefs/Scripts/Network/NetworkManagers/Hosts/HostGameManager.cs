using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string joinCode;
    private string lobbyId;

    private NetworkServer networkServer;

    private const int MaxConnections = 7;
    private const string GameSceneName = "GameplayScene";
    private const string JoinCodeKey = "JoinCode";

    public async Task StartHostAsync()
    {
        try
        {
            // Create a Relay allocation for the specified number of connections
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {joinCode}");
            // Save the join code to PlayerPrefs
            PlayerPrefs.SetString(JoinCodeKey, joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;
        UnityTransport transport = networkManager.GetComponent<UnityTransport>();

        // Configure the Unity Transport to use the Relay server data
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                        )
                }
            };

            string playerName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Unknown");

            // Set lobby options and create the lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{playerName}'s Lobby", MaxConnections, lobbyOptions
                );
            lobbyId = lobby.Id;

            // Start lobby heartbeat coroutine to keep the lobby alive
            HostHandler.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        networkServer = new NetworkServer(networkManager);

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };

        // Convert the user data to JSON
        string payload = JsonUtility.ToJson(userData);
        // Then convert to bytes for sending over the network
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        networkManager.NetworkConfig.ConnectionData = payloadBytes;
        networkManager.StartHost();
        networkManager.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void Dispose()
    {
        HostHandler.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            lobbyId = string.Empty;
        }

        networkServer?.Dispose();
    }
}
