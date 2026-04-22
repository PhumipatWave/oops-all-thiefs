using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;

public abstract class Character : NetworkBehaviour, IMoveable, IAttackable, IHealthable
{
    [Header("Character Status")]
    [SerializeField] protected CharacterBaseStat charStat;
    protected CharacterStatHandle characterStatHandle;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;

    protected Vector3 previousMovementInput;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> CurrentMoney = new NetworkVariable<int>();

    public event Action OnDeath;

    public int MaxHealth => charStat.MaxHealth;

    protected int currentMoveSpeed => charStat.MinMoveSpeed;
    protected int currentJumpForce => charStat.JumpForce;
    protected int currentRotateSpeed => charStat.MaxRotateSpeed;

    [SerializeField] protected Transform groundRayPoint;
    [SerializeField] protected bool isGrounded;
    [SerializeField] protected float groundDistance = 0.4f;
    [SerializeField] protected LayerMask groundLayer;

    protected bool isEquipeWeapon;
    [SerializeField] protected Transform weaponHoldPoint;

    [SerializeField] protected GameObject equippedWeapon;
    //[SerializeField] protected WeaponItemBaseStat equippedWeaponStat;

    [SerializeField] protected GameObject attackHitBox;

    protected bool isKnockBack;
    [SerializeField] protected float knockBackDuration = 0.5f;

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundRayPoint.position, groundRayPoint.position + Vector3.down * groundDistance);
    }

    protected void GroundCheck()
    {
        isGrounded = Physics.Raycast(groundRayPoint.position, Vector3.down, groundDistance, groundLayer);
    }

    public void Move(Vector2 dir)
    {
        previousMovementInput = dir;
    }

    protected void HandleMove()
    {
        if (isKnockBack) 
            return;

        Vector3 moveDir = transform.forward * previousMovementInput.y
            + transform.right * previousMovementInput.x;
        rb.linearVelocity = new Vector3(moveDir.x * currentMoveSpeed, rb.linearVelocity.y, moveDir.z * currentMoveSpeed);

        float moveValue = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        anim.SetFloat("velX", moveValue);

        //Debug.Log($"Player move {moveDir}");
    }

    protected void UpdateAnimation()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isJumping", !isGrounded);
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
        if (isGrounded && isEquipeWeapon)
        {
            Debug.Log("Player attack");
            anim.SetTrigger("isAttack");
            EnableAttackHitBoxServerRpc();
        }
    }

    [ServerRpc]
    public void EnableAttackHitBoxServerRpc()
    {
        EnableAttackHitBoxClientRpc();
    }

    [ClientRpc]
    public void EnableAttackHitBoxClientRpc()
    {
        attackHitBox.SetActive(true);
        // Auto disable after attack duration
        StartCoroutine(DisableHitBoxAfterDelay(0.3f));
    }

    private System.Collections.IEnumerator DisableHitBoxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackHitBox.SetActive(false);
    }

    public void Heal(int amount)
    {
        //CurrentHealth.Value = characterStatHandle.ModifyStat(CurrentHealth.Value, charStat.MaxHealth, amount);

        ModifyStatServerRpc(amount);
    }

    public void TakeDamage(int amount, Vector3 dir)
    {
        //CurrentHealth.Value = characterStatHandle.ModifyStat(CurrentHealth.Value, charStat.MaxHealth, -amount);

        ModifyStatServerRpc(-amount);
        KnockbackServerRpc(dir);
    }

    // Test Modify
    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
    public void KnockbackServerRpc(Vector3 direction)
    {
        Vector3 knockDir = new Vector3(direction.x, 0f, direction.z).normalized;
        rb.AddForce(knockDir * 30f, ForceMode.Impulse);

        Debug.Log($"Knock back dir : {knockDir * 30f}");

        KnockbackClientRpc(direction);
    }

    [ClientRpc]
    public void KnockbackClientRpc(Vector3 direction)
    {
        StartCoroutine(KnockbackRoutine());
    }

    protected IEnumerator KnockbackRoutine()
    {
        isKnockBack = true;
        yield return new WaitForSeconds(knockBackDuration);
        isKnockBack = false;
    }

    public void Death()
    {
        OnDeath?.Invoke();
    }
}
