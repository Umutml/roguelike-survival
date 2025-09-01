using _Utilities;
using GameCore.Spawner;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "WeaponDropChances", menuName = "ScriptableObjects/WeaponDropChances")]
    public class WeaponDropChances : ScriptableObject
    {
        public WeaponDropChance[] DropChances;

        public string GetRandomWeaponByChance()
        {
            int roll = Random.Range(0, 101);
            int cumulativeChance = 0;

            foreach (var dropChance in DropChances)
            {
                cumulativeChance += dropChance.Probability;
                if (roll <= cumulativeChance)
                {
                    return dropChance.Key;
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public struct WeaponDropChance
    {
        public string Key;
        public int Probability;
    }
}