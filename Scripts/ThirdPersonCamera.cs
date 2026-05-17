// ============================================================
// ThirdPersonCamera.cs
// Attach to: Main Camera
// ============================================================
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 8f;
    public float height = 4f;
    public float smoothSpeed = 5f;

    [Header("Mouse Rotation")]
    public bool enableMouseRotation = true;
    public float mouseSensitivity = 2f;
    public float minYAngle = -20f;
    public float maxYAngle = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;

    void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (enableMouseRotation)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (enableMouseRotation)
        {
            currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);
        }

        Quaternion rot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desired = target.position
            - rot * Vector3.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}
