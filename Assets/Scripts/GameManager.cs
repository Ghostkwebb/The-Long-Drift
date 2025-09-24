using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool HasCheckpoint { get; private set; } = false;
    private Vector2 checkpointRespawnPosition;
    private Vector2 checkpointRespawnVelocity;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Reset.performed += ResetLevel;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Disable();
        }
    }

    public void RespawnPlayer(float delay)
    {
        StartCoroutine(RespawnCoroutine(delay));
    }

    private IEnumerator RespawnCoroutine(float delay)
    {
        Debug.Log($"Respawn sequence started. Waiting for {delay} real-world seconds.");
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log("Respawn timer finished. Reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetLevel(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetCheckpoint(Transform planet, Vector2 respawnPosition, Vector2 respawnVelocity)
    {
        Debug.Log("CHECKPOINT SET at planet: " + planet.name);
        HasCheckpoint = true;
        checkpointRespawnPosition = respawnPosition;
        checkpointRespawnVelocity = respawnVelocity;
    }

    public Vector2 GetCheckpointPosition() => checkpointRespawnPosition;
    public Vector2 GetCheckpointVelocity() => checkpointRespawnVelocity;
}