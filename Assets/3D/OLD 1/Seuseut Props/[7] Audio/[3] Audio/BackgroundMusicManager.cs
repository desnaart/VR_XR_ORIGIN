using UnityEngine;
using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("🎵 Audio Clips (Playlist)")]
    [Tooltip("Drag beberapa BGM ke sini. Bisa 1 atau lebih.")]
    public AudioClip[] musicTracks;

    [Header("🔁 Looping Settings")]
    public bool loopPlaylist = true;
    public bool shufflePlaylist = false;

    [Header("🔊 Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    public bool allowVolumeChange = true;

    [Header("✨ Immersive Settings")]
    public bool useSpatial3D = false;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public bool smoothVolumeFade = true;
    public float fadeDuration = 1f;

    [Header("🎚 Crossfade Settings")]
    public bool useCrossfade = false;
    public float crossfadeTime = 2f;

    private AudioSource activeSource;
    private AudioSource fadeSource;

    private int currentTrackIndex = 0;

    private void Awake()
    {
        activeSource = gameObject.AddComponent<AudioSource>();
        fadeSource = gameObject.AddComponent<AudioSource>();

        SetupAudioSource(activeSource);
        SetupAudioSource(fadeSource);
    }

    private void Start()
    {
        if (musicTracks.Length > 0)
            PlayTrack(currentTrackIndex);
    }

    private void SetupAudioSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = false;
        src.volume = musicVolume;
        ApplySpatialSettings(src);
    }

    private void ApplySpatialSettings(AudioSource src)
    {
        if (useSpatial3D)
        {
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 2f;
            src.maxDistance = 50f;
        }
        else
        {
            src.spatialBlend = spatialBlend; 
        }
    }

    // =====================================================
    // 🎵 MAIN PLAYBACK
    // =====================================================
    public void PlayTrack(int index)
    {
        if (musicTracks.Length == 0) return;

        index = Mathf.Clamp(index, 0, musicTracks.Length - 1);
        currentTrackIndex = index;

        if (!useCrossfade)
        {
            activeSource.clip = musicTracks[index];
            activeSource.volume = musicVolume;
            activeSource.Play();
            StartCoroutine(TrackWatcher());
        }
        else
        {
            StartCoroutine(CrossfadeTrack(index));
        }
    }

    private IEnumerator CrossfadeTrack(int index)
    {
        fadeSource.clip = musicTracks[index];
        fadeSource.volume = 0f;
        fadeSource.Play();

        float t = 0f;

        while (t < crossfadeTime)
        {
            float alpha = t / crossfadeTime;
            fadeSource.volume = alpha * musicVolume;
            activeSource.volume = (1f - alpha) * musicVolume;
            t += Time.deltaTime;
            yield return null;
        }

        AudioSource temp = activeSource;
        activeSource = fadeSource;
        fadeSource = temp;
        activeSource.volume = musicVolume;

        fadeSource.Stop();

        StartCoroutine(TrackWatcher());
    }

    private IEnumerator TrackWatcher()
    {
        while (activeSource.isPlaying)
            yield return null;

        NextTrack();
    }

    // =====================================================
    // 🎵 PLAYLIST MANAGEMENT
    // =====================================================
    private void NextTrack()
    {
        if (!loopPlaylist && currentTrackIndex >= musicTracks.Length - 1)
            return;

        if (shufflePlaylist)
            currentTrackIndex = Random.Range(0, musicTracks.Length);
        else
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;

        PlayTrack(currentTrackIndex);
    }

    // =====================================================
    // 🔊 EXTERNAL CONTROL (UI, TRIGGER, XR ACTION)
    // =====================================================

    public void SetVolume(float vol)
    {
        if (!allowVolumeChange) return;

        musicVolume = Mathf.Clamp01(vol);
        activeSource.volume = musicVolume;
    }

    public void FadeIn()
    {
        StartCoroutine(FadeVolume(activeSource, 0f, musicVolume, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeVolume(activeSource, musicVolume, 0f, fadeDuration));
    }

    private IEnumerator FadeVolume(AudioSource src, float start, float end, float duration)
    {
        if (!smoothVolumeFade)
        {
            src.volume = end;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            src.volume = Mathf.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        src.volume = end;
    }

    public void StopMusic()
    {
        activeSource.Stop();
    }

    public void PlayMusic()
    {
        activeSource.Play();
    }
}