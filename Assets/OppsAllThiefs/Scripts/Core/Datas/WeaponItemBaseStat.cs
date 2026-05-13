using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItemBaseStat", menuName = "ScriptableObjects/WeaponItemBaseStat")]
public class WeaponItemBaseStat : ItemBaseStat
{
    public int Damage;
    public float AttackSpeed;
    public GameObject WeaponPrefab;
}
