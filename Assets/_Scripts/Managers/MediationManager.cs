using Interfaces;
using Mediation.IronSourceManagers;
using UnityEngine;
using VContainer;

namespace Managers
{
    public class MediationManager : MonoBehaviour, IMediationService
    {
        [SerializeField] private IronShowRewardedVideo ironShowRewardedVideo;
        private IAnalyticsService _analyticsService;
        
        [Inject]
        private void Initialize(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }
        public void ShowRewardedAd(string placementId)
        {
            ironShowRewardedVideo.ShowRewardedAd(placementId);
            LogAdAnalytic(placementId);
        }

        private void LogAdAnalytic(string eventName)
        {
            var lowerCaseEventName = eventName.ToLower();
            _analyticsService?.LogEvent(new EventParameters<string> { EventName = lowerCaseEventName });
        }
    }
}