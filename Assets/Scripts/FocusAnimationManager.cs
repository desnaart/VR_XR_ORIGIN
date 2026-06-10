using UnityEngine;
using System.Collections;

public class FocusAnimationManager : MonoBehaviour
{
    [Header("Setup Kamera & Layer")]
    public Camera vrCamera;
    public LayerMask layerSaatFokus;

    private int layerAwalKamera;
    private CameraClearFlags settingBackgroundAwal;
    private Color warnaBackgroundAwal;

    // FUNGSI BARU: Sekarang Unity akan bertanya "Animasi yang mana?"
    public void TriggerFocusAnimation(GameObject animasiYangDipilih)
    {
        StartCoroutine(FocusRoutine(animasiYangDipilih));
    }

    private IEnumerator FocusRoutine(GameObject animasi)
    {
        // 1. Simpan settingan kamera
        layerAwalKamera = vrCamera.cullingMask;
        settingBackgroundAwal = vrCamera.clearFlags;
        warnaBackgroundAwal = vrCamera.backgroundColor;

        // 2. Matikan HDRI jadi hitam
        vrCamera.clearFlags = CameraClearFlags.SolidColor;
        vrCamera.backgroundColor = Color.black;

        // 3. NYALAKAN ANIMASI YANG DIPILIH DARI INSPECTOR
        if (animasi != null)
            animasi.SetActive(true);

        // 4. Sembunyikan ruangan lab
        vrCamera.cullingMask = layerSaatFokus;

        // 5. Tunggu 10 detik
        yield return new WaitForSeconds(10f);

        // 6. Kembalikan kondisi normal
        vrCamera.cullingMask = layerAwalKamera;
        vrCamera.clearFlags = settingBackgroundAwal;
        vrCamera.backgroundColor = warnaBackgroundAwal;

        // 7. Matikan kembali animasinya
        if (animasi != null)
            animasi.SetActive(false);
    }
}