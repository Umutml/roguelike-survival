using System.Collections;
using System.Collections.Generic;
using _Utilities;
using GameCore.Health;
using GameCore.Spawner;
using Interfaces;
using MyBox;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Player.WeaponSystem
{
    public class AutomaticWeapon : Weapon
    {
        [SerializeField] private string ammoAssetKey = "BasicAmmo";
        [SerializeField] private int ammoPoolSize = 15;
        [SerializeField] private List<Transform> firePoints;
        [SerializeField] private List<ParticleSystem> muzzleFlashList;
        [SerializeField] private bool canReload;

        [ConditionalField(nameof(canReload), false)] [SerializeField]
        private float capacity;

        [SerializeField] private GameObject reloadEffect;
        [SerializeField] private WeaponTargetDetection weaponTargetDetection;

        private readonly WaitForSeconds ReloadTime = new(3);

        private Coroutine _autoFireCoroutine;
        private IObjectResolver _resolver;
        private MobManager _mobManager;
        private BoxManager _boxManager;
        private PlayerController _playerController;
        private Transform _parentTransform;
        private readonly DamageInfo _damageInfo = new();
        private Camera _mainCamera;
        private float _ammoCount;
        private bool _isReloading;

        protected override void Awake()
        {
            base.Awake();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            CheckTargetDetection();
            CheckStatus();
        }

        private void OnDestroy()
        {
            StopCoroutine();
        }

        private void CheckTargetDetection()
        {
            if (_playerController == null) return;
            if (_playerController.InBase)
            {
                StopCoroutine();
                return;
            }

            if (weaponTargetDetection == null) return;

            if (weaponTargetDetection.IsTarget)
                _autoFireCoroutine ??= StartCoroutine(AutoFire());
            else
                StopCoroutine();
        }

        private void CheckStatus()
        {
            if (!_isReloading) return;


            reloadEffect.transform.LookAt(_mainCamera.transform);
        }

        private IEnumerator AutoFire()
        {
            while (true)
            {
                FireAt(null);
                yield return new WaitForSeconds(fireInterval);
            }
        }

        private IEnumerator Reload()
        {
            _isReloading = true;
            reloadEffect.SetActive(true);
            yield return ReloadTime;
            reloadEffect.SetActive(false);
            _isReloading = false;
            _ammoCount = 0;
        }

        private void CheckReloadStatus()
        {
            if (!canReload) return;

            if (_ammoCount >= capacity) StartCoroutine(nameof(Reload));
        }

        public void StopCoroutine()
        {
            if (_autoFireCoroutine != null)
            {
                StopCoroutine(_autoFireCoroutine);
                _autoFireCoroutine = null;
            }
        }

        public void Initialize(Transform parentTransform, IObjectResolver resolver)
        {
            StopCoroutine();
            _parentTransform = parentTransform;
            _resolver = resolver;
            _mobManager = resolver.Resolve<MobManager>();
            _boxManager = resolver.Resolve<BoxManager>();
            _playerController = resolver.Resolve<PlayerController>();
            if (weaponTargetDetection != null) weaponTargetDetection.Initialize(_mobManager);
        }

        public void Dispose()
        {
            StopCoroutine();
            if (weaponTargetDetection != null) weaponTargetDetection.Dispose();
        }

        public override void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
            if (_isLocked)
            {
                return;
            }

            if (_mobManager == null || _boxManager == null) return;

            if (_isReloading)
            {
#if DEBUG_LOGS_ENABLED
                LoggerNS.Log("Reloading");
#endif
                return;
            }

            base.FireAt(target, damageSource);
            PlayOneShotAudio();
            _ammoCount++;

            muzzleFlashList.ForEach(x => x.Play());
            var isCriticalHit = Helper.CalculateRngChange(criticalHitChance);
            firePoints.ForEach(async firePoint =>
            {
                var ammoInstance = await ObjectManager.GetObject(ammoAssetKey);
                ammoInstance.transform.position = firePoint.position;

                var ammo = ammoInstance.GetComponent<AutomaticAmmo>();

                ammo.Resolver = _resolver;
                ammo.ParentTransform = _parentTransform;
                _damageInfo.Amount = isCriticalHit ? critDamage : damage;
                _damageInfo.Source = damageSource;
                ammo.Setup(firePoint, target, null, _damageInfo);

                var poolable = ammo as IPoolable;

                poolable.OnReturnToPool = () => { };
                poolable.OnReturnToPoolByPosition = ShowHitEffect;
            });


            CheckReloadStatus();
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
    }
}
