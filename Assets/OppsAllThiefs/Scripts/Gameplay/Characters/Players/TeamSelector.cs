using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SelectionButton 
{
    public Button teamButton;
    public GameObject selectionBox;
    public Color color;
}

public class TeamSelector : MonoBehaviour
{
    [SerializeField] private TeamColorData teamColorData;
    [SerializeField] private Material playerMesh;
    [SerializeField] private SelectionButton[] selectionButtons;
    [SerializeField] private int teamIndex = 0;

    private void OnValidate()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            selectionButtons[i].color = (Color)teamColorData.GetTeamColor(i);
        }

        foreach (SelectionButton selection in selectionButtons)
        {
            selection.teamButton.GetComponent<Image>().color = selection.color;
        }
    }

    private void Start()
    {
        LobbyDataManager.Instance.PlayerTeamIndex = teamIndex.ToString();
        LobbyDataManager.Instance.UpdatePlayerTeamIndex();

        Invoke("SetPlayerMat", 3f);
    }

    private void SetPlayerMat()
    {
        foreach (var player in LobbyDataManager.Instance.CurLobby.Players)
        {
            string playerId = player.Id;

            Player playerObj = FindPlayerById(playerId);

            if (playerObj != null)
            {
                SkinnedMeshRenderer smr = playerObj.GetComponentInChildren<SkinnedMeshRenderer>();
                playerMesh = smr.material;
            }
        }
    }

    private Player FindPlayerById(string id)
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.playerId == id)
                return player;
        }
        return null;
    }

    public void HandleTeamChanged()
    {
        foreach (SelectionButton selection in selectionButtons)
        {
            selection.selectionBox.SetActive(false);
        }

        playerMesh.color = selectionButtons[teamIndex].color;

        selectionButtons[teamIndex].selectionBox.SetActive(true);
    }

    public void SelectTeam(int teamIndex)
    {
        this.teamIndex = teamIndex;
        LobbyDataManager.Instance.PlayerTeamIndex = teamIndex.ToString();
        LobbyDataManager.Instance.UpdatePlayerTeamIndex();
        HandleTeamChanged();
    }
}
