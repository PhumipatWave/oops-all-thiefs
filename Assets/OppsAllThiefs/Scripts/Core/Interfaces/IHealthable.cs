using UnityEngine;

public interface IHealthable
{
    public void Heal(int amount);
    public void TakeDamage(int amount, Vector3 dir);
    public void Death();
}
