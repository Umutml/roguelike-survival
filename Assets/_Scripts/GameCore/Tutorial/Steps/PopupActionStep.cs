using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "PopupActionStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Popup Action Step",
        order = 0)]
    public class PopupActionStep : TutorialStep
    {
        [SerializeField] private PopupConstants.PopupType popupType;
        [SerializeField] private bool isOpen;

        private PopupManager _popupManager;

        public override async UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();


            if (isOpen)
            {
                await _popupManager.OpenPopup(popupType);
            }
            else
            {
                if (!_popupManager.IsPopupActive(popupType))
                {
                    return;
                }

                _popupManager.ClosePopup(popupType);
            }
        }
    }
}