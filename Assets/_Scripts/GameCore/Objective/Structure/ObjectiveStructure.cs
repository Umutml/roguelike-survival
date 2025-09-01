using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Scriptables;
using GameCore.Spawner;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using static _Utilities.Helper;
using Object = UnityEngine.Object;

public static class ObjectiveConstants
{
    public const string NPC_DIED_INDICATOR = "NpcDied";
    public const string WAVE_START_INDICATOR = "WaveStart";
    public const string WAVE_TIMER_INDICATOR = "WaveTimer";
    public const string WAVE_COMPLETE_INDICATOR = "WaveComplete";
    public const string PLAYER_FARAWAY_INDICATOR = "PlayerFarAway";
}
public class ObjectiveStructure
{
    public enum ObjectiveType
    {
        Defend,
        Convoy,
        WaveSurvive,
        CountdownSurvive,
        Collect
    }

    [Serializable]
    public class Damageables
    {
        public ObjectiveDamageable[] damageableChunks;
        public ObjectiveDamageable[] objectiveDamageable;

        public ObjectiveDamageable[] CollectDamageables()
        {
            var allDamageables = new List<ObjectiveDamageable>();
            foreach (var objectiveDamageable in objectiveDamageable)
                allDamageables.Add(objectiveDamageable);
            foreach (var damageableChunk in damageableChunks)
                allDamageables.Add(damageableChunk);
            return allDamageables.ToArray();
        }
    }

