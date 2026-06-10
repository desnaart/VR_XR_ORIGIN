using UnityEngine;

public class AudioLooperPersistent : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource tidak ditemukan di GameObject ini!");
            return;
        }

        // Pastikan looping ON
        audioSource.loop = true;

        // Jangan Play On Awake, biar kita kontrol manual
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        // Saat GameObject diaktifkan kembali, audio akan jalan lagi
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        // Saat GameObject dinonaktifkan, audio dihentikan
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}