using UnityEngine;
using UnityEngine.XR;

public class FootstepVR : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;
    public float volume = 1f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Movement Detection")]
    public XRNode inputSource = XRNode.LeftHand;
    public float movementThreshold = 0.1f;

    [Header("Ground Detection")]
    public string requiredTag = "Metal";      // hanya bunyi jika tag Metal
    public float rayDistance = 1.5f;          // jarak pengecekan tanah
    public bool useLayerMask = false;
    public LayerMask groundLayer;             // optional

    private AudioSource audioSource;
    private Vector2 inputAxis;
    private float stepTimer = 0f;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // Cek input stick gerak
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

        bool isWalking = inputAxis.magnitude > movementThreshold;
        bool onMetal = IsStandingOnMetal();

        if (isWalking && onMetal)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval; // reset biar tidak bunyi mendadak
        }
    }

    bool IsStandingOnMetal()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;

        // Raycast normal
        if (!useLayerMask)
        {
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance))
            {
                return hit.collider.CompareTag(requiredTag);
            }
        }
        else
        {
            // Raycast dengan LayerMask
            if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, groundLayer))
            {
                return hit.collider.CompareTag(requiredTag);
            }
        }

        return false;
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = volume;

        audioSource.PlayOneShot(clip);
    }
}