using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR; // <-- TAMBAHKAN INI

public class ControlSchemeManager : MonoBehaviour
{
    [Header("Komponen Player")]
    public PCPlayerController pcController;
    public CharacterController characterController;

    [Header("Komponen VR")]
    public ActionBasedSnapTurnProvider snapTurnProvider;
    public ActionBasedContinuousTurnProvider continuousTurnProvider;
    public ContinuousMoveProviderBase continuousMoveProvider;
    public TrackedPoseDriver trackedPoseDriver;

    [Header("Pengaturan Kamera PC")]
    public Transform mainCameraTransform;
    public float cameraHeightForPC = 1.6f;

    // DIUBAH: Gunakan Start() bukan Awake() agar perangkat punya waktu untuk inisialisasi
    void Start()
    {
        // --- LOGIKA DETEKSI VR DIUBAH TOTAL ---
        // Kita cek apakah ada perangkat Head-Mounted Display (HMD) yang aktif dan valid
        InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        bool isVREnabled = hmd.isValid;
        // ----------------------------------------

        if (isVREnabled)
        {
            Debug.Log("VR Device DETECTED. Enabling VR Controls. 🤖");
            EnableVRControls();
        }
        else
        {
            Debug.Log("No VR Device Detected. Enabling PC Controls. ⌨️");
            EnablePCControls();
        }
    }

    void EnableVRControls()
    {
        if (pcController) pcController.enabled = false;
        if (characterController) characterController.enabled = false;

        if (snapTurnProvider) snapTurnProvider.enabled = true;
        if (continuousTurnProvider) continuousTurnProvider.enabled = true;
        if (continuousMoveProvider) continuousMoveProvider.enabled = true;
        if (trackedPoseDriver) trackedPoseDriver.enabled = true;
    }

    void EnablePCControls()
    {
        if (pcController) pcController.enabled = true;
        if (characterController) characterController.enabled = true;

        if (snapTurnProvider) snapTurnProvider.enabled = false;
        if (continuousTurnProvider) continuousTurnProvider.enabled = false;
        if (continuousMoveProvider) continuousMoveProvider.enabled = false;
        if (trackedPoseDriver) trackedPoseDriver.enabled = false;

        if (mainCameraTransform != null)
        {
            mainCameraTransform.localPosition = new Vector3(0, cameraHeightForPC, 0);
            // Penting: Reset rotasi kamera juga, kalau-kalau HMD sempat aktif
            mainCameraTransform.localRotation = Quaternion.identity;
        }
    }
}