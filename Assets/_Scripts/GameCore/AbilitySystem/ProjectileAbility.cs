using Addler.Runtime.Core.Pooling;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Player;
using GameCore.Player.WeaponSystem.GameCore.Player.Weapon;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.AbilitySystem
{
    public class ProjectileAbility : Ability
    {
        protected virtual async void ShowHitEffect(Vector3 position)
        {
            //explosion object from addressable pool
            var explosionInstance = await ObjectManager.GetObject(hitEffectKey);
            explosionInstance.transform.position = position;
            await UniTask.Delay(2000);

            //second check because of await time
            explosionInstance.SetActive(false);
        }

        public override async void Execute()
        {
            if (isOnCooldown) return;
            Transform firePoint = User.transform;
            var mobManager = Resolver.Resolve<MobManager>();
            var playerController = Resolver.Resolve<PlayerController>();
            var target = mobManager.GetClosestMob(firePoint.position, Radius);
            if (target == null) return;

            base.Execute();

            var ammoInstance = await ObjectManager.GetObject(ammoAssetKey);

            ammoInstance.transform.position = firePoint.position;
            ammoInstance.transform.rotation = firePoint.rotation;

            DamageInfo dmg = new DamageInfo();
            dmg.Amount = Damage;

            var projectile = ammoInstance.GetComponent<ParabolicProjectile>();
            projectile.Setup(firePoint, target, this, dmg, default(Vector3), mobManager, playerController, true);

            var poolable = projectile as IPoolable;

            poolable.OnReturnToPool = async () =>
            {
                Vector3 pos = ammoInstance.transform.position;
                ShowHitEffect(pos);
            };
        }
    }
}
