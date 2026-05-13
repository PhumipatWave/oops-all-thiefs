using UnityEngine;

[CreateAssetMenu(fileName = "WeaponRegistry", menuName = "Game/WeaponRegistry")]
public class WeaponRegistry : ScriptableObject
{
    public GameObject[] weaponPrefabs;

    public int GetIndex(GameObject prefab)
    {
        for (int i = 0; i < weaponPrefabs.Length; i++)
            if (weaponPrefabs[i] == prefab) return i;
        return -1;
    }

    public GameObject GetPrefab(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Length) return null;
        return weaponPrefabs[index];
    }
}