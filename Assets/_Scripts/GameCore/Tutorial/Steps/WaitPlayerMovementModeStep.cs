using Cysharp.Threading.Tasks;
using GameCore.Player;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitPlayerMovementModeStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Wait Player Movement Mode Step")]
    public class WaitPlayerMovementModeStep : TutorialStep
    {
        [SerializeField] private PlayerMovementMode targetPlayerMovementMode;

        private PlayerController _playerController;

        public override async UniTask ProcessStep()
        {
            _playerController = Resolver.Resolve<PlayerController>();

            await UniTask.WaitUntil(() => _playerController.PlayerMovementMode == targetPlayerMovementMode);
        }
    }
}