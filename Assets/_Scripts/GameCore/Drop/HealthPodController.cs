using _Scripts.Utilities;
using GameCore.Drop;
using GameCore.Health;
using Managers;
using UnityEngine;
using VContainer;

namespace GameCore.Drop
{
    public class HealthPodController : FixedDropBase
    {
        public override void Use()
        {
            base.Use();
            var playerStatusController = Resolver.Resolve<PlayerStatusController>();
            AudioManager.PlayOneShot(oneShotAudioKey);
            if (playerStatusController == null)
            {
                LoggerNS.LogError("HealthPodController: Component is not PlayerStatusController");
                return;
            }

            playerStatusController.AdjustHealth((int)_value);
        }
    }
}