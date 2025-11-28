using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Look Settings")]
    public float sensitivityX = 30f;
    public float sensitivityY = 30f;
    public float clampY = 80f;

    [Header("Headbob Settings")]
    public bool enableHeadbob = true;
    public float headbobStrength = 0.05f;
    public float headbobSpeed = 12f;

    [Header("FOV Settings")]
    public Camera cam;
    public float normalFOV = 70f;
    public float dashFOV = 85f;
    public float fovSmoothSpeed = 8f;

    [SerializeField] private Transform playerBody;

    private PlayerControls controls;
    private Vector2 lookInput;
    private float xRotation = 0f;

    // Headbob internal
    private float bobTimer = 0f;
    private Vector3 camStartLocalPos;

    // Dash state comes from PlayerMovement
    public bool isDashing = false;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        camStartLocalPos = transform.localPosition;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void LateUpdate()
    {
        HandleLook();
        HandleHeadbob();
        HandleFOV();
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * sensitivityX * Time.deltaTime;
        float mouseY = lookInput.y * sensitivityY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampY, clampY);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeadbob()
    {
        if (!enableHeadbob)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                camStartLocalPos,
                Time.deltaTime * 10f
            );
            return;
        }

        // Only bob if player moves
        PlayerMovement pm = playerBody.GetComponent<PlayerMovement>();
        bool moving = pm != null && pm.CurrentlyMoving;

        if (!moving || !pm.controller.isGrounded || isDashing)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                camStartLocalPos,
                Time.deltaTime * 8f
            );
            return;
        }

        bobTimer += Time.deltaTime * headbobSpeed;

        float bobOffset = Mathf.Sin(bobTimer) * headbobStrength;

        transform.localPosition = camStartLocalPos + new Vector3(0, bobOffset, 0);
    }

    private void HandleFOV()
    {
        float targetFOV = isDashing ? dashFOV : normalFOV;

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSmoothSpeed
        );
    }
}
