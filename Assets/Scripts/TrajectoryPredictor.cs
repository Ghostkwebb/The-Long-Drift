using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    [SerializeField] private int maxPredictionSteps = 600;
    [SerializeField] private float timeStep = 0.05f;

    [Header("Scene References")]
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private LayerMask collisionLayers;

    private LineRenderer lineRenderer;
    private Rigidbody2D playerRb;

    private List<GravitySource> realAttractors = new List<GravitySource>();

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();

        if (obstaclesParent != null)
        {
            obstaclesParent.GetComponentsInChildren<GravitySource>(realAttractors);
        }
    }

    private void LateUpdate()
    {
        SimulateTrajectory();
    }

    private void SimulateTrajectory()
    {
        var positions = new List<Vector3>();

        Vector2 currentPosition = playerRb.position;
        Vector2 currentVelocity = playerRb.linearVelocity;

        positions.Add(currentPosition);

        for (int i = 0; i < maxPredictionSteps; i++)
        {
            // Use the exact same math as GravityBody.cs to calculate acceleration.
            Vector2 gravityAcceleration = CalculateGravityAtPoint(currentPosition) / playerRb.mass;

            // Apply the acceleration to our simulated velocity.
            currentVelocity += gravityAcceleration * timeStep;

            // Update our simulated position.
            Vector2 nextPosition = currentPosition + currentVelocity * timeStep;

            // Check for collisions.
            RaycastHit2D hit = Physics2D.Linecast(currentPosition, nextPosition, collisionLayers);
            if (hit.collider != null)
            {
                positions.Add(hit.point);
                break;
            }

            positions.Add(nextPosition);
            currentPosition = nextPosition;
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