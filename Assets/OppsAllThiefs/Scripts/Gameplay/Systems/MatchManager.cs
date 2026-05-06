using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public float matchDuration = 180f;

    public NetworkVariable<float> timeLeft = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> matchEnded = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool running = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartMatch();
        }
    }

    public void StartMatch()
    {
        timeLeft.Value = matchDuration;
        matchEnded.Value = false;
        running = true;
    }

    private void Update()
    {
        if (!IsServer || !running) return;
        if (matchEnded.Value) return;

        timeLeft.Value -= Time.deltaTime;

        if (timeLeft.Value <= 0)
        {
            timeLeft.Value = 0;
            EndMatch();
        }
    }

    private IEnumerator LeaveLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        LeaveGame();
    }


    private void EndMatch()
    {
        running = false;
        matchEnded.Value = true;

        StartCoroutine(LeaveLobbyAfterDelay(10f));
        Debug.Log("Match Ended!");
    }

    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostHandler.Instance.GameManager.Shutdown();
        }

        ClientHandler.Instance.GameManager.Disconnect();
    }
}