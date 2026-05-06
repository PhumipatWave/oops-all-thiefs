using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private Item[] itemPrefabs;
    [SerializeField] private int maxItems = 20;

    [SerializeField] private Vector2 xRange = new Vector2(-75, 75);
    [SerializeField] private Vector2 zRange = new Vector2(-75, 75);

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float checkRadius = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        for (int i = 0; i < maxItems; i++)
        {
            SpawnItem();
        }
    }

    private void SpawnItem()
    {
        Item prefab = GetRandomPrefab();
        Vector3 pos = GetSpawnPoint();

        Item item = Instantiate(prefab, pos, Quaternion.identity);
        item.NetworkObject.Spawn();

        item.OnCollected += HandleCollected;
    }

    private void HandleCollected(Item item)
    {
        Vector3 newPos = GetSpawnPoint();
        StartCoroutine(RespawnRoutine(item, newPos));
    }

    private IEnumerator RespawnRoutine(Item item, Vector3 pos)
    {
        yield return new WaitForSeconds(0.1f);
        item.ResetItem(pos);
    }

    private Item GetRandomPrefab()
    {
        return itemPrefabs[Random.Range(0, itemPrefabs.Length)];
    }

    private Vector3 GetSpawnPoint()
    {
        while (true)
        {
            float x = Random.Range(xRange.x, xRange.y);
            float z = Random.Range(zRange.x, zRange.y);

            Vector3 pos = new Vector3(x, 0, z);

            if (Physics.OverlapSphere(pos, checkRadius, obstacleMask).Length == 0)
                return pos;
        }
    }
}