using System;
using System.Collections.Generic;
using System.Linq;
using _Utilities;
using Cathei.LinqGen;
using GameCore.Health;
using Managers;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Player.WeaponSystem
{
    public class MeleeWeapon : Weapon
    {
        [SerializeField] private ParticleSystem[] slashEffects;
        [SerializeField] private Transform firePoint;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private Transform[] reparentObjects;
        [SerializeField] private Transform originalParent;
        [SerializeField] private float innerDamageConeAngle = 40f;
        [SerializeField] private float outerDamageConeAngle = 85f;
        [SerializeField] private int maxHitCount = 3;


        private DamageInfo _fullDamageInfo = new DamageInfo();
        private DamageInfo _outerDamageInfo = new DamageInfo();
        private Dictionary<Transform, Tuple<Vector3, Quaternion>> _originalLocalRotPos =
            new Dictionary<Transform, Tuple<Vector3, Quaternion>>();
        private AudioManager _audioManager;
        private PlayerController _playerController;
        public Transform[] ReparentObjects
        {
            get => reparentObjects;
            set => reparentObjects = value;
        }

        public Transform OriginalParent
        {
            get => originalParent;
            set => originalParent = value;
        }

        public Dictionary<Transform, Tuple<Vector3, Quaternion>> OriginalLocalRotPos
        {
            get => _originalLocalRotPos;
            set => _originalLocalRotPos = value;
        }

        protected override void Awake()
        {
            base.Awake();
            foreach (var reparentObject in reparentObjects)
            {
                _originalLocalRotPos.Add(reparentObject,
                    Tuple.Create(reparentObject.localPosition, reparentObject.localRotation));
            }
        }

        [Inject]
        public void Initialize(PlayerController player)
        {
            _playerController = player;
        }

        private void PlayOneShotAudio()
        {
            if (_playerController == null) return;

            if (!usesMultipleSounds)
                _playerController.PlayOneShotAudio(oneShotAudioKey);
            else
            {
                var randomSound = oneShotAudioKeys.PickRandom();
                _playerController.PlayOneShotAudio(randomSound);
            }
        }

        public void ToggleSlashEffect(bool value)
        {
            if (trailRenderer)
                trailRenderer.emitting = value;

            foreach (var slashEffect in slashEffects)
            {
                if (value)
                {
                    slashEffect.Play();
                }
                else
                {
                    slashEffect.Stop();
                }
            }
        }

        public override void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
            if (_isLocked)
            {
                return;
            }

            base.FireAt(target, damageSource);
            var isCriticalHit = Helper.CalculateRngChange(criticalHitChance);
            List<IDamageable> damagedMobs = new List<IDamageable>();

            PlayOneShotAudio();

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
    }
}
