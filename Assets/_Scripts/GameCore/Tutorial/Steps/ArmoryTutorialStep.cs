using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ArmoryTutorialStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Armory Tutorial Step",
        order = 0)]
    public class ArmoryTutorialStep : TutorialStep
    {
        [SerializeField] private string weaponName;
        private PopupManager _popupManager;

        public override async UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (!_popupManager.IsPopupActive(PopupConstants.PopupType.Armory))
            {
                LoggerNS.LogError("ArmoryPopup is not active");
                return;
            }

            var armoryPopup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.Armory);

            if (armoryPopup == null)
            {
                LoggerNS.LogError("ArmoryPopup is null");
                return;
            }

            armoryPopup.InitializeTutorial(weaponName);
        }
    }
}