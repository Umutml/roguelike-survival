using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GameCore.Spawner;
using VContainer;
using _Utilities;
using Interfaces;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitForBoxesDead",
        menuName = "ScriptableObjects/Tutorial/Steps/Wait For Boxes Dead",
        order = 0)]
    public class WaitForBoxesDead : TutorialStep
    {
        private BoxManager _boxManager;
        private ITutorialService _tutorialService;
        private IAnalyticsService _analyticsService;

        public override async UniTask ProcessStep()
        {
            _boxManager = Resolver.Resolve<BoxManager>();
            _tutorialService = Resolver.Resolve<ITutorialService>();
            _analyticsService = Resolver.Resolve<IAnalyticsService>();
            await UniTaskAsyncHelper.WaitWhile(() => _boxManager.SubscribedTutorialDamageables.Count > 0, 600);
            LogAnalytic();
            _tutorialService.CloseTutorialWall(isBase: true);
        }

        private void LogAnalytic()
        {
            _analyticsService.LogEvent(new EventParameters<string>
                { EventName = "tt_tutorial_chest_opened", AdjustToken = AdjustNsEventTokens.TtTutorialChestOpened });
        }
    }
}