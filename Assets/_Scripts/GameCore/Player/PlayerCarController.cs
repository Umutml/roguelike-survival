using System;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Player;
using Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

public class PlayerCarController : MonoBehaviour
{
    #region Actions

    public event Action<bool> CarExitButtonActivity;
    public event Action<bool> CarExitButtonFingerMarkActivity;
    public Action<float> ChangeStatus;
    public Action DeadCar;
    public event Action CarExitedByForce;

    #endregion

    #region Serializable Fields

    [SerializeField] private GameObject[] playerChildObjects;
    [SerializeField] private CinemachineCamera vehicleCamera;
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private Transform playerQuaternionTransform;

    #endregion

    #region Fields

    private PlayerController _playerController;
    private IObjectResolver _resolver;
    private IAnalyticsService _analyticService;
    private ITutorialService _tutorialService;

    #endregion

    #region Properties

    public CarController CarController { get; set; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }


    private void OnEnable()
    {
        _playerController.EnteredCar += EnteredCar;
        _playerController.ExitedCar += ExitedCar;
        _playerController.CutSceneEntered += CutSceneEntered;

    }

    private void OnDestroy()
    {
        _playerController.EnteredCar -= EnteredCar;
        _playerController.ExitedCar -= ExitedCar;
        _playerController.CutSceneEntered -= CutSceneEntered;

        if (_tutorialService != null)
            _tutorialService.TutorialCompleted -= OnCompleteTutorial;
    }


    private void Update()
    {
        UpdatePlayerPositionInDriveMode();
    }

    #endregion

    #region Public Methods

    public void MoveCar()
    {
        if (CarController == null) return;
        CarController.CharacterController.Move(CarController.CarSpawnPosition -
                                               CarController.CharacterController.transform.position);
    }

    public void InvokeCarExitButtonActivity(bool isEnable)
    {
        CarExitButtonActivity?.Invoke(isEnable);
    }

    public void InvokeCarExitButtonFingerMarkActivity(bool isEnable)
    {
        CarExitButtonFingerMarkActivity?.Invoke(isEnable);
    }

    public void InvokeCarExitedByForce()
    {
        CarExitedByForce?.Invoke();
    }

    #endregion

    #region Private Methods

    [Inject]
    private void Init(IObjectResolver resolver, IAnalyticsService analyticsService, ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
        _resolver = resolver;
        _analyticService = analyticsService;

        if (_tutorialService != null)
            _tutorialService.TutorialCompleted += OnCompleteTutorial;
    }

    private void OnCompleteTutorial()
    {
        if (CarController == null) return;
        if (CarController.CarMovementController != null)
        {
            CarController.CarMovementController.ResetMoveSpeed();
        }

        if (CarController.CarAmorController != null)
        {
            CarController.CarAmorController.OpenArmorObjects();
        }
        PlayerPrefs.SetInt("MoveCarToGarage", 1);
    }


