using UnityEngine;

public class SimpleOrbit : MonoBehaviour
{
    [Header("Orbital Parameters")]
    [Tooltip("The central body that this object will orbit.")]
    [SerializeField] private Transform centralBody;

    [Tooltip("The distance this body will maintain from its central body.")]
    [SerializeField] private float orbitDistance = 10f;

    [Tooltip("How many seconds it takes to complete one full orbit.")]
    [SerializeField] private float orbitPeriod = 20f;

    [Tooltip("The starting point of the orbit in degrees (0 is to the right).")]
    [SerializeField] private float startAngle = 0f;

    [Tooltip("The direction of the orbit.")]
    [SerializeField] private bool orbitClockwise = false;

    void Start()
    {
        if (centralBody == null)
        {
            Debug.LogError("Simple Orbit on " + name + " has no central body assigned!", this);
            enabled = false; // Disable the script if it's not set up correctly.
        }
    }

    // Use LateUpdate to ensure the orbit happens after the central body has moved for the frame.
    void LateUpdate()
    {
        if (centralBody == null) return;

        // 1. Calculate the current angle of the orbit.
        float direction = orbitClockwise ? -1f : 1f;
        // Ensure orbitPeriod is not zero to prevent division by zero.
        float angularSpeed = (2 * Mathf.PI) / (orbitPeriod > 0 ? orbitPeriod : 1f);
        float currentAngle = (Time.time * angularSpeed * direction) + (startAngle * Mathf.Deg2Rad);

        // 2. Calculate the offset position from the central body using trigonometry.
        Vector2 offset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitDistance;

        // 3. Apply the final position.
        transform.position = (Vector2)centralBody.position + offset;
    }
}