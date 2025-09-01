using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Player.WeaponSystem
{
    public class Weapon : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private string weaponModelKey;
        [SerializeField] protected string hitEffectKey = "BasicAmmoExplosion";
        [SerializeField] protected WeaponType typeOfWeapon;
        [SerializeField] protected float damage;
        [SerializeField] protected float fireInterval = 0.2f;
        [SerializeField] [Range(0, 100)] protected float criticalHitChance;
        [SerializeField] protected float critDamage;
        [SerializeField] protected int hitEffectPoolSize = 15;
        [SerializeField] protected float range = 4f;
        [SerializeField] protected float pellets = 1;
        [SerializeField] private Sprite weaponIcon;
        [SerializeField] protected string oneShotAudioKey;
        [SerializeField] protected bool usesMultipleSounds;
        [ConditionalField(nameof(usesMultipleSounds), false, true)]
        [SerializeField] protected string[] oneShotAudioKeys;

        #endregion

        #region Fields

        private string _weaponFireIntervalSkillId;
        private string _weaponDamageSkillId;
        private string _weaponCriticalDamageSkillId;
        private string _weaponCriticalHitChanceSkillId;
        private string _projectileSkillId;


        private Transform _lookAtTarget;
        protected PlayerStatusController _playerStatusController;
        protected float _currentFireIntervalMultiplier = 1f;
        protected float _currentDamageMultiplier = 1f;
        protected bool _isLocked;

        public enum WeaponType
        {
            Melee,
            Ranged
        }


        public float Damage
        {
            get => damage * _currentDamageMultiplier;
            set => damage = value;
        }

        public float CritDamage => critDamage;
        public float CriticalHitChance => criticalHitChance;

        public float FireInterval
        {
            get => fireInterval / _currentFireIntervalMultiplier;
            set => fireInterval = value;
        }

        public string WeaponAddressableKey { get; set; }

        public WeaponType TypeOfWeapon
        {
            get => typeOfWeapon;
            set => typeOfWeapon = value;
        }

        public float Range
        {
            get => range;
            set => range = value;
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        public MobManager MobManager { get; set; }
        public PlayerController PlayerController { get; set; }

        public Sprite WeaponIcon
        {
            get => weaponIcon;
            set => weaponIcon = value;
        }

        public Quaternion DefaultRotation { get; set; }

        #endregion

        #region Unity Methods

        protected virtual async void Awake()
        {
            if (weaponModelKey != "")
            {
                var weaponModel = await Addressables.LoadAssetAsync<GameObject>(weaponModelKey).BindTo(gameObject);
                var weaponModelGo = Instantiate(weaponModel, transform);
            }
        }

        protected void OnDestroy()
        {
            if (_playerStatusController)
            {
                _playerStatusController.AttackSpeedMultiplierChanged -= OnIntervalMultiplierChanged;
                _playerStatusController.AttackDamageMultiplierChanged -= OnAttackDamageMultiplierChanged;
            }
        }

        #endregion

        #region Public Methods

        public virtual void FireAt(IDamageable target, DamageSource damageSource = DamageSource.Player,
            float maxDistance = 0)
        {
        }

        protected virtual async void ShowHitEffect(Vector3 position)
        {
            //explosion object from addressable pool
            var explosionPoolObject = await ObjectManager.GetObject(hitEffectKey);
            explosionPoolObject.transform.position = position;
            await UniTask.Delay(2000);
            if (explosionPoolObject != null)
                explosionPoolObject.SetActive(false);
        }

        public void ListenToBuffs(PlayerStatusController playerStatusController)
        {
            _playerStatusController = playerStatusController;
            playerStatusController.AttackDamageMultiplierChanged += OnAttackDamageMultiplierChanged;
            playerStatusController.AttackSpeedMultiplierChanged += OnIntervalMultiplierChanged;
        }


        public void OnIntervalMultiplierChanged(float value)
        {
            _currentFireIntervalMultiplier = value;
        }

        public void OnAttackDamageMultiplierChanged(float value)
        {
            _currentDamageMultiplier = value;
        }

        public void AdjustPelletCount(UpgradeDetail upgradeDetail)
        {
            Debug.LogError("AdjustPelletCount");
            PlayerSkillController.Calculate(ref pellets, ref _projectileSkillId, upgradeDetail);
        }

        public void AdjustFireInterval(UpgradeDetail upgradeDetail)
        {
            PlayerSkillController.Calculate(ref fireInterval, ref _weaponFireIntervalSkillId, upgradeDetail);
        }

        public void AdjustDamage(UpgradeDetail upgradeDetail)
        {
            PlayerSkillController.Calculate(ref damage, ref _weaponDamageSkillId, upgradeDetail);
        }

        public void AdjustCriticalHitChance(UpgradeDetail upgradeDetail)
        {
            PlayerSkillController.Calculate(ref criticalHitChance, ref _weaponCriticalHitChanceSkillId, upgradeDetail);
        }

        public void AdjustCritDamage(UpgradeDetail upgradeDetail)
        {
            PlayerSkillController.Calculate(ref critDamage, ref _weaponCriticalDamageSkillId, upgradeDetail);
        }

        public void ResetSkills()
        {
            PlayerSkillController.ResetSkill(ref fireInterval, _weaponFireIntervalSkillId);
            PlayerSkillController.ResetSkill(ref damage, _weaponDamageSkillId);
            PlayerSkillController.ResetSkill(ref criticalHitChance, _weaponCriticalHitChanceSkillId);
            PlayerSkillController.ResetSkill(ref critDamage, _weaponCriticalDamageSkillId);
            PlayerSkillController.ResetSkill(ref pellets, _projectileSkillId);
        }

        #endregion
    }
}
