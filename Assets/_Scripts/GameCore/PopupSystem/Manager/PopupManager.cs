using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.GameCore.Vibration.Constants;
using Cysharp.Threading.Tasks;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace GameCore.PopupSystem
{
    public class PopupManager : MonoBehaviour
    {
        #region Actions

        public event Action<bool> OnTopBarStatus;

        #endregion


        #region Serializable Fields

        [SerializeField] private PopupResources popupResources;
        [SerializeField] private Transform popupParent;

        #endregion

        #region Fields

        private readonly List<Tuple<PopupConstants.PopupType,Popup, AsyncOperationHandle>> _activePopups = new();
        private readonly List<PopupConstants.PopupType> _activePopupTypes = new();
        private Popup _popupInstance;
        private IObjectResolver _resolver;
        private IGameService _gameService;
        private VibrationManager _vibrationManager;
        private IAnalyticsService _analyticsService;

        #endregion

        #region Public Methods

        public async UniTask OpenPopup(PopupConstants.PopupType popupType, Action onOpenAction = null, Action onCloseAction = null)
        {
            if (IsPopupActive(popupType)) return;

            _activePopupTypes.Add(popupType);
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
            var assetReference = popupResources.GetPopupReference(popupType);
            var operationHandle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            var popupPrefab = await operationHandle;
            var popup = popupResources.GetPanelData(popupType);
            if (popupPrefab == null) return;
            _gameService.PauseGame();
            _popupInstance = Instantiate(popupPrefab, popupParent).GetComponent<Popup>();
            _popupInstance.Resolver = _resolver;
            _popupInstance.ClosePopupAction += () =>
            {
                ClosePopup(popupType);
                onCloseAction?.Invoke();
            };
            _activePopups.Add(new Tuple<PopupConstants.PopupType, Popup, AsyncOperationHandle>(popupType, _popupInstance, operationHandle));
            _popupInstance.OnOpenPopup();
            onOpenAction?.Invoke();
            OnTopBarStatus?.Invoke(popup.IsTopBarShow);
            SendPopupOpenedAnalytic(popupType);
        }

        public void ClosePopup(PopupConstants.PopupType popupType)
        {
            if (!IsPopupActive(popupType)) return;

            var tuple = _activePopups.Find(t => t.Item1.Equals(popupType));
            if (tuple != null)
            {
                _popupInstance = tuple.Item2;
                _activePopupTypes.Remove(popupType);
                Destroy(_popupInstance.gameObject);
                Addressables.Release(tuple.Item3);
                _activePopups.Remove(tuple);
                _gameService.ResumeGame();
                OnTopBarStatus?.Invoke(true);
            }
            
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        }

        public Task<T> GetActivePanel<T>(PopupConstants.PopupType popupType) where T : Popup
        {
            return !IsPopupActive(popupType)
                ? Task.FromResult<T>(null)
                : Task.FromResult(_activePopups.FirstOrDefault(tuple => tuple.Item1.Equals(popupType))?.Item2 as T);
        }


        public T GetPopup<T>(PopupConstants.PopupType popupType) where T : Component
        {
            var tuple = _activePopups.Find(t => t.Item1.Equals(popupType));
            return tuple?.Item2 as T;
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(IObjectResolver resolver)
        {
            _resolver = resolver;
            _gameService = resolver.Resolve<IGameService>();
            _analyticsService = resolver.Resolve<IAnalyticsService>();
            _vibrationManager = resolver.Resolve<VibrationManager>();
        }

        public bool IsPopupActive(PopupConstants.PopupType popupType)
        {
            return _activePopupTypes.Any(x => x.Equals(popupType));
        }
        
        private void SendPopupOpenedAnalytic(PopupConstants.PopupType popupType)
        {
            var popupName = popupType.ToString().ToLower();
            _analyticsService.LogEvent(new EventParameters<string>{ EventName = $"popup_{popupName}"});
        }

        #endregion
    }
}