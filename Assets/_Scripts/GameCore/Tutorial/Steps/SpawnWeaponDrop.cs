using Cysharp.Threading.Tasks;
using GameCore.Drop;
using GameCore.Player;
using GameCore.Spawner;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "SpawnWeaponDrop",
        menuName = "ScriptableObjects/Tutorial/Steps/Spawn Weapon Drop",
        order = 0)]
    public class SpawnWeaponDrop : TutorialStep
    {
        [SerializeField] private string weaponKey;
        [SerializeField] private Vector3 dropPosition;

        public override UniTask ProcessStep()
        {
            SpawnDrop();
            return UniTask.CompletedTask;
        }

        private async void SpawnDrop()
        {
            if (!Resolver.TryResolve(out LootDropManager lootDropManager)) return;

            var dropObject = await lootDropManager.GetDropObject(DropPodType.Weapon, dropPosition);
            var weaponDrop = dropObject.GetComponent<WeaponDrop>();
            weaponDrop.WeaponKey = weaponKey;
            weaponDrop.WaitForPlayerToMoveout = false;
            weaponDrop.PlayerController = Resolver.Resolve<PlayerController>();
            weaponDrop.IsPickable = true;
            weaponDrop.Initialize(1);
        }
    }
}