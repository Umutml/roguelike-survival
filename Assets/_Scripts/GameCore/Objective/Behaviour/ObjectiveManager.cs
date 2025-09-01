using System;
using System.Linq;
using System.Threading.Tasks;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using GameCore.Spawner;
using GameCore.Tutorial;
using Interfaces;
using UnityEngine;
using VContainer;
using static ObjectiveStructure;

public class ObjectiveManager : MonoBehaviour
{
    public static int DebuggerObjectiveIndex;
    public event Action<bool> OnObjectiveStart;
    public event Action OnObjectiveComplete;
    public event Action OnObjectiveFailed;

    [SerializeField] private ObjectiveObject[] objectiveObjects;
    public ObjectiveIndicatorManager objectiveIndicatorManager;
    [SerializeField] private bool testMode;
    private ObjectiveHub _activeObjectiveHub;
    private MobManager _mobManager;
    public GameParameterManager GameParameterManager { private set;get; }
    private PopupManager _popupManager;
    private TutorialSequenceController _tutorialSequenceController;
    private IAnalyticsService _analyticService;
    private ObjectiveSaveData _objectiveSaveData;
    private int _objectiveLevel;
    private int _objectiveTime, _lastObjectiveTime;
    private float _currentTime;
    private ObjectiveActionType _objectiveActionType;
    private int _objectiveKilledZombie;
    private IAudioService _audioService;
    public bool IsProgress => _activeObjectiveHub;
    public ObjectiveHub ActiveObjectiveHub => _activeObjectiveHub;
    public Tuple<int, int> ObjectiveUpgradeProgress => ActiveObjectiveHub != null ?
        ActiveObjectiveHub?.ObjectiveUpgradeProgress :
        new Tuple<int, int>(0, 0);
    public ObjectiveActionType ObjectiveActionType => _objectiveActionType;
    private int ObjectiveLevel
    {
        get => _objectiveLevel;
        set
        {
            _objectiveLevel = value > objectiveObjects.Length - 1 ? 0 : value;
            _objectiveLevel = value;
        }
    }

    private void Start()
    {
        _objectiveSaveData = SaveLoadHelper.TryLoadPersistentData<ObjectiveSaveData>();
        ObjectiveLevel = _objectiveSaveData.GetNextObjectiveIndex(objectiveObjects);
        // if (_tutorialSequenceController.IsTutorialCompleted)
        // {
        //     Init().Forget();
        // }
    }

    private void OnEnable()
    {
        MobManager.OnZombieKilled += OnZombieKilled;
    }

    private void OnDisable()
    {
        MobManager.OnZombieKilled -= OnZombieKilled;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var objectiveObject in objectiveObjects)
        {
            if (objectiveObject == null) continue;
            if (string.IsNullOrEmpty(objectiveObject.objectiveName))
                objectiveObject.objectiveName = objectiveObject.objectiveAsset.editorAsset.name;
        }
    }
