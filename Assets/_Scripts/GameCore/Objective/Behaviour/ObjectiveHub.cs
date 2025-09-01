using System;
using System.Linq;
using System.Threading.Tasks;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using static _Utilities.Helper;
using static ObjectiveStructure;
using static ObjectiveStructure.UIIndicator;

public class ObjectiveHub : MonoBehaviour
{
    [SerializeField] private ObjectiveType objectiveType;
    [SerializeField] private int timeCondition, mobCountCondition, distanceCondition, distanceTimeCondition;
    private int _currentTime, _currentMobCount, _allKilledMobCount, _currentDistanceTime, _waveIndex;
    private ObjectiveState _objectiveState;
    public UpgradeType upgradeType;
    public bool minimapActive = false;
    public ObjectiveMilestone[] objectiveMilestones;
    [SerializeField] private UIIndicators uiIndicators;
    [SerializeField] public ObjectiveEvents allObjectiveEvents;
    [SerializeField] public ObjectiveWave[] objectiveWaves;
    [SerializeField] private NpcObject[] objectiveNpcs;
    [SerializeField] private Damageables damageables;
    private MobManager _mobManager;
    private ObjectiveManager _objectiveManager;
    private ObjectiveIndicatorManager _objectiveIndicatorManager;
    private ObjectiveWave CurrentWave { get; set; }
    public ObjectiveType ObjectiveType => objectiveType;
    public bool StartObjectiveOnAwake { get; private set; }
    private int _objectiveProgress;
    public Tuple<int, int> ObjectiveUpgradeProgress
    {
        get
        {
            var waveIndex = objectiveWaves.Length > 0 ? _waveIndex : 1;
            var killCount = _currentMobCount + _allKilledMobCount;
            return new Tuple<int, int>(waveIndex, killCount);
        }
    }
    public async void Init(MobManager mobManager, ObjectiveManager objectiveManager,ObjectiveParameters objectiveParameters)
    {
        if(objectiveParameters != null)
            await FetchGameParameterConfig(objectiveParameters);
        _mobManager = mobManager;
        _objectiveManager = objectiveManager;
        _objectiveIndicatorManager = _objectiveManager.objectiveIndicatorManager;
        foreach (var objectiveEvent in allObjectiveEvents.objectiveEvents)
            objectiveEvent.Init(_mobManager, damageables);
        foreach (var npcObject in objectiveNpcs)
            npcObject.Init();
        SubscribeNpcEvents();
    }

