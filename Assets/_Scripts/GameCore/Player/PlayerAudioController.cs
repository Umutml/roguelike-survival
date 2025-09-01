using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Misc;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using Utilities;
using VContainer;

namespace GameCore.Player
{
    public class PlayerAudioController : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private AudioSource playerAudioSource;
        [SerializeField] AssetReferenceT<AudioClip>[] footStepSoundReferences;
        [SerializeField] AssetReferenceT<AudioClip>[] footStepstrafeSoundReferences;

        #endregion

        #region Fields

        private AudioClip[] _footStepClips;
        private AudioClip[] _footStepstrafeClips;
        private OneShotAudioSourcePool _oneShotPool;
        private AudioMixer _mainMixer;

        #endregion

        #region Constructor

        [Inject]
        private void Construct (IAudioService audioService)
        {
            _mainMixer = audioService.MainMixer;
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _footStepClips = new AudioClip[footStepSoundReferences.Length];
            _footStepstrafeClips = new AudioClip[footStepstrafeSoundReferences.Length];
            _oneShotPool = new OneShotAudioSourcePool(playerAudioSource);
            playerAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("PlayerSFX")[0];
            LoadFootStepSounds();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a random footstep sound effect when called.
        /// </summary>
        /// <remarks>
        /// This method plays a random audio clip from the loaded footstep sounds
        /// </remarks>
        public void PlayFootStep()
        {
            if (_footStepClips == null || _footStepClips.Length == 0)
                return;

            AudioClip randomClip = _footStepClips.PickRandom();
            if (randomClip != null)
            {
                _oneShotPool.PlayOneShotSound(randomClip, 1f, UnityEngine.Random.Range(0.75f, 1f));
            }
        }

        public void PlayFootStepStrafe()
        {
            if (_footStepstrafeClips == null || _footStepstrafeClips.Length == 0)
                return;

            AudioClip randomClip = _footStepstrafeClips.PickRandom();
            if (randomClip != null)
            {
                _oneShotPool.PlayOneShotSound(randomClip, 0.6f, UnityEngine.Random.Range(0.6f, 1f));
            }
        }


        #endregion

        #region Private Methods

        private async void LoadFootStepSounds()
        {
            for (int i = 0; i < footStepSoundReferences.Length; i++)
            {
                try
                {
                    _footStepClips[i] = await footStepSoundReferences[i].LoadAssetAsync().BindTo(gameObject);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load footstep sound at index {i}: {e.Message}");
                }
            }
            
            for (int i = 0; i < footStepstrafeSoundReferences.Length; i++)
            {
                try
                {
                    _footStepstrafeClips[i] = await footStepstrafeSoundReferences[i].LoadAssetAsync().BindTo(gameObject);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load footstep strafe sound at index {i}: {e.Message}");
                }
            }
        }

        #endregion
    }
}
