using UnityEngine;

public class RevolutionCounter : MonoBehaviour
{
    [SerializeField] private float revolutionsToWin = 2f;

    private bool isTracking = false;
    private Transform orbitCenter;
    private Vector2 previousDirection;
    private float totalAngleTraversed = 0f;

    // Public property for the UI to read from.
    public float RevolutionsCompleted => Mathf.Abs(totalAngleTraversed) / 360f;

    public void StartTracking(Transform center)
    {
        orbitCenter = center;
        // Set the initial direction vector from the planet to the ship.
        previousDirection = (transform.position - orbitCenter.position).normalized;
        isTracking = true;
        totalAngleTraversed = 0f;
        Debug.Log("Entered orbit zone. Tracking revolutions.");
    }

    public void StopAndResetTracking()
    {
        isTracking = false;
        totalAngleTraversed = 0f;
        Debug.Log("Exited orbit zone. Tracking reset.");
    }

    private void FixedUpdate()
    {
        // Only run the calculation if we are inside the win zone.
        if (!isTracking) return;

        // 1. Get the current direction vector from the planet to the ship.
        Vector2 currentDirection = (transform.position - orbitCenter.position).normalized;

        // 2. Calculate the small angle change since the last physics frame.
        // Vector2.SignedAngle gives us a positive or negative value, which is crucial.
        float angleDelta = Vector2.SignedAngle(previousDirection, currentDirection);

        // 3. Add this change to our total.
        totalAngleTraversed += angleDelta;

        // 4. Update the previous direction for the next frame's calculation.
        previousDirection = currentDirection;

        // 5. Check for the win condition.
        if (RevolutionsCompleted >= revolutionsToWin)
        {
            Debug.Log("WIN CONDITION MET! Two revolutions completed.");
            // We disable tracking to prevent the win message from firing repeatedly.
            isTracking = false;
            // Here we would eventually call a GameManager to handle the win state.
        }
    }
}