using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusic;

    private Coroutine musicFadeCoroutine;

    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        LoadVolume();
    }

    public void RegisterSliders(Slider musicSlider, Slider sfxSlider)
    {
        this.musicVolumeSlider = musicSlider;
        this.sfxVolumeSlider = sfxSlider;

        // Add listeners directly here.
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // Immediately load and apply the correct volume to the mixer AND the sliders.
        LoadVolume();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        musicVolumeSlider = null;
        sfxVolumeSlider = null;
    }

    public void SetMusicVolume(float level)
    {
        if (level < 0.001f) mainMixer.SetFloat("MusicVolume", -80f);
        else mainMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20);
        PlayerPrefs.SetFloat("MusicVolume", level);
    }

    public void SetSFXVolume(float level)
    {
        if (level < 0.001f) mainMixer.SetFloat("SFXVolume", -80f);
        else mainMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20);
        PlayerPrefs.SetFloat("SFXVolume", level);
    }

    private void LoadVolume()
    {
        float musicLevel = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxLevel = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Set the actual mixer volume.
        SetMusicVolume(musicLevel);
        SetSFXVolume(sfxLevel);

        // If we have references to the sliders, set their visual position.
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicLevel;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxLevel;
    }

    public void CrossfadeMusic(AudioClip newClip, float fadeDuration)
    {
        // If a fade is already happening, stop it.
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        // Start the new fade.
        musicFadeCoroutine = StartCoroutine(FadeMusicRoutine(newClip, fadeDuration));
    }

    private IEnumerator FadeMusicRoutine(AudioClip newClip, float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        // --- 1. Fade Out ---
        while (timer < fadeDuration)
        {
            // Decrease volume over time.
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        musicSource.volume = 0f;

        // --- 2. Swap the Clip ---
        // Don't swap if the new clip is the same as the old one.
        if (musicSource.clip != newClip)
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }

        // --- 3. Fade In ---
        timer = 0f; // Reset the timer
        while (timer < fadeDuration)
        {
            // Increase volume over time.
            musicSource.volume = Mathf.Lerp(0f, startVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = startVolume;

        musicFadeCoroutine = null; // Mark the coroutine as finished.
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}