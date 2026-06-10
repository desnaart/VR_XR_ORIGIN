using UnityEngine;

public class SplashScreenManager : MonoBehaviour
{
    [Header("UI Intro / Splash")]
    public GameObject splashUI;

    [Header("Movement yang dikunci")]
    public Behaviour moveProvider;
    public Behaviour turnProvider;
    public Behaviour teleportProvider;

    void Start()
    {
        LockPlayer();
    }

    public void LockPlayer()
    {
        if (splashUI != null)
            splashUI.SetActive(true);

        if (moveProvider != null)
            moveProvider.enabled = false;

        if (turnProvider != null)
            turnProvider.enabled = false;

        if (teleportProvider != null)
            teleportProvider.enabled = false;
    }

    public void FinishIntro()
    {
        if (splashUI != null)
            splashUI.SetActive(false);

        if (moveProvider != null)
            moveProvider.enabled = true;

        if (turnProvider != null)
            turnProvider.enabled = true;

        if (teleportProvider != null)
            teleportProvider.enabled = true;
    }
}