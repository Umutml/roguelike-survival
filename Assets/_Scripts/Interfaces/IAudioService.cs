using UnityEngine.Audio;

namespace Interfaces
{
    public interface IAudioService
    {
        public void PlayOneShot(string clipName, float volumeScale = 1);
        public void PlayMusic(string musicType); 
        public void SoundStateChanger(string targetValue,bool onlyMusic = false,bool onlySfx = false);
        public void SetMusicSoundVolumeMultiplier(float volume);
        public void SetSfxSoundVolumeMultiplier(float volume);
        public void ToggleSFXMute(bool isMuted);
        public void MuteAllSounds();
        public void UnmuteAllSounds();
        
        public AudioMixer MainMixer { get; }
    }
}
