using UnityEngine;

public interface IHealthable
{
    public void Heal(int amount);
    public void TakeDamage(int amount);
    public void Death();
}
