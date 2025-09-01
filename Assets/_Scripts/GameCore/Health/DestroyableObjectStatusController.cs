using System;
using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using VContainer;

namespace GameCore.Health
{
    public class DestroyableObjectStatusController : MonoBehaviour, IDamageable
    {
        #region Serializable Fields

        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool hasDestroyedMesh;
        [ConditionalField(nameof(hasDestroyedMesh), false, true)]
        [SerializeField] private GameObject originalMeshObject;
        [ConditionalField(nameof(hasDestroyedMesh), false, true)]
        [SerializeField] private GameObject destroyedMeshObject;
        [SerializeField] private Transform[] damagePoints;



        public UnityEvent OnDeath;

        #endregion

        #region Fields

        private float _currentHealth;
        private bool _isDead;
        private IDamageableRegisterService _damageableRegisterService;
        private DamageSource _lastTakenDamageSource;
        #endregion

        #region Events

        public event Action<float, float, bool> HealthChanged;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        [Inject]
        private void Initialize(IDamageableRegisterService damageableRegisterService)
        {
            _damageableRegisterService = damageableRegisterService;
            _damageableRegisterService.RegisterDamageable(this);
        }

        private void OnDestroy()
        {
            _damageableRegisterService.UnregisterDamageable(this);
        }

        #endregion

        #region Private Methods

        private Transform SelectRandomValidDamagePoint()
        {
            if (damagePoints.Length == 0) return transform;

            return damagePoints.PickRandom();
        }

        private IEnumerator ApplyDOT(float damage, float duration)
        {
            float damagePerSecond = damage / duration;
            while (duration > 0)
            {
                _currentHealth -= damagePerSecond * Time.deltaTime;
                if (!_isDead && _currentHealth <= 0)
                {
                    Die();
                    yield break;
                }

                HealthChanged?.Invoke(_currentHealth, maxHealth, false);

                duration -= Time.deltaTime;
                yield return null;
            }
        }

        private void Die()
        {
            if (_isDead) { return; }

            Died?.Invoke(_lastTakenDamageSource);
            OnDeath?.Invoke();
            _isDead = true;

            SwapMeshOrDestroy();
        }

        private void SwapMeshOrDestroy()
        {
            if (hasDestroyedMesh)
            {
                originalMeshObject.SetActive(false);
                destroyedMeshObject.SetActive(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region IDamageable Members
        public string SpecificDamageType => null;
        public BoxCollider Bounds { get; }
        public float Health => _currentHealth;
        public Vector3 Position => transform.position;
        public Vector3 ForcePosition { get; }
        public float ForcePower { get; }
        public Transform RandomTransform => SelectRandomValidDamagePoint();

        public Transform[] DamagePoints => damagePoints;
        public Transform Transform => transform;
        public bool IsDead => _currentHealth <= 0;
        public bool IsNotDamageable { get; }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_currentHealth >= damageInfo.Amount)
                _currentHealth -= damageInfo.Amount;

            _lastTakenDamageSource = damageInfo.Source;
            HealthChanged?.Invoke(_currentHealth, maxHealth, false);
            if (!_isDead && _currentHealth <= 0)
            {
                Die();
            }
        }

        public void TakeDOT(DamageInfo damageInfo, float duration)
        {
            StartCoroutine(ApplyDOT(damageInfo.Amount, duration));
        }

        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
        {
            
        }

        public event Action<DamageSource> Died;
        public void OnLoseFocus()
        {
            
        }

        #endregion
    }
}
