using UnityEngine;
using UnityEngine.UI; // Required for Sliders
using UnityEngine.SceneManagement; // Required for loading scenes

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private bool isPaused = false;

    private void Start()
    {
        // Initial setup on the first frame.
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSliders(musicSlider, sfxSlider);
        }
    }

    private void Update()
    {
        // Listen for the Escape key.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause the game.
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }
        else
        {
            // Unpause the game.
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false); // Also close settings when unpausing.
        }
    }

    // --- Button Methods ---
    public void OnResumeButton()
    {
        TogglePause();
    }

    public void OnSettingsButton()
    {
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void OnHomeButton()
    {
        // Make sure to unpause before leaving the scene.
        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.CrossfadeMusic(SoundManager.Instance.mainMenuMusic, 1.0f);
        }

        // Replace "MainMenu" with the actual name of your main menu scene.
        GameManager.Instance.LoadSceneWithFade("MainMenu");
    }

    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // --- Slider Methods ---
    public void OnMusicSliderChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
}