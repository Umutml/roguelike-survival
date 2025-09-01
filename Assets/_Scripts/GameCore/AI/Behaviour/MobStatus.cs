using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GameCore.BuffSystem;
using GameCore.Health;
using Interfaces;
using Pathfinding;
using UnityEngine;

namespace GameCore.AI
{
    public class MobStatus : MonoBehaviour, IDamageable, IDebuffable, IResettable
    {
        #region Serializable Fields

        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] private int xpValue;
        [SerializeField] private ParticleSystem hitParticle;
        [SerializeField] private ParticleSystem stunParticle;
        [SerializeField] protected MobHealthManager healthManager;

        #endregion

        #region Fields

        private Transform _currentRandomTransform;
        protected bool _isDead;
        private DamageSource _lastTakenDamageSource;
        protected MobBase MobBase;
        protected PlayerStatusController PlayerStatusController;
        protected DamageNumberManager DamageNumberManager;
        protected FollowerEntity Follower;
        protected bool IsHealthbarDisabled;


        private float _currentHealth;
        protected float DefaultSpeed;

        #endregion

        #region Properties

        public int XPValue => xpValue;

        #endregion

        #region Public Methods

        public void Heal(float amount)
        {
            Health += amount;
            if (Health > maxHealth) Health = maxHealth;
        }

        public void HealOverTime(float amount, float duration)
        {
            StartCoroutine(ApplyHOT(amount, duration));
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Follower = GetComponent<FollowerEntity>();
        }

        #endregion

        #region Private Methods

        private IEnumerator ApplyDOT(float damage, float duration)
        {
            var damagePerSecond = damage / duration;
            while (duration > 0)
            {
                Health -= damagePerSecond * Time.deltaTime;
                if (Health <= 0)
                {
                    Die();
                    yield break;
                }

                duration -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator ApplyHOT(float amount, float duration)
        {
            var healPerSecond = amount / duration;
            while (duration > 0)
            {
                Health += healPerSecond * Time.deltaTime;
                if (Health > maxHealth) Health = maxHealth;

                duration -= Time.deltaTime;
                yield return null;
            }
        }

        public void Die(DamageSource damageSource)
        {
            _lastTakenDamageSource = damageSource;
            Die();
        }

        protected void Die()
        {
            IsHealthbarDisabled = true;
            healthManager.gameObject.SetActive(false);
            if (_isDead) return;

            Died?.Invoke(_lastTakenDamageSource);
            _isDead = true;
        }

        #endregion

        #region IDamageable Members

        protected bool IsCrashed;
        public string SpecificDamageType => null;
        public BoxCollider Bounds { get; }
        public float Health
        {
            get => _currentHealth;
            protected set
            {
                if (value < maxHealth)
                {
                    if (value > 0 && !IsHealthbarDisabled)
                        healthManager.gameObject.SetActive(true);

                    healthManager.OnHealthChanged(value / maxHealth);
                }

                _currentHealth = value;
            }
        }
        public Vector3 Position => transform.position;
        public Vector3 ForcePosition { get; private set; }
        public float ForcePower { get; private set; }
        public Transform RandomTransform
        {
            get
            {
                if (_currentRandomTransform == null) _currentRandomTransform = MobBase.SelectRandomValidDamagePoint();

                return _currentRandomTransform;
            }
        }
        public Transform Transform => this == null ? null : transform;
        public bool IsDead => Health <= 0;
        public bool IsNotDamageable { get; }


        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;
            ForcePosition = PlayerStatusController.Transform.position;
            ForcePower = 1;
            Health -= damageInfo.Amount;
            _lastTakenDamageSource = damageInfo.Source;
            MobBase.PlayerStatusController.RecordGivenDamage(damageInfo.Amount);
            if (Health <= 0) Die();

            _currentRandomTransform = MobBase.SelectRandomValidDamagePoint();

            MobBase.DamageNumberManager.UseDamageNumber(transform.position,
                Mathf.RoundToInt(damageInfo.Amount).ToString(),
                false);
            if (hitParticle) hitParticle.Play();
        }

        public void TakeDOT(DamageInfo damageInfo, float duration)
        {
            StartCoroutine(ApplyDOT(damageInfo.Amount, duration));
        }

        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
        {
            if (IsCrashed) return;
            IsCrashed = true;
            ForcePosition = carPosition;
            ForcePower = collisionForce;
            var damageDealt = collisionForce * ZombieConstants.ZombieCrashDeadProbability;
            Health -= damageDealt;
            Crashed?.Invoke();
        }

        public event Action<DamageSource> Died;

        public virtual void OnLoseFocus()
        {
            healthManager.gameObject.SetActive(false);
        }

        public event Action Crashed;

        #endregion

        #region IDebuffable Members

        public async UniTask ApplyDebuff(Debuff debuff)
        {
            ToggleDebuffEffect(debuff, true);
            await UniTask.Delay(TimeSpan.FromSeconds(debuff.Time));
            ToggleDebuffEffect(debuff, false);
        }

        //TODO: will be moved to mob animation controller when refactoring
        private void ToggleDebuffEffect(Debuff debuff, bool toggle)
        {
            switch (debuff.Type)
            {
                case Debuff.Debufftype.Stun:
                    if (stunParticle)
                    {
                        if (toggle)
                            stunParticle.Play();
                        else
                            stunParticle.Stop();
                    }

                    Follower.maxSpeed = toggle ? 0 : DefaultSpeed;
                    break;
            }
        }

        #endregion

        #region IResettable Members

        public virtual void Reset()
        {
            IsCrashed = false;
            ForcePosition = Vector3.zero;
            ForcePower = 0;
            Health = maxHealth;
            _isDead = false;
            _currentRandomTransform = null;
            hitParticle.Stop();
            hitParticle.Clear();
            stunParticle.Stop();
            stunParticle.Clear();
            healthManager.ResetHealthBar();
            IsHealthbarDisabled = false;
        }

        #endregion
    }
}
