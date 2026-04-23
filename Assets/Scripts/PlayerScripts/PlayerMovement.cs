using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private StatusReceiver _status;
    private PlayerStats _stats;

    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        _status = GetComponent<StatusReceiver>();
        _stats = GetComponent<PlayerStats>();

        // Grab the action once (safer than calling this repeatedly).
        moveAction = playerInput.actions["Move"];

        // Physics-friendly defaults for top-down.
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        moveAction.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        if (movementInput.sqrMagnitude > 1f)
            movementInput.Normalize();
    }

    private void FixedUpdate()
    {
        float mult = _status != null ? _status.SpeedMultiplier : 1f;
        if (_stats != null)
            mult *= _stats.MoveSpeedMultiplier;

        rb.linearVelocity = movementInput * moveSpeed * mult;
    }
}
