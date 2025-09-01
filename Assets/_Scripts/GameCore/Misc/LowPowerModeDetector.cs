using System;
using Cysharp.Threading.Tasks;
using GameCore.PopupSystem;
using UnityEngine;
using VContainer;

namespace GameCore.Misc
{
    public class LowPowerModeDetector : MonoBehaviour
    {
        public static event Action OnLowPowerModeEnabled;

        private bool _wasLowPowerModeEnabled = false;
        private PopupManager _popupManager;
        private GameSceneSetupManager _gameSceneSetupManager;
        private static bool _initialPopupShown = false;

        private const int CHECK_INTERVAL = 15;

        [Inject]
        private void Construct(PopupManager popupManager, GameSceneSetupManager gameSceneSetupManager)
        {
            _gameSceneSetupManager = gameSceneSetupManager;
            _popupManager = popupManager;
        }

        async void Start()
        {
            _wasLowPowerModeEnabled = CheckLowPowerMode();

#if UNITY_IOS && !UNITY_EDITOR
            
            OnLowPowerModeEnabled += OnLowPowerModeEntered;
            
            //initial low power check
            if (!_initialPopupShown && _wasLowPowerModeEnabled)
            {
                _initialPopupShown = true;
                Debug.Log("Device is in Low Power Mode!");
                await _gameSceneSetupManager.SceneLoadTaskCompletionSource.Task;
                await UniTask.Delay(TimeSpan.FromSeconds(2));
                OnLowPowerModeEnabled?.Invoke();
            }
#endif
        }

        private void OnDestroy()
        {
#if UNITY_IOS && !UNITY_EDITOR
            OnLowPowerModeEnabled -= OnLowPowerModeEntered;
#endif
        }

        private void OnLowPowerModeEntered()
        {
            _popupManager?.OpenPopup(PopupConstants.PopupType.LowPower);
        }

        void Update()
        {
            if (Time.frameCount % CHECK_INTERVAL == 0)
            {
                bool isCurrentlyInLowPowerMode = CheckLowPowerMode();


                if (isCurrentlyInLowPowerMode && !_wasLowPowerModeEnabled)
                {
                    OnLowPowerModeEnabled?.Invoke();
                    Debug.Log("Device entered Low Power Mode!");
                }

                _wasLowPowerModeEnabled = isCurrentlyInLowPowerMode;
            }
        }

        private bool CheckLowPowerMode()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return UnityEngine.iOS.Device.lowPowerModeEnabled;
#else
            return false;
#endif
        }

        // Public method to check current state
        public bool IsInLowPowerMode()
        {
            return CheckLowPowerMode();
        }
    }
}
