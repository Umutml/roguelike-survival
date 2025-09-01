using System;
using GameCore.Car;
using GameCore.Scriptables;
using UI.Game.Architectural;
using UnityEngine.UI;
using UnityEngine;

public class CarSegment : Content
{
    #region Constants

    private const string CarName = "CarNameText";
    private const string CarImage = "CarImage";
    private const string LockState = "LockState";
    private const string InnerBg = "InnerBg";

    private readonly Color ActiveBackgroundColor = new (0.51f, 0.54f, 0.72f, 1f);
    private readonly Color ActiveInnerBackgroundColor = new(0.23f, 0.27f, 0.40f, 1f);
    
    private readonly Color DisableBackgroundColor = new (0.14f, 0.38f, 0.68f, 1f);
    private readonly Color DisableInnerBackgroundColor = new(0.10f, 0.18f, 0.38f, 1f);

    #endregion


    #region Fields

    private Car _car;
    private Image _backgroundImage;
    private Button _segmentButton;
    private CarType _carType;

    #endregion


    #region Properties

    public CarType CarType => _carType;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        _segmentButton = GetComponent<Button>();
    }

    #endregion


    #region Public Methods

    public async void InitializeSegment(Car car, bool isLock, Action selectCarAction)
    {
        _carType = car.CarType;
        _car = car;
        var isLocked = !car.CarType.Equals(CarType.Buggy) && isLock;
        var carModelArt = await car.GetCarModelArt();
        SetText(CarName, car.CarName);
        SetImage(CarImage, carModelArt);
        SetGameObject(LockState, isLocked);
        SetColor(InnerBg, isLocked ? DisableInnerBackgroundColor : ActiveInnerBackgroundColor);
        _backgroundImage.color = isLocked ? DisableBackgroundColor : ActiveBackgroundColor;
        
        _segmentButton.onClick.RemoveAllListeners();
        _segmentButton.onClick.AddListener(() => selectCarAction?.Invoke());
    }


    public void UpdateSegment(bool isLock)
    {
        var isLocked = !_car.CarType.Equals(CarType.Buggy) && isLock;
        SetGameObject(LockState, isLocked);
        SetColor(InnerBg, isLocked ? DisableInnerBackgroundColor : ActiveInnerBackgroundColor);
        _backgroundImage.color = isLocked ? DisableBackgroundColor : ActiveBackgroundColor;
    }

    #endregion
}