    private void UpdatePlayerPositionInDriveMode()
    {
        if (_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive))
        {
            if (CarController)
            {
                Vector3 playerPos = CarController.transform.position;
                playerPos.y += 1.2f;
                transform.position = playerPos;
                playerQuaternionTransform.rotation = CarController.transform.rotation;
            }
        }
    }


    private void EnteredCar(bool carMoveToGarage = false)
    {
        if (_playerController == null)
        {
            LoggerNS.LogError("Handled: PlayerController is null in PlayerCarController EnteredCar");
            return;
        }

        if (_playerController.ItemPicker == null)
        {
            LoggerNS.LogError("Handled: ItemPicker is null in PlayerCarController EnteredCar");
            return;
        }


        InitializeCar();

        LogEnteredCarAnalytic();

        if (CanEnterCar())
        {
            _playerController.PlayerMovementMode = PlayerMovementMode.Walk;
            return;
        }

        SharedCarStatusFunctions();
    }

    private void CutSceneEntered()
    {
        playerCamera.gameObject.SetActive(false);
        vehicleCamera.Follow = null;
        vehicleCamera.gameObject.SetActive(false);
    }

    #region Analytic Events

    private void LogEnteredCarAnalytic()
    {
        if (_analyticService == null)
        {
            LoggerNS.LogError("Handled: AnalyticService is null in PlayerCarController LogEnteredCarAnalytic");
            return;
        }

        if (!_tutorialService.IsTutorialCompleted) // Tutorial car entered event send only once
        {
            _analyticService.LogEvent(new EventParameters<string> { EventName = "tt_car_entered", AdjustToken = AdjustNsEventTokens.TtCarEntered });
            return;
        }
        _analyticService.LogEvent(new EventParameters<string> { EventName = "car_entered", AdjustToken = AdjustNsEventTokens.CarEntered });
    }

    private void LogExitCarAnalytic()
    {
        _analyticService.LogEvent(new EventParameters<string>
        {
            EventName = "car_exit",
            AdjustToken = AdjustNsEventTokens.CarExit
        });
    }

    #endregion

    private void ExitedCar(bool moveCarToGarage = false)
    {
        if (_playerController == null)
        {
            LoggerNS.LogError("Handled: PlayerController is null in PlayerCarController");
            return;
        }

        if (_playerController.ItemPicker == null)
        {
            LoggerNS.LogError("Handled: ItemPicker is null in PlayerCarController");
            return;
        }

        SharedCarStatusFunctions();
        SetPlayerPositionAfterCarExit();

        CarController.SetWeapon(_playerController.PlayerMovementMode);
        CarController.SetObstacle(true);
        CarController.transform.tag = "Car";
        CarController.IsEngineOn = false;

        if (moveCarToGarage)
        {
            PlayerPrefs.SetInt("MoveCarToGarage", 0);
        }

        if (_playerController.PreviousPlayerMovementMode == PlayerMovementMode.CutScene)
        {
            MoveCar();
            CarExitedByForce?.Invoke();
            return;
        }

        LogExitCarAnalytic();
    }


    private void InitializeCar()
    {
        CarController = _playerController.ItemPicker.CarController;
        CarController.Player = _playerController;
        CarController.InitializeCar(_playerController.PlayerMovementMode, OnDeadCarInvoke, OnChangeCarStatus);
    }


    private void SetPlayerPositionAfterCarExit()
    {
        transform.position = GetPlayerCarLeavePosition();
    }


    private void OnChangeCarStatus(float value)
    {
        ChangeStatus?.Invoke(value);
    }


    private void SharedCarStatusFunctions(bool setCameras = true)
    {
        if (CarController == null)
        {
            LoggerNS.LogError("Handled: CarController is null in PlayerCarController");
            return;
        }

        if (CarController.CarEffectController == null)
        {
            LoggerNS.LogError("Handled: CarEffectController is null in PlayerCarController");
            return;
        }

        CarController.CarEffectController.SetCarStatuEffects(CarController.IsDead,
            _playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive));


        SetInputActions();
        SetChildObjectsActivity(_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Walk));

        if (setCameras)
            SetCameraActivity();
    }


    private void OnDeadCarInvoke()
    {
        DeadCar?.Invoke();
    }


    private void SetInputActions()
    {
        CarController.InputActions = _playerController.PlayerMovementMode.Equals(PlayerMovementMode.Walk)
            ? null
            : _playerController.PlayerMovementController.PlayerInputActions;
    }


    private void SetChildObjectsActivity(bool isActive)
    {
        foreach (var playerChildObject in playerChildObjects) playerChildObject.SetActive(isActive);
    }


    private void SetCameraActivity()
    {
        if (playerCamera == null || vehicleCamera == null)
        {
            LoggerNS.LogError("Handled: Camera is null in PlayerCarController");
            return;
        }


        vehicleCamera.Follow = CarController.transform;

        playerCamera.gameObject.SetActive(_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Walk));
        vehicleCamera.gameObject.SetActive(_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive));
    }


    private Vector3 GetPlayerCarLeavePosition()
    {
        var boundsPosition = CarController.transform.position + Vector3.up * 1.2f;
        var carBounds = new Bounds(boundsPosition, new Vector3(3f, 2.5f, 5.15f));

        Vector3[] exitPositions =
        {
            new(CarController.transform.position.x - 2.5f,
                transform.position.y,
                CarController.transform.position.z - 0.5f),
            new(CarController.transform.position.x + 2.5f,
                transform.position.y,
                CarController.transform.position.z - 0.5f),
            new(CarController.transform.position.x,
                transform.position.y,
                CarController.transform.position.z + 2.5f),
            new(CarController.transform.position.x, transform.position.y, CarController.transform.position.z - 2.5f)
        };

        foreach (var exitPosition in exitPositions)
        {
            var walkableExitPosition = AstarPathHelper.FindNearestWalkablePosition(exitPosition);
            if (walkableExitPosition != null && !carBounds.Contains((Vector3)walkableExitPosition))
                return (Vector3)walkableExitPosition + Vector3.up * 0.6f;
        }

        return exitPositions[0] + Vector3.up * 0.6f;
    }


    private bool CanEnterCar()
    {
        return _playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive) && CarController.IsDead;
    }

    #endregion
}