using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New WaitForPlayerInArea")]
    public class WaitForPlayerInArea : TutorialStep
    {
        [SerializeField] private Bounds areaBounds;
        [SerializeField] private bool debug;
        [SerializeField] private bool isBridgeCrossedArea;
        private IAnalyticsService _analyticsService;

        private PlayerController _playerController;

        public override async UniTask ProcessStep()
        {
            _playerController = Resolver.Resolve<PlayerController>();
            _analyticsService = Resolver.Resolve<IAnalyticsService>();
            var playerTransform = _playerController.transform;

#if UNITY_EDITOR
            if (debug) EditorHelper.DrawBounds(areaBounds, 30);
#endif

            await UniTaskAsyncHelper.WaitUntil(() => areaBounds.Contains(playerTransform.position), 1000);
            if (isBridgeCrossedArea)
                _analyticsService.LogEvent(new EventParameters<string>
                    { EventName = "tt_bridge_crossed", AdjustToken = AdjustNsEventTokens.TtBridgeCrossed });
        }
    }
}