using UnityEngine;

public class AvatarIdleMotion : MonoBehaviour
{
    [Header("Body Motion")]
    public float moveAmplitude = 0.02f;   // amplitudo naik turun badan
    public float moveSpeed = 1.0f;        // kecepatan naik turun
    public float rotateAmplitude = 5f;    // amplitudo goyang kepala
    public float rotateSpeed = 1.5f;      // kecepatan goyang kepala

    [Header("Hand Motion")]
    public Transform leftHand;
    public Transform rightHand;
    public float handAmplitude = 0.02f;   // amplitudo gerakan tangan
    public float handSpeed = 1.2f;        // kecepatan tangan

    private Vector3 bodyStartPos;
    private Quaternion bodyStartRot;
    private Vector3 leftHandStart;
    private Vector3 rightHandStart;

    void Start()
    {
        bodyStartPos = transform.localPosition;
        bodyStartRot = transform.localRotation;

        if (leftHand != null) leftHandStart = leftHand.localPosition;
        if (rightHand != null) rightHandStart = rightHand.localPosition;
    }

    void Update()
    {
        float time = Time.time;

        // 🔹 Badan naik turun (breathing motion)
        float y = Mathf.Sin(time * moveSpeed) * moveAmplitude;
        transform.localPosition = bodyStartPos + new Vector3(0, y, 0);

        // 🔹 Kepala goyang ringan
        float angle = Mathf.Sin(time * rotateSpeed) * rotateAmplitude;
        transform.localRotation = bodyStartRot * Quaternion.Euler(0, angle, 0);

        // 🔹 Gerakan tangan (halus, beda fase biar natural)
        if (leftHand != null)
        {
            float lh = Mathf.Sin(time * handSpeed) * handAmplitude;
            float lhNoise = (Mathf.PerlinNoise(time * 0.5f, 0) - 0.5f) * handAmplitude;
            leftHand.localPosition = leftHandStart + new Vector3(0, lh + lhNoise, 0);
        }

        if (rightHand != null)
        {
            float rh = Mathf.Sin(time * (handSpeed * 0.9f)) * handAmplitude;
            float rhNoise = (Mathf.PerlinNoise(time * 0.5f, 1) - 0.5f) * handAmplitude;
            rightHand.localPosition = rightHandStart + new Vector3(0, rh + rhNoise, 0);
        }
    }
}
