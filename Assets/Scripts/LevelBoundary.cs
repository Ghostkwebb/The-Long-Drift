using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("LOSS CONDITION: Out of bounds!");
            Destroy(other.gameObject);
        }
    }
}