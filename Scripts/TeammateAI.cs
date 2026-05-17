using UnityEngine;
using UnityEngine.AI;

public class TeammateAI : MonoBehaviour
{
    [Header("Auto-assigned at runtime")]
    private Transform playerTransform;
    private BallController ballController;
    private NavMeshAgent navAgent;
    private Animator animator;

    [Header("Formation Settings")]
    public Vector3 formationOffset = new Vector3(5f, 0f, -3f);
    public float runSpeed = 5f;
    public float formationSpeed = 4f;

    private bool isActive = true;
    private float timer = 0f;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // Remove PlayerController
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) Destroy(pc);

        // AUTO FIND
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;
        ballController = FindAnyObjectByType<BallController>();

        if (navAgent != null)
        {
            navAgent.speed = formationSpeed;
            navAgent.acceleration = 10f;
            navAgent.stoppingDistance = 2f;
            navAgent.angularSpeed = 360f;
            navAgent.autoBraking = true;
        }

        Debug.Log(name + " TeammateAI started. Player: "
            + (playerTransform != null ? "Found" : "NOT FOUND"));
    }

    void Update()
    {
        if (!isActive || playerTransform == null) return;
        timer += Time.deltaTime;
        if (timer < 0.15f) return;
        timer = 0f;

        MoveInFormation();
        UpdateAnimator();
    }

    void MoveInFormation()
    {
        // Calculate world position based on formation offset
        // Use player's rotation to determine formation direction
        Vector3 targetPos = playerTransform.position
            + (playerTransform.right * formationOffset.x)
            + (playerTransform.forward * formationOffset.z);

        // If player has ball, move closer to support
        if (ballController != null && ballController.isPossessed
            && ballController.currentHolder == playerTransform)
        {
            navAgent.speed = runSpeed;
            // Run into space ahead
            targetPos = playerTransform.position
                + (playerTransform.right * formationOffset.x)
                + (playerTransform.forward * (formationOffset.z + 5f));
        }
        else
        {
            navAgent.speed = formationSpeed;
        }

        // Clamp to field
        targetPos.x = Mathf.Clamp(targetPos.x, -55f, 55f);
        targetPos.z = Mathf.Clamp(targetPos.z, -70f, 40f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            float dist = Vector3.Distance(transform.position, hit.position);
            if (dist > 2f)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(hit.position);
            }
            else
                navAgent.isStopped = true;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null || navAgent == null) return;
        animator.SetFloat("Speed",
            navAgent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    public void DisableAI()
    {
        isActive = false;
        if (navAgent != null) navAgent.isStopped = true;
    }

    public void EnableAI()
    {
        isActive = true;
        if (navAgent != null) navAgent.isStopped = false;
    }
}