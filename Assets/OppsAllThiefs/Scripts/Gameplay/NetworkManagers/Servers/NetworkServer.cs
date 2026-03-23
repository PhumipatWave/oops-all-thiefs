using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new();
    private Dictionary<string, UserData> authIdToUserData = new();

    private string lobbyID;
    public string LobbyID { get { return lobbyID; } set { lobbyID = value; } }

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.networkManager = networkManager;
        this.playerPrefab = playerPrefab;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.UserAuthId;
        authIdToUserData[userData.UserAuthId] = userData;
        Debug.Log($"User Name : {userData.UserName}");

        _ = SpawnPlayerDelayed(request.ClientNetworkId, userData.UserAuthId);

        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    private async Task SpawnPlayerDelayed(ulong clientId, string authId)
    {
        await Task.Delay(1000);

        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyID);
        Unity.Services.Lobbies.Models.Player playerLobbyInfo = lobby.Players.Find(p => p.Id == authId);

        int index = lobby.Players.FindIndex(p => p.Id == authId);

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, PlayerSpawnPoint.GetSpawnIndexPos(index), Quaternion.Euler(0, 180, 0));

        playerInstance.SpawnAsPlayerObject(clientId);

        Debug.Log($"{index} : {playerInstance}");
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }
            return null;
        }
        return null;
    }

    public void Dispose()
    {
        if (networkManager == null) return;

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
