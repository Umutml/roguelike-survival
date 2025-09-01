using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "NpcDialogStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Npc Dialog Step",
        order = 0)]
    public class NpcDialogStep : TutorialStep
    {
        [SerializeField] private List<NpcDialogData> dialogDatas;

        private PopupManager _popupManager;

        public override UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (!_popupManager.IsPopupActive(PopupConstants.PopupType.NpcDialog))
            {
                Debug.LogError("NpcDialogPopup is not active");
                return UniTask.CompletedTask;
            }

            var npcDialogPopup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.NpcDialog);

            if (npcDialogPopup == null)
            {
                Debug.LogError("NpcDialogPopup is null");
                return UniTask.CompletedTask;
            }

            npcDialogPopup.Initialize(dialogDatas);
            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public struct NpcDialogData
    {
        public string title;
        public string description;
        public ConversationType conversationType;
    }

    public enum ConversationType
    {
        VoiceCall,
        InPerson,
        Hattori,
        Soldier,
    }
}