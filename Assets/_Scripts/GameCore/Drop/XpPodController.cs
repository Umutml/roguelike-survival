using _Scripts.Utilities;
using GameCore.Drop;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using GameCore.Wave;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Drop
{
    public class XpPodController : PickupDropBase
    {
        [SerializeField] private SpriteRenderer podSpriteRenderer;
        [SerializeField] private SpriteDatabase spriteDatabase;

        public override async void Initialize(int value, bool isHidden = false)
        {
            base.Initialize(value, isHidden);
            var sprite = await spriteDatabase.GetSpriteByType(value > 1 ? SpriteType.Xps : SpriteType.Xp);
            podSpriteRenderer.sprite = sprite;
        }

        public override void Use()
        {
            base.Use();

            var playerStatusController = Resolver.Resolve<PlayerStatusController>();
            if (playerStatusController == null)
            {
                LoggerNS.LogError("XpPodController: Component is not PlayerStatusController");
                return;
            }

            playerStatusController.AdjustXpValue((int)_value);
            if (Resolver.Resolve<WaveManager>().IsWaveActive)
            {
                Resolver.Resolve<LootDropManager>().StartTopBarAnimation(DropPodType.Xp, (int)_value);
            }
        }
    }
}