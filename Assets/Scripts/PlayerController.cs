using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("The AudioSource for the continuous engine idle sound.")]
    [SerializeField] private AudioSource idleAudioSource;
    [Tooltip("The AudioSource for the active thrust sound.")]
    [SerializeField] private AudioSource thrustAudioSource;
    [SerializeField] private AudioClip idleLoopClip;
    [SerializeField] private AudioClip thrustLoopClip;
    [SerializeField] private float thrustPitch = 1f;
    [SerializeField] private float boostPitch = 1.5f;

    [Header("Ship Parameters")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float rotationSpeed = 200f;

    [Tooltip("How quickly the ship aligns to its velocity vector while orbiting.")]
    [SerializeField] private float progradeRotationSpeed = 5f;

    [Header("Autopilot")]
    [Tooltip("How long the player must go without rotation input before the ship locks to prograde.")]
    [SerializeField] private float timeToLockRotation = 1.5f;


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
    [Tooltip("The closest distance the player will be to the start planet.")]
    [SerializeField] private float startOrbitPeriapsis = 8f;
    [Tooltip("The farthest distance the player will be from the start planet. Set to Periapsis for a circular orbit.")]
    [SerializeField] private float startOrbitApoapsis = 15f;
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
    private readonly int thrustStateAnimHash = Animator.StringToHash("ThrustState");

    [Header("Component References")]
    [Tooltip("The ThrusterAnimator script on the ThrusterFire child object.")]
    [SerializeField] private ThrusterAnimator thrusterAnimator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Rotate.performed += OnRotate;
        playerInputActions.Player.Rotate.canceled += OnRotate;
        CurrentFuel = maxFuel;
    }

    private void Start()
    {
        // --- THIS IS THE NEW AUDIO SETUP ---
        // Prepare the idle audio source.
        if (idleAudioSource != null)
        {
            idleAudioSource.clip = idleLoopClip;
            idleAudioSource.loop = true;
            idleAudioSource.Play(); // The idle sound is always on by default.
        }
        // Prepare the thrust audio source.
        if (thrustAudioSource != null)
        {
            thrustAudioSource.clip = thrustLoopClip;
            thrustAudioSource.loop = true;
            thrustAudioSource.Stop(); // The thrust sound is off by default.
        }

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

        if (!isOrbiting && TimeWithoutRotationInput >= timeToLockRotation)
        {
            SetOrbitingState(true);
            Debug.Log("Stable trajectory detected. Locking rotation to prograde.");
        }
        else if (isOrbiting && isRotating)
        {
            SetOrbitingState(false);
            Debug.Log("Rotation input detected. Unlocking prograde lock.");
        }

        HandleAnimation();
        HandleEngineAudio();
    }

    private void HandleEngineAudio()
    {
        if (idleAudioSource == null || thrustAudioSource == null) return;

        bool isThrusting = playerInputActions.Player.Thrust.IsPressed() && CurrentFuel > 0;
        bool isBoosting = playerInputActions.Player.Boost.IsPressed() && CurrentFuel > 0;

        if (isThrusting || isBoosting)
        {
            if (idleAudioSource.isPlaying)
            {
                idleAudioSource.Stop();
            }
            if (!thrustAudioSource.isPlaying)
            {
                thrustAudioSource.Play();
            }

            thrustAudioSource.pitch = isBoosting ? boostPitch : thrustPitch;
        }
        else
        {
            if (thrustAudioSource.isPlaying)
            {
                thrustAudioSource.Stop();
            }
            if (!idleAudioSource.isPlaying)
            {
                idleAudioSource.Play();
            }
        }
    }


    private void HandleAnimation()
    {
        if (thrusterAnimator == null) return;

        bool isBoosting = playerInputActions.Player.Boost.IsPressed();
        bool isThrusting = playerInputActions.Player.Thrust.IsPressed();

        if (isBoosting && CurrentFuel > 0)
        {
            thrusterAnimator.SetState(2); // Tell the script to play the Boost animation
        }
        else if (isThrusting && CurrentFuel > 0)
        {
            thrusterAnimator.SetState(1); // Tell the script to play the Thrust animation
        }
        else
        {
            thrusterAnimator.SetState(0); // Tell the script to go Idle
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
            HandleSpawning();
        }

        if (isOrbiting)
        {
            HandleOrbitingControls();
        }
        else
        {
            bool isBoosting = playerInputActions.Player.Boost.IsPressed();
            if (isBoosting)
            {
                HandleBoost();
            }
            else
            {
                HandleManualControls();
            }
        }
    }

    private void HandleBoost()
    {
        if (CurrentFuel > 0)
        {
            float boostThrust = thrustForce * boostMultiplier;
            float boostFuelRate = fuelConsumptionRate * boostFuelConsumptionMultiplier;

            rb.AddForce(transform.up * boostThrust);

            CurrentFuel -= boostFuelRate * Time.fixedDeltaTime;
        }
        HandleRotation();
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
            Debug.Log("Spawning in elliptical orbit around start planet: " + startPlanet.name);

            float r_p = startOrbitPeriapsis;
            float r_a = Mathf.Max(r_p, startOrbitApoapsis);

            transform.position = (Vector2)startPlanet.transform.position + new Vector2(r_p, 0);

            // We must use the GravitationalConstant from the GravityBody script to match the physics.
            // Since we can't access it directly, we will use the same value here.
            const float GravitationalConstant = 0.667f;
            float planetMass = startPlanet.mass;

            // The vis-viva equation for speed at periapsis
            float speedNumerator = 2 * GravitationalConstant * planetMass * r_a;
            float speedDenominator = r_p * (r_a + r_p);
            // Handle potential division by zero if r_p is zero
            if (speedDenominator <= 0)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            float speed = Mathf.Sqrt(speedNumerator / speedDenominator);

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
        bool isBoosting = playerInputActions.Player.Boost.IsPressed();
        if (isBoosting)
        {
            HandleBoost();
        }
        else
        {
            HandleThrust();
            AlignToPrograde();
        }
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
            rb.AddForce(transform.up * thrustForce);
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer(1.0f);
        }

        gameObject.SetActive(false);
    }
}