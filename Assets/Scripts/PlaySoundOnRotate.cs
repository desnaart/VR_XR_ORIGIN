using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlaySoundOnRotate : MonoBehaviour
{
    [Header("Angle Settings")]
    public float startAngle = 90f;     // mulai bunyi
    public float maxAngle = 150f;      // volume maksimal
    public float resetAngle = 10f;     // stop saat balik

    [Header("Audio")]
    public AudioSource sharedAudioSource;
    public bool useVolumeByAngle = true;

    private XRGrabInteractable grabInteractable;

    private bool isGrabbed = false;
    private Quaternion initialRotation;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        initialRotation = transform.rotation;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // optional: stop saat dilepas
        if (sharedAudioSource.isPlaying)
            sharedAudioSource.Stop();
    }

    void Update()
    {
        if (!isGrabbed || sharedAudioSource == null) return;

        float angle = Quaternion.Angle(initialRotation, transform.rotation);

        // ▶️ Mulai loop saat melewati startAngle
        if (angle >= startAngle)
        {
            if (!sharedAudioSource.isPlaying)
            {
                sharedAudioSource.loop = true;
                sharedAudioSource.Play();
            }

            // 🔊 Volume mengikuti sudut
            if (useVolumeByAngle)
            {
                float t = Mathf.InverseLerp(startAngle, maxAngle, angle);
                sharedAudioSource.volume = Mathf.Clamp01(t);
            }
        }

        // ⛔ Stop saat kembali ke awal
        if (angle <= resetAngle)
        {
            if (sharedAudioSource.isPlaying)
            {
                sharedAudioSource.Stop();
            }
        }
    }
}