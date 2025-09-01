using _Scripts.GameCore.Vibration.Constants;
using _Utilities;
using GameCore.PopupSystem;
using GameCore.Tutorial;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.Game.InGame.Pause
{
    public class PausePopup : Popup
    {
        #region Serializable Fields

        [SerializeField] private PausePopupContent pausePopupContent;
        [SerializeField] private ToggleUIv2 soundToggle;
        [SerializeField] private ToggleUIv2 vibrationToggle;
        [SerializeField] private GameObject fpsObject;
        [SerializeField] private RectTransform fpsHandle;
        [SerializeField] private Slider MusicSlider;
        [SerializeField] private Slider SfxSlider;
        [SerializeField] private SkillsAcquiredContent skillsAcquiredContent;

        #endregion

        private IAudioService _audioService;
        private VibrationManager _vibrationManager;
        private int _currentFPSTarget;

        #region Public Methods

        public override void OnOpenPopup()
        {
            _vibrationManager = Resolver.Resolve<VibrationManager>();
            SetSettingsValue();
            pausePopupContent.Initialize(_vibrationManager, OnClosePopup, OnRestartCheckPoint, Resolver);
            pausePopupContent.SetRestartCheckpointButtonActive(SaveLoadHelper
                .TryLoadPersistentData<TutorialCheckPoint>().HasCheckPoint);
            _audioService = Resolver.Resolve<IAudioService>();
            skillsAcquiredContent.Initialize(Resolver);
#if UNITY_ANDROID
            fpsObject.SetActive(true);
            _currentFPSTarget = PlayerPrefs.GetInt("FPS", 30);
            fpsHandle.anchoredPosition = new Vector2(_currentFPSTarget == 30 ? -55 : 55, 0);
#endif
        }

        public void OnClickToggleButton(string settingType)
        {
            var toggle = settingType.Equals("Sound") ? soundToggle : vibrationToggle;
            var state = toggle.CurrentState.Equals("On") ? "Off" : "On";

            PlayerPrefs.SetString(settingType, state);

            toggle.CurrentState = state;

            if (settingType.Equals("Sound"))
            {
                _audioService.SoundStateChanger(state);

                switch (state)
                {
                    case "On":
                        MusicSlider.value = 1;
                        SfxSlider.value = 1;
                        break;
                    case "Off":
                        MusicSlider.value = 0;
                        SfxSlider.value = 0;
                        break;
                }
            }

            _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        }

#if UNITY_ANDROID
        public void OnClickFPSToggle()
        {
            _currentFPSTarget = _currentFPSTarget == 30 ? 60 : 30;
            fpsHandle.anchoredPosition = new Vector2(_currentFPSTarget == 30 ? -55 : 55, 0);
            Application.targetFrameRate = _currentFPSTarget;
            PlayerPrefs.SetInt("FPS", _currentFPSTarget);
        }
#endif

        #endregion


        #region Private Methods

        private void SetSettingsValue()
        {
            InitializeToggle("Sound", soundToggle);
            InitializeToggle("Vibration", vibrationToggle);
            InitializeSliders();
        }

        private void InitializeToggle(string key, ToggleUIv2 toggle, string defaultValue = "On")
        {
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, defaultValue);
            }

            toggle.SetStartState(PlayerPrefs.GetString(key));
        }


        private void InitializeSliders()
        {
            var isSoundsOn = PlayerPrefs.HasKey("Sound") && PlayerPrefs.GetString("Sound").Equals("On");
            MusicSlider.value = PlayerPrefs.HasKey("MusicSliderValue") ? PlayerPrefs.GetFloat("MusicSliderValue") :
                isSoundsOn ? 1 : 0;
            SfxSlider.value = PlayerPrefs.HasKey("SfxSliderValue") ? PlayerPrefs.GetFloat("SfxSliderValue") :
                isSoundsOn ? 1 : 0;

            soundToggle.CurrentState = isSoundsOn ? "On" : "Off";
            PlayerPrefs.SetString("Sound", isSoundsOn ? "On" : "Off");

            MusicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
            SfxSlider.onValueChanged.AddListener(OnSfxSliderValueChanged);
        }

        private void OnMusicSliderValueChanged(float newValue)
        {
            _audioService.SetMusicSoundVolumeMultiplier(newValue);

            if (newValue != 0)
            {
                soundToggle.CurrentState = "On";
                PlayerPrefs.SetString("Sound", "On");
            }
        }

        private void OnSfxSliderValueChanged(float newValue)
        {
            _audioService.SetSfxSoundVolumeMultiplier(newValue);

            if (newValue != 0)
            {
                soundToggle.CurrentState = "On";
                PlayerPrefs.SetString("Sound", "On");
            }
        }

        private void OnRestartCheckPoint()
        {
            Resolver.Resolve<CarManager>().Restart();
            Resolver.Resolve<IGameService>().RestartLevel();
        }


        private void OnClosePopup()
        {
            Resolver.Resolve<IGameService>().ResumeGame();
            MusicSlider.onValueChanged.RemoveAllListeners();
            SfxSlider.onValueChanged.RemoveAllListeners();

            PlayerPrefs.SetFloat("MusicSliderValue", MusicSlider.value);
            PlayerPrefs.SetFloat("SfxSliderValue", SfxSlider.value);

            ClosePopup();
        }

        #endregion
    }
}
