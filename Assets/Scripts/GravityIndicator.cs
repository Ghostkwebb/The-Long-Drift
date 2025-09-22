using UnityEngine;

public class GravityIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the GravityArrow from the Hierarchy here.")]
    [SerializeField] private Transform arrowTransform;

    [Header("Tuning")]
    [Tooltip("The arrow's position relative to the ship (in world space).")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0, -0.7f);
    [Tooltip("Adjust this value until the arrow points towards the planet.")]
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float forceAtMaxScale = 10f;

    private GravityBody gravityBody;
    private SpriteRenderer arrowSpriteRenderer;

    void Start()
    {
        gravityBody = GetComponent<GravityBody>();

        if (arrowTransform != null)
        {
            arrowSpriteRenderer = arrowTransform.GetComponent<SpriteRenderer>();
        }
    }

    void FixedUpdate()
    {
        // Safety check to ensure the arrow hasn't been destroyed
        if (arrowTransform == null || gravityBody == null || arrowSpriteRenderer == null) return;

        // --- NEW POSITIONING LOGIC ---
        // Set the arrow's world position to be the ship's world position plus our offset.
        // This is not affected by the ship's rotation.
        arrowTransform.position = (Vector2)transform.position + positionOffset;

        Vector2 netGravityForce = gravityBody.LastNetGravityForce;
        float forceMagnitude = netGravityForce.magnitude;

        if (forceMagnitude < 0.01f)
        {
            arrowSpriteRenderer.enabled = false;
            return;
        }

        arrowSpriteRenderer.enabled = true;

        float angle = Mathf.Atan2(netGravityForce.y, netGravityForce.x) * Mathf.Rad2Deg;
        arrowTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        float scaleLerpFactor = Mathf.InverseLerp(0, forceAtMaxScale, forceMagnitude);
        float targetScale = Mathf.Lerp(minScale, maxScale, scaleLerpFactor);
        arrowTransform.localScale = new Vector3(targetScale, targetScale, 1f);
    }
}