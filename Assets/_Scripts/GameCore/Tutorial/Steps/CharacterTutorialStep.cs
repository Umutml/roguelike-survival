using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "CharacterTutorialStep",
    menuName = "ScriptableObjects/Tutorial/Steps/Character Tutorial Step",
    order = 0)]
    public class CharacterTutorialStep : TutorialStep
    {
        [SerializeField] private CharacterResources characterResources;
        private PopupManager _popupManager;
        public override UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();
            var popup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.CharacterUpgrade);
            popup.InitializeTutorial(characterResources);
            return base.ProcessStep();
        }
    }
}

