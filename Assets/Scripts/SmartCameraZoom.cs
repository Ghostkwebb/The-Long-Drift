using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Verified correct namespace

public class SmartCameraZoom : MonoBehaviour
{
    [Header("Manual Zoom Settings")]
    [Tooltip("The tightest zoom level the player can set.")]
    [SerializeField] private float minManualZoom = 15f;
    [Tooltip("The widest zoom level the player can set.")]
    [SerializeField] private float maxManualZoom = 30f;
    [SerializeField] private float zoomStep = 2f;
    [SerializeField] private float zoomSmoothSpeed = 10f;

    [Header("Auto-Framing")]
    [Tooltip("How much extra room the camera is allowed to zoom out to keep the target in frame.")]
    [SerializeField] private float autoZoomHeadroom = 20f;

    private CinemachineGroupFraming groupFramer;
    private PlayerInputActions playerInputActions;

    private float targetManualZoom;

    private void Awake()
    {
        groupFramer = GetComponent<CinemachineGroupFraming>();
        playerInputActions = new PlayerInputActions();
    }

    private void Start()
    {
        targetManualZoom = 20f;
        if (groupFramer != null)
        {
            groupFramer.OrthoSizeRange = new Vector2(targetManualZoom, targetManualZoom + autoZoomHeadroom);
        }
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    private void Update()
    {
        if (groupFramer == null) return;

        float scrollInput = playerInputActions.Player.Zoom.ReadValue<float>();
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            targetManualZoom -= Mathf.Sign(scrollInput) * zoomStep;
            targetManualZoom = Mathf.Clamp(targetManualZoom, minManualZoom, maxManualZoom);
        }

        float smoothedManualZoom = Mathf.Lerp(
            groupFramer.OrthoSizeRange.x,
            targetManualZoom,
            Time.deltaTime * zoomSmoothSpeed
        );

        groupFramer.OrthoSizeRange = new Vector2(
            smoothedManualZoom,
            smoothedManualZoom + autoZoomHeadroom
        );
    }
}