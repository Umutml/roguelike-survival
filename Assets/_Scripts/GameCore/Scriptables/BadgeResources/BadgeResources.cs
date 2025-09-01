using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine.AddressableAssets;
using Utilities;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "BadgeResources", menuName = "ScriptableObjects/BadgeResources", order = 1)]
    public class BadgeResources : ScriptableObject
    {
        [SerializeField] private List<Badge> badges = new();
        
        public List<Badge> Badges => badges;
        public Badge GetBadge(string badgeName) => badges.FirstOrDefault(badge => badge.BadgeName.Equals(badgeName));
    }


    [Serializable]
    public struct Badge
    {
        [SerializeField] private string badgeName;
        [SerializeField] private PopupConstants.PopupType popupType;
        [SerializeField] private AssetReference badgeArt;

        public string BadgeName => badgeName;
        public PopupConstants.PopupType PopupType => popupType;
        public async UniTask<Sprite> BadgeArt() => await AssetManager<Sprite>.LoadObject(badgeArt); 
    }
}

