using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPouringController : MonoBehaviour
{
    public ParticleSystem waterParticle;

    public float startAngle = 50f;
    public float maxAngle = 120f;

    private XRGrabInteractable grab;
    private bool isGrabbed = false;

    private Quaternion initialRotation;
    private ParticleSystem.EmissionModule emission;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        emission = waterParticle.emission;
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        initialRotation = transform.rotation;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        waterParticle.Stop();
        SetFlow(0);
    }

    void Update()
    {
        if (!isGrabbed) return;

        float angle = Quaternion.Angle(initialRotation, transform.rotation);

        if (angle > startAngle)
        {
            if (!waterParticle.isPlaying)
                waterParticle.Play();

            float t = Mathf.InverseLerp(startAngle, maxAngle, angle);
            SetFlow(t);
        }
        else
        {
            waterParticle.Stop();
            SetFlow(0);
        }
    }

    void SetFlow(float t)
    {
        emission.rateOverTime = Mathf.Lerp(0, 200f, t);
    }
}