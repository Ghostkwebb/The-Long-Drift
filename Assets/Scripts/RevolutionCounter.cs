using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class RevolutionCounter : MonoBehaviour
{
    [Header("Win Condition")]
    [SerializeField] private float revolutionsToWin = 2f;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI revolutionText;

    private PlayerController playerController;
    private bool winConditionMet = false;
    private bool checkpointActivated = false;
    private bool isTracking = false;
    private Transform orbitCenter;
    private Vector2 previousDirection;
    private float totalAngleTraversed = 0f;

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
        if (isTracking && orbitCenter == center) return;

        orbitCenter = center;
        previousDirection = (transform.position - orbitCenter.position).normalized;
        isTracking = true;
        totalAngleTraversed = 0f;
        winConditionMet = false;

        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(true);
        }
    }

    public void StopTracking()
    {
        isTracking = false;
        totalAngleTraversed = 0f;
        checkpointActivated = false;
        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isTracking) return;

        if (playerController.TimeWithoutRotationInput == 0f)
        {
            totalAngleTraversed = 0f;
        }
        else
        {
            UpdateOrbitProgress();
        }

        CheckForCompletion();
        UpdateRevolutionText();
    }

    // This method now only does one thing: count.
    private void UpdateOrbitProgress()
    {
        Vector2 currentDirection = (transform.position - orbitCenter.position).normalized;
        float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);
        totalAngleTraversed += angleDelta;
        previousDirection = currentDirection;
    }

    private void CheckForCompletion()
    {
        if (RevolutionsCompleted < revolutionsToWin) return;

        if (orbitCenter.TryGetComponent(out GravitySource source))
        {
            if ((source.isCheckpoint || source.isStartingPlanet) && !checkpointActivated)
            {
                checkpointActivated = true;
                Vector2 currentVelocity = GetComponent<Rigidbody2D>().linearVelocity;
                GameManager.Instance.SetCheckpoint(orbitCenter, transform.position, currentVelocity);
                playerController.RefillFuel();
                Debug.Log($"CHECKPOINT/REFUEL at {orbitCenter.name} after {revolutionsToWin} stable orbits.");
            }

            if (source.isDestination && !winConditionMet)
            {
                winConditionMet = true;
                GameManager.Instance.TriggerWinState();
                Debug.Log("WIN CONDITION MET!");
            }
        }
    }

    private void UpdateRevolutionText()
    {
        if (revolutionText != null)
        {
            float displayRevolutions = Mathf.Min(RevolutionsCompleted, revolutionsToWin);
            string displayText = $"Orbits: {displayRevolutions:F1} / {revolutionsToWin}";
            revolutionText.text = displayText;
        }
    }
}