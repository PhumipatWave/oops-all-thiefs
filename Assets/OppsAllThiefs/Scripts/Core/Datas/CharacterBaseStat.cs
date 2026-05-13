using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBaseStat", menuName = "ScriptableObjects/CharacterBaseStat")]
public class CharacterBaseStat : ScriptableObject
{
    public int MaxHealth;

    public int MinMoveSpeed;
    public int MaxMoveSpeed;

    public int MaxRotateSpeed;

    public int JumpForce;

    public int MaxMoney;
}
