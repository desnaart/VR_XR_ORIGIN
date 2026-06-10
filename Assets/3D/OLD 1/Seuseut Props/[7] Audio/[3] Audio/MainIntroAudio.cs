using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainIntroAudio : MonoBehaviour
{
    [Header("Intro Audio Settings")]
    public AudioClip introClip;
    public float introVolume = 1f;
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;

    private AudioSource introSource;
    private List<AudioSource> otherSources = new List<AudioSource>();
    private List<float> otherOriginalVolume = new List<float>();

    private void Awake()
    {
        introSource = gameObject.AddComponent<AudioSource>();
        introSource.clip = introClip;
        introSource.playOnAwake = false;
        introSource.loop = false;
        introSource.volume = 0f;
        introSource.spatialBlend = 0f; // intro biasanya non-3D agar jelas
    }

    private void Start()
    {
        // Ambil semua AudioSource di scene
        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);

        foreach (AudioSource src in allSources)
        {
            if (src != introSource)
            {
                otherSources.Add(src);
                otherOriginalVolume.Add(src.volume);
            }
        }

        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        // 1️⃣ Matikan semua suara lain
        foreach (var src in otherSources)
        {
            if (src != null)
            {
                src.volume = 0f;
                src.mute = true;
            }
        }

        // 2️⃣ Fade-in intro
        introSource.Play();
        float t = 0f;

        while (t < fadeInTime)
        {
            introSource.volume = Mathf.Lerp(0f, introVolume, t / fadeInTime);
            t += Time.deltaTime;
            yield return null;
        }

        introSource.volume = introVolume;

        // 3️⃣ Tunggu sampai intro selesai
        yield return new WaitForSeconds(introClip.length);

        // 4️⃣ Fade-out intro
        t = 0f;

        while (t < fadeOutTime)
        {
            introSource.volume = Mathf.Lerp(introVolume, 0f, t / fadeOutTime);
            t += Time.deltaTime;
            yield return null;
        }

        introSource.Stop();

        // 5️⃣ Hidupkan kembali semua suara lain
        for (int i = 0; i < otherSources.Count; i++)
        {
            if (otherSources[i] != null)
            {
                otherSources[i].mute = false;
                otherSources[i].volume = otherOriginalVolume[i];
            }
        }
    }
}