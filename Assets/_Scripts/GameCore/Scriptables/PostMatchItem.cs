using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MyBox;
using System;
using UnityEngine.Serialization;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "PostMatchItem", menuName = "ScriptableObjects/PostMatchItem")]
    public class PostMatchItem : ScriptableObject
    {
        public string itemName;
        public string itemDescription;
        public PostMatchItemType itemType;
        [ConditionalField(nameof(itemType), false, PostMatchItemType.Skill)]
        public PostMatchItemSkillDetail skillDetail;
        [ConditionalField(nameof(itemType), false, PostMatchItemType.Weapon)] public WeaponDetail weaponDetail;
        public AssetReference itemSprite;
    }

    [Serializable]
    public struct PostMatchItemSkillDetail
    {
        public AssetReference itemSpriteReference;
        public List<SkillDetailCategory> skillCategories;
    }

    [Serializable]
    public struct SkillDetailCategory
    {
        public ItemRarity rarity;
        public List<UpgradeDetail> upgradeDetails;
    }

    [Serializable]
    public struct WeaponDetail
    {
        public AssetReference weaponReference;
        public List<WeaponDetailCategory> weaponCategories;
    }

    [Serializable]
    public struct WeaponDetailCategory
    {
        public ItemRarity rarity;
        public List<WeaponAttribute> weaponAttributes;
    }

    [Serializable]
    public struct WeaponAttribute
    {
        public WeaponCategory attributeCategory;
        public float attributeValue;
        public ValueModifierType valueModifierType;
    }

    public enum PostMatchItemType
    {
        Weapon,
        Skill
    }

    public enum WeaponCategory
    {
        Damage,
    }

    public enum PurchaseOptions
    {
        Gem,
        Coin,
        XpPod,
        Ad,
        IAP,
        Free,
        Chest,
        Energy,
    }

    public enum ItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
}
