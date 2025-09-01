using UnityEngine;

namespace UI.Game.InGame.CarUpgrade.Constants
{
    public struct CarUpgradeConstants
    {
        public const string LEVEL = "Level";
        public const string LEVEL_TEXT = "LevelText";
        public const string UPGRADE_IMAGE = "UpgradeImage";
        public const string TITLE_TEXT = "TitleText";
        public const string INCREMENT_TEXT = "IncrementText";
        public const string PRICE_TEXT = "PriceText";
        public const string PRICE_IMAGE = "PriceImage";
        public const string TICK_ICON = "TickIcon";
        public const string LOCK = "Lock";
        public const string CONTENT = "Content";
        public const string SLIDER = "Slider";
        public const string PRICE_AREA = "PriceArea";
        public const string INFO_AREA = "InfoArea";
        public const string GLOW = "Glow";
        public const string TUTORIAL_HAND = "TutorialHand";
        public const string ACTIVE_SLIDER = "ActiveSlider";

        public static readonly Color ENABLED_SLIDER_COLOR = new (0.95f, 0.61f, 0.07f, 1f);
        public static readonly Color DISABLED_SLIDER_COLOR = new (0.18f, 0.8f, 0.44f, 1f);
        
        
        public const int INFO_AREA_Y_VALUE = -50;
    }
}
