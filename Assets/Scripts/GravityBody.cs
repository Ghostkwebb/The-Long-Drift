using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : MonoBehaviour
{
    private const float GravitationalConstant = 0.667f;

    public GravitySource[] Attractors { get; set; }
    public Vector2 LastNetGravityForce { get; private set; }

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Initialize the force to zero to avoid issues on the first frame.
        LastNetGravityForce = Vector2.zero;
    }

    private void Start()
    {
        if (Attractors == null)
        {
            Attractors = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);
        }
    }

    private void FixedUpdate()
    {
        if (Attractors == null) return;

        // Start with a fresh Vector2 for this frame's calculations.
        Vector2 combinedForce = Vector2.zero;

        // Loop through all sources and add their force to our 'combinedForce' variable.
        foreach (var source in Attractors)
        {
            if (source != null)
            {
                combinedForce += CalculateForceFromSource(source);
            }
        }

        // Store this final, calculated force for other scripts to read.
        LastNetGravityForce = combinedForce;

        // Apply the single, combined force to the Rigidbody.
        rb.AddForce(combinedForce);
    }

    private Vector2 CalculateForceFromSource(GravitySource source)
    {
        // Safety check to prevent division by zero or weirdness if objects are at the same spot.
        if (source.transform.position == transform.position) return Vector2.zero;

        Vector2 direction = (Vector2)source.transform.position - rb.position;
        float distanceSqr = direction.sqrMagnitude;

        if (distanceSqr <= 0.01f) return Vector2.zero;

        float forceMagnitude = GravitationalConstant * (rb.mass * source.mass) / distanceSqr;
        return direction.normalized * forceMagnitude;
    }
}