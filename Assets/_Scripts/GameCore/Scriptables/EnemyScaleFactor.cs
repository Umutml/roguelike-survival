using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "EnemyScaleFactor", menuName = "Scriptable Objects/EnemyScaleFactor")]
    public class EnemyScaleFactor : ScriptableObject
    {
        public List<ScaleCondition> scaleConditions;
    }

    [Serializable]
    public struct ScaleCondition
    {
        public string name;
        public ScaleType scaleType;
        public float value;
        public ValueModifierType valueModifierType;
        public int perLevel;
    }

    public enum ScaleType
    {
        Health,
        MovementSpeed,
        AttackDamage,
        AttackRange,
        AttackSpeed,
        DetectionRadius,
        XPDrop,
    }
}