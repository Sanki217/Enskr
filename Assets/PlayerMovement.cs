using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float sprintMultiplier = 1f; // Not using sprint, but here for future use
    public float jumpForce = 6f;
    public float gravity = -20f;

    [Header("Dash Settings")]
    public float dashDistance = 8f;
    public float dashCooldown = 1f;
    public float dashDuration = 0.15f;

    private PlayerControls controls;
    private CharacterController controller;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;

    private bool canDash = true;
    private bool isDashing = false;

    private float dashTimer;
    private Vector3 dashDirection;

    private void Awake()
    {
        controls = new PlayerControls();
        controller = GetComponent<CharacterController>();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => TryJump();
        controls.Player.Dash.performed += ctx => TryDash();
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        HandleMovement();
        HandleDash();
    }

    private void HandleMovement()
    {
        if (isDashing)
            return;

        isGrounded = controller.isGrounded;

        Vector3 move = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void TryJump()
    {
        if (isDashing) return;
        if (!controller.isGrounded) return;

        velocity.y = jumpForce;
    }

    private void TryDash()
    {
        if (!canDash) return;
        if (isDashing) return;

        canDash = false;
        isDashing = true;

        dashTimer = dashDuration;

        dashDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        if (dashDirection.magnitude < 0.1f)
            dashDirection = transform.forward;

        // No gravity during dash
        velocity.y = 0;
    }

    private void HandleDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;

        float dashSpeed = dashDistance / dashDuration;
        controller.Move(dashDirection * dashSpeed * Time.deltaTime);

        if (dashTimer <= 0)
        {
            isDashing = false;
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void ResetDash()
    {
        canDash = true;
    }
}
