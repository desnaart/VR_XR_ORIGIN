using UnityEngine;
using System.Collections;

public class ExitPraktikumTrigger : MonoBehaviour
{
    [Header("Pop Up Selesai")]
    public GameObject popupSelesai;

    [Header("Delay Muncul")]
    public float delayMuncul = 3f;

    [Header("Durasi Popup Tampil")]
    public float durasiTampil = 10f;

    [Header("Sound")]
    public AudioSource soundPopup;

    private bool sedangBerjalan = false;

    void Start()
    {
        if (popupSelesai != null)
            popupSelesai.SetActive(false);
    }

    public void SelectedExit()
    {
        if (sedangBerjalan) return;

        StartCoroutine(MunculkanLaluHilangkanPopup());
    }

    IEnumerator MunculkanLaluHilangkanPopup()
    {
        sedangBerjalan = true;

        // Tunggu 3 detik dulu
        yield return new WaitForSeconds(delayMuncul);

        // Munculkan popup
        if (popupSelesai != null)
            popupSelesai.SetActive(true);

        // Mainkan sound
        if (soundPopup != null)
            soundPopup.Play();

        // Popup tampil selama 10 detik
        yield return new WaitForSeconds(durasiTampil);

        // Hilangkan popup lagi
        if (popupSelesai != null)
            popupSelesai.SetActive(false);

        sedangBerjalan = false;
    }
}