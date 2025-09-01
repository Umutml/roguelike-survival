using System;
using System.Collections.Generic;
using GameCore.Health;
using Interfaces;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial
{
    public class TutorialBarricade : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject explosionParticle;
        [SerializeField] private Transform explosivePoint;
        private List<GameObject> _barricadeParts;
        private Collider _blockCollider;
        private IDamageableRegisterService _damageableRegisterService;
        private DamageSource _lastTakenDamageSource;
        private IAnalyticsService _analyticsService;
        private void Awake()
        {
            FillBarricadeParts();
            _blockCollider = GetComponent<Collider>();
        }


        public bool IsNotDamageable { get; }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;
            Health -= damageInfo.Amount;
            _lastTakenDamageSource = damageInfo.Source;
            if (Health <= 0)
            {
                IsDead = true;
                _damageableRegisterService.UnregisterDamageable(this);
                Died?.Invoke(_lastTakenDamageSource);
                DestroyBarricade();
            }
        }

        public void TakeDOT(DamageInfo damageInfo, float duration)
        {
        }

        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
        {
        }

        [Inject]
        public void Initialize(IDamageableRegisterService damageableRegisterService, IAnalyticsService analyticsService)
        {
            _damageableRegisterService = damageableRegisterService;
            _analyticsService = analyticsService;
        }

        public void RegisterDamageable()
        {
            _damageableRegisterService.RegisterDamageable(this);
        }

        private Transform ReturnExplosivePoint()
        {
            return explosivePoint;
        }

        private void FillBarricadeParts()
        {
            _barricadeParts = new List<GameObject>();
            foreach (Transform child in transform)
            {
                _barricadeParts.Add(child.gameObject);
            }
        }

        private void DestroyBarricade()
        {
            SetKinematicBarricadeParts(false);
            ExplosionForce();
            _blockCollider.enabled = false;
            Destroy(gameObject, 3f);
            _analyticsService.LogEvent(new EventParameters<string> { EventName = "tt_barrier_destroyed" });
        }

        private void SetKinematicBarricadeParts(bool isKinematic)
        {
            foreach (var piece in _barricadeParts)
            {
                piece.GetComponent<Rigidbody>().isKinematic = isKinematic;
            }
        }

        private void ExplosionForce()
        {
            foreach (var piece in _barricadeParts)
            {
                piece.GetComponent<Rigidbody>().isKinematic = false;
                piece.GetComponent<Rigidbody>().AddExplosionForce(1000, transform.position, 10, 1);
                Instantiate(explosionParticle, piece.transform.position, Quaternion.identity);
            }
        }

        #region IDamageable

        public event Action<DamageSource> Died;
        public void OnLoseFocus()
        {
            
        }

        public string SpecificDamageType { get; }
        public BoxCollider Bounds { get; }
        public float Health { get; private set; } = 200;

        public Vector3 Position => transform.position;
        public Vector3 ForcePosition { get; }
        public float ForcePower { get; }
        public Transform RandomTransform => ReturnExplosivePoint();
        public Transform Transform => transform;
        public bool IsDead { get; set; }

        #endregion
    }
}
