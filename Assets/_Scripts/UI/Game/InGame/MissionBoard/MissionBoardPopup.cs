using _Scripts.Utilities;
using GameCore.PopupSystem;
using GameCore.Tutorial.Steps;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Game.InGame.MissionBoard
{
    public class MissionBoardPopup : Popup
    {
        [SerializeField] private TMP_Text descriptionText;

        public override void OnOpenPopup()
        {
        }

        public override void Initialize(object data)
        {
            if (data is not MissionBoardData missionBoardData)
            {
                LoggerNS.LogError("MissionBoardData is null");
                return;
            }

            descriptionText.text = missionBoardData.description;
        }
    }
}