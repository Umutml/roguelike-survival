using System;
using System.Collections.Generic;
using GameCore.Car;
using GameCore.Player;
using GameCore.Scriptables;
using UnityEngine;
using VContainer;

public class CarMovementController : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private List<Wheel> wheels = new();

    #endregion


    #region Fields

    private CarController _carController;
    private CharacterController _characterController;
    private PlayerSkillController _playerSkillController;
    private CarInputHandler _carInputHandler;
    private IObjectResolver _resolver;

    private Vector3 _moveForce;
    public float GetCarSpeed => _moveForce.magnitude;
    private Vector3 _velocity;
    private float _originalMaxSpeed;
    private float _moveSpeed;
    private string _movementSkillId;
    private float _currentSpeed;
    private bool _isDrive;
    private bool _isDrifting;
    private bool _isMoveBlocked = false;

    #endregion


    #region Properties

    public IObjectResolver Resolver
    {
        get => _resolver;
        set
        {
            _resolver = value;
            SubscribeToEvents();
        }
    }

    public CharacterController CharacterController => _characterController;
    public CarInputHandler CarInputHandler => _carInputHandler;

    public Car Car { get; set; }

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    public bool IsDrive => _isDrive;

    public bool IsDrifting => _isDrifting;

    public bool IsMoveBlocked
    {
        get => _isMoveBlocked;
        set => _isMoveBlocked = value;
    }

    #endregion


    #region Unity Methods

    private void OnEnable()
    {
        _carController = GetComponent<CarController>();
        _characterController = GetComponent<CharacterController>();
        _carInputHandler = GetComponent<CarInputHandler>();
        _currentSpeed = Car.MaxSpeed;
        _moveSpeed = Car.MoveSpeed;
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void OnDestroy()
    {
        if (_playerSkillController != null)
        {
            _playerSkillController.OnSkillUpgrade -= AdjustMovementSpeed;
            _playerSkillController.OnResetSkill -= ResetMovementSpeed;
        }
    }

    #endregion


    #region Public Methods

    public void ResetMoveSpeed()
    {
        _currentSpeed = Car.MaxSpeed;
    }

    public float GetSpeedRatio()
    {
        // Return the ratio of current speed to the max speed
        return _characterController.velocity.magnitude / Car.MaxSpeed;
    }

    public float GetThrottleMagnitude()
    {
        return _carInputHandler.MoveInput;
    }

    public float GetSpeed()
    {
        return _characterController.velocity.magnitude;
    }

    public bool IsGoingReverse()
    {
        var forward = _characterController.transform.forward;
        var velocity = _characterController.velocity;
        return Vector3.Dot(forward, velocity) < 0;
    }


    public void CloseWheelEffects()
    {
        for (var i = 0; i < wheels.Count; i++)
        {
            wheels[i].TrailObject.enabled = false;
        }
    }

    #endregion


    #region Private Methods

    private void SubscribeToEvents()
    {
        _playerSkillController = _resolver.Resolve<PlayerSkillController>();
        _playerSkillController.OnSkillUpgrade += AdjustMovementSpeed;
        _playerSkillController.OnResetSkill += ResetMovementSpeed;
    }

    private void Movement()
    {
        if (_isMoveBlocked)
        {
            _carController.CarEffectController.ApplyBodyTilt(_carInputHandler, Car, new Vector3(0, 0, 0));
            return;
        }

        if (CanMoveCar()) return;

        if (_carController.CarStatusController.IsDead)
        {
            _carController.CarEffectController.SetExhaustParticles(false);
        }

        ApplyGravity();
        Move();
        CheckCarDrive();
        CheckDrift();
        _carController.CarEffectController.ApplyBodyTilt(_carInputHandler, Car, _moveForce);
    }


    private void Move()
    {
        _moveForce += transform.forward * SetCarSpeedByInBase() * _carInputHandler.MoveInput * Time.fixedDeltaTime;
        _moveForce *= Car.Drag;
        _moveForce = Vector3.ClampMagnitude(_moveForce, _currentSpeed);
        transform.Rotate(Vector3.up * _carInputHandler.SteerInput * _moveForce.magnitude * Car.SteerAngle *
                         Time.fixedDeltaTime);
        _moveForce =
            Vector3.Lerp(_moveForce.normalized,
                transform.forward,
                (Car.Traction + Car.DriftMultiplier) * Time.fixedDeltaTime) * _moveForce.magnitude;

        if (_characterController.enabled)
        {
            _characterController.Move((_moveForce + _velocity) * Time.fixedDeltaTime);
        }

        _carController.CarStatusController.CarVelocity = _characterController.velocity;

        _carController.CarEffectController.AnimatedWheels(wheels, _carInputHandler.SteerInput,
            _carInputHandler.MoveInput);
    }


    private void CheckDrift()
    {
        var carVelocity = _characterController.velocity;
        var forward = transform.forward;

        var angleBetween = Vector3.Angle(forward, carVelocity.normalized);

        if (Mathf.Abs(angleBetween) > Car.DriftOffset && carVelocity.magnitude > 1f)
        {
            if (!_isDrifting)
            {
                _isDrifting = true;
                _originalMaxSpeed = Car.MaxSpeed;
                _currentSpeed *= Car.DriftSpeedMultiplier;
            }
        }
        else
        {
            if (_isDrifting)
            {
                _isDrifting = false;
                _currentSpeed = Mathf.MoveTowards(Car.MaxSpeed, _originalMaxSpeed, Time.fixedDeltaTime * 5f);
            }
        }


        _carController.VibrationManager.TriggerVibrationCarDrift(_isDrifting);
        _carController.CarEffectController.WheelEffects(wheels, _isDrifting);
    }


    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = CarConstansts.GroundedGravity;
        }
        else
        {
            _velocity.y += CarConstansts.Gravity * Time.fixedDeltaTime;
        }

        _velocity.y = Mathf.Max(_velocity.y, CarConstansts.MaxFallSpeed);
    }


    private void CheckCarDrive()
    {
        if (_carInputHandler.MoveInput != 0)
        {
            _isDrive = true;
        }
        else
        {
            _isDrive = false;
            _carController.CarEffectController.SetExhaustParticles(true);
        }
    }

    private void AdjustMovementSpeed(UpgradeDetail detail)
    {
        if (detail.type != StatUpgradeType.CarSpeed)
        {
            return;
        }

        PlayerSkillController.Calculate(ref _moveSpeed, ref _movementSkillId, detail);
    }

    private void ResetMovementSpeed()
    {
        PlayerSkillController.ResetSkill(ref _moveSpeed, _movementSkillId);
    }

    private bool CanMoveCar() => _carController.PlayeMovementMode.Equals(PlayerMovementMode.Walk) ||
                                 _carController.CarStatusController.IsDead;

    private float SetCarSpeedByInBase() => _carController.Player.InBase ? Car.MoveSpeed * 0.5f : _moveSpeed;

    #endregion
}