    private Task FetchGameParameterConfig(ObjectiveParameters objectiveParameters)
    {
        for (int i = 0; i < objectiveParameters.waves.Count; i++)
        {
            var waveParameter = objectiveParameters.waves[i];
            if (objectiveWaves.Length <= i)continue;
            objectiveWaves[i].mobCount = waveParameter.mobCount;
            objectiveWaves[i].spawnDelay = waveParameter.spawnDelay;
            objectiveWaves[i].spawnDuration = waveParameter.spawnDuration;
            objectiveWaves[i].enemyDifficulty = waveParameter.difficulty;
        }
        for (int i = 0; i < objectiveParameters.behaviours.Count; i++)
        {
            var behaviourParameter = objectiveParameters.behaviours[i];
            if (allObjectiveEvents.objectiveEvents.Length <= i)continue;
            allObjectiveEvents.objectiveEvents[i].targetTime = behaviourParameter.targetTime;
            for (int j = 0; j < behaviourParameter.mobBehaviours.Count; j++)
            {
                var mobBehaviourParameter = behaviourParameter.mobBehaviours[j];
                if (allObjectiveEvents.objectiveEvents[i].mobBehaviours.Length <= j)continue;
                allObjectiveEvents.objectiveEvents[i].mobBehaviours[j].spawnCount = mobBehaviourParameter.mobCount;
                allObjectiveEvents.objectiveEvents[i].mobBehaviours[j].enemyDifficulty = mobBehaviourParameter.difficulty;
            }
        }

        return Task.CompletedTask;
    }
    private void SubscribeNpcEvents()
    {
        foreach (var npcObject in objectiveNpcs)
            npcObject.npcObjective.Died += OnNpcDied;
    }
    private void OnNpcDied(DamageSource obj)
    {
        _objectiveManager.SendObjectiveAnalytic(ObjectiveAnalytics.NPC_DIED);
        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.NPC_DIED_INDICATOR, _objectiveIndicatorManager, GetNpcAliveCount);
    }
    private void OnWaveCompleted()
    {
        var lastWave = 0;
        for (var i = 0; i < objectiveWaves.Length; i++)
        {
            if (objectiveWaves[i].IsCompleted) continue;
            lastWave = i;
            break;
        }
        _objectiveManager.SendObjectiveAnalytic(string.Format(ObjectiveAnalytics.WAVE_COMPLETED, lastWave));
        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.WAVE_COMPLETE_INDICATOR, _objectiveIndicatorManager, lastWave);
        CurrentWave = null;
    }
    public async void StartObjective()
    {
        if (_objectiveManager.IsProgress)
            return;
        _objectiveManager.StartObjective(this);
        foreach (var npcObject in objectiveNpcs)
            npcObject.npcObjective.SetNpcState(NpcState.Idle);
        foreach (var objectiveEvent in allObjectiveEvents.startEvents)
            objectiveEvent.Execute();
        // _objectiveManager.SendObjectiveAnalytic(ObjectiveAnalytics.OBJECTIVE_START); // Tutorial quests sending through tutorial sequence, this section not needed for now
        await uiIndicators.ShowStartIndicators(_objectiveIndicatorManager);
        StartObjectiveOnAwake = true;
    }
    private void CompleteObjective()
    {
        _objectiveManager.CompleteObjective(this);
        foreach (var npcObject in objectiveNpcs)
            npcObject.npcObjective.Died -= OnNpcDied;
        foreach (var objectiveEvent in allObjectiveEvents.completeEvents)
            objectiveEvent.Execute();
        foreach (var objectiveEvent in allObjectiveEvents.finishEvents)
            objectiveEvent.Execute();
        uiIndicators.ShowCompleteIndicators(_objectiveIndicatorManager);
        // _objectiveManager.SendObjectiveAnalytic(ObjectiveAnalytics.OBJECTIVE_COMPLETE); // Not works correctly, sending 2 objective at the same time
    }

    public void SkipObjective(ObjectiveState objectiveState)
    {
        if (objectiveState == ObjectiveState.Completed)
            CompleteObjective();
        else
            FailedObjective();
    }
    private void FailedObjective()
    {
        foreach (var npcObject in objectiveNpcs)
            npcObject.npcObjective.Died -= OnNpcDied;
        foreach (var objectiveEvent in allObjectiveEvents.finishEvents)
            objectiveEvent.Execute();
        foreach (var failedEvent in allObjectiveEvents.failedEvents)
            failedEvent.Execute();
        uiIndicators.ShowFailedIndicators(_objectiveIndicatorManager);
        _objectiveManager.FailedObjective(this);
        _objectiveManager.SendObjectiveAnalytic(ObjectiveAnalytics.OBJECTIVE_FAILED);
    }
    public void UpdateObjective(int time, int mobCount)
    {
        _currentMobCount = mobCount;
        _currentTime = time;
        ObjectiveState = GetObjectiveState();
        switch (ObjectiveState)
        {
            case ObjectiveState.InProgress:
                if (CurrentWave == null)
                {
                    for (var i = 0; i < objectiveWaves.Length; i++)
                    {
                        var objectiveWave = objectiveWaves[i];
                        if (objectiveWave.IsActive || objectiveWave.IsCompleted) continue;
                        _currentMobCount = 0;
                        _objectiveManager.ClearKilledZombie();
                        CurrentWave = objectiveWave;
                        _waveIndex = i + 1;
                        _objectiveIndicatorManager.WaveStarted(true);
                        CurrentWave.Spawn(_mobManager, damageables.CollectDamageables());
                        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.WAVE_TIMER_INDICATOR, _objectiveIndicatorManager, 3, 1);
                        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.WAVE_TIMER_INDICATOR, _objectiveIndicatorManager, 2, 1);
                        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.WAVE_TIMER_INDICATOR, _objectiveIndicatorManager, 1, 1);
                        uiIndicators.ShowConditionalIndicators(ObjectiveConstants.WAVE_START_INDICATOR, _objectiveIndicatorManager, i + 1, 3);
                        _objectiveIndicatorManager.WaveUpdate(_waveIndex, objectiveWaves.Length, CurrentWave.GetWaveProgress());
                        break;
                    }
                }
                else
                {
                    _objectiveIndicatorManager.WaveUpdate(null, null, CurrentWave.GetWaveProgress());
                    if (CurrentWave.IsCompleted)
                    {
                        _allKilledMobCount += _currentMobCount;
                        OnWaveCompleted();
                        CurrentWave = null;
                    }
                }
                foreach (var objectiveEvent in allObjectiveEvents.objectiveEvents)
                    objectiveEvent.Execute(time, mobCount);
                ObjectiveProgress = GetObjectiveProgress();
                break;
            case ObjectiveState.Failed:
                FailedObjective();
                break;
            case ObjectiveState.Completed:
                CompleteObjective();
                break;
        }
    }
    private ObjectiveState GetObjectiveState()
    {
        switch (objectiveType)
        {
            case ObjectiveType.Defend:
                return damageables.objectiveDamageable.All(x => x.IsDead) ?
                    ObjectiveState.Failed :
                    timeCondition <= _currentTime && mobCountCondition <= _currentMobCount && IsAllWavesCompleted ?
                        ObjectiveState.Completed :
                        ObjectiveState.InProgress;
            case ObjectiveType.Convoy:
                if (timeCondition <= _currentTime && mobCountCondition <= _currentMobCount && IsAllWavesCompleted)
                    return ObjectiveState.Completed;
                if (damageables.objectiveDamageable.All(x => x.IsDead))
                {
                    _objectiveManager.SendObjectiveAnalytic(ObjectiveAnalytics.TRUCK_EXPLODED);
                    return ObjectiveState.Failed;
                }
                if (PlayerIsCloset)
                {
                    _currentDistanceTime = 0;
                    return ObjectiveState.InProgress;
                }
                uiIndicators.ShowConditionalIndicators(ObjectiveConstants.PLAYER_FARAWAY_INDICATOR, _objectiveIndicatorManager, distanceTimeCondition - _currentDistanceTime);
                _currentDistanceTime++;
                return _currentDistanceTime > distanceTimeCondition ? ObjectiveState.Failed : ObjectiveState.InProgress;
            case ObjectiveType.WaveSurvive:
                return timeCondition <= _currentTime && mobCountCondition <= _currentMobCount && IsAllWavesCompleted ?
                    ObjectiveState.Completed :
                    ObjectiveState.InProgress;
            case ObjectiveType.CountdownSurvive:
            case ObjectiveType.Collect:
            default:
                goto case ObjectiveType.WaveSurvive;
        }
    }
    private int GetObjectiveProgress()
    {
        if (mobCountCondition > 0)
            return (int)Remap(_currentMobCount, 0, mobCountCondition, 0, 100);
        if (timeCondition > 0)
            return (int)Remap(_currentTime, 0, timeCondition, 0, 100);
        if (objectiveWaves.Length <= 0) return 100;
        float waveProgress = 0;
        foreach (var objectiveWave in objectiveWaves)
            waveProgress += objectiveWave.WaveProgress;
        return (int)waveProgress / objectiveWaves.Length;
    }

    private bool PlayerIsCloset => Vector3.Distance(damageables.objectiveDamageable[0].transform.position, MobManager.TargetPlayer.transform.position) < distanceCondition;
    private int GetNpcAliveCount => objectiveNpcs.Count(x => x.npcObjective.IsDead == false);
    private bool IsAllWavesCompleted => objectiveWaves.Length == 0 || objectiveWaves.All(x => x.IsCompleted);
    private int ObjectiveProgress
    {
        set
        {
            if (_objectiveProgress != value)
                _objectiveIndicatorManager.UpdateObjectiveProgress(value);
            _objectiveProgress = value;
        }
    }
    private ObjectiveState ObjectiveState
    {
        get => _objectiveState;
        set
        {
            transform.tag = value == ObjectiveState.Idle ? "Objective" : "Untagged";
            _objectiveState = value;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var eventObject in allObjectiveEvents.objectiveEvents)
        {
            foreach (var mobBehaviour in eventObject.mobBehaviours)
            {
                if (mobBehaviour.spawnPoint)
                    Gizmos.DrawSphere(mobBehaviour.spawnPoint.position, 0.5f);
            }
        }
    }
#endif
}