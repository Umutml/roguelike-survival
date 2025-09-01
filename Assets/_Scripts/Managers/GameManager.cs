using System;
using System.Globalization;
using _Scripts.Utilities;
using Interfaces;
using UnityEngine;
using VContainer;

namespace Managers
{
    public class GameManager : MonoBehaviour, IGameService
    {
        #region Actions

        public event Action OnGamePaused;
        public event Action OnGameResumed;

        #endregion

        #region GameState enum

        public enum GameState
        {
            MainMenu,
            InGame,
            Paused,
            GameOver
        }

        #endregion

        #region Fields

        private GameState _currentGameState;
        private ILevelService _levelService;
        private ISceneLoadService _sceneLoadService;
        private IAnalyticsService _analyticsService;
        private DateTime _pauseDateTime;
        private DateTime _resumeDateTime;
        private float _pauseDuration;
        private IAudioService _audioService;

        #endregion

        #region Properties

        public GameState CurrentGameState
        {
            get => _currentGameState;
            private set
            {
                if (_currentGameState != value)
                {
                    _currentGameState = value;
                    OnGameStateChanged();
                }
            }
        }

        public bool IsPlayerDeadInMission { get; set; }

        #endregion

        #region Private Methods

        private void OnGameStateChanged()
        {
            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    break;
                case GameState.InGame:
                    break;
                case GameState.Paused:
                    break;
                case GameState.GameOver:
                    break;
            }
        }

        #endregion

        [Inject]
        public void InjectSceneLoadService(ISceneLoadService sceneLoadService, IAnalyticsService analyticsService, IAudioService audioService)
        {
            _audioService = audioService;
            _sceneLoadService = sceneLoadService;
            _analyticsService = analyticsService;
        }

        public async void RestartLevel()
        {
            _sceneLoadService.ToggleLoadingScreen(true);
            await _sceneLoadService.UnloadLastWithCount(2); //gamescene + mapscene
            ReleaseMemory();
            await _sceneLoadService.Load("GameScene");
            ResumeGame();
            //loading screen is toggled off in gamescenesetupmanager after map load
        }



        public async void GoToMainMenu()
        {
            _sceneLoadService.ToggleLoadingScreen(true);
            await _sceneLoadService.UnloadLastWithCount(2);
            ReleaseMemory();
            await _sceneLoadService.Load("MainScene");
            ResumeGame();
            _sceneLoadService.ToggleLoadingScreen(false);
        }

        public void PauseGame()
        {
            OnGamePaused?.Invoke();
            Time.timeScale = 0;
            _audioService.ToggleSFXMute(true);
            // LogPauseEvent();
        }

        public void ResumeGame()
        {
            OnGameResumed?.Invoke();
            Time.timeScale = 1;
            _audioService.ToggleSFXMute(false);
            // LogResumeEvent();
        }

        #region GameStateEvents

        private void LogPauseEvent()
        {
            _pauseDateTime = DateTime.Now;
            _analyticsService.LogEvent(new EventParameters<string>
            {
                EventName = "app_paused",
                ParameterName = "time_paused",
                ParameterValue = _pauseDateTime.ToString(CultureInfo.InvariantCulture),
                AdjustToken = AdjustNsEventTokens.AppPaused
            });
        }

        private void ReleaseMemory()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void LogResumeEvent()
        {
            _resumeDateTime = DateTime.Now;
            _pauseDuration = (float)(_resumeDateTime - _pauseDateTime).TotalSeconds;
            _analyticsService.LogEvent(new EventParameters<float>
            {
                EventName = "app_unpaused",
                ParameterName = "time_paused_duration",
                ParameterValue = _pauseDuration,
                AdjustToken = AdjustNsEventTokens.AppUnpaused
            });
        }

        #endregion
    }
}