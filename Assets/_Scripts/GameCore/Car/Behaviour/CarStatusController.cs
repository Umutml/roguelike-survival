using UnityEngine;
using System;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Utilities;
using GameCore.Health;
using GameCore.Player;
using GameCore.Scriptables;
using Interfaces;
using VContainer;


public class CarStatusController : MonoBehaviour, IDamageable
{
    #region Actions

    public Action<float> OnChangeCarStatus;
    public Action OnDeadCar;

    #endregion


    #region Fields

    private PlayerSkillController _playerSkillController;
    private CarMovementController _carMovementController;
    private CarController _carController;

    private IObjectResolver _resolver;
    private Vector3 _carVelocity;
    private float _currentHealth;
    private float _currentArmor;
    private float _maxArmor;
    private float _fuelCost = 5;
    private const float MaxTime = 2;
    private float _currentTime;
    private bool _isDead;
    private string _maxArmorSkillId;
    private string _maxDurabilitySkillId;
    private string _fuelSkillId;
    private DamageSource _lastTakenDamageSource;

    private readonly string OBSTACLE = "Obstacle";

    #endregion


    #region Properties

    public IObjectResolver Resolver
    {
        get => _resolver;
        set
        {
            _resolver = value;
            SubscribeEvents();
        }
    }

    public Vector3 CarVelocity
    {
        get => _carVelocity;
        set => _carVelocity = value;
    }

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }


    public float MaxHealth { get; set; }

    public float FuelCost => _fuelCost;

    public float MaxArmor
    {
        get => _maxArmor;
        set
        {
            _maxArmor = value;
            _currentArmor = value;
        }
    }

    public float CurrentArmor
    {
        get => _currentArmor;
        set => _currentArmor = value;
    }

    public float Health => _currentHealth;
    public Vector3 Position { get; }
    public Vector3 ForcePosition { get; }
    public float ForcePower { get; }
    public Transform RandomTransform { get; }
    public Transform Transform => transform;
    public bool IsDead => _isDead;
    public bool IsNotDamageable { get; }

    public string SpecificDamageType => null;
    public BoxCollider Bounds { get; }

    void IDamageable.TakeDamage(DamageInfo damageInfo)
    {
        _lastTakenDamageSource = damageInfo.Source;
        TakeDamage(damageInfo.Amount);
        _carController.VibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.HitCar);
    }

    public void TakeDOT(DamageInfo damageInfo, float duration)
    {
    }

    public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce)
    {
    }

    public event Action<DamageSource> Died;

    public void OnLoseFocus()
    {
    }

    #endregion


    #region Unity Methods

    private void OnEnable()
    {
        _currentTime = MaxTime;
        GetComponents();
    }


    private void Update()
    {
        DecreaseHealthByTime();
    }

    private void OnDestroy()
    {
        if (_playerSkillController != null)
        {
            _playerSkillController.OnSkillUpgrade -= AdjustFuel;
            _playerSkillController.OnSkillUpgrade -= AdjustArmor;
            _playerSkillController.OnResetSkill -= ResetSkills;
        }

        _carController.CarManager.OnResetCarHealth -= ResetHealth;
    }

    #endregion


    #region Public Methods

    public void TakeDamage(float damage)
    {
        if (_isDead) return;


        if (_carController.CarManager.IsBridgeDrive && _currentHealth <= 50) return;

        var remainingDamage = damage;

        if (_currentArmor > 0)
        {
            var armorDamage = Math.Min(_currentArmor, remainingDamage);
            _currentArmor = Math.Clamp(_currentArmor - armorDamage, 0, MaxArmor);
            remainingDamage = Math.Clamp(remainingDamage - armorDamage, 0, damage);
        }

        if (remainingDamage <= 0)
        {
            LoggerNS.Log("Damage blocked by armor");
            return;
        }

        _currentHealth -= remainingDamage;


        if (_currentHealth <= 30)
        {
            _carController.CarEffectController.SetFireParticles(true);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }

        OnChangeCarStatus?.Invoke(_currentHealth / MaxHealth);
    }


    public void SetupHealth()
    {
        _currentHealth = MaxHealth;
    }

    public void ResetHealth()
    {
        SetupHealth();
        OnChangeCarStatus?.Invoke(_currentHealth / MaxHealth);
    }

    #endregion


    #region Private Methods

    private void DecreaseHealthByTime()
    {
        if (_isDead) return;
        if (!_carMovementController.IsDrive) return;

        if (_currentTime <= 0)
        {
            TakeDamage(_fuelCost);
            _currentTime = MaxTime;
        }
        else
        {
            _currentTime -= Time.deltaTime;
        }
    }


    private void OnCollisionEnter(Collision other)
    {
        OnHitObstacle(other);
    }


    private void OnHitObstacle(Collision other)
    {
        if (other.gameObject.CompareTag(OBSTACLE))
        {
            var damage = _carVelocity.magnitude / 2.5f;
            TakeDamage(damage);
            _carController.DamageNumberManager.UseDamageNumber(transform.position,
                Mathf.RoundToInt(damage).ToString(),
                true);
            _carController.CarEffectController.PlayHitParticle(other.contacts[0].point);
            _carController.VibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.HitCar);
        }
    }


    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        Died?.Invoke(_lastTakenDamageSource);
        _isDead = true;
        OnDeadCar?.Invoke();
        SendCarDeadAnalytic();
        StartCoroutine(_carController.CarEffectController.ExplosionCar());
    }

    private void SendCarDeadAnalytic()
    {
        _resolver.Resolve<IAnalyticsService>().LogEvent(new EventParameters<string>
        {
            EventName = "car_crashed",
            AdjustToken = AdjustNsEventTokens.CarCrashed
        });
    }


    private void GetComponents()
    {
        _carMovementController = GetComponent<CarMovementController>();
        _carController = GetComponent<CarController>();
    }

    private void SubscribeEvents()
    {
        _playerSkillController = Resolver.Resolve<PlayerSkillController>();
        _playerSkillController.OnSkillUpgrade += AdjustFuel;
        _playerSkillController.OnSkillUpgrade += AdjustArmor;
        _playerSkillController.OnSkillUpgrade += AdjustMaxDurability;
        _playerSkillController.OnResetSkill += ResetSkills;
    }

    private void AdjustFuel(UpgradeDetail detail)
    {
        if (detail.type != StatUpgradeType.FuelCapacity)
        {
            return;
        }

        PlayerSkillController.Calculate(ref _fuelCost, ref _fuelSkillId, detail);
    }

    private void AdjustMaxDurability(UpgradeDetail detail)
    {
        if (detail.type != StatUpgradeType.CarMaxDurability)
        {
            return;
        }

        var maxDurability = MaxHealth;
        PlayerSkillController.Calculate(ref maxDurability, ref _maxDurabilitySkillId, detail);
        MaxHealth = maxDurability;
    }

    private void AdjustArmor(UpgradeDetail detail)
    {
        if (detail.type != StatUpgradeType.CarShield)
        {
            return;
        }

        PlayerSkillController.Calculate(ref _maxArmor, ref _maxArmorSkillId, detail);
    }

    private void ResetSkills()
    {
        var maxDurability = MaxHealth;
        PlayerSkillController.ResetSkill(ref _fuelCost, _fuelSkillId);
        PlayerSkillController.ResetSkill(ref _maxArmor, _maxArmorSkillId);
        PlayerSkillController.ResetSkill(ref maxDurability, _maxDurabilitySkillId);
        MaxHealth = maxDurability;
    }

    #endregion
}
