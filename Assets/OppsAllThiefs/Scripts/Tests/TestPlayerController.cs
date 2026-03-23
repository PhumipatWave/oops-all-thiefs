using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Ground Check")]
    public float groundDistance = 0.4f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Animator anim;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Move();
        GroundCheck();
        Jump();
        UpdateAnimation();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v).normalized;

        // เคลื่อนที่
        rb.linearVelocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.z * moveSpeed);

        // หมุนตามทิศ
        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }

        // 🔥 ส่งค่าไป Blend Tree
        float speed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        anim.SetFloat("velX", speed);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance, groundLayer);
    }

    void UpdateAnimation()
    {
        // 👇 ตรงกับ Animator ของจาวิต
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isJumping", !isGrounded);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundDistance);
    }
}