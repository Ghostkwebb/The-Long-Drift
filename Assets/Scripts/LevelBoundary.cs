using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    [Tooltip("If the player exits the boundary slower than this speed, they will be nudged back in instead of dying. Prevents respawn death loops.")]
    [SerializeField] private float minExitSpeedForDeath = 5f;

    [Tooltip("How strongly the player is bounced back when exiting at low speed.")]
    [SerializeField] private float nudgeFactor = -0.5f;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
            {
                if (playerRb.linearVelocity.magnitude < minExitSpeedForDeath)
                {
                    Debug.Log("Player exited boundary at low speed. Nudging back into play area.");
                    playerRb.linearVelocity *= nudgeFactor;
                }
                else
                {
                    Debug.Log("LOSS CONDITION: Out of bounds at high speed!");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.TriggerLoseState();
                    }
                    other.gameObject.SetActive(false);
                }
            }
        }
    }
}