using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Paused, Won, Lost }
    public GameState CurrentState { get; private set; }

    [Header("UI Panels")]
    private GameObject winPanel;
    private GameObject losePanel;

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
        Time.timeScale = 1f;
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

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void RegisterLevelUI(GameObject winPanel, GameObject losePanel)
    {
        this.winPanel = winPanel;
        this.losePanel = losePanel;
        // Ensure they are off when registered.
        if (this.winPanel != null) this.winPanel.SetActive(false);
        if (this.losePanel != null) this.losePanel.SetActive(false);
    }

    private void Start()
    {
        // When a level starts, we are in the "Playing" state.
        CurrentState = GameState.Playing;
        EnablePlayerInput();
    }

    public void TriggerWinState()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Won;
        Debug.Log("Game State: WON");
        Time.timeScale = 0f;
        if (winPanel != null) winPanel.SetActive(true);
        else Debug.LogError("Win Panel is not registered to GameManager!");
    }

    public void TriggerLoseState()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Lost;
        Debug.Log("Game State: LOST");
        if (losePanel != null) losePanel.SetActive(true);
        else Debug.LogError("Lose Panel is not registered to GameManager!");

        RespawnPlayer(2.0f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CurrentState = GameState.Playing;
        fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
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
        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f;

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

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Always unpause before changing scenes
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.CrossfadeMusic(SoundManager.Instance.mainMenuMusic, 1.0f);
        }
        LoadSceneWithFade("MainMenu"); // Use your scene name
    }

    private void EnablePlayerInput() => playerInputActions.Player.Enable();
    private void DisablePlayerInput() => playerInputActions.Player.Disable();

    public Vector2 GetCheckpointPosition() => checkpointRespawnPosition;
    public Vector2 GetCheckpointVelocity() => checkpointRespawnVelocity;
}