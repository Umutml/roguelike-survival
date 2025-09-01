using _Scripts.Utilities;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.Spawner;
using UnityEngine;
using VContainer;
using _Utilities;
using Interfaces;


namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitForMobsDead",
        menuName = "ScriptableObjects/Tutorial/Steps/Wait For Mobs Dead",
        order = 0)]
    public class WaitForMobsDead : TutorialStep
    {
        private MobManager _mobManager;
        private IAnalyticsService _analyticsService;

        public override async UniTask ProcessStep()
        {
            _mobManager = Resolver.Resolve<MobManager>();
            _analyticsService = Resolver.Resolve<IAnalyticsService>();

            if (_mobManager.ActiveTutorialMobs is not { Count: > 0 })
            {
                LoggerNS.LogError("No mobs to wait for");
                return;
            }

            await UniTaskAsyncHelper.WaitWhile(() => _mobManager.ActiveTutorialMobs.Gen().Any(x => !x.IsDead), 1000);

            SendTutorialMobsDeadAnalytic();
        }

        private void SendTutorialMobsDeadAnalytic()
        {
            _analyticsService.LogEvent(new EventParameters<string>
            {
                EventName = "tt_tutorial_street_zombies_killed",
                AdjustToken = AdjustNsEventTokens.TtTutorialStreetZombiesKilled
            });
        }
    }
}