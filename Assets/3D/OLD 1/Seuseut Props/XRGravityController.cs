using UnityEngine;
using UnityEngine.XR;

public class XRGravityController : MonoBehaviour
{
    [Header("References")]
    public Transform xrCamera;     // camera offset / MainCamera XR
    public CharacterController controller;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private float verticalVelocity = 0f;
    private bool isGrounded = false;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (xrCamera == null)
            xrCamera = Camera.main.transform;

        // tinggi awal diset sesuai tinggi kepala
        UpdateCharacterHeight();
    }

    void Update()
    {
        GroundCheck();
        ApplyGravity();
        UpdateCharacterHeight();
    }

    // ============================================
    // CEK APAKAH KAKI MENYENTUH LANTAI
    // ============================================
    void GroundCheck()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        isGrounded = Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer
        );

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;
    }

    // ============================================
    // GRAVITAS
    // ============================================
    void ApplyGravity()
    {
        if (!isGrounded)
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = new Vector3(0, verticalVelocity, 0);

        controller.Move(move * Time.deltaTime);
    }

    // ============================================
    // UPDATE TINGGI KARAKTER SESUAI KEPALA
    // ============================================
    void UpdateCharacterHeight()
    {
        float headHeight = Mathf.Clamp(xrCamera.localPosition.y, 1f, 2f);
        controller.height = headHeight;

        // offset center mengikuti kepala
        Vector3 center = Vector3.zero;
        center.y = controller.height / 2f;
        controller.center = center;
    }
}