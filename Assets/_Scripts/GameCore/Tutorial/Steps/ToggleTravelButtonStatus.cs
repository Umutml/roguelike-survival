using Cysharp.Threading.Tasks;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleTravelButtonStatus",
      menuName = "ScriptableObjects/Tutorial/Steps/Toggle Travel Button Status",
      order = 0)]
    public class ToggleTravelButtonStatus : TutorialStep
    {
        [SerializeField] private bool isEnable;

        private PlayerController _playerController;

        public override async UniTask ProcessStep()
        {
            _playerController = Resolver.Resolve<PlayerController>();

            _playerController.InvokeTravelButtonStatusChanged(isEnable);
        }
    }

}
