using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using _Utilities;
using GameCore.Health;
using GameCore.Player;
using GameCore.Player.Input;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using UnityEngine;
using static ObjectiveStructure;

[CreateAssetMenu(fileName = "GameParameters", menuName = "Game Settings/Parameters")]
public class GameParameters : ScriptableObject
{
    [Header("Zombie Parameters")] [Range(0, 1)]
    public float ZombieCrashDeadProbability = 0.5f;
    public float ZombieAttackRange = 1.25f;
    public float ZombiePatrolDetectionRadius = 10f;
    public float ZombieWaitingDetectionRadius = 15f;
    public float ZombieAttackCooldown = 0.2f;
    public int ZombieRagdollDuration = 1500;
    public int FreeRoamZombieCount = 10;
    public PlayerStatusController PlayerStatusController;
    public PlayerController PlayerController;
    public PlayerMovementController PlayerMovement;
    public ItemPicker PlayerItemPicker;
    public CarResources CarResources;
    public SkillData SkillData;
    public LevelData LevelData;
    public EnemyTypeData EnemyTypeData;
    public List<Weapon> Weapons;
    public WaveData WaveData;
    public WaveLevelData waveLevelData;
    public BoxDropChanceData BoxDropChanceData;
    public ObjectiveHub[] Objectives;
    public GameParameterConfig GameParameterConfig { get; private set; }

    public string GetDefaultGameConfig()
    {
        GameParameterConfig = new GameParameterConfig(this);
        var json = JsonUtility.ToJson(GameParameterConfig);
        return json;
    }
    public void SaveParameters()
    {
        GameParameterConfig = new GameParameterConfig(this);
        var json = JsonUtility.ToJson(GameParameterConfig);
        var fileName = $"GameParameters_v{Application.version}.json";
        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
    }

    public void LoadParameter(GameParameterConfig newGameParameter)
    {
        GameParameterConfig = newGameParameter;
        ZombieConstants.ZombieRagdollDuration = GameParameterConfig.ZombieRagdollDuration;
        ZombieConstants.ZombieAttackCooldown = GameParameterConfig.ZombieAttackCooldown;
        ZombieConstants.ZombieAttackRange = GameParameterConfig.ZombieAttackRange;
        ZombieConstants.ZombieCrashDeadProbability = GameParameterConfig.ZombieCrashDeadProbability;
        ZombieConstants.ZombiePatrolDetectionRadius = GameParameterConfig.ZombiePatrolDetectionRadius;
        ZombieConstants.ZombieWaitingDetectionRadius = GameParameterConfig.ZombieWaitingDetectionRadius;
        ZombieConstants.FreeRoamZombieCount = GameParameterConfig.FreeRoamZombieCount;
        CarResources = GameParameterConfig.carConfigs;
        SkillData = GameParameterConfig.skillData;
        LevelData = GameParameterConfig.levelData;
        EnemyTypeData = GameParameterConfig.enemyTypeData;
        Weapons = GameParameterConfig.weapons;
        WaveData = GameParameterConfig.waveData;
        BoxDropChanceData = GameParameterConfig.boxDropChanceData;
        
    }
}
[Serializable]
public class GameParameterConfig
{
    public float ZombieCrashDeadProbability = 0.5f;
    public float ZombieAttackRange = 1.25f;
    public float ZombiePatrolDetectionRadius = 10f;
    public float ZombieWaitingDetectionRadius = 15f;
    public float ZombieAttackCooldown = 0.2f;
    public int ZombieRagdollDuration = 1500;
    public int FreeRoamZombieCount = 10;
    public float maxHealth = 100;
    public float maxArmor = 100;
    public float movementSpeed = 5;
    public CarResources carConfigs = new();
    public SkillData skillData = new();
    public LevelData levelData = new();
    public EnemyTypeData enemyTypeData = new();
    public List<Weapon> weapons = new();
    public WaveData waveData = new();
    public BoxDropChanceData boxDropChanceData = new();
    public List<ObjectiveParameters> objectives = new();
    public List<WeaponParameter> weaponParameters = new();