#endif
    [Inject]
    private void Initialize(MobManager mobManager, PopupManager popupManager,
        TutorialSequenceController tutorialSequenceController, IAnalyticsService analyticService,
        IAudioService audioService,GameParameterManager gameParameterManager)
    {
        _audioService = audioService;
        _mobManager = mobManager;
        _popupManager = popupManager;
        _tutorialSequenceController = tutorialSequenceController;
        _analyticService = analyticService;
        GameParameterManager = gameParameterManager;
    }

    private async UniTask Init()
    {
        if (testMode)
            foreach (var objectiveObject in objectiveObjects)
                await objectiveObject.SpawnObjective(_mobManager, this);
        else
            await objectiveObjects[ObjectiveLevel].SpawnObjective(_mobManager, this);
    }

    public void StartObjective(ObjectiveHub objectiveHub)
    {
        _objectiveActionType = ObjectiveActionType.Start;
        _activeObjectiveHub = objectiveHub;
        _currentTime = 0;
        _objectiveTime = -1;
        _lastObjectiveTime = -1;
        _objectiveKilledZombie = 0;
        _mobManager.IsLocked = true;
        OnObjectiveStart?.Invoke(objectiveHub.minimapActive);
        objectiveIndicatorManager.ObjectiveStart(
            objectiveHub.objectiveMilestones.Select(x => x.milestonePrefab).ToArray(),
            objectiveHub.objectiveMilestones.Select(x => x.milestoneProgress).ToArray());

        SetMissionMusic();
    }

    public void CompleteObjective(ObjectiveHub objectiveHub)
    {
        _objectiveActionType = ObjectiveActionType.Complete;
        _activeObjectiveHub = null;
        _currentTime = 0;
        _objectiveTime = -1;
        _lastObjectiveTime = -1;
        _objectiveKilledZombie = 0;
        _mobManager.IsLocked = false;
        _objectiveSaveData.CompletedObjectives.Add(objectiveObjects[ObjectiveLevel].objectiveName);
        SaveLoadHelper.SaveData(_objectiveSaveData);
        ObjectiveLevel = _objectiveSaveData.GetNextObjectiveIndex(objectiveObjects);
        OnObjectiveComplete?.Invoke();
        FinishObjective();
        // if (_tutorialSequenceController.IsTutorialCompleted)
        //     SpawnObjective();
    }

    public async void FailedObjective(ObjectiveHub objectiveHub)
    {
        _objectiveActionType = ObjectiveActionType.Failed;
        _activeObjectiveHub = null;
        _currentTime = 0;
        _objectiveTime = -1;
        _lastObjectiveTime = -1;
        _objectiveKilledZombie = 0;
        _mobManager.IsLocked = false;
        OnObjectiveFailed?.Invoke();
        FinishObjective();
        await Task.Delay(3000);
        _ = _popupManager.OpenPopup(PopupConstants.PopupType.Failed);
        SpawnObjective();
    }
    public async void SpawnObjectiveByIndex()
    {
        ObjectiveLevel = Mathf.Clamp(DebuggerObjectiveIndex, 0, objectiveObjects.Length - 1);
        objectiveObjects[ObjectiveLevel].SpawnObjective(_mobManager, this).Forget();
        var player = MobManager.TargetPlayer;
        var carController = player.GetComponent<PlayerCarController>().CarController;
        if (player.PlayerMovementMode == PlayerMovementMode.Drive)
            carController.transform.position = objectiveObjects[ObjectiveLevel].spawnPoint.position;
        else
            player.transform.position = objectiveObjects[ObjectiveLevel].spawnPoint.position;
    }
    public void SkipObjectiveFailed()
    {
        if (!_activeObjectiveHub) return;
        _activeObjectiveHub.SkipObjective(ObjectiveState.Failed);
    }
    public void SkipObjectiveCompleted()
    {
        if (!_activeObjectiveHub) return;
        _activeObjectiveHub.SkipObjective(ObjectiveState.Completed);
    }

    private void Update()
    {
        if (!_activeObjectiveHub) return;
        if (!_activeObjectiveHub.StartObjectiveOnAwake) return;
        _currentTime += Time.deltaTime;
        _objectiveTime = (int)_currentTime;
        if (_lastObjectiveTime == _objectiveTime) return;
        _activeObjectiveHub.UpdateObjective((int)_currentTime, _objectiveKilledZombie);
        _lastObjectiveTime = _objectiveTime;
    }

    private void OnZombieKilled()
    {
        if (!_activeObjectiveHub) return;
        _objectiveKilledZombie++;
    }
    public void ClearKilledZombie() => _objectiveKilledZombie = 0;

    public async UniTask SpawnObjectiveByType(ObjectiveType objectiveType)
    {
        var nextObjectiveIndex = objectiveObjects.ToList().FindIndex(x => x.objectiveType == objectiveType);
        ObjectiveLevel = nextObjectiveIndex;
        var objective = objectiveObjects.FirstOrDefault(x => x.objectiveType == objectiveType);
        if (objective == null) return;
        await objective.SpawnObjective(_mobManager, this);
    }

    private void FinishObjective()
    {
        _activeObjectiveHub = null;
        objectiveIndicatorManager.ObjectiveFinish();
    }

    private void SetMissionMusic()
    {
        string musicName = "Tutorial";
        switch (_objectiveLevel)
        {
            case 0:
                musicName = "Tutorial";
                break;
            case 1:
                musicName = "FreeRoam";
                break;
            case 2:
                musicName = "Tutorial";
                break;
            case > 2:
                musicName = "FreeRoam";
                break;
        }

        _audioService?.PlayMusic(musicName);
    }

    private async void SpawnObjective()
    {
        await AsyncHelper.WaitWhile(() => PlayerIsCloset(ObjectiveLevel));
        await Task.Delay(5000);
        await objectiveObjects[ObjectiveLevel].SpawnObjective(_mobManager, this);
    }

    private bool PlayerIsCloset(int objectiveIndex) =>
        Vector3.Distance(MobManager.TargetPlayer.transform.position,
            objectiveObjects[objectiveIndex].spawnPoint.position) < 20;

    public UniTask DestroyObjectiveByType(ObjectiveType objectiveType)
    {
        var objective = objectiveObjects.FirstOrDefault(x => x.objectiveType == objectiveType);
        if (objective == null) return UniTask.CompletedTask;
        Destroy(objective.objectiveHub);
        return UniTask.CompletedTask;
    }
    public void SendObjectiveAnalytic(string eventState)
    {
        var eventName = objectiveObjects[_objectiveLevel].objectiveName switch
        {
            "ObjectiveDefend_01" => ObjectiveAnalytics.DEFEND,
            "ObjectiveDefend_02" => ObjectiveAnalytics.DEFEND,
            "ObjectiveEscort_01" => ObjectiveAnalytics.CONVOY,
            "ObjectiveSurvive_01" => ObjectiveAnalytics.HELI,
            _ => ObjectiveAnalytics.DEFEND
        };
        eventName += eventState;
        _analyticService.LogEvent(new EventParameters<string> { EventName = eventName });
    }
    public bool CheckObjectiveCompleted(ObjectiveType objectiveType)
    {
        var targetObjectiveName = objectiveObjects.FirstOrDefault(x => x.objectiveType == objectiveType)?.objectiveName;
        return _objectiveSaveData.CompletedObjectives.Contains(targetObjectiveName);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (var objectiveObject in objectiveObjects)
        {
            if (objectiveObject == null) continue;
            if (objectiveObject.spawnPoint == null) continue;
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(objectiveObject.spawnPoint.position, 0.5f);
        }
    }
#endif
}

public enum ObjectiveActionType
{
    Start,
    Complete,
    Failed
}