using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Ship Parameters")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float rotationSpeed = 200f;

    [Tooltip("How quickly the ship aligns to its velocity vector while orbiting.")]
    [SerializeField] private float progradeRotationSpeed = 5f;

    [Header("Fuel System")]
    [SerializeField] private float maxFuel = 100f;
    public float MaxFuel => maxFuel;
    [SerializeField] private float fuelConsumptionRate = 5f;
    public float CurrentFuel { get; private set; }

    public float Speed => rb.linearVelocity.magnitude;

    public bool IsProvidingInput { get; private set; }


    private bool isOrbiting = false;

    private Rigidbody2D rb;
    private PlayerInputActions playerInputActions;
    private float rotationInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Rotate.performed += OnRotate;
        playerInputActions.Player.Rotate.canceled += OnRotate;

        CurrentFuel = maxFuel;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    private void Update()
    {
        bool isThrusting = playerInputActions.Player.Thrust.IsPressed();
        bool isRotating = Mathf.Abs(rotationInput) > 0.01f;
        IsProvidingInput = isThrusting || isRotating;
    }

    public void SetOrbitingState(bool orbiting)
    {
        isOrbiting = orbiting;
    }


    private void FixedUpdate()
    {
        if (isOrbiting)
        {
            HandleOrbitingControls();
        }
        else
        {
            HandleManualControls();
        }
    }

    private void HandleManualControls()
    {
        HandleThrust();
        HandleRotation();
    }

    private void HandleOrbitingControls()
    {
        HandleThrust();
        AlignToPrograde();
    }

    private void AlignToPrograde()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, rb.linearVelocity);

            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, progradeRotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedRotation);
        }
    }

    private void HandleThrust()
    {
        // Only allow thrust if we have fuel.
        if (playerInputActions.Player.Thrust.IsPressed() && CurrentFuel > 0)
        {
            rb.AddForce(transform.up * thrustForce);

            // Consume fuel. Multiplying by Time.fixedDeltaTime makes it rate-based.
            CurrentFuel -= fuelConsumptionRate * Time.fixedDeltaTime;
        }
    }

    private void HandleRotation()
    {
        rb.AddTorque(-rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<float>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("LOSS CONDITION: Crashed!");
        this.enabled = false;
    }
}