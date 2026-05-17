using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Settings")]
    public float passForce = 12f;
    public float kickForce = 15f;
    [Range(10f, 60f)]
    public float kickAngle = 35f;
    public float pickupRadius = 4f;

    [Header("References")]
    public Transform holdPoint;

    [HideInInspector] public bool isPossessed = false;
    [HideInInspector] public Transform currentHolder = null;

    private Rigidbody rb;
    private Collider ballCollider;
    private Transform playerTransform;
    private float pickupDelay = 1f;
    private float gameStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
        rb.mass = 0.45f;
        rb.linearDamping = 0.3f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        gameStartTime = Time.time;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogError("❌ Player tag not found!");
    }

    void Update()
    {
        // F key manual pickup
        if (Input.GetKeyDown(KeyCode.F) && !isPossessed
            && playerTransform != null)
        {
            float dist = Vector3.Distance(
                transform.position, playerTransform.position);
            if (dist <= pickupRadius * 3f)
                PickUp(playerTransform);
            else
                Debug.Log("Too far! Dist: " + dist);
        }

        if (isPossessed && holdPoint != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                holdPoint.position,
                Time.deltaTime * 20f);
            return;
        }

        if (!isPossessed && playerTransform != null)
        {
            if (Time.time - gameStartTime < pickupDelay) return;
            float dist = Vector3.Distance(
                transform.position, playerTransform.position);
            if (dist <= pickupRadius)
                PickUp(playerTransform);
        }
    }

    public void PickUp(Transform holder)
    {
        isPossessed = true;
        currentHolder = holder;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (ballCollider != null) ballCollider.enabled = false;

        if (holdPoint == null)
        {
            Transform hp = holder.Find("BallHoldPoint");
            if (hp != null) holdPoint = hp;
            else
            {
                GameObject hpObj = new GameObject("BallHoldPoint");
                hpObj.transform.SetParent(holder);
                hpObj.transform.localPosition =
                    new Vector3(0.5f, 1.2f, 0.3f);
                holdPoint = hpObj.transform;
            }
        }
        Debug.Log("✅ Ball picked up by: " + holder.name);
    }

    public void Release()
    {
        isPossessed = false;
        currentHolder = null;
        holdPoint = null;
        rb.isKinematic = false;
        if (ballCollider != null) ballCollider.enabled = true;
    }

    public void TryPassLeft(Vector3 forward)
    {
        if (!isPossessed) return;
        Vector3 left = Quaternion.Euler(0, -90, 0) * forward;
        Vector3 dir = (left - forward * 0.3f).normalized;
        Release();
        rb.AddForce((dir + Vector3.up * 0.2f).normalized
            * passForce, ForceMode.Impulse);
        Debug.Log("✅ Passed LEFT");
    }

    public void TryPassRight(Vector3 forward)
    {
        if (!isPossessed) return;
        Vector3 right = Quaternion.Euler(0, 90, 0) * forward;
        Vector3 dir = (right - forward * 0.3f).normalized;
        Release();
        rb.AddForce((dir + Vector3.up * 0.2f).normalized
            * passForce, ForceMode.Impulse);
        Debug.Log("✅ Passed RIGHT");
    }

    public void TryKick(Vector3 forward)
    {
        if (!isPossessed) return;
        Release();
        float rad = kickAngle * Mathf.Deg2Rad;
        Vector3 dir = (forward + Vector3.up
            * Mathf.Tan(rad)).normalized;
        rb.AddForce(dir * kickForce, ForceMode.Impulse);
        Debug.Log("✅ Kicked!");
    }

    public void ForceRelease()
    {
        if (!isPossessed) return;
        Release();
        rb.AddForce(Vector3.up * 2f
            + Random.insideUnitSphere * 3f, ForceMode.Impulse);
    }

    public void ResetToPosition(Vector3 pos)
    {
        Release();
        transform.position = pos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameStartTime = Time.time;
    }
}