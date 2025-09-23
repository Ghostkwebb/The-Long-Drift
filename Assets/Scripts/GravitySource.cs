using UnityEngine;

public class GravitySource : MonoBehaviour
{
    [SerializeField] public float mass = 1000f;

    [Tooltip("Is this the planet the player needs to orbit to win the level?")]
    [SerializeField] public bool isDestination = false;
}