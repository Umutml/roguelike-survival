using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Utilities;
using DG.Tweening;
using GameCore.Player;
using GameCore.Tutorial;
using UI.Game.Architectural;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Game.InGame.CarUI
{
    public class CarUIPanel : Content
    {
        #region Serializable Fields

        [SerializeField] private GameObject abilityPanel;
        [SerializeField] private GameObject fingerPointToCarExit;
        [SerializeField] private CanvasGroup joystickCanvasGroup;

        #endregion


        #region Fields

        private ItemPicker _itemPicker;
        private VibrationManager _vibrationManager;
        private PlayerCarController _playerCarController;
        private PlayerController _playerController;
        private TimerInfoController _timerInfoController;

        private Slider _carStatuBar;
        private readonly float _sliderDuration = 2.5f;

        #endregion


        #region Unity Methods

        private void OnEnable()
        {
            if (_itemPicker != null) _itemPicker.OnCarPickup += SetCarButtonActivity;

            if (_playerCarController != null)
            {
                _playerCarController.CarExitedByForce += ExitCar;
                _playerCarController.CarExitButtonActivity += SetExitButton;
                _playerCarController.CarExitButtonFingerMarkActivity += SetExitButtonFingerActive;
            }

            if (_playerController != null)
            {
                _playerController.CutSceneEntered += DisableCarUI;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_itemPicker != null) _itemPicker.OnCarPickup -= SetCarButtonActivity;


            if (_playerCarController != null)
            {
                _playerCarController.CarExitedByForce -= ExitCar;
                _playerCarController.CarExitButtonActivity -= SetExitButton;
                _playerCarController.CarExitButtonFingerMarkActivity -= SetExitButtonFingerActive;
            }

            if (_playerController != null)
                _playerController.CutSceneEntered -= DisableCarUI;
        }

        #endregion


        #region Private Methods

        [Inject]
        private void Initialize(ItemPicker itemPicker, PlayerCarController playerCarController,
            PlayerController playerController,
            TimerInfoController timerInfoController, IObjectResolver resolver)
        {
            _itemPicker = itemPicker;
            _vibrationManager = resolver.Resolve<VibrationManager>();
            _playerCarController = playerCarController;
            _playerController = playerController;
            _timerInfoController = timerInfoController;


            _carStatuBar = GetSlider(CarUIPanelConstants.CAR_STATU_BAR);
            OnClickListen(CarUIPanelConstants.EXIT_CAR_BUTTON, ExitCar, resolver);
        }


        private void SetCarStatusBar(float value)
        {
            DOTween.To(() => _carStatuBar.value, x => _carStatuBar.value = x, value, _sliderDuration);
        }


        private void SetCarButtonActivity(bool isActive)
        {
            if (isActive)
                _timerInfoController.SetTimer(1.5f, EnterCar);
            else
                _timerInfoController.StopTimer();

            _itemPicker.CarController.CarEffectController.SetColorDoorParticle(isActive);
        }


        private void EnterCar()
        {
            if (_playerController == null)
            {
                LoggerNS.LogError("Handled: PlayerController is null in CarUIPanel EnterCar");
                return;
            }


            _playerController.PlayerMovementMode = PlayerMovementMode.Drive;
            if (_playerCarController.CarController == null)
            {
                LoggerNS.LogError("Handled: CarController is null in CarUIPanel EnterCar");
                return;
            }

            _carStatuBar.value = _playerCarController.CarController.CarStatusController.CurrentHealth /
                                 _playerCarController.CarController.CarStatusController.MaxHealth;
            abilityPanel.SetActive(false);
            SetGameObject(CarUIPanelConstants.ENTRY_CAR_BUTTON, false);
            SetGameObject(CarUIPanelConstants.IN_CAR_UI, true);

            _playerCarController.CarController.CarStatusController.OnChangeCarStatus += SetCarStatusBar;
            _playerCarController.CarController.CarStatusController.OnDeadCar += ExitCar;
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        }


        private void SetExitButton(bool isActive)
        {
            SetGameObject(CarUIPanelConstants.EXIT_CAR_BUTTON, isActive);
        }

        private void SetExitButtonFingerActive(bool isActive)
        {
            fingerPointToCarExit.SetActive(isActive);
        }

        private void ExitCar()
        {
            if (_playerController == null)
            {
                LoggerNS.LogError("Handled: PlayerController is null in CarUIPanel ExitCar");
                return;
            }

            if (_playerCarController.CarController == null)
            {
                LoggerNS.LogError("Handled: CarController is null in CarUIPanel ExitCar");
                return;
            }

            if (_playerCarController.CarController != null)
            {
                //_isNearCar = !_playerCarController.CarController.IsDead;
            }

            abilityPanel.SetActive(true);
            SetGameObject(CarUIPanelConstants.IN_CAR_UI, false);
            if (joystickCanvasGroup)
            {
                joystickCanvasGroup.alpha = 1;
                joystickCanvasGroup.interactable = true;
                joystickCanvasGroup.blocksRaycasts = true;
            }

            _playerController.PlayerMovementMode = PlayerMovementMode.Walk;
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        }

        private void DisableCarUI()
        {
            SetGameObject(CarUIPanelConstants.IN_CAR_UI, false);
            if (!joystickCanvasGroup) return;
            joystickCanvasGroup.alpha = 0;
            joystickCanvasGroup.interactable = false;
            joystickCanvasGroup.blocksRaycasts = false;
        }

        #endregion
    }
}