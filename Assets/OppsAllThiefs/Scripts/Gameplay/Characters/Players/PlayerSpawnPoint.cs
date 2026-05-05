using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    private static List<PlayerSpawnPoint> spawnPoints = new();

    private void OnEnable()
    {
        spawnPoints.Add(this);
    }

    private void OnDisable()
    {
        spawnPoints.Remove(this);
    }

    public static Vector3 GetSpawnIndexPos()
    {
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index].transform.position;
    }

    public static bool HasSpawnPoints()
    {
        return spawnPoints.Count > 0;
    }
}
