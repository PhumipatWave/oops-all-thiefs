using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyDataManager : MonoBehaviour
{
    public string LobbyID;
    public Lobby CurLobby;
    public string PlayerTeamIndex = "-1";

    public static LobbyDataManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayerTeamIndex()
    {
        if (string.IsNullOrEmpty(LobbyID))
            return;

        try
        {
            var data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "Team",
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        PlayerTeamIndex)
                }
            };

            LobbyService.Instance.UpdatePlayerAsync(
                LobbyID,
                AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = data
                });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
