using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpoonPourTrigger : MonoBehaviour
{
    public GameObject isiSendok;
    public GameObject isiGelas;

    public XRGrabInteractable grab;

    public float pourAngle = 50f;

    bool nearGlass = false;
    bool poured = false;

    void Update()
    {
        if (!grab.isSelected || !nearGlass || poured) return;

        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > pourAngle)
        {
            Pour();
        }
    }

    void Pour()
    {
        isiSendok.SetActive(false);
        isiGelas.SetActive(true);

        poured = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Glass"))
            nearGlass = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Glass"))
            nearGlass = false;
    }
}