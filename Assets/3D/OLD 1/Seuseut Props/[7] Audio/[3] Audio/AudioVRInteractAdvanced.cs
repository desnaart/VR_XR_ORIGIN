using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AudioVRInteractAdvanced : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Play Mode")]
    public bool playWhileInteracting = false;   // Cetang ini untuk mode: Suara berbunyi selama interact
    public bool playFullOnce = true;            // Cetang ini untuk mode: Sekali interact, play sampai selesai

    private AudioSource audioSource;
    private XRBaseInteractable xrInteract;

    private bool isInteracting = false;

    private void Awake()
    {
        // Siapkan AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;

        xrInteract = GetComponent<XRBaseInteractable>();

        if (xrInteract != null)
        {
            xrInteract.selectEntered.AddListener(_ => OnInteractStart());
            xrInteract.selectExited.AddListener(_ => OnInteractEnd());
        }
    }

    // Ketika interaksi dimulai
    private void OnInteractStart()
    {
        isInteracting = true;

        if (playWhileInteracting)
        {
            // Play terus selama dipegang
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.time = 0f;
            audioSource.Play();
        }
        else if (playFullOnce)
        {
            // Play sekali penuh, walaupun dilepas
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.time = 0f;
            audioSource.Play();
        }
    }

    // Ketika interaksi dilepas
    private void OnInteractEnd()
    {
        isInteracting = false;

        if (playWhileInteracting)
        {
            // Berhenti ketika interaksi dilepas
            audioSource.Stop();
        }

        // Mode playFullOnce TIDAK dihentikan → audio tetap lanjut sampai habis
    }

    private void OnDisable()
    {
        audioSource.Stop();
    }
}