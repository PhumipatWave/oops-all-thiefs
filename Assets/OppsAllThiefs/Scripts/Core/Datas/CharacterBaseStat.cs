using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBaseStat", menuName = "ScriptableObects/CharacterBaseStat")]
public class CharacterBaseStat : ScriptableObject
{
    public int MaxHealth;

    public int MinMoveSpeed;
    public int MaxMoveSpeed;

    public int MaxMoney;
}
