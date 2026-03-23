using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : Character 
{
    [Header("Player Component")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera playerCam;

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName = new();
    public NetworkVariable<int> PlayerIndex = new();

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        // Initialize character stat handle
        characterStatHandle = new();

        rb = GetComponent<Rigidbody>();
        //anim = GetComponent<Animator>();

        if (IsServer)
        {
            UserData userData = HostHandler.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.UserName;

            int playerIndex = GameObject.FindGameObjectsWithTag("Player").Length;
            PlayerIndex.Value = playerIndex - 1;

            CurrentHealth.Value = MaxHealth;
        }

        if (IsOwner)
        {
            inputReader.OnMoved += Move;
            //inputReader.OnSprinted += Move;
            inputReader.OnJumped += Jump;
            inputReader.OnInteracted += Interact;
            inputReader.OnAttacked += Attack;

            playerCam.Priority = ownerPriority;
        }

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

    // Test health
    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Heal(10);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(25);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Debug.Log("Player FixedUpdate");
        HandleMove();
        RotatorToCam();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Item item))
        {
            int collectedValue = item.Collect();

            if (IsServer)
            {
                CurrentMoney.Value += collectedValue;
            }
        }
    }

    private void RotatorToCam()
    {
        Vector3 camForward = playerCam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        if (camForward.sqrMagnitude > .01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotateSpeed * Time.fixedDeltaTime);

            rb.MoveRotation(smoothRotation);
        }
    }
}