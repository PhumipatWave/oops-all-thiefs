using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [SerializeField] private float moveSpeed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        rb.linearVelocity = move * moveSpeed;
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("Attack!");
        }
    }
}
