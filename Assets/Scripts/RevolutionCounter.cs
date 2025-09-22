using UnityEngine;
using TMPro;

public class RevolutionCounter : MonoBehaviour
{
    [Header("Win Condition")]
    [SerializeField] private float revolutionsToWin = 2f;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI revolutionText;

    private bool isTracking = false;
    private Transform orbitCenter;
    private Vector2 previousDirection;
    private float totalAngleTraversed = 0f;

    public float RevolutionsCompleted => Mathf.Abs(totalAngleTraversed) / 360f;

    private void Start()
    {
        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(false);
        }
    }

    public void StartTracking(Transform center)
    {
        orbitCenter = center;
        previousDirection = (transform.position - orbitCenter.position).normalized;
        isTracking = true;
        totalAngleTraversed = 0f;

        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(true);
        }

        Debug.Log("Entered orbit zone. Tracking revolutions.");
    }

    public void StopAndResetTracking()
    {
        isTracking = false;
        totalAngleTraversed = 0f;

        if (revolutionText != null)
        {
            revolutionText.gameObject.SetActive(false);
        }

        Debug.Log("Exited orbit zone. Tracking reset.");
    }

    private void FixedUpdate()
    {
        if (!isTracking) return;

        Vector2 currentDirection = (transform.position - orbitCenter.position).normalized;
        float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);
        totalAngleTraversed += angleDelta;
        previousDirection = currentDirection;

        UpdateRevolutionText();

        if (RevolutionsCompleted >= revolutionsToWin)
        {
            Debug.Log("WIN CONDITION MET! Two revolutions completed.");
            isTracking = false;
            // We could add a GameManager call here.
        }
    }

    private void UpdateRevolutionText()
    {
        if (revolutionText != null)
        {
            // "F1" formats the float to show one decimal place (e.g., 1.2)
            string displayText = $"Orbits: {RevolutionsCompleted:F1} / {revolutionsToWin}";
            revolutionText.text = displayText;
        }
    }
}