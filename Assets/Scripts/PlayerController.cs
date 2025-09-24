using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Ship Parameters")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float rotationSpeed = 200f;

    [Tooltip("How quickly the ship aligns to its velocity vector while orbiting.")]
    [SerializeField] private float progradeRotationSpeed = 5f;


    [Header("Boost")]
    [Tooltip("Multiplier applied to thrust force when boosting.")]
    [SerializeField] private float boostMultiplier = 2.5f;
    [Tooltip("Multiplier applied to fuel consumption when boosting.")]
    [SerializeField] private float boostFuelConsumptionMultiplier = 3f;


    [Header("Fuel System")]
    [SerializeField] private float maxFuel = 100f;
    public float MaxFuel => maxFuel;
    [SerializeField] private float fuelConsumptionRate = 5f;
    public float CurrentFuel { get; private set; }

    [Header("Spawning")]
    [Tooltip("The distance from the start planet's center to spawn at.")]
    [SerializeField] private float startOrbitDistance = 8f;
    [Tooltip("Should the initial orbit be clockwise?")]
    [SerializeField] private bool startOrbitClockwise = false;


    public float Speed => rb.linearVelocity.magnitude;

    public float TimeWithoutRotationInput { get; private set; }
    public bool IsProvidingInput { get; private set; }


    private bool isOrbiting = false;

    private Rigidbody2D rb;
    private PlayerInputActions playerInputActions;
    private float rotationInput;
    private bool hasSpawned = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Rotate.performed += OnRotate;
        playerInputActions.Player.Rotate.canceled += OnRotate;

        CurrentFuel = maxFuel;
    }

    public void RefillFuel()
    {
        Debug.Log("Fuel refilled!");
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

        if (isRotating)
        {
            TimeWithoutRotationInput = 0f;
        }
        else
        {
            TimeWithoutRotationInput += Time.deltaTime;
        }
    }

    public void SetOrbitingState(bool orbiting)
    {
        isOrbiting = orbiting;
    }


    private void FixedUpdate()
    {
        if (!hasSpawned)
        {
            hasSpawned = true;
            HandleSpawning(); // Renamed for clarity
        }

        if (isOrbiting)
        {
            HandleOrbitingControls();
        }
        else
        {
            HandleManualControls();
        }
    }

    private void HandleSpawning()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasCheckpoint)
        {
            Debug.Log("Spawning at checkpoint.");
            transform.position = GameManager.Instance.GetCheckpointPosition();
            rb.linearVelocity = GameManager.Instance.GetCheckpointVelocity();
            rb.angularVelocity = 0f;
            return;
        }

        GravitySource startPlanet = FindObjectsByType<GravitySource>(FindObjectsSortMode.None)
            .FirstOrDefault(s => s.isStartingPlanet);

        if (startPlanet != null)
        {
            Debug.Log("Spawning in orbit around start planet: " + startPlanet.name);

            // 1. Set Position: Place the ship to the right of the planet.
            Vector2 startPosition = (Vector2)startPlanet.transform.position + new Vector2(startOrbitDistance, 0);
            transform.position = startPosition;

            // 2. Calculate Orbital Velocity:
            // The formula for a stable circular orbit is v = sqrt(G * M / r)
            // We need to match the constant from our GravityBody script.
            const float GravitationalConstant = 0.667f;
            float planetMass = startPlanet.mass;
            float speed = Mathf.Sqrt((GravitationalConstant * planetMass) / startOrbitDistance);

            // The velocity direction must be perpendicular to the position.
            Vector2 velocityDirection = startOrbitClockwise ? Vector2.down : Vector2.up;
            rb.linearVelocity = velocityDirection * speed;
            rb.angularVelocity = 0f;
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
        if (playerInputActions.Player.Thrust.IsPressed() && CurrentFuel > 0)
        {
            bool isBoosting = playerInputActions.Player.Boost.IsPressed();

            float currentThrust = isBoosting ? thrustForce * boostMultiplier : thrustForce;
            float currentFuelRate = isBoosting ? fuelConsumptionRate * boostFuelConsumptionMultiplier : fuelConsumptionRate;

            rb.AddForce(transform.up * currentThrust);

            CurrentFuel -= currentFuelRate * Time.fixedDeltaTime;
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer(1.0f);
        }

        gameObject.SetActive(false);
    }
}