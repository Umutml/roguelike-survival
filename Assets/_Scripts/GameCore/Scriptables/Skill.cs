using UnityEngine;
using System;
using MyBox;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill", order = 0)]
    public class Skill : ScriptableObject
    {
        public new string name;
        public RarityLevel rarity;
        public UpgradeType upgradeType;
        public Sprite icon;
        public TriggerType triggerType;

        [ConditionalField(nameof(triggerType), false, TriggerType.EventBased)]
        public EventTriggerCondition eventTriggerCondition;

        [ConditionalField(nameof(triggerType), false, TriggerType.TimeBased)]
        public TimeBasedCondition timeBasedCondition;

        [ConditionalField(nameof(triggerType), true, TriggerType.Passive)]
        public SkillEventEffect skillEventEffect;

        public StarUpgrade[] starUpgrades;
    }

    [Serializable]
    public class StarUpgrade
    {
        public int starLevel;
        public string description;
        public UpgradeDetail[] upgradeDetails;
    }

    [Serializable]
    public class UpgradeDetail
    {
        public StatUpgradeType type;
        public float value;
        public ValueModifierType valueModifierType;
        [HideInInspector] public Skill skill;


        public UpgradeDetail(StatUpgradeType type, float value, ValueModifierType valueModifierType, Skill skill = null)
        {
            this.type = type;
            this.value = value;
            this.valueModifierType = valueModifierType;
            this.skill = skill;
        }

        public bool Equals(UpgradeDetail other)
        {
            return type == other.type && value.Equals(other.value) && valueModifierType == other.valueModifierType;
        }

        public override bool Equals(object obj)
        {
            return obj is UpgradeDetail other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) type, value, (int) valueModifierType);
        }
    }

    [Serializable]
    public struct EventTriggerCondition
    {
        public EventBasedTrigger eventBasedTrigger;
        public TriggerChance[] chances;
        public TriggerCooldown[] cooldowns;
    }

    [Serializable]
    public struct TimeBasedCondition
    {
        public TriggerCooldown[] cooldowns;
    }

    [Serializable]
    public struct TriggerChance
    {
        public int level;
        public float chance;
    }

    [Serializable]
    public struct TriggerCooldown
    {
        public int level;
        public float cooldown;
    }

    [Serializable]
    public struct SkillEventEffect
    {
        public SkillEffectData[] durations;
        public SkillEffectData[] radii;
        public SkillEffectData[] damages;
    }

    [Serializable]
    public struct SkillEffectData
    {
        public float value;
        public int starLevel;
    }

    public enum TriggerType
    {
        Passive,
        EventBased,
        TimeBased
    }

    public enum EventBasedTrigger
    {
        LowHealth,
    }

    public enum StatUpgradeType
    {
        Damage,
        CarTurretDamage,
        Speed,
        CarSpeed,
        MaxHealth,
        PickupRange,
        CarPickupRange,
        AttackSpeed,
        FuelCapacity,
        HealthRestoration,
        StunNearbyZombies,
        AreaNukeDamage,
        CriticalHitChance,
        MaxShield,
        DodgeChance,
        MeleeAttacksSpeed,
        CollisionDamage,
        CarShield,
        CarMaxDurability,
        Armor,
        CriticalDamage,
        CarWeaponAttackSpeed,
        CarCriticalHitChance,
        CarCriticalDamage,
        ShieldCapacity,
        HealthRegenPercent,
        ProjectileCount,
    }

    public enum ValueModifierType
    {
        Add,
        MultiplyIncrease,
        Subtract,
        MultiplyDecrease
    }

    [Flags]
    public enum UpgradeType
    {
        None = 0,
        Character = 1 << 0,
        Weapon = 1 << 1,
        Car = 1 << 2,
        All = Character | Weapon | Car
    }

    public enum RarityLevel
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
