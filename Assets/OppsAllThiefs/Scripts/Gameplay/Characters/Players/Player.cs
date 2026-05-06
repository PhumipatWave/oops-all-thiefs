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

    [SerializeField] private WeaponRegistry weaponRegistry;

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        // Initialize character stat handle
        characterStatHandle = new();

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        attackHitBox.SetActive(false);

        if (IsServer)
        {
            UserData userData = HostHandler.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            PlayerName.Value = userData != null ? userData.UserName : "Missing name";
            Debug.Log($"Player name : {PlayerName.Value}");
            CurrentHealth.Value = MaxHealth;
        }

        if (IsOwner)
        {
            inputReader.OnMoved += Move;
            inputReader.OnJumped += Jump;
            inputReader.OnInteracted += Interact;
            inputReader.OnAttacked += Attack;

            playerCam.Priority = ownerPriority;
        }

        OnPlayerSpawned?.Invoke(this);
        Debug.Log("Player network spawned");
    }

    public override void OnNetworkDespawn()
    {
        OnPlayerDespawned?.Invoke(this);

        if (!IsOwner)
            return; 

        inputReader.OnMoved -= Move;
        inputReader.OnJumped -= Jump;
        inputReader.OnInteracted -= Interact;
        inputReader.OnAttacked -= Attack;
    }

    private void Start()
    {
        Debug.Log($"Owner? {IsOwner} | ClientId: {OwnerClientId}");
    }

    private void Update()
    {
        if (!IsOwner) return;

        GroundCheck();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleMove();
        RotatorToCam();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;

        if (other.TryGetComponent(out Item item))
        {
            RequestPickupServerRpc(item.NetworkObject);
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
    private void RequestPickupServerRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out NetworkObject obj)) return;
        if (!obj.TryGetComponent(out Item item)) return;

        // MONEY ITEM
        if (item.ItemStat is ValueItemBaseStat valueItem)
        {
            CurrentMoney.Value += valueItem.MoneyValue;

            item.CollectValueItemServerRpc(); // hide / destroy item
            return;
        }

        // WEAPON ITEM
        if (item.ItemStat is WeaponItemBaseStat weaponItem)
        {
            if (hasWeapon.Value) return;

            hasWeapon.Value = true;

            int index = weaponRegistry.GetIndex(weaponItem.WeaponPrefab);

            item.CollectWeaponItemServerRpc();
            EquipWeaponClientRpc(index);
        }
    }

    [ServerRpc]
    private void AddMoneyServerRpc(int amount)
    {
        CurrentMoney.Value += amount;
    }

    [ServerRpc]
    private void EquipWeaponServerRpc(int weaponIndex, ServerRpcParams rpcParams = default)
    {
        if (hasWeapon.Value) return; 

        hasWeapon.Value = true;

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