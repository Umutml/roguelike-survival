using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;
using UnityEngine.Audio;

namespace Managers
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        private const string MusicVolumeParam = "MusicVolume";
        private const string SfxVolumeParam = "SFXVolume";
        private const string ZombieFXVolumeParam = "ZombieFXVolume";
        private const string MasterVolume = "MasterVolume";
        private const float MinVolume = -80f;
        private const float MaxVolume = 0f;
        private const float DefaultZombieVolume = 1f;
        private CancellationTokenSource _mainCancellationTokenSource;
        private float _cachedMasterVolume;

        #region Serializable Fields

        [SerializeField] private AudioClipDatabase audioClipDatabase;
        [SerializeField] private AudioMixer audioMixer;

        [Header("Game Musics")]
        [SerializeField] private AudioSource musicAudioSource;

        [Header("One Shot Effects")]
        [SerializeField] private AudioSource oneShotAudioSource;

        [Header("Zombie Idle Sounds")]
        [SerializeField] private AudioSource zombieGroupAudioSource;

        #endregion

        #region Fields

        private Dictionary<string, AudioClip> clipDictionary;

        private bool isSoundsOn = true;

        private bool isZombieGroupSoundPlaying = false;
        private float _previousSFXVolume;
        private bool _isSfxMuted = false;

        #endregion

        #region Properties

        public bool IsSoundsOn => isSoundsOn;
        
        public AudioMixer MainMixer => audioMixer;

        #endregion

        #region Events

        public event Action<bool> OnSoundChanged;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _mainCancellationTokenSource = new CancellationTokenSource();
            InitializeClipDictionary();
        }

        private async void Start()
        {
            try
            {
                await UniTask.Delay(100,
                    cancellationToken: _mainCancellationTokenSource.Token); //make sure audio system is loaded
            }
            catch (OperationCanceledException e)
            {
                return;
            }

            await InitializeAudioSettings(_mainCancellationTokenSource.Token);
        }

        private void OnDestroy()
        {
            if (_mainCancellationTokenSource != null)
            {
                _mainCancellationTokenSource.Cancel();
            }

            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
                musicAudioSource.clip = null;
            }

            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.Stop();
                oneShotAudioSource.clip = null;
            }

            if (zombieGroupAudioSource != null)
            {
                zombieGroupAudioSource.Stop();
                zombieGroupAudioSource.clip = null;
            }

            if (audioMixer != null)
            {
                audioMixer.SetFloat(MusicVolumeParam, MinVolume);
                audioMixer.SetFloat(SfxVolumeParam, MinVolume);
                audioMixer.SetFloat(ZombieFXVolumeParam, MinVolume);
            }
        }

        #endregion

        #region Public Methods

        public void PlayOneShot(AudioSource audioSource, string clipName)
        {
            if (!isSoundsOn)
                return;

            if (clipDictionary.TryGetValue(clipName, out var clip))
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                LoggerNS.LogWarning($"Audio clip '{clipName}' not found!");
            }
        }

        public void PlayZombieGroupSound()
        {
            if (!isSoundsOn)
                return;

            if (isZombieGroupSoundPlaying)
                return;

            isZombieGroupSoundPlaying = true;
            audioMixer.SetFloat(ZombieFXVolumeParam, ConvertToMixerValue(DefaultZombieVolume));
        }

        public void StopZombieGroupSound()
        {
            if (!isSoundsOn)
                return;

            if (!isZombieGroupSoundPlaying)
                return;

            isZombieGroupSoundPlaying = false;
            audioMixer.SetFloat(ZombieFXVolumeParam, MinVolume);
        }

        #endregion

        #region Private Methods

        private async UniTask InitializeAudioSettings(CancellationToken token)
        {
            string soundState = PlayerPrefs.GetString("Sound", "On");
            float musicVolume = PlayerPrefs.GetFloat("MusicSliderValue", 1f);
            float sfxVolume = PlayerPrefs.GetFloat("SfxSliderValue", 1f);

            isSoundsOn = soundState.Equals("On");

            // Try setting volumes multiple times if needed
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                SetMusicVolume(musicVolume);
                SetSfxVolume(sfxVolume);

                float currentSfxVolume;
                if (audioMixer.GetFloat(SfxVolumeParam, out currentSfxVolume))
                {
                    float expectedValue = isSoundsOn ? ConvertToMixerValue(sfxVolume) : MinVolume;
                    if (Mathf.Abs(currentSfxVolume - expectedValue) <= 0.01f)
                    {
                        break; // Volume set successfully
                    }
                }

                try
                {
                    await UniTask.Delay(50, cancellationToken: token);
                }
                catch (OperationCanceledException e)
                {
                    return;
                }
            }

            if (isZombieGroupSoundPlaying)
            {
                audioMixer.SetFloat(ZombieFXVolumeParam, ConvertToMixerValue(DefaultZombieVolume));
            }
            else
            {
                audioMixer.SetFloat(ZombieFXVolumeParam, MinVolume);
            }
        }

        private void InitializeClipDictionary()
        {
            clipDictionary = new Dictionary<string, AudioClip>();
            foreach (var clipData in audioClipDatabase.audioClips)
            {
                if (!clipDictionary.ContainsKey(clipData.clipName) && clipData.clip != null)
                {
                    clipDictionary[clipData.clipName] = clipData.clip;
                }
            }
        }

        private void SetMusicVolume(float normalizedVolume)
        {
            float mixerValue = isSoundsOn ? ConvertToMixerValue(normalizedVolume) : MinVolume;
            audioMixer.SetFloat(MusicVolumeParam, mixerValue);
        }

        private void SetSfxVolume(float normalizedVolume)
        {
            float mixerValue = isSoundsOn ? ConvertToMixerValue(normalizedVolume) : MinVolume;
            bool success = audioMixer.SetFloat(SfxVolumeParam, mixerValue);
            _previousSFXVolume = mixerValue;

            if (!success)
            {
                LoggerNS.LogError(
                    $"Failed to set SFX volume. Parameter '{SfxVolumeParam}' might not exist in the mixer or might not be exposed.");
                return;
            }

            float currentValue;
            if (audioMixer.GetFloat(SfxVolumeParam, out currentValue))
            {
                if (Mathf.Abs(currentValue - mixerValue) > 0.01f)
                {
                    LoggerNS.LogWarning(
                        $"SFX volume verification failed. Attempted to set {mixerValue}dB but got {currentValue}dB");
                }
            }
        }

        private float ConvertToMixerValue(float normalizedVolume)
        {
            if (normalizedVolume <= 0)
                return MinVolume;

            //convert to logarithmic value for audio mixer
            return Mathf.Clamp(20f * Mathf.Log10(normalizedVolume), MinVolume, MaxVolume);
        }

        private async UniTask TransitionMusicVolume(float targetVolume, float duration, CancellationToken token)
        {
            float currentVolume = 0f;
            audioMixer.GetFloat(MusicVolumeParam, out currentVolume);

            float elapsedTime = 0f;
            float startVolume = currentVolume;

            while (!token.IsCancellationRequested && elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float newVolume = Mathf.Lerp(startVolume, targetVolume, t);
                audioMixer.SetFloat(MusicVolumeParam, newVolume);
                await UniTask.Yield();
            }

            if (!token.IsCancellationRequested)
                return;

            audioMixer.SetFloat(MusicVolumeParam, targetVolume);
        }

        #endregion

        #region IAudioService Members

        public void PlayOneShot(string clipName, float volumeScale = 1)
        {
            if (!isSoundsOn)
                return;

            if (clipDictionary.TryGetValue(clipName, out var clip))
            {
                oneShotAudioSource.PlayOneShot(clip, volumeScale);
            }
            else
            {
                LoggerNS.LogWarning($"Audio clip '{clipName}' not found!");
            }
        }

        public async void PlayMusic(string musicType)
        {
            try
            {
                // Fade out current music
                await TransitionMusicVolume(MinVolume, 2f, _mainCancellationTokenSource.Token);

                if (_mainCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                // Change clip and play
                var musicClip = await audioClipDatabase.GameMusics.GetMusic(musicType);

                if (_mainCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                musicAudioSource.clip = musicClip;
                musicAudioSource.Play();

                // Fade in to saved volume
                float savedVolume = PlayerPrefs.GetFloat("MusicSliderValue", 1f);
                float targetVolume = isSoundsOn ? ConvertToMixerValue(savedVolume) : MinVolume;
                await TransitionMusicVolume(targetVolume, 2f, _mainCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Cleanup if cancelled
                if (musicAudioSource != null)
                {
                    musicAudioSource.Stop();
                    musicAudioSource.clip = null;
                }
            }
        }

        public void SoundStateChanger(string targetValue, bool onlyMusic = false, bool onlySfx = false)
        {
            isSoundsOn = targetValue.Equals("On");

            if (onlyMusic)
            {
                SetMusicVolume(PlayerPrefs.GetFloat("MusicSliderValue", isSoundsOn ? 1 : 0));
            }
            else if (onlySfx)
            {
                SetSfxVolume(PlayerPrefs.GetFloat("SfxSliderValue", isSoundsOn ? 1 : 0));
            }
            else
            {
                SetMusicVolume(PlayerPrefs.GetFloat("MusicSliderValue", isSoundsOn ? 1 : 0));
                SetSfxVolume(PlayerPrefs.GetFloat("SfxSliderValue", isSoundsOn ? 1 : 0));
            }

            OnSoundChanged?.Invoke(isSoundsOn);
        }

        public void SetMusicSoundVolumeMultiplier(float volume)
        {
            if (volume == 0)
            {
                float sfxVolume = 0f;
                audioMixer.GetFloat(SfxVolumeParam, out sfxVolume);
                if (sfxVolume <= MinVolume) SoundStateChanger("Off");
            }
            else
            {
                SoundStateChanger("On", true);
            }

            SetMusicVolume(volume);
            PlayerPrefs.SetFloat("MusicSliderValue", volume);
            PlayerPrefs.Save();
        }

        public void SetSfxSoundVolumeMultiplier(float volume)
        {
            if (volume == 0)
            {
                float musicVolume = 0f;
                audioMixer.GetFloat(MusicVolumeParam, out musicVolume);
                if (musicVolume <= MinVolume) SoundStateChanger("Off");
            }
            else
            {
                SoundStateChanger("On", false, true);
            }

            SetSfxVolume(volume);
            PlayerPrefs.SetFloat("SfxSliderValue", volume);
            PlayerPrefs.Save();
        }

        public void ToggleSFXMute(bool isMuted)
        {
            if(isMuted == _isSfxMuted) //prevent being muted twice
                return;
            
            _isSfxMuted = isMuted;
            
            if (isMuted)
            {
                audioMixer.GetFloat(SfxVolumeParam, out float currentVolume);
                _previousSFXVolume = currentVolume;
                audioMixer.SetFloat(SfxVolumeParam, MinVolume);
            }
            else
            {
                audioMixer.SetFloat(SfxVolumeParam, _previousSFXVolume);
            }
            
            if (isMuted)
            {
                zombieGroupAudioSource.Pause();
            }
            else
            {
                zombieGroupAudioSource.UnPause();
            }
        }
        
        public void MuteAllSounds()
        {
            audioMixer.GetFloat(MasterVolume, out _cachedMasterVolume);
            audioMixer.SetFloat(MasterVolume, MinVolume);
        }

        public void UnmuteAllSounds()
        {
            audioMixer.SetFloat(MasterVolume, _cachedMasterVolume);
        }

        #endregion
    }
}
