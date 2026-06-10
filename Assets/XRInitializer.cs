using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializeXR());
    }

    IEnumerator InitializeXR()
    {
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager == null) yield break;

        if (!xrManager.isInitializationComplete)
        {
            yield return xrManager.InitializeLoader();
            xrManager.StartSubsystems();
        }
    }

    void OnDisable()
    {
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager != null && xrManager.isInitializationComplete)
        {
            xrManager.StopSubsystems();
            xrManager.DeinitializeLoader();
        }
    }
}