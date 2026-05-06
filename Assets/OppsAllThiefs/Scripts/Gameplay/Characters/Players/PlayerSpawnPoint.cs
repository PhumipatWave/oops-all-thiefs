using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public static PlayerSpawnPoint Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSpawnIndexPos(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Vector3.zero;

        if (index >= spawnPoints.Length)
            index = index % spawnPoints.Length;

        return spawnPoints[index].position;
    }

    public bool HasSpawnPoints()
    {
        return spawnPoints != null && spawnPoints.Length > 0;
    }
}