using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Scriptables;
using Michsky.UI.ModernUIPack;
using UI.Game.Architectural;
using UI.Game.InGame.LevelUp.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Game.InGame.LevelUp
{
    public class InGameLevelUpSkillArea : Content
    {
        #region Serialized Fields

        [SerializeField] private List<Image> stars;
        [SerializeField] private Sprite yellowStarSprite;
        [SerializeField] private Sprite grayStarSprite;
        [SerializeField] private Sprite carSkill;
        [SerializeField] private Sprite playerSkill;
        [SerializeField] private Sprite weaponSkill;

        #endregion

        #region Private Fields

        private UIGradient _uiGradient;
        private Button _button;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _button = GetComponent<Button>();
            _uiGradient = GetGameObject(InGameLevelUpPanelConstants.BACKGROUND).GetComponent<UIGradient>();
        }

        #endregion

        #region Public Methods

        public void Initialize((Skill skill, int starLevel) details, SkillColorData skillColorData ,Action onClick)
        {
            var starUpgrade = details.skill.starUpgrades.FirstOrDefault(x => x.starLevel == details.starLevel);
            _uiGradient.EffectGradient = skillColorData.gradient;
            SetText(InGameLevelUpPanelConstants.SKILL_NAME_TEXT, details.skill.name);
            SetText(InGameLevelUpPanelConstants.SKILL_INFO_TEXT, starUpgrade.description);
            SetImage(InGameLevelUpPanelConstants.SKILL_IMAGE, details.skill.icon);
            SetImage(InGameLevelUpPanelConstants.SKILL_TYPE, GetSkillIcon(details.skill.upgradeType));
            OnButtonClickListen(onClick);
            SetStars(starUpgrade);
        }

        #endregion

        #region Private Methods

        private Sprite GetSkillIcon(UpgradeType contentType)
        {
            return contentType switch
            {
                UpgradeType.Car => carSkill,
                UpgradeType.Character => playerSkill,
                UpgradeType.Weapon => weaponSkill,
                _ => null
            };
        }

        private void OnButtonClickListen(Action onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick());
        }

        private void SetStars(StarUpgrade starUpgrade)
        {
            var currentStarLevel = Math.Max(starUpgrade.starLevel - 1, 0);
            for (var i = 0; i < stars.Count; i++)
            {
                stars[i].sprite = i + 1 <= currentStarLevel ? yellowStarSprite : grayStarSprite;
            }

            // current star level animasyon oynayacak.
        }

        #endregion
    }
}