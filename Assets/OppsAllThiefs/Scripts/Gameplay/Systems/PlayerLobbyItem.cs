using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerColorImage;

    [SerializeField] private string teamIndex;
    [SerializeField] private bool isReady;

    public void UpdatePlayerData(string playerName, string teamIndex, bool isReady)
    {
        playerNameText.text = playerName;
        this.teamIndex = teamIndex;
        this.isReady = isReady;
    }
}
