using System;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "MobSpawnRangeData", menuName = "ScriptableObjects/MobSpawnRangeData")]
    public class MobSpawnRangeData : ScriptableObject
    {
        public int DefaultMobCount;
        public MobSpawnRange[] MobSpawnRanges;
        
        [Serializable]
        public struct MobSpawnRange
        {
            public float MaxRange;
            public int MaxMobCount;
        }
    }
}
