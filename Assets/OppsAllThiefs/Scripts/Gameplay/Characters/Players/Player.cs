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

            // Set player name on network spawn.
            PlayerName.Value = userData != null ? userData.UserName : "Missing name";
            Debug.Log($"Player name : {PlayerName.Value}");
            CurrentHealth.Value = MaxHealth;
        }

        if (IsOwner)
        {
            // Mapping input to owner player.
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

        if (item.ItemStat is ValueItemBaseStat valueItem)
        {
            CurrentMoney.Value += valueItem.MoneyValue;

            item.CollectValueItemServerRpc();
            return;
        }

        if (item.ItemStat is WeaponItemBaseStat weaponItem)
        {
            if (hasWeapon.Value) return;

            hasWeapon.Value = true;

            int index = weaponRegistry.GetIndex(weaponItem.WeaponPrefab);

            item.CollectWeaponItemServerRpc();
            EquipWeaponClientRpc(index);
        }
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

        if (equippedWeapon != null)
            Destroy(equippedWeapon);

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