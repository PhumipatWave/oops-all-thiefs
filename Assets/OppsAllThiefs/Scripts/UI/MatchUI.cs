using Unity.Netcode;
using UnityEngine;
using TMPro;

public class MatchUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI moneyResultText;
    [SerializeField] private GameObject resultPanel;

    [SerializeField] private MatchManager matchManager;

    void Update()
    {
        if (matchManager == null) return;

        float time = matchManager.timeLeft.Value;

        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);

        timerText.text = $"{min:00}:{sec:00}";

        if (matchManager.matchEnded.Value)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

            if (player != null)
            {
                Character character = player.GetComponent<Character>();

                if (character != null)
                {
                    moneyResultText.text = $"Your Money: {character.CurrentMoney.Value}";
                }
            }

            resultPanel.SetActive(true);
        }
    }
}