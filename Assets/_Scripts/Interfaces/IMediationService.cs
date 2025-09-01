using UnityEngine;

namespace Interfaces
{
    public interface IMediationService
    {
        public const string CoinPlacementId = "Coin_Reward";
        public const string RevivePlacementId = "Revive_Reward";
        public const string EnergyPlacementId = "Energy_Reward";
        public const string PerkRefreshPlacementId = "Perk_Refresh_Reward";
        public const string DoubledPlacementId = "Doubled_Reward";
        public const string ArmoryRocketPlacementId = "Armory_Rocket_Reward";
        public const string AdLadderPlacementId = "Ad_Ladder_Reward";
        public const string ShopItemPlacementId = "Shop_Item_Reward";
        public const string CarBuyPlacementId = "Car_Buy_Reward";
        void ShowRewardedAd(string placementId);
    }
}