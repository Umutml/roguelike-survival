using System;
using System.Collections.Generic;
using _Utilities;
using Addler.Runtime.Core.Pooling;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Player.WeaponSystem.GameCore.Player.Weapon;
using Interfaces;
using Managers;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Player.WeaponSystem
{
    public class RangedWeapon : Weapon
    {
        [SerializeField] protected bool debug;
        [SerializeField] protected Transform firePoint;
        [SerializeField] protected string ammoAssetKey = "BasicAmmo";
        [SerializeField] protected int ammoPoolSize = 15;

        [SerializeField] protected ParticleSystem muzzleFlash;
        [SerializeField] private bool isNpcWeapon;
        [SerializeField] private bool ammoCausesFocus;


        protected DamageInfo _damageInfo = new DamageInfo();
        private PlayerController _playerController;
        private IObjectResolver _resolver;
        private AudioSource _audioSourceForNpcShoot;
        private AudioManager _audioManagerForNpcShoot;
        protected List<IDamageable> _targets = new List<IDamageable>();

        protected override async void Awake()
        {
            base.Awake();

            if (TryGetComponent<AudioSource>(out var audioSource))
                _audioSourceForNpcShoot = audioSource;
        }

#if UNITY_EDITOR
        protected void OnDrawGizmos()
        {
            if (debug)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(firePoint.position, firePoint.forward * range);
            }
        }
#endif
        [Inject]
        public void Init(AudioManager audioManager)
        {
            if (isNpcWeapon)
            {
                _audioManagerForNpcShoot = audioManager;
            }
        }

        public void SetObjectResolver(IObjectResolver resolver)
        {
            _resolver = resolver;

            _playerController = _resolver.Resolve<PlayerController>();
        }

        public override async void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
            try
            {
                if (_isLocked)
                {
                    return;
                }

                base.FireAt(target, damageSource);

                PlayOneShotAudio();

                muzzleFlash.Play();
                var ammoObject = await ObjectManager.GetObject(ammoAssetKey);

                if (damageSource == DamageSource.Player)
                {
                    ammoObject.transform.position = firePoint.position;
                    ammoObject.transform.rotation = firePoint.rotation;
                }
                else if (damageSource == DamageSource.Npc)
                {
                    ammoObject.transform.SetParent(firePoint);
                    ammoObject.transform.localPosition = Vector3.zero;
                    ammoObject.transform.localRotation = Quaternion.identity;
                    ammoObject.transform.localScale = Vector3.one;

                    await UniTask.Delay(100);

                    if (ammoObject == null)
                        return;

                    ammoObject.transform.SetParent(null);
                }

                var isCriticalHit = Helper.CalculateRngChange(criticalHitChance);

                _damageInfo.Amount = isCriticalHit ? critDamage : damage;
                _damageInfo.Source = damageSource;
                var ammo = ammoObject.GetComponent<Ammo>();


                ammo.Setup(firePoint,
                    target,
                    null,
                    _damageInfo,
                    default,
                    MobManager,
                    PlayerController,
                    ammoCausesFocus);
                _targets.Add(target);

                var poolable = ammo as IPoolable;

                poolable.OnReturnToPool = async () =>
                {
                    Vector3 pos = ammoObject.transform.position;
                    ShowHitEffect(pos);
                };

                if (!isNpcWeapon && !ammoCausesFocus)
                    PushFocusToTargets();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ranged Weapon: {e.Message} {e.StackTrace}");
            }
        }

        protected void PushFocusToTargets()
        {
            _playerController.TargetingController.FocusDamageables(_targets);
            _targets.Clear();
        }

        private void PlayOneShotAudio()
        {
            if (_playerController != null)
            {
                if (!usesMultipleSounds)
                    _playerController.PlayOneShotAudio(oneShotAudioKey);
                else
                {
                    var randomSound = oneShotAudioKeys.PickRandom();
                    _playerController.PlayOneShotAudio(randomSound);
                }
            }
            else
            {
                //npc shoot

                if (_audioManagerForNpcShoot != null && _audioSourceForNpcShoot != null)
                {
                    _audioManagerForNpcShoot.PlayOneShot(_audioSourceForNpcShoot, oneShotAudioKey);
                }
            }
        }
    }
}
