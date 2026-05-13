using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] private ItemBaseStat itemStat;
    public ItemBaseStat ItemStat => itemStat;

    [SerializeField] private Transform itemModel;

    [SerializeField] private int currentItemValue;
    public bool alreadyCollected;

    private ItemSpawner spawner;

    public void Initialize(ItemSpawner itemSpawner)
    {
        spawner = itemSpawner;
    }

    public override void OnNetworkSpawn()
    {
        if (itemStat is ValueItemBaseStat valueItem)
        {
            SetValue(valueItem.MoneyValue);
        }

        Show(true);
        alreadyCollected = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectValueItemServerRpc()
    {
        CollectInternal();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectWeaponItemServerRpc()
    {
        CollectInternal();
    }

    private void CollectInternal()
    {
        if (alreadyCollected) return;

        alreadyCollected = true;

        HideClientRpc();

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(spawner.GetRespawnDelay());

        Vector3 newPos = spawner.GetNewRespawnPoint();

        transform.position = newPos;

        alreadyCollected = false;

        ShowClientRpc();
    }

    [ClientRpc]
    private void HideClientRpc()
    {
        Show(false);
    }

    [ClientRpc]
    private void ShowClientRpc()
    {
        Show(true);
    }

    private void Show(bool state)
    {
        if (itemModel != null)
            itemModel.gameObject.SetActive(state);
    }

    public void SetValue(int value)
    {
        currentItemValue = value;
    }
}