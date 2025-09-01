using System;
using UnityEngine;

namespace GameCore.Player
{
    /// <summary>
    /// Handles animator audio events for player character
    /// </summary>
    public class PlayerAudioEventController : MonoBehaviour
    {
        private PlayerAudioController _playerAudioController;
        private void Awake()
        {
            _playerAudioController = GetComponentInParent<PlayerAudioController>();
        }
        
        public void OnFootStep()
        {
            _playerAudioController.PlayFootStep();
        }
        
        public void OnFootStepStrafe()
        {
            _playerAudioController.PlayFootStepStrafe();
        }
    }
}
