using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Prediction Settings")]
    [SerializeField] private int predictionSteps = 150;
    [SerializeField] private float timeStep = 0.05f;
    [Tooltip("Set this to 0 to force the line to always show for debugging.")]
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

    private GravitySource[] realAttractors;

    private void Awake()
    {
        // Cleanup logic remains the same
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "PredictionScene")
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
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

    // --- THE NEW, ROBUST METHOD ---
    private void CreateGhostObjects()
    {
        // 1. Create a brand new, empty GameObject for our physics probe.
        ghostPlayer = new GameObject("Ghost Physics Probe");

        // 2. Copy the Rigidbody2D properties from the real player.
        ghostPlayerRb = ghostPlayer.AddComponent<Rigidbody2D>();
        ghostPlayerRb.mass = playerRb.mass;
        ghostPlayerRb.linearDamping = playerRb.linearDamping;
        ghostPlayerRb.angularDamping = playerRb.angularDamping;
        ghostPlayerRb.gravityScale = 0; // Essential for custom gravity

        // 3. Copy the Collider2D properties from the real player.
        // This example handles Circle and Box colliders.
        var playerCollider = GetComponent<Collider2D>();
        if (playerCollider is CircleCollider2D circle)
        {
            var ghostCircle = ghostPlayer.AddComponent<CircleCollider2D>();
            ghostCircle.radius = circle.radius;
            ghostCircle.offset = circle.offset;
        }
        else if (playerCollider is BoxCollider2D box)
        {
            var ghostBox = ghostPlayer.AddComponent<BoxCollider2D>();
            ghostBox.size = box.size;
            ghostBox.offset = box.offset;
        }

        // 4. Move the clean probe to the prediction scene.
        SceneManager.MoveGameObjectToScene(ghostPlayer, predictionScene);

        // Ghost planet creation is fine and remains the same.
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

    // ... The rest of the script (FixedUpdate, SimulateTrajectory, etc.) is correct and unchanged ...
    void FixedUpdate()
    {
        if (!predictionPhysicsScene.IsValid()) return;

        Vector2 netGravityForce = CalculateNetGravityForce();
        if (netGravityForce.magnitude < gravityThreshold)
        {
            lineRenderer.positionCount = 0; // A better way to hide the line
            return;
        }

        SimulateTrajectory();
    }

    private void SimulateTrajectory()
    {
        lineRenderer.enabled = true;
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

    private Vector2 CalculateNetGravityForce()
    {
        const float GravitationalConstant = 0.667f;
        Vector2 netForce = Vector2.zero;
        foreach (var source in realAttractors)
        {
            if (source == null) continue;
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