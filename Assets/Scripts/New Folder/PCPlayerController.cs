using UnityEngine;

public class PCPlayerController : MonoBehaviour
{
    [Header("Referensi")]
    public Camera playerCamera;
    public Transform handPoint;
    public GameObject crosshairUI;

    [Header("Pengaturan Gerakan")]
    public float walkSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;

    [Header("Pengaturan Interaksi")]
    public float interactionDistance = 3.0f;

    private CharacterController characterController;
    private float verticalLookRotation = 0f;
    private GameObject heldObject = null;
    private Rigidbody heldObjectRb;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshairUI)
            crosshairUI.SetActive(true);
    }

    void OnDisable()
    {
        if (crosshairUI)
            crosshairUI.SetActive(false);
    }

    void Update()
    {
        if (!enabled) return;
        HandleLook();
        HandleMovement();
        HandleInteraction();
    }

    void HandleLook()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);
            verticalLookRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            playerCamera.transform.localEulerAngles = Vector3.right * verticalLookRotation;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        float forwardSpeed = Input.GetAxis("Vertical") * walkSpeed;
        float sideSpeed = Input.GetAxis("Horizontal") * walkSpeed;
        Vector3 speed = new Vector3(sideSpeed, 0, forwardSpeed);
        speed = transform.rotation * speed;
        characterController.SimpleMove(speed);
    }

void HandleInteraction()
    {
        // Cek jika tombol 'Q' ditekan
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Tulis pesan ke Console untuk memastikan input bekerja
            Debug.Log("'Q' key pressed. Attempting interaction...");

            if (heldObject == null)
            {
                RaycastHit hit;
                // Tembakkan laser (raycast) dari kamera
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionDistance))
                {
                    // Jika mengenai sesuatu, tulis namanya di Console
                    Debug.Log("Raycast HIT: " + hit.collider.name);

                    // Cek apakah objek yang terkena memiliki Mesh Collider
                    if (hit.collider.GetComponent<MeshCollider>() != null)
                    {
                        // Jika ya, kondisi terpenuhi!
                        Debug.Log("SUCCESS: Object has a Mesh Collider. Grabbing now!");
                        GrabObject(hit.collider.gameObject);
                    }
                    else
                    {
                        // Jika tidak, ini adalah masalahnya
                        Debug.LogError("GRAB FAILED: The object '" + hit.collider.name + "' does NOT have a Mesh Collider component.");
                    }
                }
                else
                {
                    // Jika laser tidak mengenai apa pun
                    Debug.LogWarning("Raycast did not hit any object within range.");
                }
            }
            else
            {
                ReleaseObject();
            }
        }
    }

    // --- FUNGSI INI DIUPDATE DENGAN PENGECEKAN NULL ---
    void GrabObject(GameObject objToGrab)
    {
        heldObject = objToGrab;
        // Coba dapatkan Rigidbody, tapi tidak apa-apa jika tidak ada
        heldObjectRb = heldObject.GetComponent<Rigidbody>();

        // Hanya matikan fisika JIKA ada Rigidbody
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
        }

        heldObject.transform.SetParent(handPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // --- FUNGSI INI DIUPDATE DENGAN PENGECEKAN NULL ---
    void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        // Hanya aktifkan fisika dan beri dorongan JIKA ada Rigidbody
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.AddForce(playerCamera.transform.forward * 2f, ForceMode.VelocityChange);
        }
        
        heldObject = null;
        heldObjectRb = null;
    }
}