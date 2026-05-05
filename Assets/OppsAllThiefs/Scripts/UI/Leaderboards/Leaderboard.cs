using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardHolder;
    [SerializeField] private LeaderboardVisual leaderboardPrefab;
    [SerializeField] private int leaderboardToDisplay = 8;

    private Dictionary<ulong, NetworkVariable<int>.OnValueChangedDelegate> moneyHandlers = new();

    private NetworkList<LeaderboardState> leaderboardStates = new NetworkList<LeaderboardState>();
    private List<LeaderboardVisual> leaderboardVisuals = new List<LeaderboardVisual>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardStates.OnListChanged += HandleLeaderboardChanged;

            foreach (LeaderboardState state in leaderboardStates)
            {
                HandleLeaderboardChanged(new NetworkListEvent<LeaderboardState>
                {
                    Type = NetworkListEvent<LeaderboardState>.EventType.Add,
                    Value = state
                });
            }
        }

        if (IsServer)
        {
            StartCoroutine(DelayFindPlayer());
        }
    }

    private void HandleMoneyChanged(ulong clientId, int newMoneys)
    {
        for (int i = 0; i < leaderboardStates.Count; i++)
        {
            if (leaderboardStates[i].ClientId != clientId)
                continue;

            leaderboardStates[i] = new LeaderboardState
            {
                ClientId = leaderboardStates[i].ClientId,
                PlayerName = leaderboardStates[i].PlayerName,
                Moneys = newMoneys
            };

            return;
        }
    }

    private void AvailableLeaderboard(Player player)
    {
        if (!IsSpawned) return;

        leaderboardStates.Add(new LeaderboardState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Moneys = 0
        });

        Debug.Log($"ClientId: {player.OwnerClientId}, PlayerName: {player.PlayerName.Value}");

        NetworkVariable<int>.OnValueChangedDelegate handler = (oldMoneys, newMoneys) =>
            HandleMoneyChanged(player.OwnerClientId, newMoneys);

        moneyHandlers[player.OwnerClientId] = handler;
        player.CurrentMoney.OnValueChanged += handler;
    }

    private void UnavailableLeaderboard(Player player)
    {
        if (NetworkManager.ShutdownInProgress) return;

        foreach (LeaderboardState state in leaderboardStates)
        {
            if (state.ClientId != player.OwnerClientId) continue;
            leaderboardStates.Remove(state);
            break;
        }

        if (moneyHandlers.TryGetValue(player.OwnerClientId, out var handler))
        {
            player.CurrentMoney.OnValueChanged -= handler;
            moneyHandlers.Remove(player.OwnerClientId);
        }
    }

    private System.Collections.IEnumerator DelayFindPlayer()
    {
        yield return new WaitUntil(() =>
        NetworkManager.Singleton.IsListening &&
        NetworkManager.Singleton.IsServer &&
        NetworkManager.Singleton.ConnectedClientsList.Count > 0);

        yield return new WaitForSeconds(0.5f);

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
            AvailableLeaderboard(player);

        Player.OnPlayerSpawned += AvailableLeaderboard;
        Player.OnPlayerDespawned += UnavailableLeaderboard;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
            leaderboardStates.OnListChanged -= HandleLeaderboardChanged;

        if (IsServer)
        {
            Player.OnPlayerSpawned -= AvailableLeaderboard;
            Player.OnPlayerDespawned -= UnavailableLeaderboard;
        }
    }

    private void HandleLeaderboardChanged(NetworkListEvent<LeaderboardState> changeEvent)
    {
        if (!gameObject.scene.isLoaded)
            return;
        if (!IsSpawned) return;

        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardState>.EventType.Add:
                if (!leaderboardVisuals.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardVisual leaderboardEntity = Instantiate(leaderboardPrefab, leaderboardHolder);
                    leaderboardEntity.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Moneys);
                    leaderboardVisuals.Add(leaderboardEntity);
                }
                break;

            case NetworkListEvent<LeaderboardState>.EventType.Remove:
                LeaderboardVisual displayToRemove = leaderboardVisuals.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    leaderboardVisuals.Remove(displayToRemove);
                }
                break;

            case NetworkListEvent<LeaderboardState>.EventType.Value:
                LeaderboardVisual displayToUpdate = leaderboardVisuals.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateMoneys(changeEvent.Value.Moneys);
                }
                break;
        }

        leaderboardVisuals.Sort((x, y) => y.Moneys.CompareTo(x.Moneys));

        for (int i = 0; i < leaderboardVisuals.Count; i++)
        {
            leaderboardVisuals[i].transform.SetSiblingIndex(i);
            leaderboardVisuals[i].UpdateTextDisplay();
            leaderboardVisuals[i].gameObject.SetActive(i <= leaderboardToDisplay - 1);
        }

        LeaderboardVisual ownerDisplay = leaderboardVisuals.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if (ownerDisplay != null)
        {
            if (ownerDisplay.transform.GetSiblingIndex() >= leaderboardToDisplay)
            {
                leaderboardHolder.GetChild(leaderboardToDisplay - 1).gameObject.SetActive(false);
                ownerDisplay.gameObject.SetActive(true);
            }
        }
    }
}
