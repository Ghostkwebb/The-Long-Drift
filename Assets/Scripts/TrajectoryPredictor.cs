using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    [SerializeField] private int maxPredictionSteps = 600;
    [SerializeField] private float timeStep = 0.05f;
    [SerializeField] private float gravityThreshold = 0.01f;

    [Header("Orbit Visualization")]
    [SerializeField] private float orbitCloseThreshold = 1.5f;
    [SerializeField] private int minStepsForOrbitCheck = 50;

    [Header("Scene References")]
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private LayerMask collisionLayers;

    private LineRenderer lineRenderer;
    private Rigidbody2D playerRb;
    private GravityBody gravityBody;

    private readonly List<GravitySource> realAttractors = new List<GravitySource>();

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        gravityBody = GetComponent<GravityBody>();

        if (obstaclesParent != null)
        {
            foreach (Transform child in obstaclesParent)
            {
                if (child.TryGetComponent(out GravitySource source))
                {
                    realAttractors.Add(source);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (gravityBody == null) return;

        if (gravityBody.LastNetGravityForce.magnitude < gravityThreshold)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        SimulateTrajectory();
    }

    private void SimulateTrajectory()
    {
        var positions = new List<Vector3>();

        // 1. Get the current state from the Rigidbody.
        Vector2 currentPosition = playerRb.position;
        Vector2 currentVelocity = playerRb.linearVelocity;

        Vector2 initialAcceleration = gravityBody.LastNetGravityForce / playerRb.mass;
        currentVelocity += initialAcceleration * Time.fixedDeltaTime;
        // --- END FIX ---

        positions.Add(currentPosition);

        for (int i = 0; i < maxPredictionSteps; i++)
        {
            Vector2 gravityAcceleration = CalculateGravityAtPoint(currentPosition) / playerRb.mass;
            currentVelocity += gravityAcceleration * timeStep;
            Vector2 nextPosition = currentPosition + currentVelocity * timeStep;

            RaycastHit2D hit = Physics2D.Linecast(currentPosition, nextPosition, collisionLayers);
            if (hit.collider != null)
            {
                positions.Add(hit.point);
                break;
            }

            positions.Add(nextPosition);
            currentPosition = nextPosition;

            if (i > minStepsForOrbitCheck && Vector2.Distance(positions[0], currentPosition) < orbitCloseThreshold)
            {
                positions.Add(positions[0]);
                break;
            }
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
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