using _Scripts.Utilities;
using GameCore.Health;
using Managers;
using UnityEngine;
using VContainer;

namespace GameCore.Drop
{
    public class ArmorPodController : FixedDropBase
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

            playerStatusController.AdjustArmor(_value);
        }
    }
}