using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarStatusUI : MonoBehaviour
{
    [Header("UI References")]
    public Image recordIcon;   // ikon mic merah
    public Image speakIcon;    // ikon bicara

    // Singleton supaya bisa diakses dari script lain
    public static AvatarStatusUI Instance;

    private void Awake()
    {
        Instance = this;
        if (recordIcon != null) recordIcon.enabled = false;
        if (speakIcon != null) speakIcon.enabled = false;
    }

    public void ShowRecording(bool show)
    {
        if (recordIcon != null)
            recordIcon.enabled = show;
    }

    public void ShowSpeaking(bool show)
    {
        if (speakIcon != null)
            speakIcon.enabled = show;
    }
}
