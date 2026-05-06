using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Character : NetworkBehaviour, IMoveable, IAttackable, IHealthable
{
    [Header("Component Reference")]
    [SerializeField] protected CharacterBaseStat charStat;
    protected CharacterStatHandle characterStatHandle;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;

    [Header("Network Variable")]
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    public NetworkVariable<int> CurrentMoney = new NetworkVariable<int>();
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);

    public event Action OnDeath;

    public int MaxHealth => charStat.MaxHealth;

    protected int currentMoveSpeed => charStat.MinMoveSpeed;
    protected int currentJumpForce => charStat.JumpForce;
    protected int currentRotateSpeed => charStat.MaxRotateSpeed;

    protected Vector3 previousMovementInput;


    [Header("Ground Check Reference")]
    [SerializeField] protected Transform groundRayPoint;
    [SerializeField] protected bool isGrounded;
    [SerializeField] protected float groundDistance = 0.4f;
    [SerializeField] protected LayerMask groundLayer;

    [Header("Combat Reference")]
    public NetworkVariable<int> weaponDurability = new(3);
    public NetworkVariable<bool> hasWeapon = new(false);
    [SerializeField] protected Transform weaponHoldPoint;
    [SerializeField] protected GameObject equippedWeapon;
    [SerializeField] protected GameObject attackHitBox;

    protected bool isAttacking;
    private float lastAttackTime;

    protected bool isKnockBack;
    [SerializeField] protected float knockBackPower = 50.5f;
    [SerializeField] protected float knockBackUpPower = 25f;
    [SerializeField] protected float knockBackDuration = 0.8f;


    /// <OnDrawGizmos>
    /// Draw gizmos to check ground lenght.
    /// </OnDrawGizmos>
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
        if (IsDead.Value) return;
        if (isKnockBack) return;

        Vector3 moveDir = transform.forward * previousMovementInput.y
            + transform.right * previousMovementInput.x;

        rb.linearVelocity = new Vector3(moveDir.x * currentMoveSpeed, rb.linearVelocity.y, moveDir.z * currentMoveSpeed);

        float moveValue = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        anim.SetFloat("velX", moveValue);
    }

    protected void UpdateAnimation()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isJumping", !isGrounded);
    }

    /// <MoveRotator>
    /// Rotate the player to the movement direction.
    /// </MoveRotator>
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
        if (IsDead.Value) return;
        if (!isGrounded) return;

        Vector3 velocity = new Vector3(rb.linearVelocity.x, currentJumpForce, rb.linearVelocity.z);
        rb.linearVelocity = velocity;
    }

    public void Interact()
    {
        Debug.Log($"Player interact");
    }

    public void Attack()
    {
        if (!isGrounded) return;
        if (!hasWeapon.Value) return;
        if (isAttacking) return;
        if (IsDead.Value) return;

        if (Time.time - lastAttackTime < 0.4f) return;
        lastAttackTime = Time.time;

        isAttacking = true;

        anim.SetTrigger("isAttack");

        EnableAttackHitBoxServerRpc();
        ReduceWeaponDurabilityServerRpc();

        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    [ServerRpc]
    private void ReduceWeaponDurabilityServerRpc()
    {
        weaponDurability.Value--;

        if (weaponDurability.Value <= 0)
        {
            RemoveWeapon();
        }
    }

    private void RemoveWeapon()
    {
        hasWeapon.Value = false;
        weaponDurability.Value = 0;
        DestroyWeaponClientRpc();
    }

    [ClientRpc]
    private void DestroyWeaponClientRpc()
    {
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
            equippedWeapon = null;
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

    private IEnumerator DisableHitBoxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackHitBox.SetActive(false);
    }

    public void Heal(int amount)
    {
        ModifyStatServerRpc(amount);
    }

    public void TakeDamage(int amount, Vector3 dir)
    {
        ModifyStatServerRpc(-amount);
        KnockbackServerRpc(dir);
    }

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
        KnockbackClientRpc(direction);
    }

    [ClientRpc]
    public void KnockbackClientRpc(Vector3 direction)
    {
        Vector3 knockDir = new Vector3(direction.x, knockBackUpPower, direction.z).normalized;
        rb.AddForce(knockDir * knockBackPower, ForceMode.Impulse);

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
        if (!IsServer) return;
        if (IsDead.Value) return;

        IsDead.Value = true;
        OnDeath?.Invoke();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        Debug.Log($"Player {OwnerClientId} died. Respawning soon...");

        // freeze player while dead
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(3f);

        int spawnIndex = (int)OwnerClientId;
        Vector3 respawnPos = PlayerSpawnPoint.Instance.GetSpawnIndexPos(spawnIndex);

        RespawnPlayer(respawnPos);
    }

    private void RespawnPlayer(Vector3 pos)
    {
        CurrentHealth.Value = MaxHealth;

        hasWeapon.Value = false;
        weaponDurability.Value = 3;

        isKnockBack = false;
        isAttacking = false;

        previousMovementInput = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        rb.rotation = Quaternion.identity;

        transform.position = pos;
        transform.rotation = Quaternion.identity;

        Physics.SyncTransforms();

        ForceRespawnClientRpc(pos);

        StartCoroutine(ReleaseDeadFlag());
    }

    private IEnumerator ReleaseDeadFlag()
    {
        yield return new WaitForSeconds(0.2f);
        IsDead.Value = false;
    }

    [ClientRpc]
    private void ForceRespawnClientRpc(Vector3 pos)
    {
        StartCoroutine(ClientRespawnFix(pos));
    }

    private IEnumerator ClientRespawnFix(Vector3 pos)
    {
        yield return null; // wait one frame after rpc arrives

        previousMovementInput = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        rb.rotation = Quaternion.identity;

        transform.position = pos;
        transform.rotation = Quaternion.identity;

        Physics.SyncTransforms();
    }
}
