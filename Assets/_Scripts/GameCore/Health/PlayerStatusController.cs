using System;
using System.Collections;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.BuffSystem;
using GameCore.Player;
using GameCore.Player.Input;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Health
{
    public class PlayerStatusController : MonoBehaviour, IDamageable, IBuffable
    {
        #region Serializable Fields

        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxArmor = 100f;
        [Range(0, 100)] [SerializeField] private float dodgeChance;
        [SerializeField] private int xpCount;
        [SerializeField] private int purchasePodCount;
        [SerializeField] private int killCount;
        [SerializeField] private float givenDamage;
        [SerializeField] private GameObject playerRefillFX;
        [SerializeField] private ParticleSystem healthChargeFX;

        /// <summary>
        ///     Called when the player's health changes. Current health and max health are passed as arguments.
        /// </summary>
        public event Action<float, float, bool> HealthChanged;

        public event Action<float, float, bool> ArmorChanged;

        public event Action<float> AttackSpeedMultiplierChanged;
        public event Action<float> AttackDamageMultiplierChanged;
        public event Action<float> MovementSpeedMultiplierChanged;

        public event Action<int> PurchasePodCountChanged;
        public event Action<float> WaveXpCountChanged;
        public event Action<float> XpCountChanged;
        public event Action<int> KillCountChanged;

        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
        {
        }

        public event Action<DamageSource> Died;

        public void OnLoseFocus()
        {
        }

        public event Action Refill;

        #endregion

        #region Fields

        private PlayerSkillController _playerSkillController;
        private PlayerController _playerController;
        private PlayerMovementController _playerMovementController;
        private DamageNumberManager _damageNumberManager;
        private WaveManager _waveManager;
        private Coroutine _healthCoroutine;
        private ObjectiveManager _objectiveManager;
        private ObjectiveManager ObjectiveManager => _objectiveManager ??= FindFirstObjectByType<ObjectiveManager>();
        private IGameService _gameService;
        private float _currentHealth;
        private string _maxArmorSkillId;
        private string _armorSkillId;
        private string _healthSkillId;
        private string _dodgeSkillId;
        private string _maxHealthSkillId;
        private float _currentArmor;
        private float _attackSpeedMultiplier = 1f;
        private float _attackDamageMultiplier = 1f;
        private float _movementSpeedMultiplier = 1f;
        private DamageSource _lastTakenDamageSource;
        private IAnalyticsService _analyticsService;

        #endregion

        public string SpecificDamageType => null;
        public BoxCollider Bounds { get; }
        public float Health => _currentHealth;
        public float Armor => _currentArmor;
        public float MaxArmor => maxArmor;
        public float MaxHealth => maxHealth;
        public Vector3 Position => transform.position;
        public Vector3 ForcePosition { get; }
        public float ForcePower { get; }
        public Transform RandomTransform { get; }

        public Transform Transform => transform;

        public int PurchasePodCount => purchasePodCount;

        public int KillCount
        {
            get => killCount;

            set => killCount = value;
        }


        public bool IsRefillActive { get; set; } = true;

        public float GivenDamage => givenDamage;
        public float DodgeChance => dodgeChance;

        public bool IsDead { get; private set; }
        public bool IsNotDamageable { get; }

        public float AttackSpeedMultiplier
        {
            get => _attackSpeedMultiplier;
            set
            {
                _attackSpeedMultiplier = value;
                AttackSpeedMultiplierChanged?.Invoke(value);
            }
        }

        public float AttackDamageMultiplier
        {
            get => _attackDamageMultiplier;
            set
            {
                _attackDamageMultiplier = value;
                AttackDamageMultiplierChanged?.Invoke(value);
            }
        }

        public float MovementSpeedMultiplier
        {
            get => _movementSpeedMultiplier;
            set
            {
                _movementSpeedMultiplier = value;
                MovementSpeedMultiplierChanged?.Invoke(value);
            }
        }

        #region Unity Methods

        [Inject]
        private void Initialize(IAnalyticsService analyticsService, DamageNumberManager damageNumberManager,
            WaveManager waveManager, IGameService gameService)
        {
            _damageNumberManager = damageNumberManager;
            _analyticsService = analyticsService;
            _waveManager = waveManager;
            _gameService = gameService;
        }

        private void Awake()
        {
            _playerSkillController = GetComponent<PlayerSkillController>();
            _playerController = GetComponent<PlayerController>();
            _playerMovementController = GetComponent<PlayerMovementController>();
            _currentHealth = maxHealth;
            _currentArmor = 0;
        }

        private void Start()
        {
            HealthChanged?.Invoke(_currentHealth, maxHealth, true);
            ArmorChanged?.Invoke(_currentArmor, maxArmor, true);
        }

        private void OnEnable()
        {
            _playerMovementController.InBaseChanged += ChargeHealthByInBase;
            _playerSkillController.OnSkillUpgrade += AdjustHealth;
            _playerSkillController.OnSkillUpgrade += AdjustMaxHealth;
            _playerSkillController.OnSkillUpgrade += AdjustMaxArmor;
            _playerSkillController.OnSkillUpgrade += AdjustDodgeChance;
            _playerSkillController.OnResetSkill += ResetManagement;
        }


        private void OnDestroy()
        {
            _playerMovementController.InBaseChanged -= ChargeHealthByInBase;
            _playerSkillController.OnSkillUpgrade -= AdjustHealth;
            _playerSkillController.OnSkillUpgrade -= AdjustMaxHealth;
            _playerSkillController.OnSkillUpgrade -= AdjustMaxArmor;
            _playerSkillController.OnSkillUpgrade -= AdjustDodgeChance;
            _playerSkillController.OnResetSkill -= ResetManagement;
        }

        #endregion

        #region Public Methods

        public void AdjustMaxArmor(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.MaxShield) return;

            PlayerSkillController.Calculate(ref maxArmor, ref _maxArmorSkillId, upgradeDetail);

            AdjustArmor(maxArmor);
        }

        public void AdjustArmor(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.ShieldCapacity) return;

            PlayerSkillController.Calculate(ref _currentArmor, ref _armorSkillId, upgradeDetail);

            ArmorChanged?.Invoke(_currentArmor,
                maxArmor,
                upgradeDetail.valueModifierType is ValueModifierType.Add or ValueModifierType.MultiplyIncrease);
        }

        public void AdjustArmor(float value)
        {
            _currentArmor = Math.Clamp(_currentArmor + value, 0, maxArmor);
            ArmorChanged?.Invoke(_currentArmor, maxArmor, value > 0);
        }

        private void AdjustHealth(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.HealthRestoration &&
                upgradeDetail.type != StatUpgradeType.HealthRegenPercent)
            {
                return;
            }


            if (upgradeDetail.type == StatUpgradeType.HealthRegenPercent)
            {
                var healthRegen = maxHealth;
                PlayerSkillController.Calculate(ref healthRegen, ref _healthSkillId, upgradeDetail);
                healthRegen -= maxHealth;
                _currentHealth += healthRegen;
            }
            else
            {
                PlayerSkillController.Calculate(ref _currentHealth, ref _healthSkillId, upgradeDetail);
            }


            if (_currentHealth > maxHealth) _currentHealth = maxHealth;

            HealthChanged?.Invoke(_currentHealth,
                maxHealth,
                upgradeDetail.valueModifierType is ValueModifierType.Add or ValueModifierType.MultiplyIncrease);
        }

        public void AdjustHealth(float value)
        {
            _currentHealth += value;

            if (_currentHealth > maxHealth) _currentHealth = maxHealth;

            HealthChanged?.Invoke(_currentHealth, maxHealth, value > 0);
        }


        public void RefillPlayer()
        {
            _currentHealth = maxHealth;
            HealthChanged?.Invoke(_currentHealth, maxHealth, true);
            IsDead = false;
            Refill?.Invoke();
            PlayRefillFX().Forget();
        }


        private void AdjustMaxHealth(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.MaxHealth) return;

            PlayerSkillController.Calculate(ref maxHealth, ref _maxHealthSkillId, upgradeDetail);

            HealthChanged?.Invoke(_currentHealth,
                maxHealth,
                upgradeDetail.valueModifierType is ValueModifierType.Add or ValueModifierType.MultiplyIncrease);
            AdjustHealth(new UpgradeDetail(StatUpgradeType.HealthRestoration,
                upgradeDetail.value,
                upgradeDetail.valueModifierType));
        }

        public void AdjustDodgeChance(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.DodgeChance) return;

            PlayerSkillController.Calculate(ref dodgeChance, ref _dodgeSkillId, upgradeDetail);
        }

        public void HealOverTime(float amount, float duration)
        {
            StartCoroutine(ApplyHOT(amount, duration));
        }

        public void AdjustXpValue(int value)
        {
            XpCountChanged?.Invoke(value);
            if (!_waveManager.IsWaveActive && !ObjectiveManager.IsProgress) return;

            xpCount += value;
            AdjustPurchasePodValue(value);
            WaveXpCountChanged?.Invoke(value);
        }

        public void AdjustPurchasePodValue(int value)
        {
            purchasePodCount = Mathf.Max(0, purchasePodCount + value);
            PurchasePodCountChanged?.Invoke(purchasePodCount);
        }

        public void RecordGivenDamage(float damage)
        {
            givenDamage += damage;
        }

        public void RecordKill(EnemyType enemyType)
        {
            killCount++;
            KillCountChanged?.Invoke(killCount);
            AdjustXpValue((int) enemyType.baseXpDropValue);
        }

        public void SetupHealthAndArmor()
        {
            _currentHealth = maxHealth;
            _currentArmor = 0;
            HealthChanged?.Invoke(_currentHealth, maxHealth, true);
            ArmorChanged?.Invoke(_currentArmor, maxArmor, true);
        }

        #endregion

        #region Private Methods

        private void ResetManagement()
        {
            givenDamage = 0;
            killCount = 0;
            xpCount = 0;
            PlayerSkillController.ResetSkill(ref maxArmor, _maxArmorSkillId);
            PlayerSkillController.ResetSkill(ref dodgeChance, _dodgeSkillId);
            PlayerSkillController.ResetSkill(ref _currentHealth, _healthSkillId);
            PlayerSkillController.ResetSkill(ref maxHealth, _maxHealthSkillId);
            KillCountChanged?.Invoke(killCount);
            WaveXpCountChanged?.Invoke(xpCount);
        }

        private IEnumerator ApplyDOT(float damage, float duration, float tickInterval = 0.5f)
        {
            var damagePerTick = damage / duration * tickInterval;
            var nextTickTime = 0f;

            while (duration > 0)
            {
                if (Time.time >= nextTickTime)
                {
                    _currentHealth -= damagePerTick;
                    TakeDamageActions(new DamageInfo(damagePerTick));

                    if (!IsDead && _currentHealth <= 0)
                    {
                        Die();
                        yield break;
                    }

                    HealthChanged?.Invoke(_currentHealth, maxHealth, false);
                    nextTickTime = Time.time + tickInterval;
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
                _currentHealth += healPerSecond * Time.deltaTime;
                if (_currentHealth > maxHealth) _currentHealth = maxHealth;

                HealthChanged?.Invoke(_currentHealth, maxHealth, amount > 0);

                duration -= Time.deltaTime;
                yield return null;
            }
        }


        private void Die()
        {
            if (IsDead) return;

            SendDeathAnalyticEvent();
            Died?.Invoke(_lastTakenDamageSource);
            IsDead = true;

            if (ObjectiveManager != null && ObjectiveManager.IsProgress) _gameService.IsPlayerDeadInMission = true;
        }

        private void SendDeathAnalyticEvent()
        {
            // var waveNumber = _waveManager.ActiveWaveIndex;
            // var timeSurvived = Time.timeSinceLevelLoad;
            _analyticsService.LogEvent(new EventParameters<string> {EventName = "player_dead"});
        }


        private void ChargeHealthByInBase(bool inBase)
        {
            if (_currentHealth >= maxHealth) return;

            switch (inBase)
            {
                case true when _healthCoroutine == null:
                    _healthCoroutine = StartCoroutine(ChargeHealth());
                    break;
                case false when _healthCoroutine != null:
                    StopCoroutine(_healthCoroutine);
                    _healthCoroutine = null;
                    break;
            }
        }

        private IEnumerator ChargeHealth()
        {
            var increaseAmount = maxHealth * 0.1f;
            while (_currentHealth < maxHealth)
            {
                yield return new WaitForSecondsRealtime(0.7f);
                healthChargeFX.Play();
                _damageNumberManager.UseHealDamageNumber(new Vector3(transform.position.x,
                        transform.position.y - 1.5f,
                        transform.position.z),
                    $"+{increaseAmount}");
                AdjustHealth(increaseAmount);
                _playerController.PlayOneShotAudio("Healing", 0.65f);
            }
        }

        private async UniTask PlayRefillFX()
        {
            playerRefillFX.SetActive(true);
            _playerController.VibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.Refill);
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            playerRefillFX.SetActive(false);
        }

        #endregion

        #region IDamageable Members

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (Helper.CalculateRngChange(dodgeChance))
            {
                _damageNumberManager.UseDamageNumber(transform.position, "Dodge!", true);
                LoggerNS.Log("Player dodged the attack");
                return;
            }


            var remainingDamage = damageInfo.Amount;
            if (_currentArmor > 0)
            {
                var armorDamage = Math.Min(_currentArmor, remainingDamage);
                _currentArmor = Math.Clamp(_currentArmor - armorDamage, 0, maxArmor);
                remainingDamage = Math.Clamp(remainingDamage - armorDamage, 0, damageInfo.Amount);
                ArmorChanged?.Invoke(_currentArmor, maxArmor, false);
            }

            if (remainingDamage <= 0) return;

            _currentHealth = Math.Clamp(_currentHealth - remainingDamage, 0, maxHealth);
            _lastTakenDamageSource = damageInfo.Source;

            TakeDamageActions(damageInfo);

            HealthChanged?.Invoke(_currentHealth, maxHealth, false);
            if (!IsDead && _currentHealth <= 0) Die();
        }

        private void TakeDamageActions(DamageInfo damageInfo)
        {
            try
            {
                _playerController.DamageNumberManager.UseDamageNumber(transform.position,
                    Mathf.CeilToInt(damageInfo.Amount).ToString(),
                    true);
                _playerController.VibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.HitPlayer);
            }
            catch (Exception ex)
            {
                LoggerNS.LogError($"Error in TakeDamage Actions: {ex.Message}");
            }
        }

        public void TakeDOT(DamageInfo damageInfo, float duration)
        {
            StartCoroutine(ApplyDOT(damageInfo.Amount, duration));
        }

        #endregion

        public async UniTask ApplyBuff(Buff buff)
        {
            switch (buff.Type)
            {
                case Buff.BuffType.Damage:
                    AttackDamageMultiplier = buff.Value;
                    var originalDamageMultiplier = buff.Value;
                    await UniTask.Delay(TimeSpan.FromSeconds(buff.Time));
                    if (AttackDamageMultiplier == originalDamageMultiplier)
                        AttackDamageMultiplier = 1f;
                    else
                        AttackDamageMultiplier /= originalDamageMultiplier;

                    break;
                case Buff.BuffType.MovementSpeed:
                    MovementSpeedMultiplier *= buff.Value;
                    var originalSpeedMultiplier = buff.Value;
                    await UniTask.Delay(TimeSpan.FromSeconds(buff.Time));
                    if (MovementSpeedMultiplier == originalSpeedMultiplier)
                        MovementSpeedMultiplier = 1f;
                    else
                        MovementSpeedMultiplier /= originalSpeedMultiplier;

                    break;
                case Buff.BuffType.AttackSpeed:
                    AttackSpeedMultiplier *= buff.Value;
                    var originalMultiplier = buff.Value;
                    await UniTask.Delay(TimeSpan.FromSeconds(buff.Time));
                    if (AttackSpeedMultiplier == originalMultiplier)
                        AttackSpeedMultiplier = 1f;
                    else
                        AttackSpeedMultiplier /= originalMultiplier;

                    break;
            }
        }
    }
}
