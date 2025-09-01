using System.Collections.Generic;
using System.Linq;
using _Utilities;
using Cathei.LinqGen;
using GameCore.Health;
using Managers;
using UnityEngine;
using VContainer;

namespace GameCore.Player.WeaponSystem
{
    public class FlameThrower : Weapon
    {
        #region Serializable Fields

        [SerializeField] private ParticleSystem[] slashEffects;
        [SerializeField] private Transform firePoint;
        [SerializeField] private ParticleSystem flameFX;
        [SerializeField] private float innerDamageConeAngle = 40f;
        [SerializeField] private float outerDamageConeAngle = 85f;
        [SerializeField] private int maxHitCount = 3;
        [SerializeField] private FlameThrowerAudioController flameThrowerAudioController;

        #endregion

        #region Fields

        private AudioManager _audioManager;
        private float _firingStopDelay = 0.1f; // Time to wait before considering firing stopped


        private DamageInfo _fullDamageInfo = new DamageInfo();

        private bool _isFiring = false;
        private float _lastFireTime = 0f;
        private DamageInfo _outerDamageInfo = new DamageInfo();
        private PlayerController _playerController;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            // Check if firing has stopped
            if (_isFiring && Time.time - _lastFireTime > _firingStopDelay)
            {
                _isFiring = false;
                OnStopFiring();
            }
        }

        #endregion

        #region Public Methods

        [Inject]
        public void Initialize(PlayerController player)
        {
            _playerController = player;
        }

        public override void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
            if (_isLocked)
            {
                return;
            }

            if (!_isFiring)
            {
                _isFiring = true;
                OnStartFiring();
            }

            _lastFireTime = Time.time;

            base.FireAt(target, damageSource);
            var isCriticalHit = Helper.CalculateRngChange(criticalHitChance);
            List<IDamageable> damagedMobs = new List<IDamageable>();

            int currenthitCount = 1;

            _fullDamageInfo.Amount = isCriticalHit ? critDamage : damage;
            _fullDamageInfo.Source = damageSource;

            _outerDamageInfo.Amount = _fullDamageInfo.Amount * 0.75f;
            _outerDamageInfo.Source = damageSource;

            var playerTransform = transform;
            var playerPosition = playerTransform.position;
            var direction = (target.Position - playerPosition).normalized;

            target.TakeDamage(_fullDamageInfo);
            ShowHitEffect(target.RandomTransform.position);
            damagedMobs.Add(target);


            var innerMobs = MobManager.GetMobsInConeDirection(playerPosition, direction, innerDamageConeAngle, 2.5f)
                .Gen().Where(mob => mob != target).ToList();

            foreach (var innerMob in innerMobs)
            {
                innerMob.TakeDamage(_fullDamageInfo);
                damagedMobs.Add(innerMob);
                ShowHitEffect(innerMob.RandomTransform.position);
                currenthitCount++;

                if (currenthitCount >= maxHitCount)
                {
                    _playerController.TargetingController.FocusDamageables(damagedMobs);
                    return;
                }
            }

            var outerMobs = MobManager.GetMobsInConeDirection(playerPosition, direction, outerDamageConeAngle, 2.5f);
            var outerMobsExcludingInnerMobs = outerMobs.Except(innerMobs).Where(mob => mob != target);

            foreach (var outerMob in outerMobsExcludingInnerMobs)
            {
                outerMob.TakeDamage(_outerDamageInfo);
                damagedMobs.Add(outerMob);
                ShowHitEffect(outerMob.RandomTransform.position);
                currenthitCount++;

                if (currenthitCount >= maxHitCount)
                {
                    _playerController.TargetingController.FocusDamageables(damagedMobs);
                    return;
                }
            }

            _playerController.TargetingController.FocusDamageables(damagedMobs);
        }

        #endregion

        #region Private Methods

        private void OnStartFiring()
        {
            if(flameFX)
                flameFX.Play();
            if(flameThrowerAudioController)
                flameThrowerAudioController.StartFiringSound();
        }

        private void OnStopFiring()
        {
            if(flameFX)
                flameFX.Stop();
            if(flameThrowerAudioController)
                flameThrowerAudioController.StopFiringSound();
        }

        #endregion
    }
}
