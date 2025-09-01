using RootMotion;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "EnemyType", menuName = "ScriptableObjects/EnemyType")]
    public class EnemyType : ScriptableObject
    {
        public new string name;
        public ZombieType zombieType;
        public EnemyCategory enemyCategory;
        public int health;
        public float movementSpeed;
        public float attackDamage;
        public float attackRange;
        public float attackSpeed;
        public float detectionRadius;
        public float baseXpDropValue;
        public float xpDropValue;
        public float xpDropChance;
        public float softCurrencyChance;
        public float minSoftCurrencyInWave;
        public float maxSoftCurrencyInWave;
        public float minSoftCurrencyInFreeRoam;
        public float maxSoftCurrencyInFreeRoam;
        public int largeHordeCount;
        public int mediumHordeCount;
        public int smallHordeCount;
        public string prefabPath;
        public string skinKey;
        //----------------------- Special Variables -----------------------//
        [ShowIf(nameof(zombieType), ZombieType.RangedZombie)]
        public AssetReferenceGameObject projectileReference; // Ranged enemies
        [ShowIf(nameof(enemyCategory), EnemyCategory.ExplodingSwarmer)]
        public AssetReferenceGameObject deathVFXReference; // Exploding enemies and if we want to add more death VFX
        [ShowIf(nameof(enemyCategory), EnemyCategory.ToxicBrute)]
        public AssetReferenceGameObject toxicVFXReference; // Toxic enemies constant VFX
        [ShowIf(nameof(enemyCategory), EnemyCategory.Spitter)]
        public RuntimeAnimatorController spitterRuntimeController; // Spitter enemy animation controller
    }

    public enum EnemyCategory
    {
        Walker,
        Runner,
        Spitter,
        ArmoredZombie,
        ToxicBrute,
        MutatedBrute,
        TutorialZombie,
        ExplodingSwarmer,
    }

    public enum ZombieType
    {
        StandardZombie,
        ZombieBoss,
        RangedZombie,
    }
}