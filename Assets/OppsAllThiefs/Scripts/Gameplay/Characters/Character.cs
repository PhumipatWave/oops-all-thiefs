using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Character : NetworkBehaviour, IMoveable, IAttackable, IHealthable
{
    [SerializeField] protected CharacterBaseStat charStat;
    protected CharacterStatHandle characterStatHandle;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;
    public event Action OnDeath;

    protected Vector3 previousMovementInput;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> CurrentMoney = new NetworkVariable<int>();

    public int MaxHealth => charStat.MaxHealth;

    protected int currentMoveSpeed => charStat.MinMoveSpeed;
    protected int currentJumpForce => charStat.JumpForce;

    protected int currentRotateSpeed = 5;

    protected bool isGrounded;

    public void Move(Vector2 dir)
    {
        previousMovementInput = dir;
    }

    protected void HandleMove()
    {
        Vector3 moveDir = transform.forward * previousMovementInput.y
            + transform.right * previousMovementInput.x;
        rb.linearVelocity = moveDir * currentMoveSpeed;

        Debug.Log($"Player move {moveDir}");
    }

    /// <summary>
    /// Rotate the player to the movement direction.
    /// </summary>
    public void MoveRotator()
    {
        Vector3 moveDir = transform.forward * previousMovementInput.y
            + transform.right * previousMovementInput.x;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                currentRotateSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(smoothRotation);
        }
    }

    public void Jump()
    {
        if (isGrounded)
        {
            Vector3 velocity = new Vector3(rb.linearVelocity.x, currentJumpForce, rb.linearVelocity.z);
            rb.linearVelocity = velocity;

            Debug.Log($"Player jump");
        }
    }

    public void Interact()
    {
        Debug.Log($"Player interact");
    }

    public void Attack()
    {
        Debug.Log("Player attack");
    }

    public void Heal(int amount)
    {
        //CurrentHealth.Value = characterStatHandle.ModifyStat(CurrentHealth.Value, charStat.MaxHealth, amount);

        ModifyStatServerRpc(amount);
    }

    public void TakeDamage(int amount)
    {
        //CurrentHealth.Value = characterStatHandle.ModifyStat(CurrentHealth.Value, charStat.MaxHealth, -amount);

        ModifyStatServerRpc(-amount);
    }

    // Test Modify
    [ServerRpc]
    public void ModifyStatServerRpc(int amount)
    {
        if (CurrentHealth.Value <= 0) return;

        int newHealth = CurrentHealth.Value + amount;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
        Debug.Log($"Player health modify : {CurrentHealth.Value}");

        if (CurrentHealth.Value <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        OnDeath?.Invoke();
    }
}
