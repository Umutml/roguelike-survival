using System;
using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    public class FlameThrowerAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource flamethrowerAudioSource;
        [SerializeField] private AudioClip startClip;

        public void StartFiringSound()
        {
            flamethrowerAudioSource.PlayOneShot(startClip);
            flamethrowerAudioSource.Play();
        }

        public void StopFiringSound()
        {
            flamethrowerAudioSource.Stop();
        }
    }
}
