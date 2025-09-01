using System;
using GameCore.Player;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.AbilitySystem
{
    public abstract class Ability : ScriptableObject, IAbility
    {
        #region Public Fields

        public string Name;
        public IAbilityService.AbilityType Type;
        public float CooldownTime;
        public Sprite IconSprite;

        #endregion

        #region Serializable Fields

        [SerializeField] protected string ammoAssetKey;
        [SerializeField] protected int ammoPoolSize = 15;
        [SerializeField] protected string hitEffectKey = "BasicAmmoExplosion";
        [SerializeField] protected int hitEffectPoolSize = 15;
        [SerializeField] protected float damage;
        [SerializeField] protected float range = 10f;
        [SerializeField] protected float duration = 2f;

        #endregion

        #region Fields

        protected float currentCooldown;
        protected bool isOnCooldown;

        protected GameObject User;
        protected IObjectResolver Resolver;

        #endregion

        #region Public Methods

        public void Setup(IObjectResolver resolver, GameObject user)
        {
            Resolver = resolver;
            User = user;

            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        public virtual bool CanUse()
        {
            return !isOnCooldown;
        }

        public virtual void Execute()
        {
            isOnCooldown = true;
            currentCooldown = CooldownTime;
            AbilityUsed?.Invoke();
        }

        public virtual void UpdateCooldown()
        {
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown <= 0)
                {
                    isOnCooldown = false;
                }
            }
        }

        #endregion

        public string AbilityName => Name;
        public bool IsOnCooldown => isOnCooldown;
        public float MaxCooldownTime => CooldownTime;
        public float CurrentCooldownTime => currentCooldown;
        public float Radius
        {
            get => range;
            set => range = value;
        }
        public float Damage
        {
            get => damage;
            set => damage = value;
        }
        public float Duration
        {
            get => duration;
            set => duration = value;
        }
        public Sprite Icon => IconSprite;

        public event Action AbilityUsed;
    }
}
