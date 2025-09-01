using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore
{
    public class GameSceneSetupManager : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private string[] maps;

        #endregion

        #region Fields

        private UniTask _task;
        private ISceneLoadService _sceneLoadService;
        private CarManager _carManager;

        #endregion

        #region Properties

        public UniTaskCompletionSource<bool> SceneLoadTaskCompletionSource { get; private set; } = new();

        #endregion

        #region Unity Methods

        [Inject]
        private async void Init(ISceneLoadService sceneLoadService, PlayerSkillController playerSkillController,
            CarManager carManager, GridManager gridManager, IGameService gameService, IObjectResolver resolver)
        {
            ObjectManager.Resolver = resolver;
            _sceneLoadService = sceneLoadService;
            await _sceneLoadService.Load(maps[0], false);
            await gridManager.GridSystemInitialized.Task;
            _sceneLoadService.ToggleLoadingScreen(false);
            _sceneLoadService.ToggleSplashScreen(false);
            canvasGroup.alpha = 1;
            SceneLoadTaskCompletionSource.TrySetResult(true);
            _carManager = carManager;
            await Task.Delay(900);
            _carManager.Setup();
            playerSkillController.ConfigureCharacterMetaSkills();
            gameService.IsPlayerDeadInMission = false;
        }

        private void OnDestroy()
        {
            ObjectManager.Resolver = null;
        }


        public void RestartLevel()
        {
            _sceneLoadService.UnloadLastWithCount(2);
        }

        #endregion
    }
}
