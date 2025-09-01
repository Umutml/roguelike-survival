using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace GameCore.Player
{
    public class MeleeAttackAnimationEventHandler : MonoBehaviour
    {
            private PlayerController _playerController;

            [Inject]
            public void Initialize(PlayerController playerController)
            {
                _playerController = playerController;
            }

            public void HandleWeaponHit()
            { 
                _playerController.OnMeleeAttackHappened();
            }
            
            public void HandleWeaponSwingStart()
            {
                _playerController.OnMeleeSwingStarted();
            }
            
            public void HandleWeaponSwingEnd()
            {
                _playerController.OnMeleeSwingEnded();
            }
    }
}
