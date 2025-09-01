using GameCore.Health;
using GameCore.Spawner;
using Interfaces;
using MyBox;
using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    namespace GameCore.Player.Weapon
    {
        public class Ammo : ProjectileBase
        {
            #region Serializable Fields

            [ConditionalField(nameof(projectileHitType), false, ProjectileHitType.AreaOfEffect)]
            [SerializeField] protected float radius = 5f;
            [SerializeField] private string oneShotAudioKey;

            private float _maxDistance;

            #endregion

            #region Fields

            protected DamageInfo _damageInfo;
            protected bool _isMissing;
            protected Vector3 _missingTarget;
            protected IAbility _ability;

            #endregion

            #region Unity Methods

            public override void Reset()
            {
                base.Reset();
                _missingTarget = default;
                _isMissing = false;
            }

            private void Update()
            {
                if (!_isSetup) return;

                if (_isMissing)
                {
                    //if the ammo has no target, and missing the target as a visual effect
                    transform.position = Vector3.MoveTowards(transform.position,
                        _missingTarget,
                        velocity * Time.deltaTime);
                    if (transform.position != _missingTarget) return;
                    Reset();
                    gameObject.SetActive(false);
                    return;
                }

                //if the ammo has an actual target, deal damage
                if (!_targetRandomTransform)
                {
                    Reset();
                    gameObject.SetActive(false);
                    return;
                }

                if (_maxDistance > 0 && Vector3.Distance(transform.position, _targetRandomTransform.position) >
                    _maxDistance)
                {
                    Reset();
                    gameObject.SetActive(false);
                    return;
                }

                transform.LookAt(_targetRandomTransform);
                transform.position = Vector3.MoveTowards(transform.position,
                    _targetRandomTransform.position,
                    velocity * Time.deltaTime);
                if (transform.position != _targetRandomTransform.position) return;
                if (!_target.IsDead)
                {
                    DealDamage();
                }


                Reset();
                gameObject.SetActive(false);
            }

            #endregion

            #region Public Methods

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
                _ability = ability;
                _maxDistance = maxDistance;
                _damageInfo = damageInfo;
                if (trailParticle) trailParticle.Clear();

                if (missingTarget != default)
                {
                    _missingTarget = missingTarget;
                    _isMissing = true;
                }

                transform.position = firePoint.position;

                if (_isMissing)
                    transform.LookAt(_missingTarget);
                else
                {
                    _targetRandomTransform = target.RandomTransform ?? target.Transform;
                    transform.LookAt(_targetRandomTransform);
                }

                _target = target;
                _isSetup = true;
            }

            #endregion

            protected void DealDamage()
            {
                if (projectileHitType == ProjectileHitType.SingleTarget)
                {
                    _target.TakeDamage(_damageInfo);
                    if (_causesFocus)
                        _playerController.TargetingController.FocusDamageables(new[] {_target});
                }
                else if (projectileHitType == ProjectileHitType.AreaOfEffect)
                {
                    if (_playerController != null)
                        _playerController.PlayOneShotAudio(oneShotAudioKey);

                    var mobs = _mobManager.GetMobsInRange(_target.Position, radius);

                    if (_causesFocus)
                        _playerController.TargetingController.FocusDamageables(mobs);

                    foreach (var mob in mobs)
                    {
                        if (mob == null || mob.IsDead) continue;
                        mob.TakeDamage(_damageInfo);
                    }
                }
            }
        }
    }
}
