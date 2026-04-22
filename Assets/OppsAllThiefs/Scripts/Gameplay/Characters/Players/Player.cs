using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : Character 
{
    [Header("Player Component")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CinemachineCamera playerCam;

    [SerializeField] private WeaponRegistry weaponRegistry;

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
        if (!IsOwner)
            return;
        
        inputReader.OnMoved -= Move;
        //inputReader.OnSprinted -= Move;
        inputReader.OnJumped -= Jump;
        inputReader.OnInteracted -= Interact;
        inputReader.OnAttacked -= Attack;
    }

    private void Start()
    {
        Debug.Log($"Owner? {IsOwner} | ClientId: {OwnerClientId}");
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
            if (item.ItemStat is ValueItemBaseStat valueItem)
            {
                item.CollectValueItemServerRpc();
                AddMoneyServerRpc(valueItem.MoneyValue);
            }
            else if (item.ItemStat is WeaponItemBaseStat weaponItem && !isEquipeWeapon)
            {
                int weaponIndex = weaponRegistry.GetIndex(weaponItem.WeaponPrefab);
                if (weaponIndex < 0)
                {
                    Debug.LogError("Weapon not found in WeaponRegistry!");
                    return;
                }

                isEquipeWeapon = true;

                item.CollectWeaponItemServerRpc();

                // Tell server → broadcast to ALL clients to spawn weapon locally
                EquipWeaponServerRpc(weaponIndex);
            }
        }

        if (other.CompareTag("HitBox"))
        {
            // Make sure we don't hit ourselves
            // Check the hitbox doesn't belong to this player
            if (other.transform.root != transform)
            {
                Debug.Log("Player hit by another player's attack");
                Vector3 knockbackDir = (transform.position - other.transform.position).normalized;

                TakeDamage(10, knockbackDir);
            }
        }
    }

    [ServerRpc]
    private void AddMoneyServerRpc(int amount)
    {
        CurrentMoney.Value += amount;
    }

    [ServerRpc]
    private void EquipWeaponServerRpc(int weaponIndex)
    {
        EquipWeaponClientRpc(weaponIndex);
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(int weaponIndex)
    {
        GameObject prefab = weaponRegistry.GetPrefab(weaponIndex);
        if (prefab == null)
        {
            Debug.LogError($"No prefab at index {weaponIndex} in WeaponRegistry");
            return;
        }

        // Destroy old weapon if any
        if (equippedWeapon != null)
            Destroy(equippedWeapon);

        // Spawn directly under weaponHoldPoint (your hand bone Transform)
        equippedWeapon = Instantiate(prefab, weaponHoldPoint);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        Debug.Log($"Weapon equipped on {gameObject.name}: {prefab.name}");
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