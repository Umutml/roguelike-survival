using System.Collections;
using System.Collections.Generic;
using _Scripts.Utilities;
using GameCore.Drop;
using GameCore.Health;
using GameCore.Spawner;
using Managers;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Drop
{
    public class BombPodController : FixedDropBase
    {
        public float damage = 100f;
        public float radius = 8f;

        private readonly DamageInfo _damageInfo = new();

        public override void Use()
        {
            base.Use();
            var mobManager = Resolver.Resolve<MobManager>();
            AudioManager.PlayOneShot(oneShotAudioKey);
            if (mobManager == null)
            {
                LoggerNS.LogError("BombPodController: Component is not MobManager");
                return;
            }

            var closestMob = mobManager.GetMobsInRange(transform.position, radius);
            if (closestMob is not {Count: > 0})
            {
                return;
            }

            closestMob.ForEach(mob =>
            {
                _damageInfo.Amount = damage;
                mob.TakeDamage(_damageInfo);
            });
        }
    }
}
