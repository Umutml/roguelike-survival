using Addler.Runtime.Core.Pooling;
using GameCore.Health;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Player.WeaponSystem
{
    public class AutomaticAmmo : ProjectileBase
    {
        public Transform ParentTransform { get; set; }
        public IObjectResolver Resolver { get; set; }

        private Camera _mainCamera;
        private MobManager _mobManager;
        private Vector3 _direction;
        private DamageInfo _damageInfo = new();
        protected IAbility _ability;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isSetup) return;

            _target = GetClosestMob();

            MoveProjectile();

            if (IsOutOfViewport() || IsTargetHit())
            {
                ReturnToPool();
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
            _ability = ability;
            _mobManager = Resolver.Resolve<MobManager>();
            _damageInfo = damageInfo;
            transform.position = firePoint.position;
            _direction = ParentTransform.TransformDirection(Vector3.forward);
            transform.localRotation = Quaternion.LookRotation(_direction);
            if (trailParticle) trailParticle.Clear();
            _isSetup = true;
        }

        private void MoveProjectile()
        {
            transform.Translate(Vector3.forward * velocity * Time.deltaTime);
        }

        private void ReturnToPool()
        {
            Reset();
            gameObject.SetActive(false);
        }


        private bool IsOutOfViewport()
        {
            return !_mainCamera.IsInViewport(transform.position, 0);
        }

        private bool IsTargetHit()
        {
            if (_target != null && !_target.IsDead)
            {
                _target.TakeDamage(_damageInfo);
                OnReturnToPoolByPosition?.Invoke(_target.RandomTransform.position);
                return true;
            }

            return false;
        }

        private IDamageable GetClosestMob()
        {
            return _mobManager?.GetClosestMob(transform.position + Vector3.down, 2f);
        }
    }
}
