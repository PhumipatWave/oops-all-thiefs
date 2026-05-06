using System;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] private ItemBaseStat itemStat;
    public ItemBaseStat ItemStat => itemStat;

    [SerializeField] private Transform itemModel;

    public event Action<Item> OnCollected;

    [SerializeField] private int currentItemValue;
    public bool alreadyCollected;

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

        OnCollected?.Invoke(this);

        HideClientRpc();
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

    public void ResetItem(Vector3 newPos)
    {
        alreadyCollected = false;

        transform.position = newPos;

        ShowClientRpc();
    }

    public void SetValue(int value)
    {
        currentItemValue = value;
    }
}