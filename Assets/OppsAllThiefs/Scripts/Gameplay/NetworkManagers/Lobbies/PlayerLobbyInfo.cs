using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobbyInfo
{
    private Unity.Services.Lobbies.Models.Player playerLobbyData;

    public PlayerLobbyInfo(string playerName, string playerColor, string playerReady)
    {
        PlayerDataObject playerDO_name = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName);
        PlayerDataObject playerDO_color = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerColor);
        PlayerDataObject playerDO_isReady = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerReady);

        playerLobbyData = new Unity.Services.Lobbies.Models.Player(id: AuthenticationService.Instance.PlayerId, data: new Dictionary<string, PlayerDataObject>
        {
            { "Name", playerDO_name },
            { "Color", playerDO_color },
            { "Ready", playerDO_isReady },
        });
    }

    public Unity.Services.Lobbies.Models.Player GetPlayerLobbyData()
    {
        return playerLobbyData;
    }

    public async void UpdateLobbyInfo()
    {

    }

    public void ClearPlayerLobbyData()
    {
        playerLobbyData = null;
    }
}
