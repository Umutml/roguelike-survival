using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleCarExitButtonStatus",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Car Exit Button Status",
        order = 0)]
    public class ToggleCarExitButtonStatus : TutorialStep
    {
        [SerializeField] private bool isEnable;

        private PlayerCarController _playerCarController;

        public override async UniTask ProcessStep()
        {
            _playerCarController = Resolver.Resolve<PlayerCarController>();

            _playerCarController.InvokeCarExitButtonActivity(isEnable);
        }
    }
}