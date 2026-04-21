using System;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] private ItemBaseStat itemStat;
    public ItemBaseStat ItemStat { get { return itemStat; } }

    [SerializeField] private Transform itemModel;

    public event Action<Item> OnCollected;
    private Vector3 previousPosition;

    [SerializeField] protected int currentItemValue;
    [SerializeField] protected bool alreadyCollected = false;

    public override void OnNetworkSpawn()
    {
        if (itemStat is ValueItemBaseStat valueItemBaseStat)
        {
            SetValue(valueItemBaseStat.MoneyValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectValueItemServerRpc(ServerRpcParams rpcParams = default)
    {
        if (alreadyCollected)
        {
            return;
        }

        alreadyCollected = true;
        OnCollected?.Invoke(this); 
        HideItemClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectWeaponItemServerRpc()
    {
        if (alreadyCollected)
        {
            return;
        }

        alreadyCollected = true;
        HideItemClientRpc();
    }

    public void SetValue(int value)
    {
        currentItemValue = value;
    }

    [ClientRpc]
    private void HideItemClientRpc()
    {
        Show(false);
    }

    protected void Show(bool show)
    {
        itemModel.gameObject.SetActive(show);
    }
}