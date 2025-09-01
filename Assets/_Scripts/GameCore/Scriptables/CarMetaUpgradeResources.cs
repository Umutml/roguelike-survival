using UnityEngine;
using System;
using System.Collections.Generic;
using MyBox;


namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CarMetaUpgradeResources", menuName = "Scriptables/CarMetaUpgradeResources", order = 1)]
    public class CarMetaUpgradeResources : ScriptableObject
    {
        #region Serializable Fields

        [SerializeField] private List<CarMetaUpgrade> carMetaUpgradeList = new();
        [SerializeField] private List<double> priceMultiplier = new();
        [SerializeField] private List<UpgradeIconData> upgradeIconDataList = new();

        #endregion


        #region Properties

        public List<CarMetaUpgrade> CarMetaUpgradeList => carMetaUpgradeList;

        public UpgradeIconData GetUpgradeIconData(StatUpgradeType statUpgradeType) =>
            upgradeIconDataList.Find(data => data.StatUpgradeType.Equals(statUpgradeType));

        #endregion


#if UNITY_EDITOR

        #region CarUpgradeEditor

        private static readonly float[] IncrementValues = {10f, 9f, 8f, 7f, 6f, 5f, 4f, 3f, 2f, 1f};
        private static readonly int[] IncrementRanges = {0, 14, 28, 42, 56, 70, 98, 140, 196, 266};
        private static readonly int[] PriceRanges = {1, 4, 11, 21, 35, 54, 79, 111, 151, 200, 259};

        private static readonly StatUpgradeType[] StatUpgradeTypes =
        {
            StatUpgradeType.CarMaxDurability,
            StatUpgradeType.CarSpeed,
            StatUpgradeType.CarShield,
            StatUpgradeType.CarCriticalHitChance,
            StatUpgradeType.CarCriticalDamage,
            StatUpgradeType.CarWeaponAttackSpeed,
            StatUpgradeType.CarPickupRange,
        };

        private const int BasePrice = 100;

        [ButtonMethod]
        public void GenerateIncrementValues()
        {
            for (var i = 0; i < carMetaUpgradeList.Count; i++)
            {
                var incrementIndex = GetRangeIndex(i, IncrementRanges);
                carMetaUpgradeList[i].UpgradeDetail = new UpgradeDetail(StatUpgradeTypes[i % StatUpgradeTypes.Length],
                    1,
                    ValueModifierType.MultiplyIncrease);
            }
        }


        [ButtonMethod]
        public void GenerateUpgradePrices()
        {
            for (var i = 0; i < carMetaUpgradeList.Count; i++)
            {
                if (i == 0)
                {
                    carMetaUpgradeList[i].Price = BasePrice;
                    continue;
                }

                var multiplierIndex = GetRangeIndex(i, PriceRanges);
                carMetaUpgradeList[i].Price = CalculatePrice(i, multiplierIndex);
            }
        }


        private int GetRangeIndex(int value, int[] ranges) => Array.FindLastIndex(ranges, range => value >= range);

        private double CalculatePrice(int currentIndex, int multiplierIndex)
        {
            var previousPrice = carMetaUpgradeList[currentIndex - 1].Price;
            return Math.Floor(previousPrice * priceMultiplier[multiplierIndex] / 5) * 5;
        }

        #endregion

#endif
    }


    [Serializable]
    public class CarMetaUpgrade
    {
        [SerializeField] private UpgradeDetail upgradeDetail;
        [SerializeField] private double price;


        public UpgradeDetail UpgradeDetail
        {
            get => upgradeDetail;
            set => upgradeDetail = value;
        }

        public double Price
        {
            get => price;
            set => price = value;
        }
    }


    [Serializable]
    public struct UpgradeIconData
    {
        [SerializeField] private StatUpgradeType statUpgradeType;
        [SerializeField] private Sprite icon;

        public StatUpgradeType StatUpgradeType => statUpgradeType;
        public Sprite Icon => icon;
    }
}
