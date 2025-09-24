using UnityEngine;

public class GravitySource : MonoBehaviour
{
    [SerializeField] public float mass = 1000f;

    [Tooltip("If the player achieves a stable orbit here, they win! hehehehe")]
    [SerializeField] public bool isDestination = false;

    [Tooltip("If the player achieves a stable orbit here, their fuel is refilled and their respawn point is set.")]
    [SerializeField] public bool isCheckpoint = false;

    [Tooltip("If true, the player will start the level in a stable orbit around this planet (if no checkpoint is set).")]
    [SerializeField] public bool isStartingPlanet = false;
}