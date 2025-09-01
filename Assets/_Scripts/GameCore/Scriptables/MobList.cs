using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "MobList", menuName = "Scriptables/MobList", order = 1)]
    public class MobList : ScriptableObject
    {
        public MobPair[] Mobs;
        
        [System.Serializable]
        public struct MobPair
        {
            public MobType Type;
            public string Name;
            public float SpawnChance;
            public int PoolWarmupCount;
        }
    }


    public enum MobType
    {
        Standard,
        Boss
    }
}
