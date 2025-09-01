using UnityEngine;

namespace Managers
{
    public class DeviceManager : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_ANDROID
            Application.targetFrameRate = 30; // 30 FPS for Android platforms
#endif
            Application.targetFrameRate = 60; // 60 FPS for all other platforms
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_STANDALONE_WIN
            Screen.SetResolution(600, 1000,FullScreenMode.Windowed);
#endif
        }
    }
}
