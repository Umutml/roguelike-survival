using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ObjectFoundStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Object Found Step",
        order = 0)]
    public class ObjectFoundStep : TutorialStep
    {
        [SerializeField] private FoundedObjectType foundedObjectType;

        private PopupManager _popupManager;

        public override UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (!_popupManager.IsPopupActive(PopupConstants.PopupType.FoundedObject))
            {
                Debug.LogError("FoundedObject is not active");
                return UniTask.CompletedTask;
            }

            var objectFounded = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.FoundedObject);

            if (objectFounded == null)
            {
                Debug.LogError("FoundedObject is null");
                return UniTask.CompletedTask;
            }

            objectFounded.Initialize(foundedObjectType);
            return UniTask.CompletedTask;
        }
    }
}