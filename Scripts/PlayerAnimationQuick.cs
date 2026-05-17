using UnityEngine;

public class PlayerAnimationQuick : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float speed = new Vector3(h, 0, v).magnitude;
        animator.SetFloat("Speed", speed);

        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("Kick");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            animator.SetTrigger("Pass");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Defend");
        }
    }
}