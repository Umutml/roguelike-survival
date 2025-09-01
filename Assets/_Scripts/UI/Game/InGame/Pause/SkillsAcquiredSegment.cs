using System;
using GameCore.Scriptables;
using UI.Game.Architectural;

namespace _Scripts.UI.Game.InGame.Pause
{
    public class SkillsAcquiredSegment : Content
    {
        #region Private Fields

        private const string Icon = "Icon";
        private const string SegmentButton = "SegmentButton";

        #endregion

        #region Properties

        public Skill Skill { get; private set; }

        #endregion

        #region Public Methods

        public void Initialize(Skill skill, Action<SkillsAcquiredSegment> onClickSegment = null)
        {
            Skill = skill;
            SetImage(Icon, Skill.icon);
            OnClickListen(SegmentButton, () => onClickSegment?.Invoke(this));
        }

        #endregion
    }
}
