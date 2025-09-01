using System;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "MissionBoardStep",
        menuName = "ScriptableObjects/Tutorial/Steps/MissionBoardStep",
        order = 0)]
    public class MissionBoardStep : TutorialStep
    {
        [SerializeField] private MissionBoardData missionBoardData;
        private PopupManager _popupManager;

        public override UniTask ProcessStep()
        {
            _popupManager = Resolver.Resolve<PopupManager>();

            if (!_popupManager.IsPopupActive(PopupConstants.PopupType.MissionBoard))
            {
                Debug.LogError("MissionBoardPopup is not active");
                return UniTask.CompletedTask;
            }

            var missionBoardPopup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.MissionBoard);

            if (missionBoardPopup == null)
            {
                Debug.LogError("MissionBoardPopup is null");
                return UniTask.CompletedTask;
            }

            missionBoardPopup.Initialize(missionBoardData);

            return UniTask.CompletedTask;
        }
    }

    [Serializable]
    public struct MissionBoardData
    {
        public string description;
    }
}