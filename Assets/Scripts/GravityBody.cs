using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBody : MonoBehaviour
{
    [Header("Gravity Setup")]
    [Tooltip("Drag the parent object containing all planets/attractors here.")]
    [SerializeField] private Transform attractorsParent;

    [SerializeField] private float GravitationalConstant = 0.667f;
    private List<GravitySource> attractors;
    public Vector2 LastNetGravityForce { get; private set; }
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        LastNetGravityForce = Vector2.zero;

        attractors = new List<GravitySource>();
        if (attractorsParent != null)
        {
            foreach (Transform child in attractorsParent)
            {
                if (child.TryGetComponent(out GravitySource source))
                {
                    attractors.Add(source);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (attractors == null) return;

        Vector2 combinedForce = Vector2.zero;
        foreach (var source in attractors)
        {
            if (source != null)
            {
                combinedForce += CalculateForceFromSource(source);
            }
        }

        LastNetGravityForce = combinedForce;

        // --- THE DEFINITIVE FIX ---
        // Instead of using AddForce, we calculate the acceleration and add it directly to the velocity.
        // This is a form of Euler integration and is much more stable for orbital calculations.
        Vector2 acceleration = combinedForce / rb.mass;
        rb.linearVelocity += acceleration * Time.fixedDeltaTime;
    }

    private Vector2 CalculateForceFromSource(GravitySource source)
    {
        if (source.transform.position == transform.position) return Vector2.zero;
        Vector2 direction = (Vector2)source.transform.position - rb.position;
        float distanceSqr = direction.sqrMagnitude;
        if (distanceSqr <= 0.01f) return Vector2.zero;
        float forceMagnitude = GravitationalConstant * (rb.mass * source.mass) / distanceSqr;
        return direction.normalized * forceMagnitude;
    }
}