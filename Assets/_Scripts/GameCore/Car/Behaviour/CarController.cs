using System;
using System.Collections;
using _Scripts.Utilities;
using GameCore.Car;
using GameCore.Player;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using GameCore.Spawner;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using Pathfinding;
using UnityEngine;
using VContainer;


public class CarController : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private CarResources carResources;
    [SerializeField] private CarType carType;
    [SerializeField] private AutomaticWeapon automaticWeapon;
    [SerializeField] private DynamicGridObstacle dynamicGridObstacle;
    [SerializeField] private Collider dynamicGridObstacleCollider;
    [SerializeField] private CarAdditionalPartsController carAdditionalPartsController;
    [SerializeField] private GameObject BodyParent;

    #endregion


    #region Fields

    private CarMovementController _carMovementController;
    private CarZombieDetection _carZombieDetection;
    private CarEffectController _carEffectController;
    private CarStatusController _carStatusController;
    private CarArmorController _carArmorController;
    private CarAdditionalPartsController _carAdditionalPartsController;
    private MobManager _mobManager;
    private CarManager _carManager;
    private WaveManager _waveManager;
    private PlayerCarController _playerCarController;
    private TutorialSequenceController _tutorialSequenceController;
    private IObjectResolver _resolver;
    private PlayerMovementMode _playerMovementMode;
    private Vector3 _carSpawnPosition;
    private PlayerSkillController _playerSkillController;
    private Quaternion _carSpawnRotation;
    private PlayerController _player;
    private bool _isTutorialCar;
    private bool _isEngineOn = false;
    private EngineSound _engineSound;

    #endregion


    #region Properties

    public Vector3 CarSpawnPosition
    {
        get => _carSpawnPosition;
        set => _carSpawnPosition = value;
    }

    public Quaternion CarSpawnRotation
    {
        get => _carSpawnRotation;
        set => _carSpawnRotation = value;
    }


    public CarType CarType => carType;

    public PlayerController Player
    {
        get => _player;
        set { _player = value; }
    }

    public AutomaticWeapon AutomaticWeapon => automaticWeapon;


    public bool IsTutorialCar
    {
        get => _isTutorialCar;
        set
        {
            _isTutorialCar = value;
            _carStatusController.CurrentHealth = carResources.TutorialCarHealth;
            _carStatusController.MaxHealth = carResources.TutorialCarHealth;
        }
    }

    public float GetCarSpeed => _carMovementController.GetCarSpeed;

    public IObjectResolver Resolver
    {
        get => _resolver;
        set
        {
            _resolver = value;
            Setup();
        }
    }

    public CarZombieDetection CarZombieDetection => _carZombieDetection;
    public CarStatusController CarStatusController => _carStatusController;
    public CarEffectController CarEffectController => _carEffectController;
    public CarArmorController CarAmorController => _carArmorController;
    public CarAdditionalPartsController CarAdditionalPartsController => _carAdditionalPartsController;
    public CharacterController CharacterController => _carMovementController.CharacterController;
    public CarMovementController CarMovementController => _carMovementController;
    public PlayerMovementMode PlayeMovementMode => _playerMovementMode;

    public MobManager MobManager
    {
        get
        {
            if (_mobManager != null)
            {
                return _mobManager;
            }

            _mobManager = Resolver.Resolve<MobManager>();
            return _mobManager;
        }
    }

    public CarManager CarManager
    {
        get
        {
            if (_carManager != null)
            {
                return _carManager;
            }

            _carManager = Resolver.Resolve<CarManager>();
            _carManager.OnResetCarHealth += _carStatusController.ResetHealth;
            return _carManager;
        }
    }

    public WaveManager WaveManager
    {
        get
        {
            if (_waveManager != null)
            {
                return _waveManager;
            }

            _waveManager = Resolver.Resolve<WaveManager>();
            return _waveManager;
        }
    }
    public PlayerSkillController PlayerSkillController
    {
        get
        {
            if (_playerSkillController == null && Resolver != null)
            {
                _playerSkillController = Resolver.Resolve<PlayerSkillController>();
            }

            return _playerSkillController;
        }
    }

    public PlayerCarController PlayerCarController
    {
        get => _playerCarController;
        set
        {
            _playerCarController = value;

            if (_playerCarController == null && Resolver != null)
            {
                _playerCarController = Resolver.Resolve<PlayerCarController>();
            }
        }
    }

    public TutorialSequenceController TutorialSequenceController
    {
        get
        {
            if (_tutorialSequenceController != null)
            {
                return _tutorialSequenceController;
            }

            _tutorialSequenceController = Resolver.Resolve<TutorialSequenceController>();
            return _tutorialSequenceController;
        }
    }

    public DamageNumberManager DamageNumberManager => Resolver.Resolve<DamageNumberManager>();
    public VibrationManager VibrationManager => Resolver.Resolve<VibrationManager>();
    public bool IsDead => _carStatusController.IsDead;

    public event Action<bool> EngineStatusChanged;

    public bool IsEngineOn
    {
        get => _isEngineOn;
        set
        {
            _isEngineOn = value;

            if (EngineStatusChanged != null)
            {
                EngineStatusChanged.Invoke(_isEngineOn);
            }
        }
    }

    public PlayerInputActions InputActions
    {
        get => _carMovementController.CarInputHandler.PlayerInputActions;
        set => _carMovementController.CarInputHandler.PlayerInputActions = value;
    }

    #endregion


    #region Unity Methods

    private void Awake()
    {
        GetComponents();
    }

    private void OnDestroy()
    {
        if (PlayerSkillController != null)
        {
            PlayerSkillController.OnSkillUpgrade -= AdjustDamage;
            PlayerSkillController.OnSkillUpgrade -= AdjustAttackSpeed;
            PlayerSkillController.OnSkillUpgrade -= AdjustCritChange;
            PlayerSkillController.OnSkillUpgrade -= AdjustCritDamage;
            PlayerSkillController.OnResetSkill -= automaticWeapon.ResetSkills;
        }
    }

    #endregion


    #region Public Methods

    public void InitializeCar(PlayerMovementMode playerMovementMode, Action onDeadCar, Action<float> onChangeCarStatus)
    {
        _playerMovementMode = playerMovementMode;
        _engineSound.Init();
        IsEngineOn = _playerMovementMode.Equals(PlayerMovementMode.Drive);
        transform.tag = "Untagged";
        CharacterController.enabled = _playerMovementMode.Equals(PlayerMovementMode.Drive);
        SetWeapon(_playerMovementMode);
        _carStatusController.OnDeadCar += onDeadCar;
        _carStatusController.OnChangeCarStatus += onChangeCarStatus;
        SetObstacle(_playerMovementMode != PlayerMovementMode.Drive);
        if (_playerMovementMode.Equals(PlayerMovementMode.Drive) && TutorialSequenceController.IsTutorialCompleted)
        {
            CarManager.CheckFoundedCar(carType);
        }

        SetupCarAdditionalPartsController();
    }


    public void SetWeapon(PlayerMovementMode playerMovementMode)
    {
        if (automaticWeapon == null) return;

        if (playerMovementMode.Equals(PlayerMovementMode.Drive))
        {
            if (automaticWeapon.gameObject.activeInHierarchy)
            {
                automaticWeapon.Initialize(transform, _resolver);
            }
        }
        else
        {
            automaticWeapon.Dispose();
        }
    }

    public void SetObstacle(bool isActive)
    {
        dynamicGridObstacle.enabled = isActive;
        dynamicGridObstacleCollider.enabled = isActive;
    }

    private void SetupCarAdditionalPartsController()
    {
        var tutorialService = Resolver.Resolve<ITutorialService>();
        carAdditionalPartsController.SetPlayerCarController(PlayerCarController, tutorialService);
    }

    #endregion


    #region Private Methods

    private void Setup()
    {
        if (_resolver == null)
        {
            LoggerNS.LogError("Resolver is not set. Setup cannot proceed.");
            return;
        }

        _carStatusController.Resolver = _resolver;
        _carMovementController.Resolver = _resolver;

        if (PlayerSkillController != null)
        {
            PlayerSkillController.OnSkillUpgrade += AdjustDamage;
            PlayerSkillController.OnSkillUpgrade += AdjustAttackSpeed;
            PlayerSkillController.OnSkillUpgrade += AdjustCritChange;
            PlayerSkillController.OnSkillUpgrade += AdjustCritDamage;
            PlayerSkillController.OnSkillUpgrade += _carZombieDetection.AdjustCollisionDamage;
            PlayerSkillController.OnResetSkill += automaticWeapon.ResetSkills;
            PlayerSkillController.OnResetSkill += _carZombieDetection.ResetCollisionDamage;
        }
        else
        {
            LoggerNS.LogWarning("PlayerSkillController is null.");
        }
    }


    private void AdjustDamage(UpgradeDetail upgradeDetail)
    {
        if (upgradeDetail.type != StatUpgradeType.CarTurretDamage)
        {
            return;
        }

        automaticWeapon.AdjustDamage(upgradeDetail);
    }

    private void AdjustAttackSpeed(UpgradeDetail upgradeDetail)
    {
        if (upgradeDetail.type != StatUpgradeType.CarWeaponAttackSpeed)
        {
            return;
        }

        automaticWeapon.AdjustFireInterval(upgradeDetail);
    }

    private void AdjustCritChange(UpgradeDetail upgradeDetail)
    {
        if (upgradeDetail.type != StatUpgradeType.CarCriticalHitChance)
        {
            return;
        }

        automaticWeapon.AdjustCriticalHitChance(upgradeDetail);
    }

    private void AdjustCritDamage(UpgradeDetail upgradeDetail)
    {
        if (upgradeDetail.type != StatUpgradeType.CarCriticalDamage)
        {
            return;
        }

        automaticWeapon.AdjustCritDamage(upgradeDetail);
    }

    private void GetComponents()
    {
        var car = carResources.GetCar(carType);
        _carEffectController = GetComponent<CarEffectController>();
        _carStatusController = GetComponent<CarStatusController>();
        _carMovementController = GetComponent<CarMovementController>();
        _carAdditionalPartsController = GetComponent<CarAdditionalPartsController>();
        _carArmorController = GetComponent<CarArmorController>();
        _carZombieDetection = GetComponent<CarZombieDetection>();
        _engineSound = GetComponent<EngineSound>();
        _carMovementController.Car = car;
        _carStatusController.CurrentHealth = car.MaxHealt;
        _carStatusController.MaxHealth = car.MaxHealt;
        _carStatusController.MaxArmor = car.MaxArmor;
    }

    #endregion
}
