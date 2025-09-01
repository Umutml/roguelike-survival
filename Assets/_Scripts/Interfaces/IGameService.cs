using System;

namespace Interfaces
{
    public interface IGameService
    {
        public bool IsPlayerDeadInMission { get; set; }
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public void RestartLevel();

        public void GoToMainMenu();

        public void PauseGame();

        public void ResumeGame();
    }
}