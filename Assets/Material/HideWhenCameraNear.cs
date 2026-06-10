using UnityEngine;

public class HideWhenCameraNear : MonoBehaviour
{
    [Header("Target VR Camera")]
    public Transform vrCamera;

    [Header("Jarak")]
    public float hideDistance = 1.5f;

    [Header("Object yang akan di-hide")]
    public GameObject objectToHide;

    private bool alreadyHidden = false;

    void Update()
    {
        if (alreadyHidden) return;
        if (vrCamera == null || objectToHide == null) return;

        float distance = Vector3.Distance(vrCamera.position, transform.position);

        if (distance <= hideDistance)
        {
            objectToHide.SetActive(false);
            alreadyHidden = true;
        }
    }
}