using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : Character 
{
    [Header("Player Component")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera playerCamera;

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public override void OnNetworkSpawn()
    {
        // Initialize character stat handle
        characterStatHandle = new();

        rb = GetComponent<Rigidbody>();
        //anim = GetComponent<Animator>();

        if (IsOwner)
        {
            inputReader.OnMoved += Move;
            //inputReader.OnSprinted += Move;
            inputReader.OnJumped += Jump;
            inputReader.OnInteracted += Interact;
            inputReader.OnAttacked += Attack;

            playerCamera.Priority = ownerPriority;
        }

        if (IsServer)
            CurrentHealth.Value = MaxHealth;

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
        MoveRotator();
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
}