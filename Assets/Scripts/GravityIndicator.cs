using UnityEngine;

public class GravityIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the GravityArrow from the Hierarchy here.")]
    [SerializeField] private Transform arrowTransform;

    [Header("Tuning")]
    [SerializeField] private Vector2 positionOffset = new Vector2(0, -0.7f);
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float forceAtMaxScale = 10f;

    private GravityBody gravityBody;
    private SpriteRenderer arrowSpriteRenderer;


    private Quaternion smoothedRotation;
    private Vector3 smoothedScale;
    private bool isVisible = false;

    void Start()
    {
        gravityBody = GetComponent<GravityBody>();
        if (arrowTransform != null)
        {
            arrowSpriteRenderer = arrowTransform.GetComponent<SpriteRenderer>();

            smoothedRotation = arrowTransform.rotation;
            smoothedScale = arrowTransform.localScale;
        }
    }

    void Update()
    {
        if (arrowTransform == null || gravityBody == null || arrowSpriteRenderer == null) return;

        arrowTransform.position = (Vector2)transform.position + positionOffset;

        arrowTransform.rotation = smoothedRotation;
        arrowTransform.localScale = smoothedScale;
        arrowSpriteRenderer.enabled = isVisible;
    }

    void FixedUpdate()
    {
        if (gravityBody == null) return;

        Vector2 netGravityForce = gravityBody.LastNetGravityForce;
        float forceMagnitude = netGravityForce.magnitude;

        isVisible = forceMagnitude >= 0.01f;
        if (!isVisible) return;

        float angle = Mathf.Atan2(netGravityForce.y, netGravityForce.x) * Mathf.Rad2Deg;
        smoothedRotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        float scaleLerpFactor = Mathf.InverseLerp(0, forceAtMaxScale, forceMagnitude);
        float targetScale = Mathf.Lerp(minScale, maxScale, scaleLerpFactor);
        smoothedScale = new Vector3(targetScale, targetScale, 1f);
    }
}