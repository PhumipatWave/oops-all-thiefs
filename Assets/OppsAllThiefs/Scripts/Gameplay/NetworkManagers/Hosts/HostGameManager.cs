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
    private NetworkObject playerPrefab;

    private string lobbyId;
    public string JoinCode {  get; private set; }
    public NetworkServer NetworkServer { get; private set; }

    private const int MaxConnections = 8;
    private const string GameSceneName = "GameplayScene";
    private const string JoinCodeKey = "JoinCode";

    public HostGameManager(NetworkObject playerPrefab)
    {
        this.playerPrefab = playerPrefab;
    }

    public async Task StartHostAsync(bool isPrivate)
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
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {JoinCode}");
            // Save the join code to PlayerPrefs
            PlayerPrefs.SetString(JoinCodeKey, JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Configure the Unity Transport to use the Relay server data
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = isPrivate;
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
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

        NetworkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab);

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId,
        };

        // Convert the user data to JSON
        string payload = JsonUtility.ToJson(userData);
        // Then convert to bytes for sending over the network
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // In HostGameManager.StartHostAsync, just before NetworkManager.Singleton.StartHost()
        var allObjects = GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            var behaviours = obj.GetComponents<NetworkBehaviour>();
            foreach (var b in behaviours)
                Debug.Log($"[PreHost] {obj.name} → {b.GetType().Name}");
        }

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkServer.OnClientLeft += HandleClientLeft;
        
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        };

        NetworkManager.Singleton.StartHost();
    }

    /// <HeartbeatLobby>
    /// Sends heartbeat to the lobby service to keep the lobby active.
    /// </HeartbeatLobby>
    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void Shutdown()
    {
        HostHandler.Instance.StartCoroutine(nameof(HeartbeatLobby));

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

        NetworkServer.OnClientLeft -= HandleClientLeft;
        NetworkServer?.Dispose();
    }

    public async void Dispose()
    {
        Shutdown();
    }
}
