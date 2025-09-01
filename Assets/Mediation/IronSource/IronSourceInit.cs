using _Scripts.Utilities;
using com.unity3d.mediation;
using UnityEngine;

namespace Mediation.IronSourceManagers
{
    public class IronSourceInit : MonoBehaviour
    {
        public static string uniqueUserId = "demoUser";
        
        [Tooltip("Enable-Disable all debug logs")]
        [SerializeField] private bool loggingEnabled = true;

#if UNITY_ANDROID
        private readonly string appKey = "208aebccd";
#elif UNITY_IPHONE
    private readonly string appKey = "208aebccd";
#else
    private readonly string appKey = "unexpected_platform";
    private readonly string bannerAdUnitId = "unexpected_platform"; 
    private readonly string interstitialAdUnitId = "unexpected_platform"; 
#endif

        private void Awake()
        {
            uniqueUserId = System.Guid.NewGuid().ToString();

            // Dynamic config example
            IronSourceConfig.Instance.setClientSideCallbacks(true);

            var id = IronSource.Agent.getAdvertiserId();
            Log("unity-script: IronSource.Agent.getAdvertiserId : " + id);

            Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();

            // SDK init
            Log("unity-script: LevelPlay Init");
            LevelPlay.Init(appKey, uniqueUserId, new[] { LevelPlayAdFormat.REWARDED });

            LevelPlay.OnInitSuccess += OnInitializationCompleted;
            LevelPlay.OnInitFailed += error => Log("Initialization error: " + error);
        }

        private void OnInitializationCompleted(LevelPlayConfiguration configuration)
        {
            Log("Initialization completed");
        }

        private void Log(string message)
        {
            if (loggingEnabled)
            {
                LoggerNS.Log(message);
            }
        }
        
        // It is recommended to pass the state of the application by executing the following event function during the Application Lifecycle.
        void OnApplicationPause(bool isPaused) 
        { 	 
            IronSource.Agent.onApplicationPause(isPaused);	 
        }
        
        private void SetIronSourceConsent(bool consent)
        {
            IronSource.Agent.setConsent(consent);
        }
    }
}