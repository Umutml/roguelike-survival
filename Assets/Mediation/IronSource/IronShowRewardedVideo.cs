using System;
using _Scripts.Utilities;
using Interfaces;
using UnityEngine;
using VContainer;

namespace Mediation.IronSourceManagers
{
    public class IronShowRewardedVideo : MonoBehaviour
    {
        public static String REWARDED_INSTANCE_ID = "0";
        
        [Tooltip("Enable-Disable all debug logs")]
        [SerializeField] private bool EnableLogging = true;
        private IGameService _gameService;
        private IAudioService _audioService;

        // Use this for initialization
        void Start ()
        {	
            // Add Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
        }
        
        [Inject]
        private void Initialize(IGameService gameService, IAudioService audioService)
        {
            _gameService = gameService;
            _audioService = audioService;
        }
        
        /************* RewardedVideo API *************/ 
        public void ShowRewardedAd (string placementid)
        {
            Log("unity-script: ShowRewardedAD called");
            if (IronSource.Agent.isRewardedVideoAvailable ()) 
            {
                IronSource.Agent.showRewardedVideo (placementid);
            } else {
                LogError("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
            }
        }
	
        void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            Log("RewardedVideoAdOpenedEvent " + DateTime.Now.ToString("HH:mm:ss"));
            _audioService.MuteAllSounds();
            _gameService.PauseGame();
        }

        void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            Log("RewardedVideoAdClosedEvent " + DateTime.Now.ToString("HH:mm:ss"));
            _audioService.UnmuteAllSounds();
            _gameService.ResumeGame();
        }

        void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
            Log("unity-script: I got RewardedVideoOnAdAvailable With AdInfo " + adInfo);
        }

        void RewardedVideoOnAdUnavailable()
        {
            Log("unity-script: I got RewardedVideoOnAdUnavailable");
        }

        void RewardedVideoOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            Log("unity-script: I got RewardedVideoAdOpenedEvent With Error" + ironSourceError + "And AdInfo " + adInfo);
        }

        void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
        {
            if (ironSourcePlacement.getPlacementName().Equals("PLACEMENT_NAME"))
            {
                // Example method
                // Give reward to user
                Log("unity-script: I got RewardedVideoOnAdRewardedEvent With Placement CoinPlacementId " + ironSourcePlacement + "And AdInfo " + adInfo);
            }
        }

        void RewardedVideoOnAdClickedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
        {
            Log("unity-script: I got RewardedVideoOnAdClickedEvent With Placement" + ironSourcePlacement + "And AdInfo " + adInfo);
        }

        private void Log(string message)
        {
            if (EnableLogging)
            {
                LoggerNS.Log(message);
            }
        }

        private void LogError(string message)
        {
            if (EnableLogging)
            {
                LoggerNS.LogError(message);
            }
        }
    }
}