using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -20f;
    public bool movementLocked;

    CharacterController controller;
    Animator animator;
    float verticalVelocity;
    Vector3 lockedHorizontal;

    public bool IsGrounded => controller.isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Launch(Vector3 horizontalVelocity, float upSpeed)
    {
        movementLocked = true;
        lockedHorizontal = horizontalVelocity;
        verticalVelocity = upSpeed;
    }

    public void EndLaunch()
    {
        movementLocked = false;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(x, 0, z).normalized;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = movementLocked ? lockedHorizontal : direction * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        if (!movementLocked && direction != Vector3.zero)
            transform.forward = direction;

        // Tell the animator whether we're running
        if (animator != null)
            animator.SetFloat("Speed", movementLocked ? 0f : direction.magnitude);
    }
}