using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -20f;
    public float stoppingDistance = 0.25f;
    public bool movementLocked;

    CharacterController controller;
    Animator animator;
    float verticalVelocity;
    Vector3 lockedHorizontal;
    Plane groundPlane;
    Vector3 targetPosition;
    bool hasTarget;

    public bool IsGrounded => controller.isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        targetPosition = transform.position;
    }

    public void Launch(Vector3 horizontalVelocity, float upSpeed)
    {
        movementLocked = true;
        lockedHorizontal = horizontalVelocity;
        verticalVelocity = upSpeed;
        hasTarget = false;
    }

    public void EndLaunch()
    {
        movementLocked = false;
    }

    void Update()
    {
        // Left Click: Set movement destination to mouse location in world
        if (Input.GetMouseButton(0) && !movementLocked)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enterDistance))
            {
                targetPosition = ray.GetPoint(enterDistance);
                hasTarget = true;
            }
        }

        // Apply Gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 moveVelocity = Vector3.zero;

        if (movementLocked)
        {
            moveVelocity = lockedHorizontal;
        }
        else if (hasTarget)
        {
            Vector3 diff = targetPosition - transform.position;
            diff.y = 0; // Flatten movement to XZ plane

            if (diff.magnitude > stoppingDistance)
            {
                Vector3 moveDir = diff.normalized;
                moveVelocity = moveDir * speed;

                // Character turns to face where it is RUNNING
                transform.rotation = Quaternion.LookRotation(moveDir);
            }
            else
            {
                hasTarget = false; // Reached target position
            }
        }

        moveVelocity.y = verticalVelocity;
        controller.Move(moveVelocity * Time.deltaTime);

        // Update movement animation state
        if (animator != null)
        {
            float horizontalSpeed = new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
            animator.SetFloat("Speed", movementLocked ? 0f : horizontalSpeed);
        }
    }

    // Call this before casting a skill so the character immediately aims at the cursor
    public void FaceMouse()
    {
        hasTarget = false; // Stop auto-running to previous move destination when attacking
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 hitPoint = ray.GetPoint(enterDistance);
            Vector3 lookTarget = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
            Vector3 lookDir = (lookTarget - transform.position).normalized;

            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}