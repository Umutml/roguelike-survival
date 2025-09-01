using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "ChestResources", menuName = "ScriptableObjects/ChestResources", order = 1)]
    public class ChestResources : ScriptableObject
    {
        [SerializeField] private List<Chest> chests = new();
    }

    [Serializable]
    public struct Chest
    {
        [SerializeField] private string chestName;
        [SerializeField] private ChestType chestType;
        [SerializeField] private AssetReference chestImage;
        [SerializeField] private int rewardAmount;
        [SerializeField] private PurchaseOptions purchaseOptions;
    }


    public enum ChestType
    {
        Regular,
        Big,
        Mega
    }
}

