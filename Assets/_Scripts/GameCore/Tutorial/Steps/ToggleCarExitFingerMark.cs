using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleCarExitFingerMark",
  menuName = "ScriptableObjects/Tutorial/Steps/Toggle Car Exit Finger Mark",
  order = 0)]
    public class ToggleCarExitFingerMark : TutorialStep
    {
        [SerializeField] private bool isEnable;
        private PlayerCarController _playerCarController;
        private PlayerController _playerController;

        public override UniTask ProcessStep()
        {
            _playerController = Resolver.Resolve<PlayerController>();
            _playerCarController = Resolver.Resolve<PlayerCarController>();
            _playerCarController.InvokeCarExitButtonFingerMarkActivity(isEnable);
            return UniTask.CompletedTask;
        }
    }
}

