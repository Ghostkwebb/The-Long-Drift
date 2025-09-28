using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSliders(musicSlider, sfxSlider);
            SoundManager.Instance.CrossfadeMusic(SoundManager.Instance.mainMenuMusic, 0.5f);
        }
    }

    // --- Button Methods ---
    public void OnPlayButton()
    {
        if (SoundManager.Instance != null)
        {
            // When we press play, tell the SoundManager to crossfade to the game music.
            SoundManager.Instance.CrossfadeMusic(SoundManager.Instance.gameMusic, 1.5f);
        }

        // Load your first game level. Replace "GameScene" with your actual scene name.
        GameManager.Instance.LoadSceneWithFade("MainLevel");
    }

    public void OnSettingsButton()
    {
        settingsPanel.SetActive(true);
    }

    public void OnQuitButton()
    {
        Application.Quit();
        Debug.Log("Application Quit!"); // For testing in the editor
    }

    public void OnBackButton()
    {
        settingsPanel.SetActive(false);
    }

    // --- Slider Methods (Identical to PauseMenu) ---
    public void OnMusicSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }
}