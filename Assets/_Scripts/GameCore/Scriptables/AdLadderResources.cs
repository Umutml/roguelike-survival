using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace _Scripts.GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "AdLadderResources", menuName = "ScriptableObjects/adLadderResources", order = 1)]
    public class AdLadderResources : ScriptableObject
    {
        [SerializeField] private List<AdRewardData> adLadderRewardData;
        [SerializeField] private float adButtonCooldown = 15f;
        [SerializeField] private float adLadderRefreshTime = 24f; // in hours

        public List<AdRewardData> AdLadderRewardData => adLadderRewardData;
        public float AdButtonCooldown => adButtonCooldown;
        public float AdLadderRefreshTime => adLadderRefreshTime;
    }

    [Serializable]
    public struct AdRewardData
    {
        [SerializeField] private string rewardName;
        [SerializeField] private int rewardCount;
        [SerializeField] private AssetReference rewardSprite;
        [SerializeField] private RewardType rewardType;
        

        public string RewardName => rewardName;
        public int RewardCount => rewardCount;
        public RewardType RewardType => rewardType;
        public async UniTask<Sprite> RewardSprite() => await AssetManager<Sprite>.LoadObject(rewardSprite);
    }
    
    public enum RewardType
    {
        Coin,
        Gem,
        Energy,
    }
}

[Serializable]
public class AdLadderClaimStatus
{
    public List<bool> ClaimedStatus = new List<bool>();
}