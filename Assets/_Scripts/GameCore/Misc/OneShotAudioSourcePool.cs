using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Misc
{
    public class OneShotAudioSourcePool
    {
        private List<AudioSource> audioSourcePool = new List<AudioSource>();
        private AudioSource _mainSource;

        public OneShotAudioSourcePool(AudioSource mainSource)
        {
            _mainSource = mainSource;
            audioSourcePool.Add(mainSource);
        }

        public void PlayOneShotSound(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            AudioSource source = GetFreeAudioSource();
            source.volume = volume;
            source.pitch = pitch;
            source.clip = clip;
            source.loop = false;
            source.Play();
        }

        public AudioSource GetFreeAudioSource()
        {
            // Find or create an AudioSource with the child group assigned
            AudioSource source = audioSourcePool.Find(s => !s.isPlaying);
            if (source == null)
            {
                source = _mainSource.gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = _mainSource.outputAudioMixerGroup;
                source.volume = _mainSource.volume;
                audioSourcePool.Add(source);
            }
            return source;
        }
    }
}
