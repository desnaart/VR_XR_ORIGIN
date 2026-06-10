using UnityEngine;
using UnityEngine.UI; // <-- PENTING: Tambahkan baris ini!

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform crosshairRect;
    private Image crosshairImage; // Komponen untuk menampilkan/menyembunyikan gambar

    void Awake()
    {
        crosshairRect = GetComponent<RectTransform>();
        crosshairImage = GetComponent<Image>(); // Dapatkan komponen Image

        // Pastikan crosshair disembunyikan saat game pertama kali dimulai
        if (crosshairImage != null)
        {
            crosshairImage.enabled = false;
        }
    }

    void Update()
    {
        // Jika karena suatu alasan komponen Image tidak ada, hentikan fungsi
        if (crosshairImage == null) return;

        // Cek jika tombol kiri mouse sedang DITAHAN (mode membidik/melihat)
        if (Input.GetMouseButton(0))
        {
            // TAMPILKAN crosshair
            crosshairImage.enabled = true;

            // Kunci posisinya di tengah layar
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            crosshairRect.position = screenCenter;
        }
        else
        {
            // JIKA TIDAK, SEMBUNYIKAN crosshair
            crosshairImage.enabled = false;
        }
    }
}