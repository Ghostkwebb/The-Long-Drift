using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class RevolutionCounter : MonoBehaviour
{
    [Header("Win Condition")]
    [SerializeField] private float revolutionsToWin = 1f;

    [Header("Orbit Lock")]
    [Tooltip("How long the player must go without rotation input before the ship locks to prograde.")]
    [SerializeField] private float timeToLockRotation = 1.5f;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI revolutionText;

    private PlayerController playerController;
    private bool winConditionMet = false;

    private bool isTracking = false;
    private Transform orbitCenter;
    private Vector2 previousDirection;
    private float totalAngleTraversed = 0f;
    private bool isOrbitLocked = false;

    public float RevolutionsCompleted => Mathf.Abs(totalAngleTraversed) / 360f;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out GravitySource source))
        {
            StartTracking(source.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out GravitySource source))
        {
            if (isTracking && orbitCenter == source.transform)
            {
                StopTracking();
            }
        }
    }

    public void StartTracking(Transform center)
    {
        orbitCenter = center;
        previousDirection = (transform.position - orbitCenter.position).normalized;
        isTracking = true;
        totalAngleTraversed = 0f;
        winConditionMet = false;

        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(true);
        }
        Debug.Log("Entered orbit zone around " + center.name + ". Tracking orbits.");
    }

    public void StopTracking()
    {
        isTracking = false;
        totalAngleTraversed = 0f;

        if (isOrbitLocked)
        {
            isOrbitLocked = false;
            playerController.SetOrbitingState(false);
            Debug.Log("Exited orbit zone. Manual control restored.");
        }

        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isTracking || playerController == null || orbitCenter == null) return;

        if (playerController.IsProvidingInput)
        {
            HandlePlayerInput();
        }
        else // Player is hands-free
        {
            HandleStableFlight();
        }

        CheckWinCondition();
        UpdateRevolutionText();
    }

    private void HandlePlayerInput()
    {
        if (isOrbitLocked)
        {
            isOrbitLocked = false;
            playerController.SetOrbitingState(false);
            Debug.Log("Player input detected, unlocking orbit control.");
        }
    }

    private void HandleStableFlight()
    {
        UpdateOrbitProgress();

        if (!isOrbitLocked && playerController.TimeWithoutRotationInput >= timeToLockRotation)
        {
            isOrbitLocked = true;
            playerController.SetOrbitingState(true);
            Debug.Log("Stable trajectory detected for " + timeToLockRotation + "s. Locking rotation.");
        }
    }

    private void UpdateOrbitProgress()
    {
        if (playerController.TimeWithoutRotationInput == 0f)
        {
            totalAngleTraversed = 0f;
        }

        Vector2 currentDirection = (transform.position - orbitCenter.position).normalized;
        float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);
        totalAngleTraversed += angleDelta;
        previousDirection = currentDirection;
    }


    private void CheckWinCondition()
    {
        if (winConditionMet || RevolutionsCompleted < revolutionsToWin) return;

        if (orbitCenter.TryGetComponent(out GravitySource source) && source.isDestination)
        {
            winConditionMet = true;
            Debug.Log("WIN CONDITION MET! " + revolutionsToWin + " orbits completed around destination.");
        }
    }

    private void UpdateRevolutionText()
    {
        if (revolutionText != null)
        {
            string displayText = $"Orbits: {RevolutionsCompleted:F1} / {revolutionsToWin}";
            revolutionText.text = displayText;
        }
    }
}