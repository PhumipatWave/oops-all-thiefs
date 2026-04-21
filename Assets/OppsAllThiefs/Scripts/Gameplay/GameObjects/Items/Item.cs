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

    public int CollectValueItem()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }

        if (alreadyCollected)
        {
            return 0;
        }

        alreadyCollected = true;
        OnCollected?.Invoke(this);
        return currentItemValue;
    }

    public void CollectWeaponItem()
    {
        if (!IsServer)
        {
            Show(false);
            return;
        }

        if (alreadyCollected)
        {
            return;
        }
    }

    public void SetValue(int value)
    {
        currentItemValue = value;
    }

    protected void Show(bool show)
    {
        itemModel.gameObject.SetActive(show);
    }
}