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

    private Dictionary<ulong, int> clientSpawnIndex = new();
    private int nextSpawnIndex = 0;

    private bool sceneReady = false;

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

        // detect scene load via Unity (simple + reliable)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameplayScene")
        {
            sceneReady = true;

            Debug.Log("GameplayScene READY → spawning all clients");

            SpawnAllClients();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!clientSpawnIndex.ContainsKey(clientId))
        {
            clientSpawnIndex[clientId] = nextSpawnIndex;
            nextSpawnIndex++;
        }

        Debug.Log($"Client {clientId} assigned spawn index {clientSpawnIndex[clientId]}");

        // if scene already loaded → spawn immediately
        if (sceneReady)
        {
            SpawnClient(clientId);
        }
    }

    private void SpawnAllClients()
    {
        foreach (var client in networkManager.ConnectedClientsIds)
        {
            SpawnClient(client);
        }
    }

    private void SpawnClient(ulong clientId)
    {
        if (!networkManager.IsServer)
            return;

        if (!networkManager.ConnectedClients.ContainsKey(clientId))
            return;

        if (networkManager.ConnectedClients[clientId].PlayerObject != null)
            return;

        if (PlayerSpawnPoint.Instance == null || !PlayerSpawnPoint.Instance.HasSpawnPoints())
        {
            Debug.LogError("Spawn points not ready");
            return;
        }

        int index = clientSpawnIndex[clientId];

        Vector3 pos = PlayerSpawnPoint.Instance.GetSpawnIndexPos(index);

        NetworkObject player = GameObject.Instantiate(
            playerPrefab,
            pos,
            Quaternion.identity
        );

        player.SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned client {clientId} at index {index} pos {pos}");
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
            clientSpawnIndex.Remove(clientId);

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

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (networkManager.IsListening)
            networkManager.Shutdown();
    }
}