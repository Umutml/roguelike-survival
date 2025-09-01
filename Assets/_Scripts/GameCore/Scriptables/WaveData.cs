using System.Collections.Generic;
using _Scripts.Utilities;
using MyBox;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 0)]
    public class WaveData : ScriptableObject
    {
        public Wave[] waves;

#if UNITY_EDITOR
        [ButtonMethod]
        public void SetWaves()
        {
            if (waves is not { Length: > 0 })
            {
                LoggerNS.LogWarning("No waves found to set.");
                return;
            }

            for (var i = 0; i < waves.Length; i++)
            {
                var index = i + 1;
                waves[i].name = $"{index}th Wave";
                waves[i].level = index;
                waves[i].duration = 30;
            }

            LoggerNS.Log("Waves have been updated!");
        }
#endif
    }

    [System.Serializable]
    public class Wave
    {
        public string name;
        public int level;
        public int duration;
        public int large;
        public int medium;
        public int small;
        public List<SpawnBehaviorState> behaviorStates = new();
    }


    [System.Serializable]
    public class SpawnBehaviorState
    {
        public SpawnType spawnType = SpawnType.PizzaSlice;
        public int spawnCount = 12;
        public bool isOnlyTutorialActive;
        public float spawnAngle = 45;

        [Range(0, 100)] public float attackerProbability = 100;
        [Range(0, 100)] public float waitingProbability = 0;
        [Range(0, 100)] public float patrolProbability = 0;

        public BehaviorType GetBehaviourType()
        {
            var debugTotalProbability = attackerProbability + waitingProbability + patrolProbability;
            var debugRandomPoint = UnityEngine.Random.Range(0, debugTotalProbability);
            var debugCumulativeProbability = 0f;
            debugCumulativeProbability += attackerProbability;
            if (debugRandomPoint <= debugCumulativeProbability)
                return BehaviorType.Attacker;
            debugCumulativeProbability += waitingProbability;
            if (debugRandomPoint <= debugCumulativeProbability)
                return BehaviorType.Waiting;
            debugCumulativeProbability += patrolProbability;
            if (debugRandomPoint <= debugCumulativeProbability)
                return BehaviorType.Patrolling;
            return BehaviorType.Attacker;
        }
    }

    public enum BehaviorType
    {
        Attacker,
        AttackToObject,
        Patrolling,
        Waiting
    }

    public enum SpawnType
    {
        PizzaSlice,
        Cluster
    }
}