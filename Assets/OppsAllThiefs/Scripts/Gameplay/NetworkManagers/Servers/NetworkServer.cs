using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    public Action<string> OnClientLeft;

    private Dictionary<ulong, string> clientIdToAuth = new();
    private Dictionary<string, UserData> authIdToUserData = new();

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.networkManager = networkManager;
        this.playerPrefab = playerPrefab;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.UserAuthId;
        authIdToUserData[userData.UserAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;

        networkManager.OnServerStarted -= OnNetworkReady;
    }

    private void OnClientConnected(ulong clientId)
    {
        HostHandler.Instance.StartCoroutine(SpawnWhenReady(clientId));
    }

    private System.Collections.IEnumerator SpawnWhenReady(ulong clientId)
    {
        // wait until gameplay scene is actually active
        yield return new WaitUntil(() =>
            SceneManager.GetActiveScene().name == "GameplayScene");

        // wait until all spawn points are registered
        yield return new WaitUntil(() => PlayerSpawnPoint.HasSpawnPoints());

        // extra safety frame
        yield return null;

        if (!networkManager.ConnectedClients.ContainsKey(clientId))
            yield break;

        if (networkManager.ConnectedClients[clientId].PlayerObject != null)
            yield break;

        NetworkObject playerInstance = GameObject.Instantiate(
            playerPrefab,
            PlayerSpawnPoint.GetSpawnIndexPos(),
            Quaternion.identity
        );

        playerInstance.SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned Player for Client {clientId}");
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

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
                return data;
        }

        return null;
    }

    public void Dispose()
    {
        if (networkManager == null) return;

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
            networkManager.Shutdown();
    }
}