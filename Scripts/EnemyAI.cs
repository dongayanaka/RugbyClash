// ============================================================
// EnemyAI.cs  — FIXED (static FieldBoundary)
// ============================================================
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyRole { Chaser, Marker, Defender }
    public enum AIState { Idle, Chase, Mark, Intercept, Tackle, Retreat, CarryBall }

    [Header("Role")]
    public EnemyRole role = EnemyRole.Chaser;

    [Header("References")]
    public Transform playerTransform;
    public BallController ballController;
    public Animator animator;

    [Header("Detection")]
    public float detectionRadius = 22f;
    public float tackleDistance = 1.6f;

    [Header("NavMesh")]
    public float chaseSpeed = 4.5f;
    public float interceptSpeed = 5.5f;
    public float retreatSpeed = 3.5f;

    [Header("Marking")]
    public Transform markTarget;

    private NavMeshAgent agent;
    private Rigidbody playerRb;
    private AIState currentState = AIState.Idle;
    private bool isTackling = false;
    private bool isActive = true;
    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.12f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = animator != null ? animator : GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.acceleration = 9f;
            agent.stoppingDistance = 1.2f;
            agent.angularSpeed = 360f;
            agent.radius = 0.35f;
            agent.height = 1.8f;
        }

        if (playerTransform != null)
            playerRb = playerTransform.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isActive || isTackling) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < UPDATE_INTERVAL) return;
        updateTimer = 0f;

        CheckIfWeHaveBall();
        DecideState();
        ExecuteState();
        SyncAnimator();
        ClampToBoundary();
    }

    void CheckIfWeHaveBall()
    {
        bool weBall = ballController != null
            && ballController.isPossessed
            && ballController.currentHolder == transform;

        if (weBall && currentState != AIState.CarryBall) currentState = AIState.CarryBall;
        if (!weBall && currentState == AIState.CarryBall) currentState = AIState.Chase;
    }

    void DecideState()
    {
        if (currentState == AIState.CarryBall) return;

        Transform ballCarrier = ballController?.currentHolder;
        float distToCarrier = ballCarrier != null
            ? Vector3.Distance(transform.position, ballCarrier.position)
            : float.MaxValue;
        bool playerHasBall = ballController != null
            && ballController.isPossessed
            && ballController.currentHolder == playerTransform;

        switch (role)
        {
            case EnemyRole.Chaser:
                if (ballCarrier != null && distToCarrier <= detectionRadius)
                    currentState = distToCarrier <= tackleDistance ? AIState.Tackle : AIState.Intercept;
                else
                    currentState = AIState.Chase;
                break;

            case EnemyRole.Marker:
                if (markTarget == null) markTarget = FindNearestTeammate();
                currentState = AIState.Mark;
                if (ballCarrier == markTarget && distToCarrier <= tackleDistance)
                    currentState = AIState.Tackle;
                break;

            case EnemyRole.Defender:
                if (playerHasBall)
                {
                    float d = Vector3.Distance(transform.position, playerTransform.position);
                    currentState = d <= detectionRadius ? AIState.Chase : AIState.Retreat;
                }
                else currentState = AIState.Retreat;
                break;
        }
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case AIState.Chase: ChasePlayer(); break;
            case AIState.Intercept: InterceptCarrier(); break;
            case AIState.Mark: MarkTarget(); break;
            case AIState.Tackle: StartTackle(); break;
            case AIState.Retreat: Retreat(); break;
            case AIState.CarryBall: RunWithBall(); break;
            case AIState.Idle:
                if (agent != null) agent.isStopped = true;
                break;
        }
    }

    void ChasePlayer()
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        Transform target = ballController?.currentHolder ?? playerTransform;
        SetDestinationSafe(target.position);
    }

    void InterceptCarrier()
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.speed = interceptSpeed;
        Transform carrier = ballController?.currentHolder ?? playerTransform;
        Rigidbody cRb = carrier.GetComponent<Rigidbody>();
        float dist = Vector3.Distance(transform.position, carrier.position);
        float t = dist / agent.speed;
        Vector3 vel = cRb != null ? cRb.linearVelocity : Vector3.zero;
        SetDestinationSafe(carrier.position + vel * t);
    }

    void MarkTarget()
    {
        if (agent == null || markTarget == null) return;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        SetDestinationSafe(markTarget.position);
    }

    void Retreat()
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.speed = retreatSpeed;
        Vector3 home = FieldBoundary.Clamp(transform.position + new Vector3(-30f, 0f, 0f));  // ← static
        SetDestinationSafe(home);
    }

    void RunWithBall()
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.speed = interceptSpeed;
        Vector3 tryZone = FieldBoundary.Clamp(transform.position + new Vector3(-25f, 0f, 0f)); // ← static
        SetDestinationSafe(tryZone);
    }

    void StartTackle()
    {
        if (isTackling) return;
        isTackling = true;
        if (agent != null) agent.isStopped = true;
        if (animator != null) animator.SetTrigger("Tackle");

        ballController?.ForceRelease();

        Transform carrier = ballController?.currentHolder ?? playerTransform;
        PlayerController pc = carrier?.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.DisableMovement();
            Invoke(nameof(EndTackle), 1.5f);
        }
        else
        {
            TeammateAI tm = carrier?.GetComponent<TeammateAI>();
            if (tm != null) { tm.DisableAI(); Invoke(nameof(EndTackleTeammate), 1.5f); }
            else isTackling = false;
        }
        Debug.Log($"[EnemyAI] {name} tackled {carrier?.name}");
    }

    void EndTackle()
    {
        isTackling = false;
        if (agent != null) agent.isStopped = false;
        playerTransform?.GetComponent<PlayerController>()?.EnableMovement();
    }

    void EndTackleTeammate()
    {
        isTackling = false;
        if (agent != null) agent.isStopped = false;
        foreach (var tm in FindObjectsByType<TeammateAI>(FindObjectsSortMode.None)) tm.EnableAI();
    }

    Transform FindNearestTeammate()
    {
        var teammates = FindObjectsByType<TeammateAI>(FindObjectsSortMode.None);
        float best = float.MaxValue;
        Transform found = playerTransform;
        foreach (var tm in teammates)
        {
            float d = Vector3.Distance(transform.position, tm.transform.position);
            if (d < best) { best = d; found = tm.transform; }
        }
        return found;
    }

    void SetDestinationSafe(Vector3 dest)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        dest = FieldBoundary.Clamp(dest);                          // ← static
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else if (playerTransform != null)
            agent.SetDestination(playerTransform.position);
    }

    void ClampToBoundary()
    {
        if (agent == null || !FieldBoundary.IsOutOfBounds(transform.position)) return; // ← static
        agent.Warp(FieldBoundary.Clamp(transform.position));                            // ← static
    }

    void SyncAnimator()
    {
        if (animator == null || agent == null) return;
        animator.SetFloat("Speed", agent.velocity.magnitude, 0.1f, Time.deltaTime);
        animator.SetBool("HasBall", currentState == AIState.CarryBall);
    }

    public void EnableAI()
    {
        isActive = true;
        isTackling = false;
        if (agent != null) agent.isStopped = false;
    }

    public void DisableAI()
    {
        isActive = false;
        if (agent != null) agent.isStopped = true;
        if (animator != null) animator.SetFloat("Speed", 0f);
    }

    public void ResetPosition(Vector3 pos)
    {
        if (agent != null && agent.isOnNavMesh) agent.Warp(pos);
        else transform.position = pos;
        isTackling = false;
        currentState = AIState.Idle;
        isActive = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tackleDistance);
    }
}
