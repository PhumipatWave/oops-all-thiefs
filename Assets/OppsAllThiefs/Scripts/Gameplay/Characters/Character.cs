using UnityEngine;

public abstract class Character : IMoveable, IAttackable, IHealthable
{
    protected CharacterBaseStat baseStat;
    protected CharacterStatHandle characterStatHandle;

    protected Rigidbody rb;
    protected Animator anim;

    protected int currentHealth;
    protected int currentMoney;

    public virtual void Move(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Attack()
    {
        throw new System.NotImplementedException();
    }

    public void Heal()
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage()
    {
        throw new System.NotImplementedException();
    }

    public void Death()
    {
        throw new System.NotImplementedException();
    }
}
