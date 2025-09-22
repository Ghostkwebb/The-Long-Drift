using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float rotationSpeed = 200f;

    private Rigidbody2D rb;
    private PlayerInputActions playerInputActions;
    private float rotationInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInputActions = new PlayerInputActions();

        // Subscribe our OnRotate method to the input action's events
        playerInputActions.Player.Rotate.performed += OnRotate;
        playerInputActions.Player.Rotate.canceled += OnRotate;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    // Physics-based movement should always be in FixedUpdate for consistency.
    private void FixedUpdate()
    {
        HandleThrust();
        HandleRotation();
    }

    private void HandleThrust()
    {
        if (playerInputActions.Player.Thrust.IsPressed())
        {
            // Apply force in the ship's forward direction (its local "up" axis)
            rb.AddForce(transform.up * thrustForce);
        }
    }

    private void HandleRotation()
    {
        // Apply torque. We negate the input to make D rotate clockwise.
        rb.AddTorque(-rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<float>();
    }
}