    public GameParameterConfig(GameParameters gameParameters)
    {
        ZombieCrashDeadProbability = gameParameters.ZombieCrashDeadProbability;
        ZombieAttackRange = gameParameters.ZombieAttackRange;
        ZombiePatrolDetectionRadius = gameParameters.ZombiePatrolDetectionRadius;
        ZombieWaitingDetectionRadius = gameParameters.ZombieWaitingDetectionRadius;
        ZombieAttackCooldown = gameParameters.ZombieAttackCooldown;
        ZombieRagdollDuration = gameParameters.ZombieRagdollDuration;
        FreeRoamZombieCount = gameParameters.FreeRoamZombieCount;
        maxHealth = gameParameters.PlayerStatusController.MaxHealth;
        maxArmor = gameParameters.PlayerStatusController.Armor;
        movementSpeed = gameParameters.PlayerMovement.MovementSpeed;
        carConfigs = gameParameters.CarResources;
        skillData = gameParameters.SkillData;
        levelData = gameParameters.LevelData;
        enemyTypeData = gameParameters.EnemyTypeData;
        weapons = gameParameters.Weapons;
        waveData = gameParameters.WaveData;
        boxDropChanceData = gameParameters.BoxDropChanceData;
        objectives = new List<ObjectiveParameters>();
        foreach (var objective in gameParameters.Objectives)
        {
            var objectiveParameters = new ObjectiveParameters(objective.name,objective.objectiveWaves, objective.allObjectiveEvents.objectiveEvents);
            objectives.Add(objectiveParameters);
        }
    }
}

[Serializable]
public class ObjectiveParameters
{
    public string objectiveName;
    public List<ObjectiveWaveParameter> waves = new();
    public List<ObjectiveBehaviourParameter> behaviours = new();

    public ObjectiveParameters(string newObjectiveName,ObjectiveWave[] objectiveWave,ObjectiveEvent[] objectiveEvent)
    {
        objectiveName = newObjectiveName;
        foreach (var newObjectiveWave in objectiveWave)
            waves.Add(new ObjectiveWaveParameter(newObjectiveWave));
        foreach (var newObjectiveEvent in objectiveEvent)
            behaviours.Add(new ObjectiveBehaviourParameter(newObjectiveEvent));
    }
}
[Serializable]
public class ObjectiveWaveParameter
{
    public int mobCount;
    public int spawnDelay;
    public int spawnDuration;
    public EnemyDifficulty difficulty;
    public ObjectiveWaveParameter(ObjectiveWave objectiveWave)
    {
        mobCount = objectiveWave.mobCount;
        spawnDelay = objectiveWave.spawnDelay;
        spawnDuration = objectiveWave.spawnDuration;
        difficulty = objectiveWave.enemyDifficulty;
    }
}
[Serializable]
public class ObjectiveBehaviourParameter
{
    public int targetTime;
    public List<MobBehaviourParameter> mobBehaviours = new();
    public ObjectiveBehaviourParameter(ObjectiveEvent mobBehaviour)
    {
        targetTime = mobBehaviour.targetTime;
        foreach (var newMobBehaviour in mobBehaviour.mobBehaviours)
            mobBehaviours.Add(new MobBehaviourParameter(newMobBehaviour));
    }
}
[Serializable]
public class MobBehaviourParameter
{
    public int mobCount;
    public EnemyDifficulty difficulty;
    public MobBehaviourParameter(MobBehaviour mobBehaviour)
    {
        mobCount = mobBehaviour.spawnCount;
        difficulty = mobBehaviour.enemyDifficulty;
    }
}
[Serializable]
public class WeaponParameter
{
    public float damage;
    public float fireInterval;
    public float range;
}
