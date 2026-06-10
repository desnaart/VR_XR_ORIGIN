using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AudioVRInteract : MonoBehaviour
{
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;

    public enum InteractMode { XRSelect, XRActivate, Trigger, Click }
    public InteractMode interactMode = InteractMode.XRSelect;

    private AudioSource audioSource;
    private XRBaseInteractable xrInteract;

    private void Awake()
    {
        // Siapkan AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.clip = null;

        // XR Interaction
        xrInteract = GetComponent<XRBaseInteractable>();

        if (xrInteract != null)
        {
            if (interactMode == InteractMode.XRSelect)
                xrInteract.selectEntered.AddListener(_ => PlaySound());

            if (interactMode == InteractMode.XRActivate)
                xrInteract.activated.AddListener(_ => PlaySound());
        }
    }

    // ==========
    // PLAY SOUND
    // ==========
    public void PlaySound()
    {
        if (audioClip == null) return;

        audioSource.Stop();    // reset agar selalu mulai dari awal
        audioSource.clip = audioClip;
        audioSource.time = 0f;
        audioSource.Play();    // PLAY LENGKAP SAMPAI HABIS
    }

    // MODE INTERAKSI LAIN (PC/Trigger)
    private void OnMouseDown()
    {
        if (interactMode == InteractMode.Click)
            PlaySound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (interactMode == InteractMode.Trigger && other.CompareTag("Player"))
            PlaySound();
    }

    private void OnDisable()
    {
        audioSource.Stop();
    }
}