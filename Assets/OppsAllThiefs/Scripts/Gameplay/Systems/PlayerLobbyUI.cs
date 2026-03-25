using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLobbyUI : MonoBehaviour
{
    [SerializeField] private Transform playerDataParrent;
    [SerializeField] private Transform playerDataItemPrefab;

    private void Start()
    {
        Invoke("UpdateLobbyInfo", 5f);
    }

    public async void UpdateLobbyInfo()
    {
        while (Application.isPlaying)
        {
            if (string.IsNullOrWhiteSpace(LobbyDataManager.Instance.LobbyID))
            {
                Debug.Log("Lobby id is null or empty");
                await Task.Delay(1000);
                continue;
            }

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(LobbyDataManager.Instance.LobbyID);

            foreach (Transform t in playerDataParrent)
            {
                Destroy(t.gameObject);
            }

            foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
            {
                Transform newPlayerItem = Instantiate(playerDataItemPrefab, playerDataParrent);

                if (newPlayerItem == null)
                {
                    Debug.Log("New player Item : Null");
                    continue;
                }

                string playerName = "Missing";
                string playerTeam = "-1";
                string playerReady = "false";

                if (player.Data != null)
                {
                    playerName = player.Data.ContainsKey("Name") ? player.Data["Name"].Value : "Missing";
                    playerTeam = player.Data.ContainsKey("Team") ? player.Data["Team"].Value : "-1";
                    playerReady = player.Data.ContainsKey("Ready") ? player.Data["Ready"].Value : "false";
                }
                else
                {
                    Debug.Log("Player data is Null");
                }

                newPlayerItem.GetComponent<PlayerLobbyItem>().UpdatePlayerData(
                    playerName,
                    playerTeam,
                    bool.Parse(playerReady));
            }

            await Task.Delay(1000);
        }
    }
}