    [Serializable]
    public class DamageableChunk
    {
        public ObjectiveDamageable[] Damageables;
    }
    [Serializable]
    public class UIIndicators
    {
        public UIIndicator[] startIndicators;
        public UIIndicator[] conditionalIndicators;
        public UIIndicator[] failedIndicators;
        public UIIndicator[] completeIndicators;
        public float delay;
        public async UniTask ShowStartIndicators(ObjectiveIndicatorManager objectiveIndicatorManager)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            foreach (var indicator in startIndicators)
                indicator.Show(objectiveIndicatorManager);
        }
        public void ShowFailedIndicators(ObjectiveIndicatorManager objectiveIndicatorManager)
        {
            foreach (var indicator in failedIndicators)
                indicator.Show(objectiveIndicatorManager);
        }
        public async void ShowCompleteIndicators(ObjectiveIndicatorManager objectiveIndicatorManager)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            foreach (var indicator in completeIndicators)
                indicator.Show(objectiveIndicatorManager);
        }
        public void ShowConditionalIndicators(string indicatorKey, ObjectiveIndicatorManager objectiveIndicatorManager, params object[] values)
        {
            foreach (var indicator in conditionalIndicators)
            {
                if (indicator.indicatorText.textKey != indicatorKey) continue;
                indicator.Show(objectiveIndicatorManager, values);
                break;
            }
        }
    }
    [Serializable]
    public class UIIndicator
    {
        public AdvancedRichTextContainer indicatorText;
        public float showTime;
        public bool isRepeatable = true;
        private bool _isActive;
        public IndicatorType indicatorType;
        public Color backgroundColor = new Color(0, 0, 1, 1);

        public async void Show(ObjectiveIndicatorManager objectiveIndicatorManager, params object[] values)
        {
            if (_isActive)
                return;
            if (indicatorType == IndicatorType.Time && values.Length > 0)
            {
                var timeValue = (int)values[0];
                var minutes = Mathf.FloorToInt(timeValue / 60);
                var seconds = Mathf.FloorToInt(timeValue % 60);
                values = new object[] { minutes, seconds };
            }
            var indicatorMessage = CheckAndFormat(indicatorText.ToString(), values);
            objectiveIndicatorManager.UpdateObjectiveStatus(indicatorMessage, backgroundColor, showTime, indicatorType);
            if (!isRepeatable) return;
            await Task.Delay(TimeSpan.FromSeconds(showTime));
            _isActive = false;
        }

        public string CheckAndFormat(string format, params object[] args)
        {
            try
            {
                var formattedString = string.Format(format, args);
                return formattedString;
            }
            catch (FormatException)
            {
                return format;
            }
        }
        public enum IndicatorType
        {
            Time,
            Text
        }
    }
    [Serializable]
    public class ObjectiveWave
    {
        public Transform[] spawnPoints;
        public int mobCount;
        public int spawnDelay;
        public int spawnDuration;
        public bool lateReturnToPool = true;
        public EnemyDifficulty enemyDifficulty;
        public BehaviorType behaviorType;
        private int _mobCountCondition;
        private bool _isActive, _isCompleted, _isInitialized;
        private int _currentMobCount;
        public bool IsActive => _isActive;
        public bool IsCompleted => _isCompleted;
        public float WaveProgress => !_isInitialized ? 0 : Remap(_currentMobCount, 0, _mobCountCondition, 0, 100);
        private MobManager _mobManager;
        public async void Spawn(MobManager mobManager, params ObjectiveDamageable[] objectiveDamageables)
        {
            if (IsActive) return;
            _isActive = true;
            _mobManager = mobManager;
            _mobManager.KillAllMobs();
            _currentMobCount = 0;
            await Task.Delay(TimeSpan.FromSeconds(spawnDelay * 2));
            spawnDuration = Mathf.Clamp(spawnDuration, 1, 10);
            var currentMobCount = Mathf.Clamp(mobCount, 1, 300);
            var mobCountPerSpawn = currentMobCount / spawnDuration;
            _mobCountCondition = mobCountPerSpawn * spawnDuration;
            for (var y = 0; y < mobCountPerSpawn; y++)
            {
                var spawnIndex = y % spawnPoints.Length;
                _mobManager.SpawnObjectiveMob(spawnPoints[spawnIndex], spawnDuration, enemyDifficulty, behaviorType, lateReturnToPool, AddMobCount, objectiveDamageables);
                await Task.Delay(1000);
            }
            _isInitialized = true;
        }
        protected void AddMobCount()
        {
            _currentMobCount++;
            if (_mobManager.ActiveMobs.Count == 0)
            {
                _isCompleted = true;
            }
            if (_currentMobCount < _mobCountCondition) return;
            _isCompleted = true;
        }
        public int GetWaveProgress() => _mobCountCondition - _currentMobCount;
    }
    [Serializable]
    public class ObjectiveEvents
    {
        public ObjectiveEvent[] startEvents;
        public ObjectiveEvent[] objectiveEvents;
        public ObjectiveEvent[] failedEvents;
        public ObjectiveEvent[] completeEvents;
        public ObjectiveEvent[] finishEvents;
    }
    [Serializable]
    public class ObjectiveEvent
    {
        public string objectiveName;
        public int targetProgress;
        public int targetTime;
        public int targetMobCount;
        public int delay;
        public bool isRepeatable;
        private bool _isCompleted;
        private MobManager _mobManager;
        [SerializeField] internal MobBehaviour[] mobBehaviours;
        [SerializeField] private ObjectCreator[] objectCreators;
        [SerializeField] private ObjectEnabler[] objectEnablers;
        [SerializeField] private ObjectTransform[] objectTransforms;
        [SerializeField] private ObjectPath[] objectPaths;
        [SerializeField] private ObjectFunction[] objectFunctions;
        [SerializeField] private ObjectSnapper[] objectSnappers;
        [SerializeField] private AnimationObject[] animationObjects;
        private ObjectiveDamageable[] _objectiveDamageables;
        private CancellationTokenSource _cancellationTokenSource;

        public void Init(MobManager mobManager, Damageables damageables)
        {
            _mobManager = mobManager;
            _objectiveDamageables = damageables.CollectDamageables();
        }

        public async void Execute(int time = 0, int mobCount = 0)
        {
            try
            {
                if (isRepeatable)
                {
                    if (time % targetTime != 0) return;
                }
                else
                {
                    if (time != targetTime || _isCompleted) return;
                }
                _isCompleted = true;
                if (delay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                foreach (var mobBehaviour in mobBehaviours)
                    _mobManager.SpawnObjectiveMob(mobBehaviour.spawnPoint, mobBehaviour.spawnCount, mobBehaviour.enemyDifficulty, mobBehaviour.behaviorType, mobBehaviour.lateReturnToPool, null, _objectiveDamageables);
                foreach (var objectCreator in objectCreators)
                    objectCreator.Execute();
                foreach (var objectEnabler in objectEnablers)
                    objectEnabler.Execute();
                foreach (var objectSnapper in objectSnappers)
                    objectSnapper.Execute();
                foreach (var objectPath in objectPaths)
                    objectPath.Execute();
                foreach (var objectFunction in objectFunctions)
                    objectFunction.Execute();
                foreach (var objectTransform in objectTransforms)
                    objectTransform.Execute();
                foreach (var animationObject in animationObjects)
                    animationObject.Execute();
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        }
    }

    [Serializable]
    public class ObjectCreator
    {
        public AssetReferenceGameObject objectReference;
        public Transform objectCreationPoint;

        public async void Execute()
        {
            await ObjectManager.GetObjectWithoutPool(objectReference, objectCreationPoint.position,
                objectCreationPoint.rotation);
        }
    }

    [Serializable]
    public class ObjectTransform
    {
        public Transform targetObject;
        public Vector3 targetTransform;
        public float transformTime;

        public void Execute()
        {
            targetObject.DOKill();
            var targetPosition = targetObject.position + targetTransform;
            targetObject.DOMove(targetPosition, transformTime).SetEase(Ease.Linear);
        }
    }

    [Serializable]
    public class ObjectEnabler
    {
        public GameObject targetObject;
        public bool isActive;

        public void Execute()
        {
            targetObject.SetActive(isActive);
        }
    }

    [Serializable]
    public class ObjectSnapper
    {
        public GameObject targetObject;
        public GameObject snapObject;
        public float snapTime;
        public SnapType snapType;

        public enum SnapType
        {
            Position,
            Rotation,
            All
        }

        public void Execute()
        {
            targetObject.transform.DOKill();
            switch (snapType)
            {
                case SnapType.Position:
                    targetObject.transform.DOMove(snapObject.transform.position, snapTime).SetEase(Ease.Linear);
                    break;
                case SnapType.Rotation:
                    targetObject.transform.DORotate(snapObject.transform.rotation.eulerAngles, snapTime)
                        .SetEase(Ease.Linear);
                    break;
                case SnapType.All:
                    targetObject.transform.DOMove(snapObject.transform.position, snapTime).SetEase(Ease.Linear);
                    targetObject.transform.DORotate(snapObject.transform.rotation.eulerAngles, snapTime)
                        .SetEase(Ease.Linear);
                    break;
            }
        }
    }

    [Serializable]
    public class AnimationObject
    {
        public Animation targetObject;
        public string animationKey;
        public AnimationType animationType;

        public void Execute()
        {
            switch (animationType)
            {
                case AnimationType.Crossfade:
                    targetObject.CrossFade(animationKey);
                    break;
                case AnimationType.Play:
                    targetObject.Play();
                    break;
                case AnimationType.Stop:
                    targetObject.Stop();
                    break;
            }
        }

        public enum AnimationType
        {
            Crossfade,
            Play,
            Stop,
        }
    }

    [Serializable]
    public class ObjectPath
    {
        public Transform targetObject;
        public Transform[] pathPoints;
        public LookType lookType;
        public float pathTime;
        public Ease pathEase = Ease.Linear;
        private Vector3[] GetPathPoints()
        {
            var pathPointList = new List<Vector3>();
            foreach (var i in pathPoints)
            {
                pathPointList.Add(i.position);
            }

            return pathPointList.ToArray();
        }

        public void Execute()
        {
            targetObject.DOKill();
            switch (lookType)
            {
                case LookType.LookAt:
                    targetObject.DOPath(GetPathPoints(), pathTime).SetEase(pathEase).SetLookAt(0.01f, Vector3.forward, Vector3.up);
                    break;
                case LookType.Snap:
                    targetObject.DOPath(GetPathPoints(), pathTime).SetEase(pathEase).OnWaypointChange(index =>
                    {
                        if (index < pathPoints.Length - 1)
                        {
                            targetObject.DORotate(pathPoints[index + 1].rotation.eulerAngles, 2f);
                        }
                    });
                    break;
                case LookType.Car:
                    targetObject.DOPath(GetPathPoints(), pathTime, PathType.Linear).SetEase(pathEase).SetLookAt(0.01f);
                    break;
            }
        }

        public enum LookType
        {
            LookAt,
            Snap,
            Car
        }
    }

    [Serializable]
    public class ObjectFunction
    {
        public GameObject targetObject;
        public string functionName;
        public FunctionParameter functionParameter;

        public void Execute()
        {
            if (targetObject.activeSelf)
                targetObject.SendMessage(functionName, functionParameter);
        }
    }

    [Serializable]
    public struct FunctionParameter
    {
        public float functionFloat;
        public string functionString;
        public GameObject functionGameObject;

        public T GetParameter<T>()
        {
            if (typeof(T) == typeof(float))
                return (T)(object)functionFloat;
            if (typeof(T) == typeof(int))
                return (T)(object)(int)functionFloat;
            if (typeof(T) == typeof(string))
                return (T)(object)functionString;
            if (typeof(T) == typeof(GameObject))
                return (T)(object)functionGameObject;
            return default;
        }
    }

    [Serializable]
    public class MobBehaviour
    {
        public BehaviorType behaviorType;
        public EnemyDifficulty enemyDifficulty;
        public int spawnCount;
        public bool lateReturnToPool;
        public Transform spawnPoint;
    }

    [Serializable]
    public class ObjectiveObject
    {
        public string objectiveName;
        public AssetReferenceGameObject objectiveAsset;
        [HideInInspector] public ObjectiveHub objectiveHub;
        public Transform spawnPoint;
        public ObjectiveType objectiveType;

        public async UniTask SpawnObjective(MobManager mobManager, ObjectiveManager objectiveManager)
        {
            if (objectiveHub)
                Object.Destroy(objectiveHub.gameObject);
            mobManager ??= Object.FindObjectOfType<MobManager>();
            var hubObject = await ObjectManager.GetObjectWithoutPool(objectiveAsset, spawnPoint.position, spawnPoint.rotation);
            objectiveHub = hubObject.GetComponent<ObjectiveHub>();
            objectiveHub.name = objectiveName;
            objectiveHub.Init(mobManager, objectiveManager,objectiveManager.GameParameterManager.GetObjectiveParameters(objectiveName));
        }
    }
    [Serializable]
    public class EnemyDifficulty
    {
        [Range(1, 10)] public int healthDifficulty = 5;
        [Range(1, 10)] public int attackSpeed = 5;
        [Range(1, 10)] public int attackDamage = 5;
    }
    [Serializable]
    public class NpcStats
    {
        public float health = 100;
        public float attackRate = 1;
        public float fireRate = 0.2f;
        public float targetCooldown = 1;
        public float damage = 5;
        public float attackRange = 20;
        public int npcType = 0;
    }

    public enum NpcState
    {
        Idle,
        Move,
        Attack,
        Dead
    }

    public static class NpcAnimationConstants
    {
        public static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Reload = Animator.StringToHash("Reload");
        public static readonly int Dead = Animator.StringToHash("Dead");
        public static readonly int Run = Animator.StringToHash("Run");
    }

    [Serializable]
    public class NpcObject
    {
        public NpcObjective npcObjective;
        public NpcStats npcStats;

        public void Init()
        {
            npcObjective.SetNpcStats(npcStats);
        }
    }

    public enum ObjectiveState
    {
        Idle,
        InProgress,
        Failed,
        Completed
    }
    [Serializable]
    public class ObjectiveMilestone
    {
        public AssetReferenceGameObject milestonePrefab;
        public int milestoneProgress;
        public MilestoneType milestoneType;
        public float milestoneAmount;
        public enum MilestoneType
        {
            Health,
            Xp,
            Gold
        }
    }

    public class ObjectiveSaveData
    {
        public readonly List<string> CompletedObjectives = new List<string>();
        public int GetNextObjectiveIndex(ObjectiveObject[] objectiveObjects)
        {
            var nextObjectiveIndex = 0;
            if (CompletedObjectives.Count == 0)
                return nextObjectiveIndex;
            var lastObjectiveName = objectiveObjects.FindIndex(x => x.objectiveName == CompletedObjectives.Last()) + 1;
            return lastObjectiveName >= objectiveObjects.Length ? 0 : lastObjectiveName;
        }
    }
}

public static class ObjectiveAnalytics
{
    public const string CONVOY = "convoy_";
    public const string DEFEND = "holdoutdefend_";
    public const string HELI = "heli_";
    public const string TRUCK_EXPLODED = "truckexploded";
    public const string WAVE_COMPLETED = "wave_{0}_completed";
    public const string NPC_DIED = "npc_died";
    public const string OBJECTIVE_FAILED = "objective_failed";
    public const string OBJECTIVE_COMPLETE = "objective_complete";
    public const string OBJECTIVE_START = "objective_start";
}