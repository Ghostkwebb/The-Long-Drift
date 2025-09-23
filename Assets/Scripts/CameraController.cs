using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minOrthographicSize = 15f;
    [SerializeField] private float maxOrthographicSize = 30f;
    [SerializeField] private float defaultOrthographicSize = 20f;
    [SerializeField] private float zoomStep = 2f;
    [SerializeField] private float zoomSpeed = 10f;

    private CinemachineCamera virtualCamera;
    private PlayerInputActions playerInputActions;

    private float targetOrthographicSize;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        playerInputActions = new PlayerInputActions();
    }

    private void Start()
    {
        targetOrthographicSize = defaultOrthographicSize;
        virtualCamera.Lens.OrthographicSize = defaultOrthographicSize;
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
        // 1. Read the raw scroll value.
        float scrollInput = playerInputActions.Player.Zoom.ReadValue<float>();

        // 2. Check if there was any scroll input this frame.
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            // 3. Determine direction and adjust the target size.
            // We invert the input because scrolling up (positive) should zoom in (decrease size).
            if (scrollInput > 0)
            {
                targetOrthographicSize -= zoomStep;
            }
            else if (scrollInput < 0)
            {
                targetOrthographicSize += zoomStep;
            }

            // 4. Clamp the target size to our limits.
            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthographicSize, maxOrthographicSize);
        }

        // 5. Smoothly interpolate the camera's actual size towards the target size every frame.
        virtualCamera.Lens.OrthographicSize = Mathf.Lerp(
            virtualCamera.Lens.OrthographicSize,
            targetOrthographicSize,
            Time.deltaTime * zoomSpeed
        );
    }
}