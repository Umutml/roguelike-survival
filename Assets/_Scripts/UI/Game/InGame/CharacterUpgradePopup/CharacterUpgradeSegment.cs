using System;
using GameCore.Player;
using GameCore.Scriptables;
using UI.Game.Architectural;
using Utilities;

namespace UI.Game.InGame.CharacterUpgradePopup
{
    public class CharacterUpgradeSegment : Content
    {
        private const string UpgradedValue = "UpgradedValue";
        private const string StatIcon = "StatIcon";
        private const string TitleText = "TitleText";


        public void InitializeSegment(float defaultValue, UpgradeDetail detail)
        {
            var upgradeValue = GetUpgradeValue(defaultValue, detail);
            SetText(UpgradedValue, FormatStatWithUpgrade(defaultValue, upgradeValue));
            SetText(TitleText, detail.type.ToString().ToFormattedTitle());
        }

        private float GetUpgradeValue(float value, UpgradeDetail detail)
        {
            var upgradeValue = value;
            var upgradeId = string.Empty;
            PlayerSkillController.Calculate(ref upgradeValue, ref upgradeId, detail);
            return Math.Max(0.1f, (float)(upgradeValue - value));
        }

        private string FormatStatWithUpgrade(float currentValue, float upgradeValue)
        {
            return $"{(int)currentValue} <size=24><color=#2DDD1B>+{(upgradeValue % 1 == 0 ? upgradeValue.ToString("0") : upgradeValue.ToString("0.0"))}";
        }
    }
}