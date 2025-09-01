using System;
using System.Collections.Generic;
using GameCore.Scriptables;
using MyBox;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CharacterUpgradeResources",
        menuName = "Scriptables/CharacterUpgradeResources",
        order = 1)]
    public class CharacterUpgradeResources : ScriptableObject
    {
        #region Serializable Fields

        [SerializeField] private List<CharacterUpgrade> characterUpgradeList = new();

        #endregion

        #region Properties

        public List<CharacterUpgrade> CharacterUpgradeList => characterUpgradeList;

        #endregion


#if UNITY_EDITOR

        private static readonly StatUpgradeType[] StatUpgradeTypes =
        {
            StatUpgradeType.MaxHealth,
            StatUpgradeType.Speed,
            StatUpgradeType.Armor,
            StatUpgradeType.CriticalHitChance,
            StatUpgradeType.CriticalDamage,
            StatUpgradeType.AttackSpeed,
            StatUpgradeType.PickupRange,
        };


        private int _currentUpgradeIndex;


        [ButtonMethod]
        public void GeneraterCharacterUpgrade()
        {
            _currentUpgradeIndex = 0;

            for (var i = 0; i < characterUpgradeList.Count; i++)
            {
                for (var k = 0; k < characterUpgradeList[i].UpgradeDetails.Count; k++)
                {
                    characterUpgradeList[i].UpgradeDetails[k] = new UpgradeDetail(
                        StatUpgradeTypes[_currentUpgradeIndex],
                        10f,
                        ValueModifierType.MultiplyIncrease);

                    _currentUpgradeIndex++;

                    if (_currentUpgradeIndex >= StatUpgradeTypes.Length) _currentUpgradeIndex = 0;
                }
            }
        }

#endif
    }
}


[Serializable]
public struct CharacterUpgrade
{
    [SerializeField] private List<UpgradeDetail> upgradeDetails;
    [SerializeField] private float price;


    public List<UpgradeDetail> UpgradeDetails
    {
        get => upgradeDetails;
        set => upgradeDetails = value;
    }


    public float Price
    {
        get => price;
        set => price = value;
    }
}
