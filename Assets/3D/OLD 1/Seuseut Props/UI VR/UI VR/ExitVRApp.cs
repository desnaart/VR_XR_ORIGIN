using UnityEngine;

public class ExitVRApp : MonoBehaviour
{
    // Fungsi ini dipanggil oleh tombol UI
    public void ExitApplication()
    {
#if UNITY_EDITOR
        // Kalau masih di Editor Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Untuk build VR / Android / Windows
        Application.Quit();
#endif
    }
}