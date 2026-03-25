using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new();
            options.Count = 25;
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(field: QueryFilter.FieldOptions.AvailableSlots,
                // GT : Greater than
                op: QueryFilter.OpOptions.GT,
                value: "0"),
                new QueryFilter(field: QueryFilter.FieldOptions.IsLocked,
                // EQ : Equal
                op: QueryFilter.OpOptions.EQ,
                value: "0"),
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initalize(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isRefreshing = false;
    }

    public async void JoinLobby(Lobby lobby)
    {
        if (isJoining) return;
        isJoining = true;

        try
        {
            Debug.Log(LobbyService.Instance);
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string lobbyCode = joiningLobby.Data["JoinCode"].Value;
            LobbyDataManager.Instance.LobbyID = lobby.Id;
            LobbyDataManager.Instance.CurLobby = lobby;

            await ClientHandler.Instance.GameManager.StartClientAsync(lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isJoining = false;
    }
}
