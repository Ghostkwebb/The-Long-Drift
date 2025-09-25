using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerCameraRig : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform planetsParent;

    [Header("Manual Zoom")]
    [SerializeField] private float minZoom = 15f;
    [SerializeField] private float maxZoom = 40f;
    [SerializeField] private float defaultZoom = 25f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Smart Framing")]
    [Tooltip("The distance at which the camera starts blending its focus towards the planet.")]
    [SerializeField] private float framingDistanceStart = 60f;
    [Tooltip("The distance at which the camera is fully focused on the group midpoint.")]
    [SerializeField] private float framingDistanceEnd = 30f;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothTime = 0.2f; // Slightly increased for a smoother feel
    [SerializeField] private float zoomSmoothTime = 0.15f;

    // Private State
    private Camera mainCamera;
    private PlayerInputActions playerInputActions;
    private List<GravitySource> allPlanetSources;
    private Transform closestPlanet;
    private float targetZoom;
    private Vector3 followVelocity;
    private float zoomVelocity;

    // --- (Awake, OnEnable, OnDisable, Start, and Update methods are all correct and unchanged) ---
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    private void Start()
    {
        targetZoom = defaultZoom;
        allPlanetSources = new List<GravitySource>();
        if (planetsParent != null)
        {
            planetsParent.GetComponentsInChildren<GravitySource>(allPlanetSources);
        }
    }

    private void Update()
    {
        float scrollInput = playerInputActions.Player.Zoom.ReadValue<float>();
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            targetZoom -= Mathf.Sign(scrollInput) * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }


    // --- THE DEFINITIVE FIX IS IN LATEUPDATE ---
    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. Find the closest planet.
        FindClosestPlanet();

        // 2. Calculate the target position.
        Vector3 playerPos = playerTransform.position;
        Vector3 targetPosition;

        if (closestPlanet == null)
        {
            targetPosition = playerPos;
        }
        else
        {
            // Calculate the midpoint between the player and the planet.
            Vector3 groupCenter = (playerPos + closestPlanet.position) / 2f;

            // Calculate the blend factor (0 = player focus, 1 = group focus).
            float distance = Vector3.Distance(playerPos, closestPlanet.position);
            float blendFactor = 1f - Mathf.Clamp01(Mathf.InverseLerp(framingDistanceEnd, framingDistanceStart, distance));

            // Blend the camera's target position between the player and the group center.
            targetPosition = Vector3.Lerp(playerPos, groupCenter, blendFactor);
        }

        // 3. Apply the smooth movement and zoom.
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), ref followVelocity, followSmoothTime);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
    }

    private void FindClosestPlanet()
    {
        if (allPlanetSources == null || allPlanetSources.Count == 0)
        {
            closestPlanet = null;
            return;
        }
        closestPlanet = allPlanetSources
            .OrderBy(p => Vector3.Distance(playerTransform.position, p.transform.position))
            .FirstOrDefault()?.transform;
    }
}