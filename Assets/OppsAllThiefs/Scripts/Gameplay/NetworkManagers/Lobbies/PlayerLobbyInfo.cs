using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;

public class PlayerLobbyInfo
{
    private Dictionary<string, PlayerDataObject> playerLobbyData;

    public PlayerLobbyInfo(string playerName, string playerTeamIndex, string playerReady)
    {
        playerLobbyData = new Dictionary<string, PlayerDataObject>
        {
            { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { "Team", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerTeamIndex) },
            { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerReady) },
        };
    }

    public Dictionary<string, PlayerDataObject> GetPlayerLobbyData()
    {
        return playerLobbyData;
    }

    public void ClearPlayerLobbyData()
    {
        playerLobbyData = null;
    }
}
