using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Boundary - Match Your Field")]
    public float boundaryX = 55f;
    public float boundaryZMin = -75f;
    public float boundaryZMax = 35f;

    [Header("References")]
    public Animator animator;
    public BallController ballController;

    private Rigidbody rb;
    private Vector3 moveDirection;
    [HideInInspector] public bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Auto find animator
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Auto find ball
        if (ballController == null)
            ballController = FindAnyObjectByType<BallController>();
    }

    // ─────────────────────────────────────────
    void Update()
    {
        if (!canMove) return;
        GetInput();
        RotatePlayer();
        UpdateAnimator();
        HandleActions();
        ClampToBoundary();
    }

    // ─────────────────────────────────────────
    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = new Vector3(
                0f, rb.linearVelocity.y, 0f);
            return;
        }
        MovePlayer();
    }

    // ─────────────────────────────────────────
    void GetInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(h, 0f, v).normalized;
    }

    // ─────────────────────────────────────────
    void MovePlayer()
    {
        if (moveDirection.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(
                0f, rb.linearVelocity.y, 0f);
            return;
        }

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = moveDirection * speed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    // ─────────────────────────────────────────
    void RotatePlayer()
    {
        if (moveDirection.magnitude < 0.1f) return;
        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────────
    void UpdateAnimator()
    {
        if (animator == null) return;
        float currentSpeed = new Vector3(
            rb.linearVelocity.x, 0f,
            rb.linearVelocity.z).magnitude;
        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    // ─────────────────────────────────────────
    void HandleActions()
    {
        if (ballController == null) return;

        // ── F key = Pick up ball ──
        if (Input.GetKeyDown(KeyCode.F))
        {
            float dist = Vector3.Distance(
                transform.position,
                ballController.transform.position);

            if (!ballController.isPossessed
                && dist <= ballController.pickupRadius * 3f)
            {
                ballController.PickUp(transform);

                // Play receive animation
                if (animator != null)
                    animator.SetTrigger("IsReceiving");

                Debug.Log("✅ Ball picked up!");
            }
            else if (ballController.isPossessed
                && ballController.currentHolder == transform)
            {
                Debug.Log("Already have the ball!");
            }
            else
            {
                Debug.Log("Too far from ball! Distance: " + dist);
            }
        }

        // ── Q key = Pass LEFT ──
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (ballController.isPossessed
                && ballController.currentHolder == transform)
            {
                ballController.TryPassLeft(transform.forward);

                // Play pass animation
                if (animator != null)
                    animator.SetTrigger("IsPassing");

                Debug.Log("✅ Passed LEFT!");
            }
            else
                Debug.Log("Cannot pass - no ball!");
        }

        // ── E key = Pass RIGHT ──
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (ballController.isPossessed
                && ballController.currentHolder == transform)
            {
                ballController.TryPassRight(transform.forward);

                // Play pass animation
                if (animator != null)
                    animator.SetTrigger("IsPassing");

                Debug.Log("✅ Passed RIGHT!");
            }
            else
                Debug.Log("Cannot pass - no ball!");
        }

        // ── Space key = Kick ──
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ballController.isPossessed
                && ballController.currentHolder == transform)
            {
                ballController.TryKick(transform.forward);

                // Play kick animation
                if (animator != null)
                    animator.SetTrigger("IsKicking");

                Debug.Log("✅ Kicked!");
            }
            else
                Debug.Log("Cannot kick - no ball!");
        }

        // ── R key = Defend pose ──
        if (Input.GetKey(KeyCode.R))
        {
            if (animator != null)
                animator.SetBool("IsDefending", true);
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsDefending", false);
        }
    }

    // ─────────────────────────────────────────
    // Keep player inside field boundaries
    void ClampToBoundary()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -boundaryX, boundaryX);
        pos.z = Mathf.Clamp(pos.z, boundaryZMin, boundaryZMax);
        transform.position = pos;
    }

    // ─────────────────────────────────────────
    // Called by EnemyAI when tackled
    public void DisableMovement()
    {
        canMove = false;
        rb.linearVelocity = Vector3.zero;
        if (animator != null)
            animator.SetFloat("Speed", 0f);
        Debug.Log("Player tackled! Movement disabled.");
    }

    // Called after tackle ends
    public void EnableMovement()
    {
        canMove = true;
        Debug.Log("Player recovered! Movement enabled.");
    }
}