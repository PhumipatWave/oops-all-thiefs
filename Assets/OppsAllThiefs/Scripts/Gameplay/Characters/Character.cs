using Unity.Netcode;
using UnityEngine;

public abstract class Character : NetworkBehaviour, IMoveable, IAttackable, IHealthable
{
    [SerializeField] protected CharacterBaseStat charStat;
    protected CharacterStatHandle characterStatHandle;

    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator anim;

    protected Vector3 previousMovementInput;

    protected int currentHealth => charStat.MaxHealth;
    protected int currentMoveSpeed => charStat.MinMoveSpeed;
    protected int currentJumpForce => charStat.JumpForce;

    protected int currentRotateSpeed;
    protected int currentMoney;

    protected bool isGrounded;

    public override void OnNetworkSpawn()
    {
        characterStatHandle = new();

        rb = GetComponent<Rigidbody>();
        //anim = GetComponent<Animator>();

        Debug.Log("Character network spawn");
        Debug.Log($"Cur speed : {currentMoveSpeed}, Max speed : {charStat.MaxMoveSpeed}");
    }

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

    public void MoveRotator(Vector2 dir, Transform transform)
    {
        if (dir == Vector2.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotateSpeed * Time.deltaTime);
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
