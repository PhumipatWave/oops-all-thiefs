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

    [SerializeField] private float respawnDelay = 3f;

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

        item.Initialize(this);
    }

    public Vector3 GetNewRespawnPoint()
    {
        return GetSpawnPoint();
    }

    public float GetRespawnDelay()
    {
        return respawnDelay;
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