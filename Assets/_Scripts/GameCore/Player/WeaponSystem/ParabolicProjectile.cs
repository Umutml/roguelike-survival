using _Utilities;
using GameCore.Health;
using GameCore.Spawner;
using Interfaces;
using MyBox;
using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    namespace GameCore.Player.Weapon
    {
        public class ParabolicProjectile : ProjectileBase
        {
            [ConditionalField(nameof(projectileHitType), false, ProjectileHitType.AreaOfEffect)]
            [SerializeField] protected float radius = 5f;


            protected DamageInfo _damageInfo;
            protected float _counter, _travelDuration, _distance, _maxHeight;
            protected Transform _firePoint;
            protected IAbility _ability;

            private void Update()
            {
                if (!_isSetup) return;
                if (!_targetRandomTransform)
                {
                    Reset();
                    gameObject.SetActive(false);
                    return;
                }

                _counter += Time.deltaTime;

                var t = _counter / _travelDuration;

                transform.position = Helper.SampleParabola(_firePoint.position,
                    _targetRandomTransform.position,
                    _maxHeight,
                    t);

                if (t < 1f) return;

                if (projectileHitType == ProjectileHitType.SingleTarget && !_target.IsDead)
                {
                    _target.TakeDamage(_damageInfo);
                    if (_causesFocus)
                        _playerController.TargetingController.FocusDamageables(new[] {_target});
                }
                else if (projectileHitType == ProjectileHitType.AreaOfEffect)
                {
                    TakeAoeDamage();
                }


                Reset();
                gameObject.SetActive(false);
            }

            protected virtual void TakeAoeDamage()
            {
                if (_mobManager == null) return;

                var mobs = _mobManager.GetMobsInRange(transform.position, radius);
                foreach (var mob in mobs)
                {
                    if (mob == null || mob.IsDead) continue;
                    mob.TakeDamage(_damageInfo);
                }

                if (_causesFocus && _playerController != null)
                    _playerController.TargetingController.FocusDamageables(mobs);
            }


            public override void Setup(Transform firePoint, IDamageable target, IAbility ability, DamageInfo damageInfo,
                Vector3 missingTarget = default, MobManager mobManager = null, PlayerController playerController = null,
                bool causesFocus = false, float maxDistance = 0)
            {
                base.Setup(firePoint,
                    target,
                    ability,
                    damageInfo,
                    missingTarget,
                    mobManager,
                    playerController,
                    causesFocus,
                    maxDistance);
                if (trailParticle) trailParticle.Clear();

                _firePoint = firePoint;
                _ability = ability;
                transform.position = firePoint.position;
                _targetRandomTransform = target.RandomTransform ?? target.Transform;
                transform.LookAt(_targetRandomTransform);
                _target = target;
                _damageInfo = damageInfo;

                _distance = Vector3.Distance(transform.position, _targetRandomTransform.position);
                _travelDuration = .5f + _distance / velocity;
                _maxHeight = 1f + _distance / velocity;

                _isSetup = true;
            }

            public override void Reset()
            {
                base.Reset();
                _counter = 0;
                _distance = 0;
                _travelDuration = 0;
                _maxHeight = 0;
            }
        }
    }
}
