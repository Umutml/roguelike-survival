using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New WaitForUserInput")]
    public class WaitForUserInput : TutorialStep
    {
        private PlayerController _playerController;

        public override async UniTask ProcessStep()
        {
            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
            
            void OnMovementInputAcquired()
            {
                Unpause();
                completionSource.TrySetResult();
                _playerController.PlayerMovementController.MovementInputAcquired -= OnMovementInputAcquired;
            }
            
            await base.ProcessStep();
            
            _playerController = Resolver.Resolve<PlayerController>();
            _playerController.PlayerMovementController.MovementInputAcquired += OnMovementInputAcquired;

            await completionSource.Task;
        }
        
    }
}
