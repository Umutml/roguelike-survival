using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "UnlockObjectStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Unlock Object Step",
        order = 0)]
    public class UnlockObjectStep : TutorialStep
    {
        [SerializeField] private UnlockObjectType unlockObjectType;

        private PopupManager _popupManager;

        public override UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (!_popupManager.IsPopupActive(PopupConstants.PopupType.Unlock))
            {
                Debug.LogError("UnlockObject is not active");
                return UniTask.CompletedTask;
            }

            var unlockObject = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.Unlock);

            if (unlockObject == null)
            {
                Debug.LogError("UnlockObject is null");
                return UniTask.CompletedTask;
            }

            unlockObject.Initialize(unlockObjectType);
            return UniTask.CompletedTask;
        }
    }
}