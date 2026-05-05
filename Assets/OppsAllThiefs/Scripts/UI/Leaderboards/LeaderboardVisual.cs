using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color ownerColor = Color.blue;
    private FixedString32Bytes playerName;

    public ulong ClientId {  get; private set; }
    public int Moneys { get; private set; }

    public void Initialize(ulong clientId, FixedString32Bytes playerName, int moneys)
    {
        ClientId = clientId;
        this.playerName = playerName;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = ownerColor;
        }

        UpdateMoneys(moneys);
    }

    public void UpdateMoneys(int moneys)
    {
        Moneys = moneys;
        UpdateTextDisplay();
    }

    public void UpdateTextDisplay()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {playerName} ({Moneys})";
    }
}
