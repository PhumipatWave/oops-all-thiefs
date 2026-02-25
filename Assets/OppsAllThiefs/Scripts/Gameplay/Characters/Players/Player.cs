using Unity.Cinemachine;
using UnityEngine;

public class Player : Character 
{
    [Header("Player Component")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("UI")]
    [SerializeField] private GameObject healthBar;

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        base.OnNetworkSpawn();

        healthBar.SetActive(false);

        inputReader.OnMoved += Move;
        //inputReader.OnSprinted += Move;
        inputReader.OnJumped += Jump;
        inputReader.OnInteracted += Interact;
        inputReader.OnAttacked += Attack;

        playerCamera.Priority = ownerPriority;

        Debug.Log("Player network spawned");
    }

    public override void OnNetworkDespawn()
    {
        inputReader.OnMoved -= Move;
        //inputReader.OnSprinted -= Move;
        inputReader.OnJumped -= Jump;
        inputReader.OnInteracted -= Interact;
        inputReader.OnAttacked -= Attack;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Debug.Log("Player FixedUpdate");
        HandleMove();
    }
}