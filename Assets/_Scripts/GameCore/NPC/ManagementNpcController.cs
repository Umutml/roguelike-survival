using System;
using System.Collections.Generic;
using _Scripts.Utilities;
using _Utilities;
using GameCore.DynamicGridObstacle;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.NPC;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using Utilities;
using VContainer;

namespace _Scripts.GameCore.NPC
{
    public class ManagementNpcController : NpcBase
    {
        #region Actions

        public event Action OnStartManagement;
        public event Action OnCompleteManagement;
        public event Action<string> OnShowIndicator;
        public event Action OnDisableIndicator;

        #endregion

        #region Serialized Fields

        [SerializeField] private List<ManagementNpcPosition> managementNpcPositions;
        [SerializeField] private List<GameObject> disableObjects;
        [SerializeField] private ManagementNpcData managementNpcData;
        [SerializeField] private Transform parentTransform;
        [SerializeField] private PlayerStatusController playerStatusController;

        #endregion

        #region Private Fields

        private const string IndicatorName = "CommenderNPC";

        private PlayerSkillController _playerSkillController;
        private WaveManager _waveManager;
        private Animator _animator;
        private IInventoryManager _inventoryManager;
        private TimerInfoController _timerInfoController;
        private IAudioService _audioService;

        #endregion

        #region Properties

        public bool IsProgress { get; private set; }

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            SetRandomPosition();
            ResetManagement();
            if (!IsTutorialCompleted()) SetDisableObjects(false);
        }


        private void OnEnable()
        {
            TutorialService.TutorialCompleted += TutorialCompleted;
            _waveManager.WaveCompleted += OnWaveCompleted;
            TutorialSequenceController.SequenceFinished += ShowIndicatorInvoke;
        }

        private void OnDestroy()
        {
            _waveManager.WaveCompleted -= OnWaveCompleted;

            if (TutorialService != null)
                TutorialService.TutorialCompleted -= TutorialCompleted;

            TutorialSequenceController.SequenceFinished -= ShowIndicatorInvoke;
        }

        protected override void OnCompleteTimer()
        {
            if (IsInProgress()) return;

            _ = PopupManager.OpenPopup(PopupConstants.PopupType.Management);
            SetDisableObjects(true);
        }

        #endregion

        #region Public Methods

        public override void Execute(bool isActive)
        {
            if (IsInProgress()) return;

            base.Execute(isActive);
        }

        #endregion

        #region Private Methods

        public override void Init(IObjectResolver resolver)
        {
            base.Init(resolver);

            _playerSkillController = resolver.Resolve<PlayerSkillController>();
            _inventoryManager = resolver.Resolve<IInventoryManager>();
            _waveManager = resolver.Resolve<WaveManager>();
            _audioService = resolver.Resolve<IAudioService>();
        }

        private void ResetManagement()
        {
            if (!SaveLoadHelper.IsDataExists(nameof(ManagementStateData))) return;

            var data = LoadManagementStateData();
            data.IsInProgress = false;
            SaveManagementStateData(data);
        }

        public void StartManagement()
        {
            IsProgress = true;
            SetDisableObjects(false);
            playerStatusController.KillCount = 0;
            PersistManagementData();

            _audioService.PlayMusic("Wave");
            _waveManager.ActiveWaveIndex = 1;

            SendQuestStartedEvent();
            OnDisableIndicator?.Invoke();
            OnStartManagement?.Invoke();
        }

        private void SendQuestStartedEvent()
        {
            IAnalyticsService.LogEvent(new EventParameters<string>
            {
                EventName = "the_old_man_quest_started",
                AdjustToken = AdjustNsEventTokens.TheOldManQuestStarted
            });
        }

        private void SendQuestCompletedEvent(int index)
        {
            var runGaneratorNumber = index + 1;
            IAnalyticsService.LogEventParameterArray("the_old_man_quest_completed",
                new Dictionary<string, object> {{"run_generator_number", runGaneratorNumber}});
        }

        private void SendQuestRejectedEvent()
        {
            // TODO: Cancel Button Not active on quest panel yet implement after that
            //_analyticsService.LogEvent(new EventParameters<string>
            //{
            //    EventName = "the_old_man_quest_canceled",
            //});
        }

        private async void CompleteManagement()
        {
            _audioService.PlayMusic("FreeRoam");
            var data = LoadManagementStateData();
            _inventoryManager.ModifyCurrencyBalance(new PurchaseDetails(
                managementNpcData.managementStateDetails[data.Index].softCurrency,
                PurchaseOptions.Coin));
            IsProgress = false;

            await PopupManager.OpenPopup(PopupConstants.PopupType.GameWin);
            _playerSkillController.OnResetSkillInvoke();
            SetDisableObjects(true);
            SendQuestCompletedEvent(data.Index);
            data.IsInProgress = false;
            data.Index += 1;

            SaveManagementStateData(data);
            SetRandomPosition();
            OnCompleteManagement?.Invoke();
        }

        private void OnWaveCompleted(Wave wave)
        {
            var index = LoadManagementStateData().Index;
            var managementStateDetails = managementNpcData.managementStateDetails[index];

            if (managementStateDetails.waveCount == wave.level) CompleteManagement();
        }

        private bool IsInProgress()
        {
            return LoadManagementStateData().IsInProgress;
        }

        private void PersistManagementData()
        {
            var data = LoadManagementStateData();
            data.IsInProgress = true;
            SaveManagementStateData(data);
        }

        private void SetRandomPosition()
        {
            if (managementNpcPositions is not {Count: > 0}) return;

            var managementNpcPosition = managementNpcPositions.PickRandom();
            parentTransform.position = managementNpcPosition.position;
            parentTransform.rotation = Quaternion.Euler(managementNpcPosition.rotation);

            if (IsTutorialCompleted())
            {
                ShowIndicatorInvoke();
            }
        }

        private void SetDisableObjects(bool isActive)
        {
            if (disableObjects is not {Count: > 0}) return;

            _animator.enabled = isActive;

            disableObjects.ForEach(x => x.SetActive(isActive));
        }

        private void TutorialCompleted()
        {
            SetDisableObjects(true);
        }

        public void ShowIndicatorInvoke()
        {
            OnShowIndicator?.Invoke(IndicatorName);
        }

        private bool IsTutorialCompleted()
        {
            try
            {
                return SaveLoadHelper.TryLoadPersistentData<TutorialData>().IsCompleted;
            }
            catch (Exception e)
            {
                LoggerNS.LogError("Error while checking if tutorial is completed: " + e);
                return true;
            }
        }


        public ManagementStateData LoadManagementStateData()
        {
            return SaveLoadHelper.TryLoadPersistentData<ManagementStateData>();
        }

        public (ManagementStateDetails, int) GetManagementStateDetails()
        {
            var index = LoadManagementStateData().Index;
            return (managementNpcData.managementStateDetails[index], index);
        }


        private void SaveManagementStateData(ManagementStateData data)
        {
            SaveLoadHelper.SaveData(data);
        }

        #endregion
    }

    [Serializable]
    public struct ManagementNpcPosition
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    public record ManagementStateData
    {
        public int Index { get; set; }
        public bool IsInProgress { get; set; }
    }
}
