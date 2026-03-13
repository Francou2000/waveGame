using UnityEngine;

public sealed class FrameRateCapper : MonoBehaviour
{
    [SerializeField] private int targetFps = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFps;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
        }
    }
}