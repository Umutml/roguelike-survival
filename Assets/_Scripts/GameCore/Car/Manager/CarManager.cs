using GameCore.Scriptables;
using UnityEngine;
using System.Collections.Generic;
using GameCore.Car;
using GameCore.Tutorial;
using System;
using _Utilities;
using VContainer;
using _Scripts.Utilities;
using GameCore.PopupSystem;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Cathei.LinqGen;
using GameCore.Player;

public class CarManager : MonoBehaviour
{
    #region Actions

    public event Action OnCarSpawned;
    public Action OnResetCarHealth;

    #endregion

    #region Serializable Fields

    [SerializeField] private CarResources carResources;
    [SerializeField] private Transform carParents;

    #endregion

    #region Fields

    private FoundedCarsData _foundedCarsData;
    private CarData _carData;
    private IObjectResolver _resolver;
    private AlertManager _alertManager;
    private PopupManager _popupManager;
    private CarController _carInstance;
    private List<CarController> _carControllers = new();
    private bool _isBridgeDrive;
    private PlayerSkillController _playerSkillController;

    #endregion

    #region Properties

    public CarType SelectedCarType => _carData.SelectedCarType;

    #endregion

    #region Properties

    public bool IsBridgeDrive
    {
        get => _isBridgeDrive;
        set
        {
            _isBridgeDrive = value;
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _carData = GetCarData();
    }


    private void OnEnable()
    {
        _foundedCarsData = GetFoundedCarsData();
    }

    #endregion


    #region Public Methods

    public void Setup()
    {
        SetupBaseCar();
    }

    public async UniTask SpawnCar(CarType carType, CarSpawnType carSpawnType)
    {
        if (_resolver is null)
        {
            await UniTaskAsyncHelper.WaitUntil(() => _resolver != null);
        }

        DestroyCurrentCar();
        var (position, rotation) = GetCarTransform(carSpawnType);
        var car = carResources.GetCar(carType);
        _carInstance = Instantiate(car.CarPrefab, position, rotation);
        _carInstance.transform.SetParent(carParents);
        _carInstance.Resolver = _resolver;
        _carInstance.GetComponent<CarController>().IsTutorialCar = true;
        _carControllers.Add(_carInstance);
        SetCar(position, rotation, carType.ToString());
        SetCarUpgrade(_carData.SelectedCarType);
    }

    public async void CheckFoundedCar(CarType targetCar)
    {
        _foundedCarsData = GetFoundedCarsData();
        if (_foundedCarsData is null)
        {
            LoggerNS.LogError("Founded Cars Data is null");
            return;
        }

        if (_foundedCarsData.CarTypes.Contains(targetCar)) { return; }

        SaveLoadHelper.UpdateData<FoundedCarsData>(data => { data.CarTypes.Add(targetCar); });
        var car = carResources.CarList.FirstOrDefault(c => c.CarType == targetCar) as Car?;

        if (car is null)
        {
            LoggerNS.LogError($"Car with type {targetCar} not found in Car List");
            return;
        }

        await _popupManager.OpenPopup(PopupConstants.PopupType.Unlock);
        var unlockPopup = _popupManager.GetPopup<Popup>(PopupConstants.PopupType.Unlock);
        unlockPopup.Initialize(car.Value.UnlockObjectType);
    }

    public async UniTask Recover()
    {
        if (_carControllers is {Count: > 0})
        {
            var controllers = _carControllers.Gen().Where(x => x != null).ToList();

            foreach (var controller in controllers)
            {
                if (controller == null || controller.gameObject == null) continue;

                _carControllers.Remove(controller);
                await UniTask.Yield();
                Destroy(controller.gameObject);
            }
        }

        await SpawnCar(_carData.SelectedCarType, CarSpawnType.Garage);
    }


    public void Restart()
    {
        for (var i = 0; i < _carControllers.Count; i++)
        {
            if (_carControllers[i] == null) continue;
            Destroy(_carControllers[i].gameObject);
        }

        _carControllers.Clear();
    }

    public void SetEnableDoorCircles(bool isActive)
    {
        if (_carControllers is not {Count: > 0})
        {
            Debug.LogError("Car Controllers is null or empty");
            return;
        }

        foreach (var controller in _carControllers)
        {
            controller.CarEffectController.SetEnableDoorCircles(isActive);
        }
    }


    public void SetSelectedCar(CarType selectedCarType)
    {
        _carData.SelectedCarType = selectedCarType;
        SaveLoadHelper.UpdateData<CarData>(data => { data.SelectedCarType = selectedCarType; });
        SpawnCar(selectedCarType, CarSpawnType.Garage).Forget();
        _alertManager.CallAlert($"{carResources.GetCar(selectedCarType).CarName} selected!");
    }

    #endregion


    #region Private Methods

    [Inject]
    private void Initialize(IObjectResolver resolver)
    {
        _resolver = resolver;
        _popupManager = _resolver.Resolve<PopupManager>();
        _playerSkillController = _resolver.Resolve<PlayerSkillController>();
        _alertManager = _resolver.Resolve<AlertManager>();
    }

    private async void SetupBaseCar()
    {
        try
        {
            if (!_resolver.Resolve<TutorialSequenceController>().IsTutorialCompleted) return;

            await SpawnCar(_carData.SelectedCarType, CarSpawnType.Garage);

            OnCarSpawned?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SetupBaseCar: {e.Message}");
        }
    }

    private void SetCar(Vector3 position, Quaternion rotation, string name)
    {
        _carInstance.transform.name = name;
        _carInstance.CarSpawnPosition = position;
        _carInstance.CarSpawnRotation = rotation;
    }

    private void SetCarUpgrade(CarType carType)
    {
        var carMetaData = SaveLoadHelper.TryLoadPersistentData<CarMetaUpgradeData>();
        var targetUpgrade = carMetaData?.CarMetaList.Gen().Where(x => x.CarType == carType).FirstOrDefault();
        if (targetUpgrade is null)
        {
            return;
        }

        for (var i = 0; i < targetUpgrade.UpgradeIndex; i++)
        {
            if (i >= _playerSkillController.CarMetaUpgradeResources.CarMetaUpgradeList.Count)
            {
                break;
            }

            var upgradeDetail = _playerSkillController.CarMetaUpgradeResources.CarMetaUpgradeList[i].UpgradeDetail;

            _playerSkillController.ApplyStatUpgrade(new List<UpgradeDetail> {upgradeDetail});
        }

        SetupCarHealths();
    }

    private void DestroyCurrentCar()
    {
        if (_carControllers is not {Count: > 0})
        {
            return;
        }

        foreach (var controller in _carControllers)
        {
            if (controller == null || controller.gameObject == null) continue;
            Destroy(controller.gameObject);
        }

        _carControllers.Clear();
    }

    private void SetupCarHealths()
    {
        foreach (var carController in _carControllers)
        {
            carController.CarStatusController.SetupHealth();
        }
    }

    public CarController GetAnyCarController()
    {
        return _carControllers is not {Count: > 0}
            ? null
            : _carControllers.Gen().Where(x => x != null).FirstOrDefault();
    }

    private (Vector3 Position, Quaternion Rotation) GetCarTransform(CarSpawnType carSpawnType)
    {
        return carSpawnType == CarSpawnType.Garage
            ? (carResources.GarageTransform.Position, carResources.GarageTransform.Rotation)
            : (carResources.TutorialCarTransform.Position, carResources.TutorialCarTransform.Rotation);
    }

    private FoundedCarsData GetFoundedCarsData()
    {
        return SaveLoadHelper.TryLoadPersistentData<FoundedCarsData>();
    }

    private CarData GetCarData() => SaveLoadHelper.TryLoadPersistentData<CarData>();

    #endregion
}

[Serializable]
public class CarData
{
    public CarType SelectedCarType = CarType.Buggy;
}


[Serializable]
public class FoundedCarsData
{
    public List<CarType> CarTypes = new List<CarType> {CarType.Buggy};
}

public enum CarSpawnType
{
    Tutorial,
    Garage
}
