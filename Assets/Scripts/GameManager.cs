using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Fade Transition")]
    private CanvasGroup fadeCanvasGroup;
    [Tooltip("How long it takes to fade TO black.")]
    [SerializeField] private float fadeOutDuration = 1.5f;
    [Tooltip("How long it takes to fade FROM black.")]
    [SerializeField] private float fadeInDuration = 1.0f;
    private bool isFading = false;

    public static GameManager Instance { get; private set; }

    public bool HasCheckpoint { get; private set; } = false;
    private Vector2 checkpointRespawnPosition;
    private Vector2 checkpointRespawnVelocity;

    private PlayerInputActions playerInputActions;
    private float currentAlpha = 0f;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = currentAlpha;
        }
        else
        {
            Debug.LogError("No CanvasGroup found in the new scene. Fading will not work.");
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadScene(sceneName));
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true;

        // Use the new fadeOutDuration
        yield return StartCoroutine(Fade(1f, fadeOutDuration));
        yield return SceneManager.LoadSceneAsync(sceneName);
        // Use the new fadeInDuration
        yield return StartCoroutine(Fade(0f, fadeInDuration));

        isFading = false;
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("Fade Canvas Group is not assigned or could not be found!");
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = currentAlpha;
        float timer = 0f;

        while (timer < duration) // Use the new duration parameter
        {
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            fadeCanvasGroup.alpha = currentAlpha;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        currentAlpha = targetAlpha;
        fadeCanvasGroup.alpha = currentAlpha;
        fadeCanvasGroup.blocksRaycasts = (currentAlpha >= 1f);
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