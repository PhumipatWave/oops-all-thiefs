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
    private PlayerLobbyInfo playerLobbyInfo;
    private Allocation allocation;
    private NetworkObject playerPrefab;

    private string lobbyId;
    public string JoinCode {  get; private set; }
    public NetworkServer NetworkServer { get; private set; }

    private const int MaxConnections = 7;
    private const string LobbySceneName = "LobbyScene";
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

            string playerName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Unknown");
            Color playerColor = Color.gray;
            string playerColorHex = ColorUtility.ToHtmlStringRGB(playerColor);
            bool playerReady = false;

            playerLobbyInfo = new PlayerLobbyInfo(playerName, playerColorHex, playerReady.ToString());

            lobbyOptions.IsPrivate = isPrivate;
            lobbyOptions.Player = playerLobbyInfo.GetPlayerLobbyData();
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                        )
                }
            };

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
        NetworkServer.LobbyID = lobbyId;

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), "Missing Name"),
            UserAuthId = AuthenticationService.Instance.PlayerId,
        };

        // Convert the user data to JSON
        string payload = JsonUtility.ToJson(userData);
        // Then convert to bytes for sending over the network
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkServer.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(LobbySceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Sends periodic heartbeat pings to the lobby service to keep the lobby active.
    /// </summary>
    /// <param name="waitTimeSeconds">The interval, in seconds, to wait between each heartbeat ping.</param>
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
        HostHandler.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                playerLobbyInfo.ClearPlayerLobbyData();
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

    /// <summary>
    /// Unsubscribe all events in NetworkServer
    /// </summary>
    public async void Dispose()
    {
        Shutdown();
    }
}
