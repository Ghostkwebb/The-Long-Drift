using UnityEngine;
using System.Collections.Generic; // Required for lists
using System.Linq; // Required for FirstOrDefault

public class GravityIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowTransform;
    [Tooltip("The parent object containing all planets.")]
    [SerializeField] private Transform planetsParent;

    [Header("Tuning")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0, -0.7f);
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float forceAtMaxScale = 10f;

    private SpriteRenderer arrowSpriteRenderer;
    private Rigidbody2D playerRb; // We need the Rigidbody for its mass
    private List<GravitySource> realAttractors = new List<GravitySource>();

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();

        if (arrowTransform != null)
        {
            arrowSpriteRenderer = arrowTransform.GetComponent<SpriteRenderer>();
        }

        // Find all real planets at the start.
        if (planetsParent != null)
        {
            foreach (Transform child in planetsParent)
            {
                if (child.TryGetComponent(out GravitySource source))
                {
                    realAttractors.Add(source);
                }
            }
        }
    }

    void Update()
    {
        if (arrowTransform == null || playerRb == null || arrowSpriteRenderer == null) return;

        // --- THE FIX ---
        // 1. Calculate the force in Update, using the ship's current visual position.
        Vector2 netGravityForce = CalculateGravityAtPoint(transform.position);
        float forceMagnitude = netGravityForce.magnitude;

        // 2. Set the arrow's position based on the current visual position.
        arrowTransform.position = (Vector2)transform.position + positionOffset;

        // 3. Update visibility.
        arrowSpriteRenderer.enabled = forceMagnitude >= 0.01f;
        if (!arrowSpriteRenderer.enabled) return;

        // 4. Update rotation and scale based on the fresh data.
        float angle = Mathf.Atan2(netGravityForce.y, netGravityForce.x) * Mathf.Rad2Deg;
        arrowTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        float scaleLerpFactor = Mathf.InverseLerp(0, forceAtMaxScale, forceMagnitude);
        float targetScale = Mathf.Lerp(minScale, maxScale, scaleLerpFactor);
        arrowTransform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    private Vector2 CalculateGravityAtPoint(Vector2 point)
    {
        const float GravitationalConstant = 0.667f;
        Vector2 netForce = Vector2.zero;
        foreach (var source in realAttractors)
        {
            if (source == null) continue;
            Vector2 direction = (Vector2)source.transform.position - point;

            float distanceSqr = direction.sqrMagnitude;
            if (distanceSqr <= 0.01f) continue;

            float forceMagnitude = GravitationalConstant * (playerRb.mass * source.mass) / distanceSqr;
            netForce += direction.normalized * forceMagnitude;
        }
        return netForce;
    }
}