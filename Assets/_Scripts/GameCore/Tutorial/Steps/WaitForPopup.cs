using System;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine;
using VContainer;


namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New WaitForPopup")]
    public class WaitForPopup : TutorialStep
    {
        [SerializeField] private PopupConstants.PopupType popupType;
        [SerializeField] private bool isOpen;

        private PopupManager _popupManager;

        public override async UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (isOpen)
            {
                await UniTaskAsyncHelper.WaitUntil(() =>
                    _popupManager.IsPopupActive(popupType) && _popupManager.GetPopup<Popup>(popupType) != null, 50, true);
            }
            else
            {
                await UniTaskAsyncHelper.WaitUntil(() =>
                    !_popupManager.IsPopupActive(popupType) && _popupManager.GetPopup<Popup>(popupType) == null, 50, true);
            }
        }
    }
}