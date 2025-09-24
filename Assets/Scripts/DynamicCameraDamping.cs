using UnityEngine;
using Unity.Cinemachine;

public class DynamicPositionDamping : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Rigidbody2D of the player's ship.")]
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Damping Settings")]
    [Tooltip("Positional damping at zero speed (higher value = more lag).")]
    [SerializeField] private float minDamping = 1f;
    [Tooltip("Positional damping at max speed (lower value = more responsive).")]
    [SerializeField] private float maxDamping = 0f;
    [Tooltip("The speed at which the camera will be at its most responsive.")]
    [SerializeField] private float speedForMaxDamping = 50f;

    private CinemachinePositionComposer positionComposer;

    private void Awake()
    {
        positionComposer = GetComponent<CinemachinePositionComposer>();
    }

    private void LateUpdate()
    {
        if (positionComposer == null || playerRigidbody == null)
        {
            return;
        }

        float speed = playerRigidbody.linearVelocity.magnitude;
        float speedLerpFactor = Mathf.InverseLerp(0, speedForMaxDamping, speed);
        float dynamicDamping = Mathf.Lerp(minDamping, maxDamping, speedLerpFactor);

        positionComposer.Damping = new Vector3(dynamicDamping, dynamicDamping, 0);
    }
}