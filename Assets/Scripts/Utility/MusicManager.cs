using System.Collections;
using UnityEngine;

/// <summary>
/// Manages background music tracks with smooth crossfading transitions.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance = null;

    [Header("Audio Tracks")]
    [Tooltip("The normal background music track.")]
    public AudioClip normalBGM;
    [Tooltip("The boss fight music track.")]
    public AudioClip bossBGM;

    [Header("Transition Settings")]
    [Tooltip("Duration of the crossfade in seconds.")]
    public float fadeDuration = 1.5f;
    [Tooltip("Maximum volume for normal background music.")]
    [Range(0f, 1f)]
    public float maxVolume = 0.5f;
    [Tooltip("Maximum volume for the boss music.")]
    [Range(0f, 1f)]
    public float maxBossVolume = 0.8f;

    // Two AudioSources to allow smooth crossfading
    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null, false); // Detach using SetParent(null, false) to resolve RectTransform warning
            }
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start playing normal BGM automatically if assigned
        if (normalBGM != null)
        {
            PlayTrack(normalBGM);
        }
    }

    private void InitializeAudioSources()
    {
        // Add two AudioSources to the GameObject
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        // Configure them for loop and volume
        ConfigureSource(sourceA);
        ConfigureSource(sourceB);

        activeSource = sourceA;
    }

    private void ConfigureSource(AudioSource source)
    {
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    /// <summary>
    /// Switches to the Normal BGM track.
    /// </summary>
    public void PlayNormalBGM()
    {
        PlayTrack(normalBGM);
    }

    /// <summary>
    /// Switches to the Boss BGM track.
    /// </summary>
    public void PlayBossBGM()
    {
        PlayTrack(bossBGM);
    }

    /// <summary>
    /// Swaps to the target audio clip with a crossfade.
    /// </summary>
    private void PlayTrack(AudioClip clip)
    {
        if (clip == null) return;

        // If the track is already playing on the active source, do nothing
        if (activeSource.isPlaying && activeSource.clip == clip) return;

        // Determine the inactive source to fade in
        AudioSource newSource = (activeSource == sourceA) ? sourceB : sourceA;

        newSource.clip = clip;
        newSource.Play();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float targetVolume = (clip == bossBGM) ? maxBossVolume : maxVolume;
        fadeCoroutine = StartCoroutine(CrossFadeRoutine(activeSource, newSource, targetVolume, fadeDuration));
        activeSource = newSource;
    }

    private IEnumerator CrossFadeRoutine(AudioSource fadeOutSource, AudioSource fadeInSource, float targetVolume, float duration)
    {
        float elapsed = 0f;

        // Store initial volumes
        float startFadeOutVol = fadeOutSource.volume;
        float startFadeInVol = fadeInSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smoothly interpolate volumes
            fadeOutSource.volume = Mathf.Lerp(startFadeOutVol, 0f, t);
            fadeInSource.volume = Mathf.Lerp(startFadeInVol, targetVolume, t);

            yield return null;
        }

        fadeOutSource.volume = 0f;
        fadeOutSource.Stop();
        fadeInSource.volume = targetVolume;

        fadeCoroutine = null;
    }
}
