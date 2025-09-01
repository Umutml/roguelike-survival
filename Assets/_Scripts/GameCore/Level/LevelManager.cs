using System;
using _Utilities;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using VContainer;
using Cathei.LinqGen;
using GameCore.Health;

namespace _Scripts.GameCore.Level
{
    public class LevelManager : MonoBehaviour, ILevelService
    {
        #region Actions

        public event Action WaveLevelFailed;
        public event Action<int> WaveLevelChanged;
        public event Action<float, float> WaveLevelSliderChanged;

        #endregion

        #region Serialized Fields

        [SerializeField] private LevelData levelData;

        #endregion

        #region Private Fields

        private IObjectResolver _resolver;
        private WaveManager _waveManager;
        private LevelDetails _currentLevelDetails;
        private int _xpValue;
        private PlayerStatusController _playerStatusController;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            var levelInfo = GetNextLevelInfo();
            _currentLevelDetails = GetLevelDetailsByLevel(levelInfo.Level);
            _xpValue = levelInfo.XpValue;
        }

        private void OnEnable()
        {
            _waveManager.WaveCompleted += OnWaveCompleted;
            _playerStatusController.XpCountChanged += OnChanged;
        }

        private void OnDestroy()
        {
            _waveManager.WaveCompleted -= OnWaveCompleted;
            _playerStatusController.XpCountChanged -= OnChanged;
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(IObjectResolver resolver)
        {
            _resolver = resolver;
            _waveManager = _resolver.Resolve<WaveManager>();
            _playerStatusController = _resolver.Resolve<PlayerStatusController>();
        }

        private void OnWaveCompleted(Wave wave)
        {
            OnChanged(10);
        }

        private void OnChanged(float xpValue)
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
                SaveLoadHelper.UpdateData<LevelInfo>(info =>
                {
                    info.Level = newLevelDetails.level;
                });

                WaveLevelChanged?.Invoke(_currentLevelDetails.level);
            }
            catch (Exception e)
            {
                Debug.LogError("Error in OnChanged: " + e.Message);
            }
        }

        private LevelDetails GetCurrentLevelDetails(float xpValue = 0)
        {
            return levelData.levels.Gen().Where(level => xpValue >= level.expPodToUnlock).LastOrDefault();
        }

        private LevelDetails GetNextLevelDetails(LevelDetails currentLevelDetails)
        {
            return levelData.levels.Gen().Where(level => currentLevelDetails.level < level.level).FirstOrDefault();
        }

        private LevelDetails GetLevelDetailsByLevel(int level)
        {
            return levelData.levels.Gen().Where(x => x.level.Equals(level)).FirstOrDefault();
        }

        private LevelInfo GetNextLevelInfo()
        {
            return SaveLoadHelper.TryLoadPersistentData<LevelInfo>();
        }

        #endregion
    }

    public class LevelInfo
    {
        public int Level;
        public int XpValue;
    }
}
