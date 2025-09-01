using System;
using System.Collections.Generic;
using GameCore.Spawner;
using MyBox;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "BoxDropChanceData", menuName = "ScriptableObjects/BoxDropChanceData")]
    public class BoxDropChanceData : ScriptableObject
    {
        public int boxBetweenDistance;
        public int boxCount;
        public float boxSpawnRadius;
        public List<DropChance> dropChances = new();
    }

    [Serializable]
    public struct DropChance
    {
        public DropPodType dropPodType;
        public int probability;
        public bool isWaveOnly;
        public bool canIncrementDrop;
        public bool hasValue;
        public bool isDelay;

        [ConditionalField(nameof(isDelay), false)]
        public int delayValue;

        [ConditionalField(nameof(hasValue), false)]
        public int minValue;

        [ConditionalField(nameof(hasValue), false)]
        public int maxValue;
    }
}