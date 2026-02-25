using UnityEngine;

public class CharacterStatHandle
{
    public int ModifyStat(int curHealth, int maxStat, int amount)
    {
        if (curHealth <= 0) return 0;
        return Mathf.Clamp(curHealth + amount, 0, maxStat);
    }
}
