using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : MonoBehaviour
{
    // This constant determines the strength of gravity. Tweak this to change the feel.
    private const float GravitationalConstant = 0.667f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Loop through every GravitySource in the simulation.
        foreach (var source in GravitySource.AllSources)
        {
            ApplyGravityFromSource(source);
        }
    }

    private void ApplyGravityFromSource(GravitySource source)
    {
        // Newton's Law of Universal Gravitation: F = G * (m1*m2) / r^2

        // 1. Calculate the direction vector from the ship to the source.
        Vector2 direction = source.transform.position - transform.position;

        // 2. Calculate the distance (r). We use SqrMagnitude for performance.
        float distanceSqr = direction.sqrMagnitude;

        // Avoid division by zero if the ship is exactly at the planet's center.
        if (distanceSqr <= 0.01f) return;

        // 3. Calculate the force magnitude.
        float forceMagnitude = GravitationalConstant * (rb.mass * source.mass) / distanceSqr;

        // 4. Create the final force vector (magnitude + direction).
        Vector2 forceVector = direction.normalized * forceMagnitude;

        // 5. Apply the force.
        rb.AddForce(forceVector);
    }
}