using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Utilities;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace GameCore.Player.WeaponSystem
{
    public class PlayerWeaponController
    {
        public event Action OnWeaponInitialized;
        private IObjectResolver _resolver;

        #region PlayerShootingMode enum

        public enum PlayerShootingMode
        {
            Ranged,
            Melee
        }

        #endregion

        #region Constructor

        public PlayerWeaponController(PlayerController pc, PlayerStatusController playerStatusController,
            MobManager mobManager, PlayerAnimationController playerAnimationController,
            PlayerSkillController playerSkillController, IObjectResolver resolver)
        {
            _playerSkillController = playerSkillController;
            _playerStatusController = playerStatusController;
            _mobManager = mobManager;
            _playerAnimationController = playerAnimationController;
            _playerController = pc;

            _resolver = resolver;

            if (_Rweapon != null)
                _playerStatusController.AttackSpeedMultiplierChanged += _Rweapon.OnIntervalMultiplierChanged;
            if (_Lweapon != null)
                _playerStatusController.AttackSpeedMultiplierChanged += _Lweapon.OnIntervalMultiplierChanged;

            _playerSkillController.OnSkillUpgrade += AdjustWeapon;
            _playerSkillController.OnResetSkill += ResetWeapon;
            _rangeIndicatorCheckCancellationSource = new CancellationTokenSource();
            var token = _rangeIndicatorCheckCancellationSource.Token;
            CheckRangeIndicatorEnable(token);
        }

        #endregion

        #region Fields

        private readonly List<StatUpgradeType> _statUpgradeTypes = new()
        {
            StatUpgradeType.Damage,
            StatUpgradeType.CriticalHitChance,
            StatUpgradeType.AttackSpeed,
            StatUpgradeType.MeleeAttacksSpeed,
            StatUpgradeType.ProjectileCount
        };

        private PlayerShootingMode _currentShootingMode = PlayerShootingMode.Ranged;
        private bool _isFirstTimeLWeapon = true;
        private bool _isFirstTimeRWeapon = true;
        private DateTime _lastFireTimeL = DateTime.MinValue, _lastFireTimeR = DateTime.MinValue;
        private DateTime _lastMeleeTime = DateTime.MinValue;
        private Weapon _Lweapon, _Rweapon;
        private MeleeWeapon _meleeWeapon;
        private MobManager _mobManager;
        private PlayerAnimationController _playerAnimationController;
        private PlayerController _playerController;
        private PlayerSkillController _playerSkillController;
        private PlayerStatusController _playerStatusController;
        private CancellationTokenSource _rangeIndicatorCheckCancellationSource;
        private List<string> _weaponKeys;


        private WeaponSlot[] _weaponSlots;

        #endregion

        #region Properties

        public int EquippedWeaponCount => (_Lweapon == null ? 0 : 1) + (_Rweapon == null ? 0 : 1);

        public Weapon Lweapon
        {
            get => _Lweapon;
            set => _Lweapon = value;
        }

        public Weapon Rweapon
        {
            get => _Rweapon;
            set => _Rweapon = value;
        }

        public bool IsFirstTimeLWeapon
        {
            get => _isFirstTimeLWeapon;
            set => _isFirstTimeLWeapon = value;
        }

        public bool IsFirstTimeRWeapon
        {
            get => _isFirstTimeRWeapon;
            set => _isFirstTimeRWeapon = value;
        }

        public DateTime LastMeleeTime
        {
            get => _lastMeleeTime;
            set => _lastMeleeTime = value;
        }

        public DateTime LastFireTimeL
        {
            get => _lastFireTimeL;
            set => _lastFireTimeL = value;
        }

        public DateTime LastFireTimeR
        {
            get => _lastFireTimeR;
            set => _lastFireTimeR = value;
        }

        public PlayerShootingMode CurrentShootingMode
        {
            get => _currentShootingMode;
            set => _currentShootingMode = value;
        }

        public MeleeWeapon MeleeWeapon
        {
            get => _meleeWeapon;
            set => _meleeWeapon = value;
        }

        #endregion

        #region Public Methods

        public async void ToggleMelee(bool isMelee)
        {
            _currentShootingMode = isMelee ? PlayerShootingMode.Melee : PlayerShootingMode.Ranged;

            if (isMelee)
                _lastMeleeTime = DateTime.Now;

            if(_Lweapon) _Lweapon.gameObject.SetActive(!isMelee);
            if(_Rweapon) _Rweapon.gameObject.SetActive(!isMelee);
            if(_meleeWeapon) _meleeWeapon.gameObject.SetActive(isMelee);

            if (_playerAnimationController)
            {
                _playerAnimationController.ToggleAiming(false);
                _playerAnimationController.ToggleMeleeState(isMelee, 0);
            }
        }

        /// <summary>
        /// Switches to the weapon with the given name
        /// </summary>
        /// <param name="weaponName">Weapon addressable key as string</param>
        /// <param name="weaponSlotType">Weapon slot type</param>
        /// <returns>Old weapon name</returns>
        public async UniTask<string> SwitchToWeapon(string weaponName, WeaponSlot.SlotType weaponSlotType)
        {
            string oldWeaponName = String.Empty;
            Weapon oldWeapon = null;
            Weapon newWeapon = null;

            //First slot should be melee weapon, then R and L hand weapons
            foreach (var weaponSlot in _weaponSlots)
            {
                if (weaponSlot.SlotPlacement != weaponSlotType) continue;

                oldWeaponName = weaponSlot.CurrentWeapon != null ? weaponSlot.CurrentWeapon.WeaponAddressableKey : null;
                oldWeapon = weaponSlot.CurrentWeapon;

                var operation = Addressables.LoadAssetAsync<GameObject>(weaponName);
                var weaponPrefab = await operation;

                var weaponGo = _resolver.Instantiate(weaponPrefab);

                if (weaponGo.TryGetComponent<RangedWeapon>(out var rangedWeapon))
                {
                    rangedWeapon.SetObjectResolver(_resolver);
                }

                operation.BindTo(weaponPrefab);

                var weapon = weaponGo.GetComponent<Weapon>();
                weapon.WeaponAddressableKey = weaponName;

                weaponSlot.RemoveWeapon();
                weaponSlot.InstallWeapon(weapon);

                newWeapon = weaponSlot.CurrentWeapon;
                weapon.ListenToBuffs(_playerStatusController);
                weapon.MobManager = _mobManager;
                weapon.PlayerController = _playerController;

                if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.LeftHand)
                {
                    _Lweapon = weaponSlot.CurrentWeapon;
                    _Lweapon.DefaultRotation = _Lweapon.transform.localRotation;
                }
                else if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.RightHand)
                {
                    _Rweapon = weaponSlot.CurrentWeapon;
                    _Rweapon.DefaultRotation = _Rweapon.transform.localRotation;
                }
                else if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.Melee)
                {
                    _meleeWeapon = (MeleeWeapon) weaponSlot.CurrentWeapon;
                    _meleeWeapon.gameObject.SetActive(false);
                    _playerAnimationController.SetMeleeWeapon(_meleeWeapon);
                }

                _playerController.InvokeWeaponSwitched(oldWeapon, newWeapon);
            }

            _playerAnimationController.SetHandWields(_Lweapon != null, _Rweapon != null);

            return oldWeaponName;
        }

        public bool IsDamageTypeCompatible(IDamageable damageable)
        {
            if (string.IsNullOrEmpty(damageable.SpecificDamageType))
            {
                return true;
            }

            return Enum.TryParse(damageable.SpecificDamageType, out PlayerShootingMode parsedMode) &&
                parsedMode == _currentShootingMode;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            if (_playerSkillController != null)
            {
                _playerSkillController.OnSkillUpgrade -= AdjustWeapon;
                _playerSkillController.OnResetSkill -= ResetWeapon;
            }

            if (_rangeIndicatorCheckCancellationSource != null)
            {
                _rangeIndicatorCheckCancellationSource.Cancel();
                _rangeIndicatorCheckCancellationSource.Dispose();
                _rangeIndicatorCheckCancellationSource = null;
            }
        }


        public void HandleRangeIndicatorCheck(bool isDriving)
        {
            if (_rangeIndicatorCheckCancellationSource != null)
            {
                _rangeIndicatorCheckCancellationSource.Cancel();
                _rangeIndicatorCheckCancellationSource.Dispose();
                _rangeIndicatorCheckCancellationSource = null;
            }

            if (!isDriving)
            {
                _rangeIndicatorCheckCancellationSource = new CancellationTokenSource();
                var token = _rangeIndicatorCheckCancellationSource.Token;
                CheckRangeIndicatorEnable(token);
            }
            else
            {
                _playerController.ToggleRangeIndicator(false);
            }
        }

        public float CalculateFireDamage()
        {
            return _Rweapon != null ? _Rweapon.Damage : _Lweapon != null ? _Lweapon.Damage : 1;
        }

        public float CalculateFireInterval()
        {
            return _Rweapon != null ? _Rweapon.FireInterval : _Lweapon != null ? _Lweapon.FireInterval : 1;
        }

        public float CalculateCriticalHitChance()
        {
            return _Rweapon != null ? _Rweapon.CriticalHitChance : _Lweapon != null ? _Lweapon.CriticalHitChance : 1;
        }

        public float CalculateCriticalHitDamage()
        {
            return _Rweapon != null ? _Rweapon.CritDamage : _Lweapon != null ? _Lweapon.CritDamage : 1;
        }

        #endregion

        #region Private Methods

        private async UniTask InitializeWeapons()
        {
            int i = 0;
            _weaponKeys = _playerController.StartWeaponKeys.ToList();
            var weaponData = GetPlayerWeaponData();
            if (weaponData.unlockedWeapons is not {Count: > 0})
            {
                SaveLoadHelper.UpdateData<PlayerWeaponData>(data =>
                {
                    data.unlockedWeapons.Add(weaponData.usingWeapon);
                });
            }

            var usingWeapon = GetPlayerWeaponData().usingWeapon;
            _weaponKeys.Add(weaponData.usingWeapon);

            //First slot should be melee weapon, then R and L hand weapons
            foreach (var weaponSlot in _weaponSlots)
            {
                if (i >= _weaponKeys.Count) break;

                string weaponWeaponAddressableKey = _weaponKeys[i];
                var operation = Addressables.LoadAssetAsync<GameObject>(weaponWeaponAddressableKey);
                var weaponPrefab = await operation;

                var weaponGo = _resolver.Instantiate(weaponPrefab);
                operation.BindTo(weaponPrefab);

                if (weaponGo.TryGetComponent<RangedWeapon>(out var rangedWeapon))
                {
                    rangedWeapon.SetObjectResolver(_resolver);
                }

                var weapon = weaponGo.GetComponent<Weapon>();
                weapon.WeaponAddressableKey = weaponWeaponAddressableKey;

                weaponSlot.InstallWeapon(weapon);
                _playerController.UpdateRangeIndicatorRange(weapon.Range);
                weapon.ListenToBuffs(_playerStatusController);
                weapon.MobManager = _mobManager;
                weapon.PlayerController = _playerController;


                if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.LeftHand)
                {
                    _Lweapon = weaponSlot.CurrentWeapon;
                    _Lweapon.DefaultRotation = _Lweapon.transform.localRotation;
                }
                else if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.RightHand)
                {
                    _Rweapon = weaponSlot.CurrentWeapon;
                    _Rweapon.DefaultRotation = _Rweapon.transform.localRotation;
                    _playerController.InvokeWeaponInitialized(_Rweapon);
                }
                else if (weaponSlot.SlotPlacement == WeaponSlot.SlotType.Melee)
                {
                    _meleeWeapon = (MeleeWeapon) weaponSlot.CurrentWeapon;
                    _meleeWeapon.gameObject.SetActive(false);
                    _playerAnimationController.SetMeleeWeapon(_meleeWeapon);
                }

                i++;
            }

            _playerAnimationController.SetHandWields(_Lweapon != null, _Rweapon != null);
            OnWeaponInitialized?.Invoke();
        }


        private void AdjustWeapon(UpgradeDetail upgradeDetail)
        {
            if (!_statUpgradeTypes.Contains(upgradeDetail.type))
            {
                return;
            }

            void Adjust(Weapon weapon)
            {
                if (weapon == null) return;

                switch (upgradeDetail.type)
                {
                    case StatUpgradeType.Damage:
                        weapon.AdjustDamage(upgradeDetail);
                        break;
                    case StatUpgradeType.CriticalHitChance:
                        weapon.AdjustCriticalHitChance(upgradeDetail);
                        break;
                    case StatUpgradeType.AttackSpeed or StatUpgradeType.MeleeAttacksSpeed:
                        weapon.AdjustFireInterval(upgradeDetail);
                        break;
                    case StatUpgradeType.ProjectileCount:
                        weapon.AdjustPelletCount(upgradeDetail);
                        break;
                }
            }

            // Refactor melee weapon adjustment later on to be more clear
            if (upgradeDetail.type == StatUpgradeType.MeleeAttacksSpeed)
            {
                Adjust(_meleeWeapon);
            }
            else
            {
                Adjust(_Lweapon);
                Adjust(_Rweapon);
            }
        }

        private void ResetWeapon()
        {
            if (_meleeWeapon != null)
            {
                _meleeWeapon.ResetSkills();
            }

            if (_Lweapon != null)
            {
                _Lweapon.ResetSkills();
            }

            if (_Rweapon != null)
            {
                _Rweapon.ResetSkills();
            }
        }

        private async UniTask CheckRangeIndicatorEnable(CancellationToken token)
        {
            if (_playerController.PlayerMovementMode == PlayerMovementMode.Drive) return;

            await UniTask.WaitUntil(() => _Rweapon != null || _Lweapon != null, cancellationToken: token);

            if (_Rweapon == null && _Lweapon == null) return;

            var range = _Rweapon?.Range ?? _Lweapon?.Range ?? 0;

            while (!token.IsCancellationRequested)
            {
                var isEnemyInRange = _mobManager.IsEnemyInRangeAndVisible(range * 1.5f);
                _playerController.ToggleRangeIndicator(isEnemyInRange);

                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, token);
            }
        }

        private PlayerWeaponData GetPlayerWeaponData() => SaveLoadHelper.TryLoadPersistentData<PlayerWeaponData>();

        #endregion

        public void UpdateWeaponSlots(WeaponSlot[] weaponSlots)
        {
            _weaponSlots = weaponSlots;

            InitializeWeapons();
        }
    }

    [Serializable]
    public class PlayerWeaponData
    {
        public string usingWeapon = "Pistol";
        public List<string> unlockedWeapons = new List<string>();
    }
}
