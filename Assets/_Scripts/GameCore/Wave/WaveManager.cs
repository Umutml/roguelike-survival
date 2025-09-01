using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using _Utilities;
using Cathei.LinqGen;
using Firebase.Analytics;
using GameCore.Health;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Wave
{
    public class WaveManager : MonoBehaviour
    {
        #region Public Fields

        public event Action<Scriptables.Wave> WaveUpdated;
        public event Action<Scriptables.Wave> WaveCompleted;
        public event Action<Scriptables.Wave> OnWaveUIInitialized;
        public event Action<string> WaveTimeRemainingUpdated;
        public event Action<string> WaveStatusUpdated;
        public static bool EnableZombieBehaviourSetting { get; set; }
        public static int AttackerZombieProbability { get; set; }
        public static int WaitingZombieProbability { get; set; }
        public static int PetrolZombieProbability { get; set; }

        #endregion

        #region Serialize Fields

        [SerializeField] private WaveData waveSettings;
        [SerializeField] private PlayerStatusController playerStatusController;
        [SerializeField] private ManagementNpcController managementNpcController;

        #endregion

        #region Private Fields

        private const float STATUS_START_VALUE = 5f;

        private readonly WaitForSeconds _delayBetweenTicks = new(1);
        private int _activeWaveIndex;
        private IAnalyticsService _analyticsService;
        private IAudioService _audioService;

        #endregion

        #region Properties

        public Scriptables.Wave CurrentWave =>
            waveSettings.waves.Gen().Where(wave => wave.level == ActiveWaveIndex).FirstOrDefault() ??
            waveSettings.waves.Gen().First();

        public bool IsWaveActive { get; private set; }

        public int ActiveWaveIndex
        {
            get => _activeWaveIndex;
            set
            {
                _activeWaveIndex = value;
                var wave = waveSettings.waves.Gen().Where(wave => wave.level == ActiveWaveIndex).FirstOrDefault() ??
                           waveSettings.waves.Gen().Last();
                WaveUpdated?.Invoke(wave);
                StartCoroutine(MonitorWaveDuration(wave));
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator MonitorWaveDuration(Scriptables.Wave wave)
        {
            if (!SaveLoadHelper.TryLoadRuntimeData<ManagementStateData>().IsInProgress)
            {
                WaveTimeRemainingUpdated?.Invoke("-");
                yield break;
            }

            IsWaveActive = true;

            var waveTime = wave.duration;
            var currentTime = 0;

            while (currentTime < waveTime)
            {
                yield return _delayBetweenTicks;
                currentTime++;
                UpdateWaveStatus(waveTime - currentTime);
                WaveTimeRemainingUpdated?.Invoke((waveTime - currentTime).ToString());
            }

            WaveCompleted?.Invoke(wave);
            WaveStatusUpdated?.Invoke("Wave Completed");
            SendWaveCompletedEvent(wave.level);

            yield return _delayBetweenTicks;
            IsWaveActive = false;
            IncreaseWaveIndex();
        }

        private void UpdateWaveStatus(float waveValue)
        {
            if (waveValue > STATUS_START_VALUE) return;

            WaveStatusUpdated?.Invoke(waveValue.ToString(CultureInfo.InvariantCulture));
        }

        // Sends analytic event for each wave completion
        private void SendWaveCompletedEvent(int waveNumber)
        {
            var zombieKillCount = playerStatusController.KillCount;
            var podCount = playerStatusController.PurchasePodCount;

            _analyticsService.LogEventParameterArray("wave_completed", new Dictionary<string, object>
            {
                { "wave_number", waveNumber },
                { "zombie_kill", zombieKillCount },
                { "pod_count", podCount }
            });
        }

        #endregion

        #region Public Methods

        [Inject]
        public void Initialize(IAnalyticsService analyticsService, IAudioService audioService)
        {
            _analyticsService = analyticsService;
            _audioService = audioService;
        }

        public void IncreaseWaveIndex()
        {
            if (!managementNpcController.IsProgress)
            {
                return;
            }

            ActiveWaveIndex++;
        }

        public void ToggleWave(bool isWaveActive)
        {
            IsWaveActive = isWaveActive;
        }

        #endregion
    }
}