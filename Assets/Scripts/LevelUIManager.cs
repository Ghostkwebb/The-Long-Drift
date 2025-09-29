using UnityEngine;

// This script lives in your GameScene and manages its specific UI.
public class LevelUIManager : MonoBehaviour
{
    [Header("Level UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private void Start()
    {
        // On start, find the persistent GameManager and give it our UI references.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLevelUI(winPanel, losePanel);
        }
        else
        {
            Debug.LogError("FATAL: GameManager not found! Win/Loss UI will not work.");
        }
    }
}