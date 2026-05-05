using Unity.Netcode;
using UnityEngine;

public class NPC : NetworkBehaviour
{
    private Rigidbody rb;
    private Animator anim;

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 2f;

    private int currentIndex = 0; 
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (waypoints.Length == 0)
        {
            HandleWaiting();
            return;
        }

        anim.SetBool("isMove", !isWaiting);

        if (isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        Transform target = waypoints[currentIndex];

        Vector3 dir = (target.position - transform.position).normalized;

        rb.linearVelocity = dir * speed;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, rot, 360f * Time.fixedDeltaTime));
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < 0.2f)
        {
            isWaiting = true;
            waitTimer = waitTime;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            isWaiting = false;
            currentIndex += direction;

            if (currentIndex >= waypoints.Length)
            {
                direction = -1;
                currentIndex = waypoints.Length - 2;
            }
            else if (currentIndex < 0)
            {
                direction = 1;
                currentIndex = 1;
            }
        }
    }
}
