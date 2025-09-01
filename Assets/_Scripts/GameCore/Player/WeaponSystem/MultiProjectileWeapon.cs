using System;
using _Utilities;
using GameCore.Health;
using GameCore.Player.WeaponSystem.GameCore.Player.Weapon;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Player.WeaponSystem
{
    /// <summary>
    /// This class represents weapons, such as shotguns, that fire multiple pellets or projectiles in a single shot.
    /// Don't use for automatic weapons that fire single projectiles.
    /// </summary>
    public class MultiProjectileWeapon : RangedWeapon
    {
        [SerializeField] private float coneOfFireAngle = 45f;


        public override async void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
            try
            {
                if (_isLocked)
                {
                    return;
                }

                muzzleFlash.Play();

                var direction = firePoint.forward;
                var mobs = MobManager.GetMobsInConeDirection(firePoint.position, direction, coneOfFireAngle, range);

                var extraShots = (int) (pellets - mobs.Count);

                var hittingExtraShots = 0;

                if (target != default && Vector3.Distance(target.Position, firePoint.position) <= 1)
                    hittingExtraShots = extraShots;
                else if (Helper.CalculateRngChange(80))
                {
                    hittingExtraShots = Random.Range(0, extraShots + 1);
                }

                for (int i = 0; i < pellets; i++)
                {
                    var pelletTarget = i < mobs.Count ? mobs[i] : null;

                    if (pelletTarget == null && hittingExtraShots > 0)
                    {
                        pelletTarget = target;
                        hittingExtraShots--;
                    }

                    var ammoInstance = await ObjectManager.GetObject(ammoAssetKey);
                    var isCriticalHit = Helper.CalculateRngChange(criticalHitChance);
                    _damageInfo.Amount = isCriticalHit ? critDamage : damage;
                    _damageInfo.Source = damageSource;
                    var ammo = ammoInstance.GetComponent<Ammo>();

                    if (pelletTarget != null)
                    {
                        ammo.Setup(firePoint, pelletTarget, null, _damageInfo);
                        _targets.Add(pelletTarget);
                    }
                    else
                    {
                        var missingTarget =
                            GetRandomMissingTarget(firePoint.position, direction, coneOfFireAngle, range);
                        ammo.Setup(firePoint, null, null, _damageInfo, missingTarget);
                    }

                    var poolable = ammo as IPoolable;

                    poolable.OnReturnToPool = async () =>
                    {
                        Vector3 pos = ammoInstance.transform.position;
                        ShowHitEffect(pos);
                    };
                }

                base.FireAt(target, damageSource);
            }
            catch (Exception e)
            {
                Debug.LogError("Shotgun FireAt Exception: " + e);
            }
        }

        private Vector3 GetRandomMissingTarget(Vector3 position, Vector3 direction, float angle, float range)
        {
            var randomAngle = Random.Range(-angle, angle);
            var randomDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * direction;
            var randomDistance = Random.Range(0, range);
            return position + randomDirection * randomDistance;
        }
    }
}
