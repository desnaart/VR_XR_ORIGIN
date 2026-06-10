using UnityEngine;
using System.Collections;

public class MicPermissionRequester : MonoBehaviour
{
    private IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("🔒 Memeriksa izin microphone...");

        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("📢 Meminta izin microphone...");
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            Debug.Log("✅ Izin microphone diberikan.");
        else
            Debug.LogWarning("🚫 Izin microphone ditolak!");
#endif
        yield break; // ✅ tambahkan ini untuk mengakhiri coroutine dengan benar
    }
}
