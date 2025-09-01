using System.Globalization;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.PopupSystem;
using Interfaces;
using GameCore.Scriptables;
using GameCore.Wave;
using VContainer;
using UI.Game.Architectural;
using UI.Game.InGame.TopStats.Constants;
using UnityEngine;
using System.Collections;
using _Scripts.GameCore.Vibration.Constants;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace UI.Game.InGame.TopStats
{
    public class InGameTopStatsUI : Content
    {
        #region Serialized Fields

        [SerializeField] private AllCharacterResources allCharacterResources;
        [SerializeField] private SpriteDatabase spriteDatabase;
        [SerializeField] private CanvasGroup parentCanvasGroup;

        #endregion


        #region Private Fields

        private PlayerSkillController _playerSkillController;
        private PlayerStatusController _playerStatusController;
        private ILevelService _levelService;
        private WaveManager _waveManager;
        private VibrationManager _vibrationManager;
        private AlertManager _alertManager;
        private PlayerController _playerController;
        private PopupManager _popupManager;
        private GameInventoryManager _gameInventoryManager;
        private bool _isInitialized;
        private bool _isPaused;
        private IEnergyService _energyService;
        private int _maxEnergy = 30;
        private Coroutine _energyTimerCoroutine;
        private IObjectResolver _resolver;

        #endregion


        #region Unity Methods

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();

            if (_energyTimerCoroutine != null)
            {
                StopCoroutine(_energyTimerCoroutine);
            }
        }

        #endregion


        #region Private Methods

        [Inject]
        private void Initialize(PlayerController playerController, PlayerSkillController playerSkillController,
            PlayerStatusController statusController, ILevelService levelService, WaveManager waveManager,
            IInventoryManager inventoryManager, PopupManager popupManager, IEnergyService energyService,
            IObjectResolver resolver, AlertManager alertManager, VibrationManager vibrationManager)
        {
            _gameInventoryManager = inventoryManager as GameInventoryManager;
            _playerSkillController = playerSkillController;
            _playerStatusController = statusController;
            _levelService = levelService;
            _waveManager = waveManager;
            _popupManager = popupManager;
            _energyService = energyService;
            _alertManager = alertManager;
            _vibrationManager = vibrationManager;
            _maxEnergy = _energyService.MaxEnergy;
            _playerController = playerController;
            _resolver = resolver;
            SubscribeToEvents();
            SetupPanel();

            _isInitialized = true;
        }

        private async void SetAvatarImage(string modelAddressableKey)
        {
            var character = allCharacterResources.GetCharacter(modelAddressableKey);
            var characterTopBarImage =
                await Addressables.LoadAssetAsync<Sprite>(character.CharacterTopBarImage).BindTo(gameObject);
            SetImage(InGameTopStatsConstants.AVATAR_IMAGE, characterTopBarImage);
        }

        private void OnResetSkill()
        {
            SetLevelText();
        }

        private void SetExperienceProgress(float progress, float maxValue)
        {
            AnimateSlider(InGameTopStatsConstants.EXP_PROGRESS, progress, maxValue);
        }

        private void OnWaveUpdated(Wave wave)
        {
            ActivateWaveArea(wave, true);
            SetActivity(true);
        }


        private void OpenShopPopup()
        {
            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
            _popupManager.OpenPopup(PopupConstants.PopupType.Shop);
        }


        private void OnWaveCompleted(Wave wave)
        {
            ActivateWaveArea(wave, false);
        }

        private void SubscribeToEvents()
        {
            if (_isInitialized)
            {
                return;
            }

            _playerSkillController.OnResetSkill += OnResetSkill;
            _playerStatusController.PurchasePodCountChanged += SetDropText;
            _levelService.WaveLevelChanged += SetLevelText;
            _levelService.WaveLevelSliderChanged += SetExperienceProgress;
            _waveManager.WaveUpdated += SetWaveText;
            _waveManager.WaveTimeRemainingUpdated += SetWaveTime;
            _waveManager.WaveUpdated += OnWaveUpdated;
            _waveManager.WaveCompleted += OnWaveCompleted;
            _gameInventoryManager.OnCoinsChanged += SetCoinText;
            _gameInventoryManager.OnGemsChanged += SetGemText;
            _energyService.OnEnergyChanged += SetEnergyText;
            _popupManager.OnTopBarStatus += SetEnabledTopBar;
            _playerController.OnSkinChanged += SetAvatarImage;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_isInitialized)
            {
                return;
            }

            _playerSkillController.OnResetSkill -= OnResetSkill;
            _playerStatusController.PurchasePodCountChanged -= SetDropText;
            _levelService.WaveLevelChanged -= SetLevelText;
            _levelService.WaveLevelSliderChanged -= SetExperienceProgress;
            _waveManager.WaveUpdated -= SetWaveText;
            _waveManager.WaveTimeRemainingUpdated -= SetWaveTime;
            _waveManager.WaveUpdated -= OnWaveUpdated;
            _waveManager.WaveCompleted -= OnWaveCompleted;
            _gameInventoryManager.OnCoinsChanged -= SetCoinText;
            _gameInventoryManager.OnGemsChanged -= SetGemText;
            _energyService.OnEnergyChanged -= SetEnergyText;
            _popupManager.OnTopBarStatus -= SetEnabledTopBar;
            _playerController.OnSkinChanged -= SetAvatarImage;
        }

        private void SetupPanel()
        {
            SetLevelText();
            SetDropText();
            OnClickListen(InGameTopStatsConstants.PAUSE_BUTTON, OnClickPauseButton, _resolver);
            OnClickListen(InGameTopStatsConstants.REFILL_ENERGY_BUTTON, OnClickRefillEnergyButton, _resolver);
            OnClickListen(InGameTopStatsConstants.COIN_SHOP_BUTTON, OpenShopPopup);
            OnClickListen(InGameTopStatsConstants.GEM_SHOP_BUTTON, OpenShopPopup);

            if (_energyService != null) SetEnergyText(_energyService.CurrentEnergy);
        }

        private void OnClickRefillEnergyButton()
        {
            if (_energyService.CurrentEnergy >= _maxEnergy)
            {
                _alertManager.CallAlert("Energy is already fully charged");
                return;
            }

            _popupManager.OpenPopup(PopupConstants.PopupType.EnergyRefill);
        }

        private void SetLevelText(int level = 0)
        {
            SetText(InGameTopStatsConstants.LEVEL_TEXT, level.ToString());
        }

        private void SetEnergyText(int energy)
        {
            var energyStr = $"{energy.ToString()}/{_maxEnergy}";

            SetText(InGameTopStatsConstants.ENERGY_TEXT, energyStr);

            if (_energyTimerCoroutine == null)
            {
                _energyTimerCoroutine = StartCoroutine(SetEnergyCooldownText());
            }
        }

        private IEnumerator SetEnergyCooldownText()
        {
            string timeSpanString;

            var energy = _energyService.CurrentEnergy;

            if (energy >= _maxEnergy)
            {
                timeSpanString = "MAX";
            }
            else
            {
                var timeSpan = _energyService.TimeLeftToNextEnergy;
                timeSpanString = timeSpan.TotalMinutes >= 1
                    ? $"{(int) timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}"
                    : $"{timeSpan.Seconds}";
            }

            SetText(InGameTopStatsConstants.ENERGY_COOLDOWN_TEXT, timeSpanString);
            yield return new WaitForSecondsRealtime(1);
            _energyTimerCoroutine = StartCoroutine(SetEnergyCooldownText());
        }


        private void OnClickPauseButton()
        {
            _popupManager.OpenPopup(PopupConstants.PopupType.Pause);
        }

        private void ActivateWaveArea(Wave wave, bool value)
        {
            SetGameObject(InGameTopStatsConstants.WAVE_AREA, value);
            //SetGameObject(InGameTopStatsConstants.SLIDER_PARENT, value);
        }

        private void SetDropText(int value = 0)
        {
            SetText(InGameTopStatsConstants.EXPERIENCE_TEXT, GetFormattedIconWithText(value));
        }

        private void SetCoinText(int value = 0)
        {
            SetText(InGameTopStatsConstants.COIN_TEXT, GetFormattedIconWithText(value));
        }

        private void SetGemText(int value = 0)
        {
            SetText(InGameTopStatsConstants.GEM_TEXT, GetFormattedIconWithText(value));
        }

        private void SetWaveText(Wave wave)
        {
            SetText(InGameTopStatsConstants.WAVE_TEXT, $"WAVE {wave.level}");
            SetWaveTime(wave.duration.ToString());
        }

        private void SetWaveTime(string waveTime)
        {
            SetText(InGameTopStatsConstants.WAVE_TIME_TEXT, waveTime.ToString(CultureInfo.InvariantCulture));
        }

        private void SetEnabledTopBar(bool isShow)
        {
            parentCanvasGroup.alpha = isShow ? 1 : 0;
            parentCanvasGroup.interactable = isShow;
            parentCanvasGroup.blocksRaycasts = isShow;
        }

        private string GetFormattedIconWithText(int value)
        {
            return $"{value}";
        }

        private void SetActivity(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        #endregion
    }
}
