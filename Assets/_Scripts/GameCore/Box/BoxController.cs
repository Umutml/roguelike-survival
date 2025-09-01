using System;
using System.Collections.Generic;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Drop;
using GameCore.Health;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace GameCore.Box
{
    public class BoxController : FixedDropBase, IDamageable
    {
        #region Events

        public event Action<DamageSource> Died;
        public void OnLoseFocus()
        {

        }

        #endregion

        #region Serialized Fields

        [SerializeField] private GameObject chestObject;
        [SerializeField] private GameObject chestPartObject;
        [SerializeField] private GameObject lineObject;
        [SerializeField] private List<Rigidbody> explosionRigidbodies;

        #endregion

        #region Constants

        private const float ExplosionForce = 300f;
        private const float ExplosionRadius = 2f;

        #endregion

        #region Private Fields

        private bool _isAnimating;
        private BoxManager _boxManager;
        private DamageSource _lastTakenDamageSource;

        #endregion

        #region Properties

        public BoxConfig? Config { get; set; }
        public string SpecificDamageType => PlayerWeaponController.PlayerShootingMode.Melee.ToString();
        public BoxCollider Bounds { get; }
        public float Health => 1;
        public Vector3 Position => transform.position;
        public Vector3 ForcePosition { get; }
        public float ForcePower { get; }
        public Transform RandomTransform => transform;
        public bool IsDead { get; private set; }
        public bool IsNotDamageable { get; }

        #endregion

        #region Public Methods

        public override async void Use()
        {
            base.Use();

            if (!ResolveDependencies())
            {
                LoggerNS.LogError("Required components not found");
                return;
            }

            await HandleItemPickup();
            if (Config?.IsDisabledDrop != true)
            {
                await _boxManager.DropBox();
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            _lastTakenDamageSource = damageInfo.Source;
            IsDead = true;
            Died?.Invoke(_lastTakenDamageSource);
            HandleDestruction();
        }

        public void TakeDOT(DamageInfo damageInfo, float duration)
        {
        }

        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
        {
        }

        public override void Reset()
        {
            base.Reset();

            chestObject.SetActive(false);
            chestPartObject.SetActive(true);
            lineObject.SetActive(true);
            Config = null;
            IsDead = false;
            SetAnimation(true);
        }

        #endregion

        #region Private Methods

        private bool ResolveDependencies()
        {
            _boxManager = Resolver.Resolve<BoxManager>();

            return _boxManager != null;
        }

        private async UniTask HandleItemPickup()
        {
            if (Config is { IsDisabledPickup: true }) return;
            await _boxManager.GetDropObject(transform.position + Vector3.up, Config);
        }

        private void HandleDestruction()
        {
            lineObject.SetActive(false);
            SetAnimation(false);
            IsPickedUp = true;

            PlayExplosionAnimation();
            Use();
            AudioManager.PlayOneShot(oneShotAudioKey);
        }

        private void SetAnimation(bool isAnimating)
        {
            _animator.enabled = isAnimating;
            _isAnimating = !isAnimating;
        }

        private async void PlayExplosionAnimation()
        {
            chestObject.SetActive(false);
            chestPartObject.SetActive(true);

            foreach (var rigidbody in explosionRigidbodies)
            {
                rigidbody.isKinematic = false;
                rigidbody?.AddExplosionForce(ExplosionForce, transform.position + RandomOffset(), ExplosionRadius);
            }

            await UniTask.Delay(2000);

            explosionRigidbodies.ForEach(x => x.isKinematic = true);
            _isAnimating = false;
            Reset();
        }

        private Vector3 RandomOffset()
        {
            return new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        }

        #endregion

        #region Structs

        public struct BoxConfig
        {
            public bool IsDisabledPickup;
            public bool IsDisabledDrop;
            public bool IsDisabledDropIncrement;
            public DropPodType? ForcedDropPodType;
        }

        #endregion
    }
}