using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-10000)] // jalan paling awal
public class AudioGlobalLock : MonoBehaviour
{
    private List<AudioSource> cachedSources = new List<AudioSource>();

    void Awake()
    {
        // Ambil semua AudioSource (termasuk inactive)
        var sources = FindObjectsOfType<AudioSource>(true);

        foreach (var src in sources)
        {
            if (src == null) continue;

            cachedSources.Add(src);

            src.Stop();        // stop kalau sempat bunyi
            src.enabled = false; // DISABLE total (ini kunci utamanya)
        }
    }

    void Start()
    {
        Invoke(nameof(UnlockAudio), 1.5f); // kasih waktu scene settle
    }

    void UnlockAudio()
    {
        foreach (var src in cachedSources)
        {
            if (src != null)
                src.enabled = true;
        }

        Debug.Log("Audio Unlocked");
    }
}