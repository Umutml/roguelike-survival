using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using Interfaces;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.AnalyticSteps
{
    [CreateAssetMenu(fileName = "GenericAnalytic", menuName = "ScriptableObjects/Tutorial/Steps/Analytics/AnalyticGeneric", order = 0)]
    public class TutorialGenericAnalytic : TutorialStep
    {
        [SerializeField] private string eventName;
        [SerializeField] private string adjustToken;
        
        private IAnalyticsService _analyticsService;
        public override UniTask ProcessStep()
        {
            _analyticsService = Resolver.Resolve<IAnalyticsService>();
            _analyticsService.LogEvent(new EventParameters<string> { EventName = eventName, AdjustToken = adjustToken });
            return UniTask.CompletedTask;
        }
    }
}
