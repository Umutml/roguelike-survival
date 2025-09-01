using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.GameCore.Zone;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Player;
using GameCore.Tutorial.Steps;
using Interfaces;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Tutorial
{
    public class TutorialSequenceController : MonoBehaviour, ITutorialService
    {
        #region Actions

        public event Action SequenceFinished;
        public event Action<string> ChangedStatusPrompt;

        public event Action TutorialCompleted;

        public bool IsTutorialCompleted
        {
            get
            {
                if (_initialCheck)
                {
                    _initialCheck = false;
                    _isTutorialCompleted = GetPersistentTutorialCompletedStatus();
                }

                return _isTutorialCompleted;
            }
            set => _isTutorialCompleted = value;
        }

        #endregion

        #region Serializable Fields

        [SerializeField] private TutorialSequence sequence;

        #endregion

        #region Fields

        private PlayerStatusController _playerStatusController;
        private IAnalyticsService _analyticsService;
        private IAudioService _audioService;
        private IGameService _gameService;
        private List<ITutorialService.TutorialObject> _tutorialObjects = new();
        private IObjectResolver _resolver;
        private bool _initialized;
        private DateTime _tutorialStartTime;
        private TutorialCheckPointData? _tutorialCheckPointData;
        private TutorialCheckPoint? _tutorialCheckPoint;
        private bool _isTutorialCompleted;
        private bool _initialCheck = true;

        #endregion

        #region Properties

        public UniTaskCompletionSource TutorialObjectCompletionSource { get; } = new();

        public bool IsCheckPointInit { get; private set; }

        #endregion

        #region Public Methods

        [Inject]
        public void Initialize(IObjectResolver resolver, PlayerStatusController playerStatusController,
            IAnalyticsService analyticsService, IGameService gameService, IAudioService audioService)
        {
            if (_initialized) return;
            _initialized = true;
            _resolver = resolver;
            _playerStatusController = playerStatusController;
            _analyticsService = analyticsService;
            _audioService = audioService;
            _gameService = gameService;
            _tutorialCheckPoint = GetTutorialCheckPoint();

            if (IsTutorialCompleted)
            {
                LoggerNS.Log("Tutorial is already completed");
                return;
            }

            _tutorialStartTime = DateTime.Now;
            SendTutorialSpawnAnalyticEvent();

            foreach (var step in sequence.TutorialStepsCompositie.SelectMany(composite => composite.Steps))
            {
                step.Resolver = _resolver;
                step.TutorialService = this;
            }

            sequence.OnTutorialStepChanged += str => TutorialStepChanged?.Invoke(str);
            sequence.OnTutorialStepCompleted += str => TutorialStepCompleted?.Invoke(str);
            sequence.OnSequenceFinished += () => SequenceFinished?.Invoke();
            sequence.StartSequence(GetTutorialCheckPointData());
        }


        private void OnEnable()
        {
            _audioService.PlayMusic(IsTutorialCompleted ? "FreeRoam" : "Tutorial");
        }

        private void OnTutorialCompleted()
        {
            SendCompletedAnalyticEvent();
        }

        private void SendTutorialSpawnAnalyticEvent()
        {
            _analyticsService.LogEvent(new EventParameters<string>
            {
                EventName = "tt_tutorial_spawn",
                AdjustToken = AdjustNsEventTokens.TtTutorialSpawn
            });
        }

        private void SendCompletedAnalyticEvent()
        {
            var timeToComplete = (DateTime.Now - _tutorialStartTime).TotalSeconds;
            var zombieKill = _playerStatusController.KillCount;
            var podCount = _playerStatusController.PurchasePodCount;

            _analyticsService.LogEventParameterArray("ftue_completed",
                new Dictionary<string, object>
                {
                    {"time_to_complete", timeToComplete},
                    {"zombie_kill", zombieKill},
                    {"pod_count", podCount}
                });
            _analyticsService.LogEvent(new EventParameters<string>
            {
                AdjustToken = AdjustNsEventTokens.FtueCompleted
            });
        }

        #endregion

        #region Private Methods

        private TutorialCheckPoint? GetTutorialCheckPoint()
        {
            return SaveLoadHelper.IsDataExists(nameof(TutorialCheckPoint))
                ? SaveLoadHelper.TryLoadPersistentData<TutorialCheckPoint>()
                : null;
        }

        #endregion

        #region ITutorialService Members

        public string CurrentTutorialStepName { get; set; }
        public event Action<string> TutorialStepChanged;
        public event Action<string> TutorialStepCompleted;

        public async void CloseTutorialWall(bool isBase)
        {
            var baseWall = await GetTutorialObject("BaseWall");
            var bridgeWall = await GetTutorialObject("BridgeWall");

            var wall = isBase ? baseWall : bridgeWall;
            wall.SetActive(false);
        }

        public void SetObjects(ITutorialService.TutorialObject[] objects, bool isCleanCityScene)
        {
            _tutorialObjects.AddRange(objects);

            if (isCleanCityScene)
            {
                TutorialObjectCompletionSource.TrySetResult();
            }
        }

        public void InvokeStatusPrompt(string status)
        {
            ChangedStatusPrompt?.Invoke(status);
        }


        public async UniTask<GameObject> GetTutorialObject(string name)
        {
            await TutorialObjectCompletionSource.Task;

            foreach (var tutorialObject in _tutorialObjects)
            {
                if (tutorialObject.Name == name)
                {
                    return tutorialObject.Object;
                }
            }

            return null;
        }

        private bool GetPersistentTutorialCompletedStatus()
        {
            try
            {
                return SaveLoadHelper.TryLoadPersistentData<TutorialData>().IsCompleted;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void SetTutorialCompleted(bool isCompleted)
        {
            var tutorialData = SaveLoadHelper.TryLoadPersistentData<TutorialData>();
            tutorialData.IsCompleted = isCompleted;
            SaveLoadHelper.SaveData(tutorialData);

            if (isCompleted)
            {
                IsTutorialCompleted = true;
                TutorialCompleted?.Invoke();
                OnTutorialCompleted();
            }
        }

        public async void OpenBaseDoor(Transform player, string doorName)
        {
            var door = await GetTutorialObject(doorName);

            if (door == null)
            {
                LoggerNS.LogError("Door is null");
                return;
            }

            if (!door.TryGetComponent(out ZoneDoorController zoneDoorController))
            {
                LoggerNS.LogError("ZoneDoorController is null");
                return;
            }

            zoneDoorController.OpenDoors(player);
        }

        public TutorialCheckPointData? GetTutorialCheckPointData()
        {
            var tutorialCheckPoint = _tutorialCheckPoint ?? GetTutorialCheckPoint();

            if (!tutorialCheckPoint.HasValue)
            {
                return null;
            }

            if (!tutorialCheckPoint.Value.HasCheckPoint)
            {
                return null;
            }

            var checkPointType =
                _gameService.IsPlayerDeadInMission ? CheckPointType.MissionFailed : CheckPointType.Start;

            bool exists = tutorialCheckPoint.Value.TutorialCheckPointDatas.Any(x => x.type.Equals(checkPointType));

            if (!exists)
            {
                return null;
            }

            return tutorialCheckPoint.Value.TutorialCheckPointDatas.FirstOrDefault(x => x.type.Equals(checkPointType));
        }

        #endregion

        public void StartTutorial()
        {
        }
    }

    public record TutorialData
    {
        public bool IsCompleted { get; set; }
    }

    public struct TutorialCheckPoint
    {
        public List<TutorialCheckPointData> TutorialCheckPointDatas;
        public bool HasCheckPoint;
    }

    public struct TutorialCheckPointData
    {
        public CheckPointType type;
        public string Position;
        public string StartStepName;
        public List<InitialSteps> InitialSteps;
    }

    [Serializable]
    public struct InitialSteps
    {
        public string stepName;
        public bool isAwaited;
    }

    public enum CheckPointType
    {
        Start,
        MissionFailed,
    }
}
