using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Cathei.LinqGen;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "EnemyTypeData", menuName = "ScriptableObjects/EnemyTypeData")]
    public class EnemyTypeData : ScriptableObject
    {
        public List<EnemyTypeSpawnData> enemyTypes;
        public List<EnemyType> tutorialEnemyTypes;

        public EnemyType GetRandomEnemyType(bool isTutorialCompleted)
        {
            if (!isTutorialCompleted)
                return tutorialEnemyTypes.PickRandom();
            
            var totalSpawnChance = 0f;
            foreach (var enemyType in enemyTypes)
            {
                totalSpawnChance += enemyType.SpawnChance;
            }
            var randomValue = UnityEngine.Random.Range(0, totalSpawnChance);
            foreach (var enemyType in enemyTypes)
            {
                if (randomValue < enemyType.SpawnChance)
                {
                    return enemyType.EnemyType;
                }
                randomValue -= enemyType.SpawnChance;
            }
            return null;
        }

        public int CalculateZombieCountForHorde(Wave wave, ZombieType zombieType)
        {
            var enemyType = enemyTypes.Gen().Where(enemyType => enemyType.EnemyType.zombieType == zombieType);

            var smallHordeCount = enemyType.Sum(x => x.EnemyType.smallHordeCount) * wave.small;
            var mediumHordeCount = enemyType.Sum(x => x.EnemyType.mediumHordeCount) * wave.medium;
            var largeHordeCount = enemyType.Sum(x => x.EnemyType.largeHordeCount) * wave.large;

            return smallHordeCount + mediumHordeCount + largeHordeCount;
        }
        
        [Serializable]
        public struct EnemyTypeSpawnData
        {
            public EnemyType EnemyType;
            public float SpawnChance;
        }
    }
}