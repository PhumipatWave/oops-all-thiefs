using Unity.Netcode;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class NPC : NetworkBehaviour
{
    private Rigidbody rb;
    private Animator anim;

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 2f;

    private int currentIndex = 0;

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
        anim.SetFloat("vel", rb.linearVelocity.magnitude);

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, rot, 5f * Time.fixedDeltaTime));
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance < 0.1f)
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

            currentIndex++;

            if (currentIndex >= waypoints.Length)
                currentIndex = 0;
        }
    }
}
