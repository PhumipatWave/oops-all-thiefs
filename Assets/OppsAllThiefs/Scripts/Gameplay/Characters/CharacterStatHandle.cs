using UnityEngine;

public class CharacterStatHandle
{
    public int ModifyStat(int curHealth, int maxStat, int amount)
        => Mathf.Clamp(curHealth + amount, 0, maxStat);
}
