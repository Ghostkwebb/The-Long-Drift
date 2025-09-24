using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("LOSS CONDITION: Out of bounds!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(0.5f);
            }

            other.gameObject.SetActive(false);
        }
    }
}