using UnityEngine;
using System;
using System.Collections.Generic;
using Addler.Runtime.Core.Pooling;
using GameCore.Health;
using GameCore.Spawner;
using Interfaces;

namespace GameCore.Player.WeaponSystem
{
    public class ProjectileBase : MonoBehaviour, IPoolable, IResettable
    {
        #region Serializable Fields

        [SerializeField] protected ParticleSystem trailParticle;
        [SerializeField] protected float velocity = 10f;
        [SerializeField] protected ProjectileHitType projectileHitType = ProjectileHitType.SingleTarget;

        #endregion

        public enum ProjectileHitType
        {
            SingleTarget,
            AreaOfEffect
        }

        #region Fields

        protected bool _isSetup;
        protected IDamageable _target;
        protected List<IDamageable> _targetsAOE;
        protected Transform _targetRandomTransform;
        protected MobManager _mobManager;
        protected PlayerController _playerController;
        protected bool _causesFocus;

        #endregion

        #region Public Methods

        public virtual void Setup(Transform firePoint, IDamageable target, IAbility ability, DamageInfo _damageInfo,
            Vector3 missingTarget, MobManager mobManager, PlayerController playerController, bool causesFocus = false,
            float maxDistance = 0)
        {
            _mobManager = mobManager;
            _playerController = playerController;
            _causesFocus = causesFocus;
        }

        #endregion

        protected virtual void OnDisable()
        {
            OnReturnToPool?.Invoke();
        }

        #region IPoolable Members

        public Action OnReturnToPool { get; set; }
        public Action<Vector3> OnReturnToPoolByPosition { get; set; }

        #endregion

        #region IResettable Members

        public virtual void Reset()
        {
            _isSetup = false;
            _target = null;
            if (trailParticle) trailParticle.Clear();
        }

        #endregion
    }
}
