using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : Character 
{
    [Header("Player Component")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera playerCam;

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public override void OnNetworkSpawn()
    {
        // Initialize character stat handle
        characterStatHandle = new();

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        attackHitBox.SetActive(false);

        if (IsOwner)
        {
            inputReader.OnMoved += Move;
            //inputReader.OnSprinted += Move;
            inputReader.OnJumped += Jump;
            inputReader.OnInteracted += Interact;
            inputReader.OnAttacked += Attack;

            playerCam.Priority = ownerPriority;
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

        /*if (Input.GetKeyDown(KeyCode.E))
        {
            Heal(10);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(25);
        }*/

        GroundCheck();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        //Debug.Log("Player FixedUpdate");
        HandleMove();
        RotatorToCam();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;

        if (other.TryGetComponent(out Item item))
        {
            if (item.ItemStat is ValueItemBaseStat)
            {
                int collectedValue = item.CollectValueItem();

                if (IsServer)
                {
                    CurrentMoney.Value += collectedValue;
                    Debug.Log($"Player collected item: {collectedValue}, Current Money: {CurrentMoney.Value}");
                }
            }
            else if (item.ItemStat is WeaponItemBaseStat weaponItem && !isEquipeWeapon)
            {
                item.CollectWeaponItem();
                equippedWeapon = weaponItem.WeaponPrefab;
                isEquipeWeapon = true;

                GameObject weapon = Instantiate(weaponItem.WeaponPrefab, Vector3.zero, Quaternion.identity, weaponHoldPoint);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
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