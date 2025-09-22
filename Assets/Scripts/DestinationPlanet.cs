using UnityEngine;

// This component requires a CircleCollider2D to function as a trigger zone.
[RequireComponent(typeof(CircleCollider2D))]
public class DestinationPlanet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the player.
        if (other.CompareTag("Player"))
        {
            // Try to get the RevolutionCounter script from the player.
            if (other.TryGetComponent(out RevolutionCounter counter))
            {
                // Tell the counter to start tracking, using this planet as the center.
                counter.StartTracking(transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object leaving the trigger is the player.
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out RevolutionCounter counter))
            {
                // Tell the counter to stop tracking and reset its progress.
                counter.StopAndResetTracking();
            }
        }
    }
}