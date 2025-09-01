using System;
using System.Collections.Generic;
using _Scripts.Utilities;
using GameCore.Health;
using GameCore.Scriptables;
using UnityEngine;
using Cathei.LinqGen;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Wave;
using Interfaces;
using VContainer;

namespace GameCore.Level
{
    public class WaveLevelManager : MonoBehaviour, ILevelService
    {
        #region Public Fields

        public event Action<int> WaveLevelChanged;
        public event Action<float, float> WaveLevelSliderChanged;

        public event Action WaveLevelFailed;

        #endregion

        #region Serialize Fields

        [SerializeField] private PlayerStatusController playerStatusController;
        [SerializeField] private ObjectiveManager objectiveManager;
        [SerializeField] private WaveLevelData waveLevelData;

        #endregion

        #region Private Fields

        private PlayerSkillController _playerSkillController;
        private LevelDetails _currentLevelDetails;
        private IAnalyticsService _analyticsService;
        private PopupManager _popupManager;
        private WaveManager _waveManager;
        private int _xpValue;

        #endregion

        #region Properties

        public LevelDetails CurrentLevelDetails => _currentLevelDetails;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _currentLevelDetails = GetCurrentLevelDetails();
        }

        private void OnEnable()
        {
            playerStatusController.WaveXpCountChanged += OnChanged;
            playerStatusController.Died += OnLevelFailed;
            objectiveManager.OnObjectiveComplete += ResetLevel;
        }

        private void OnDestroy()
        {
            playerStatusController.WaveXpCountChanged -= OnChanged;
            playerStatusController.Died -= OnLevelFailed;
            objectiveManager.OnObjectiveComplete -= ResetLevel;
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(PlayerSkillController playerSkillController, IAnalyticsService analyticsService,
            PopupManager popupManager, WaveManager waveManager)
        {
            _playerSkillController = playerSkillController;
            _analyticsService = analyticsService;
            _popupManager = popupManager;
            _waveManager = waveManager;

            _playerSkillController.OnResetSkill += ResetLevel;
        }

        private void ResetLevel()
        {
            _currentLevelDetails = GetCurrentLevelDetails();
            _xpValue = 0;
            WaveLevelSliderChanged?.Invoke(0,
                GetNextLevelDetails(_currentLevelDetails).expPodToUnlock - _currentLevelDetails.expPodToUnlock);
            WaveLevelChanged?.Invoke(_currentLevelDetails.level);
        }

        private async void OnChanged(float xpValue)
        {
            try
            {
                _xpValue += (int) xpValue;
                var newLevelDetails = GetCurrentLevelDetails(_xpValue);
                var nextLevelDetails = GetNextLevelDetails(newLevelDetails);
                WaveLevelSliderChanged?.Invoke(_xpValue - newLevelDetails.expPodToUnlock,
                    nextLevelDetails.expPodToUnlock - newLevelDetails.expPodToUnlock);
                if (newLevelDetails.level <= _currentLevelDetails.level) return;
                _currentLevelDetails = newLevelDetails;
                SendLevelUpAnalytics(_currentLevelDetails.level);
                await _popupManager.OpenPopup(PopupConstants.PopupType.LevelUp,
                    () => WaveLevelSliderChanged?.Invoke(1, 1),
                    () => WaveLevelSliderChanged?.Invoke(0, 1));
                WaveLevelChanged?.Invoke(_currentLevelDetails.level);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in OnChanged: " + e.Message);
            }
        }

        private void SendLevelUpAnalytics(int level)
        {
            _analyticsService.LogEventParameterArray("player_lvl_up",
                new Dictionary<string, object>
                {
                    {"level_number", level},
                    {"pod_to_level_up", _currentLevelDetails.expPodToUnlock}
                });
        }

        private LevelDetails GetCurrentLevelDetails(float podCount = 0)
        {
            return waveLevelData.levels.Gen().Where(level => podCount >= level.expPodToUnlock).LastOrDefault();
        }

        private LevelDetails GetNextLevelDetails(LevelDetails currentLevelDetails)
        {
            return waveLevelData.levels.Gen().Where(level => currentLevelDetails.level < level.level).FirstOrDefault();
        }

        private LevelDetails GetPreviousLevelDetails(LevelDetails currentLevelDetails)
        {
            return waveLevelData.levels.Gen().Where(level => currentLevelDetails.level > level.level).FirstOrDefault();
        }

        private void OnLevelFailed(DamageSource damageSource)
        {
            LoggerNS.Log("Level Failed");
            //_gameService.PauseGame();  not required since death popup will pause the game anyway

            if (playerStatusController.IsRefillActive && _waveManager.IsWaveActive)
            {
                _popupManager.OpenPopup(PopupConstants.PopupType.Refill);
                playerStatusController.IsRefillActive = false;
            }
            else
            {
                _popupManager.OpenPopup(PopupConstants.PopupType.GameLose);
            }

            WaveLevelFailed?.Invoke();
        }

        #endregion
    }
}
