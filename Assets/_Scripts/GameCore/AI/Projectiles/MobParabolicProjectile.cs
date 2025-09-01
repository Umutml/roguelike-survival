using System;
using System.Collections;
using System.Threading.Tasks;
using _Scripts.Utilities;
using _Utilities;
using DG.Tweening;
using GameCore.Health;
using GameCore.Player;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using Interfaces;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;

namespace _Scripts.GameCore.AI
{
    public class MobParabolicProjectile : ProjectileBase
    {
        [ConditionalField(nameof(projectileHitType), false, ProjectileHitType.AreaOfEffect)]
        [SerializeField] protected float radius = 5f;
        [SerializeField] protected AssetReferenceGameObject projectileHitParticleRef;
        [SerializeField] protected AssetReferenceGameObject endpointIndicatorRef;

        protected float _counter, _travelDuration, _distance, _maxHeight;

        protected DamageInfo _damageInfo;
        protected Transform _firePoint;
        private Vector3 _tempTargetPosition;
        private bool _indicatorCreated;
        private GameObject _rangeIndicator;
        private GameObject _projectileHitParticle;
        private DecalProjector _projector;
        protected IAbility _ability;

        public override void Reset()
        {
            base.Reset();
            _counter = 0;
            _distance = 0;
            _travelDuration = 0;
            _maxHeight = 0;
            _indicatorCreated = false;
            _rangeIndicator.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isSetup)
                return;
            if (!_targetRandomTransform)
            {
                Reset();
                return;
            }

            _counter += Time.deltaTime;

            var t = _counter / _travelDuration;

            transform.position = Helper.SampleParabola(_firePoint.position, _tempTargetPosition, _maxHeight, t);

            if (!_indicatorCreated && t >= 0.1f) // Create indicator after half of the path
                CreateEndPointIndicator();

            if (t < 1f)
                return;

            // REACHES TARGET
            PlayHitParticleEffect();
            if (projectileHitType == ProjectileHitType.SingleTarget && !_target.IsDead)
            {
                _target.TakeDamage(_damageInfo);
            }
            else if (projectileHitType == ProjectileHitType.AreaOfEffect)
            {
                TakeAoeDamage();
            }

            Reset();
        }

        private async void CreateEndPointIndicator()
        {
            _indicatorCreated = true;
            _rangeIndicator =
                await ObjectManager.GetObject(endpointIndicatorRef, _tempTargetPosition, Quaternion.Euler(90, 0, 0));
            _projector = _rangeIndicator.GetComponent<DecalProjector>();
            _projector.size = new Vector3(radius, radius, _projector.size.z);
            // Animate the size to total radius
            DOTween.To(() => _projector.size,
                x => _projector.size = x,
                new Vector3(radius * 2, radius * 2, _projector.size.z),
                _travelDuration / 2);
        }

        private async void PlayHitParticleEffect()
        {
            if (projectileHitParticleRef != null)
            {
                _projectileHitParticle = await ObjectManager.GetObject(projectileHitParticleRef,
                    transform.position,
                    Quaternion.Euler(90, 0, 0));
                var tempParticle = _projectileHitParticle.GetComponent<ParticleSystem>();
                _projectileHitParticle.GetComponent<ParticleSystem>().Play();
                var timeSpan =
                    TimeSpan.FromSeconds(tempParticle.main.duration); // Get the duration of the particle for waiting
                await Task.Delay(timeSpan);
                if (_projectileHitParticle != null) _projectileHitParticle.SetActive(false);
            }
        }

        protected virtual void TakeAoeDamage()
        {
            // Check for player damage
            if (_playerController != null)
            {
                var player = _playerController.GetDamageable;
                if (player != null && !player.IsDead)
                {
                    float distance = Vector3.Distance(transform.position, player.Position);
                    if (distance <= radius)
                        player.TakeDamage(_damageInfo);
                }
            }

            // Check for target damage
            if (_target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, _target.Transform.position);
                if (distanceToTarget <= radius)
                    _target?.TakeDamage(_damageInfo);
            }
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
            if (trailParticle)
                trailParticle.Clear();

            _firePoint = firePoint;
            _ability = ability;
            transform.position = firePoint.position;
            _targetRandomTransform = target.RandomTransform ?? target.Transform;
            _tempTargetPosition = _targetRandomTransform.position + -Vector3.up; // to make it hit the ground level
            transform.LookAt(_targetRandomTransform);
            _target = target;
            _damageInfo = damageInfo;

            _distance = Vector3.Distance(transform.position, _targetRandomTransform.position);
            _travelDuration = .5f + _distance / velocity;
            _maxHeight = 1f + _distance / velocity;
            _isSetup = true;
        }
    }
}
