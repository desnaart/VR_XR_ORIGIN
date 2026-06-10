using UnityEngine;

public class AudioVRController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Ambil atau buat AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Pastikan audio TIDAK play on awake
        audioSource.playOnAwake = false;
        audioSource.Stop();

        // Set pengaturan audio
        audioSource.loop = false;        // Play sekali
        audioSource.volume = volume;

        // Biarkan clip dari script, bukan dari AudioSource di Inspector
        audioSource.clip = null;
    }

    private void Start()
    {
        // Pastikan audioSource benar-benar berhenti saat awal Play
        audioSource.Stop();

        // Jika object aktif saat scene mulai → play sekali
        if (gameObject.activeInHierarchy && audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.time = 0f;
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        // Cegah play otomatis sebelum Start
        if (!this.enabled) return;

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.time = 0f;     // reset sebelum main
            audioSource.Play();        // play sekali
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}