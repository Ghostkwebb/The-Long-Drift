using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    [SerializeField] private int predictionSteps = 150;
    [SerializeField] private float timeStep = 0.05f;

    // --- NEW VARIABLE ---
    [Tooltip("The minimum gravitational force needed to show the prediction line.")]
    [SerializeField] private float gravityThreshold = 0.1f;

    [Header("Scene References")]
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private LayerMask collisionLayers;

    private Scene predictionScene;
    private PhysicsScene2D predictionPhysicsScene;
    private LineRenderer lineRenderer;
    private Rigidbody2D playerRb;

    private readonly List<GravitySource> ghostAttractors = new List<GravitySource>();
    private GameObject ghostPlayer;
    private Rigidbody2D ghostPlayerRb;

    // --- NEW VARIABLE ---
    // A list of the REAL attractors in the main scene.
    private GravitySource[] realAttractors;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();

        // --- NEW ---
        // Find all real planets at the start for the threshold check.
        realAttractors = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);

        CreatePredictionScene();
        CreateGhostObjects();
    }

    private void CreatePredictionScene()
    {
        CreateSceneParameters sceneParams = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        predictionScene = SceneManager.CreateScene("PredictionScene", sceneParams);
        predictionPhysicsScene = predictionScene.GetPhysicsScene2D();
    }

    private void CreateGhostObjects()
    {
        ghostPlayer = Instantiate(gameObject);
        SceneManager.MoveGameObjectToScene(ghostPlayer, predictionScene);
        ghostPlayerRb = ghostPlayer.GetComponent<Rigidbody2D>();

        if (ghostPlayer.TryGetComponent<GravityBody>(out var gb))
        {
            gb.enabled = false;
        }
        if (ghostPlayer.TryGetComponent<GravityIndicator>(out var gi))
        {
            gi.enabled = false;
        }

        foreach (var renderer in ghostPlayer.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.enabled = false;
        }

        ghostAttractors.Clear();
        if (obstaclesParent != null)
        {
            foreach (Transform child in obstaclesParent)
            {
                GameObject ghostObstacle = Instantiate(child.gameObject);
                SceneManager.MoveGameObjectToScene(ghostObstacle, predictionScene);

                foreach (var renderer in ghostObstacle.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.enabled = false;
                }
                ghostAttractors.Add(ghostObstacle.GetComponent<GravitySource>());
            }
        }
    }

    // --- MODIFIED METHOD ---
    void FixedUpdate()
    {
        if (!predictionPhysicsScene.IsValid()) return;

        // Calculate the current net gravity on the REAL player
        Vector2 netGravityForce = CalculateNetGravityForce();

        // If the force is too weak, hide the line and do nothing else.
        if (netGravityForce.magnitude < gravityThreshold)
        {
            lineRenderer.enabled = false;
            return;
        }

        // If the force is strong enough, show the line and run the simulation.
        lineRenderer.enabled = true;
        SimulateTrajectory();
    }

    private void SimulateTrajectory()
    {
        ghostPlayer.transform.position = transform.position;
        ghostPlayer.transform.rotation = transform.rotation;
        ghostPlayerRb.linearVelocity = playerRb.linearVelocity;
        ghostPlayerRb.angularVelocity = playerRb.angularVelocity;

        var positions = new List<Vector3> { ghostPlayer.transform.position };

        for (int i = 0; i < predictionSteps; i++)
        {
            ApplyGhostGravity();

            Vector3 previousPosition = ghostPlayer.transform.position;
            predictionPhysicsScene.Simulate(timeStep);
            Vector3 currentPosition = ghostPlayer.transform.position;

            RaycastHit2D hit = Physics2D.Linecast(previousPosition, currentPosition, collisionLayers);
            if (hit.collider != null)
            {
                positions.Add(hit.point);
                break;
            }

            positions.Add(currentPosition);
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    private void ApplyGhostGravity()
    {
        const float GravitationalConstant = 0.667f;
        foreach (var source in ghostAttractors)
        {
            Vector2 direction = (Vector2)source.transform.position - ghostPlayerRb.position;
            float distanceSqr = direction.sqrMagnitude;
            if (distanceSqr <= 0.01f) continue;

            float forceMagnitude = GravitationalConstant * (ghostPlayerRb.mass * source.mass) / distanceSqr;
            Vector2 forceVector = direction.normalized * forceMagnitude;
            ghostPlayerRb.AddForce(forceVector);
        }
    }

    // --- NEW METHOD ---
    // Calculates the combined gravity from all real planets on the real player.
    private Vector2 CalculateNetGravityForce()
    {
        const float GravitationalConstant = 0.667f;
        Vector2 netForce = Vector2.zero;

        foreach (var source in realAttractors)
        {
            Vector2 direction = (Vector2)source.transform.position - playerRb.position;
            float distanceSqr = direction.sqrMagnitude;
            if (distanceSqr <= 0.01f) continue;

            float forceMagnitude = GravitationalConstant * (playerRb.mass * source.mass) / distanceSqr;
            netForce += direction.normalized * forceMagnitude;
        }

        return netForce;
    }

    private void OnDestroy()
    {
        if (predictionScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(predictionScene);
        }
    }
}