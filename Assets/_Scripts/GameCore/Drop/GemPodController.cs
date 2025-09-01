using _Scripts.Utilities;
using GameCore.Drop;
using GameCore.Inventory;
using GameCore.Scriptables;
using GameCore.Spawner;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Drop
{
    public class GemPodController : PickupDropBase
    {
        [SerializeField] private SpriteRenderer podSpriteRenderer;
        [SerializeField] private SpriteDatabase spriteDatabase;

        public override async void Initialize(int value, bool isHidden = false)
        {
            base.Initialize(value, isHidden);
            var sprite = await spriteDatabase.GetSpriteByType(value > 1 ? SpriteType.Gems : SpriteType.Gem);
            podSpriteRenderer.sprite = sprite;
        }

        public override void Use()
        {
            base.Use();
            var inventoryManager = Resolver.Resolve<IInventoryManager>();
            if (inventoryManager == null)
            {
                LoggerNS.LogError("GemPodController: Component is not IInventoryManager");
                return;
            }

            inventoryManager.ModifyCurrencyBalance(new PurchaseDetails((int)_value, PurchaseOptions.Gem));
            Resolver.Resolve<LootDropManager>().StartTopBarAnimation(DropPodType.Gem, (int)_value);
        }
    }
}