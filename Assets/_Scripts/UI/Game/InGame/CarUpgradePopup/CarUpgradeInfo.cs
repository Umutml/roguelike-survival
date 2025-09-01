using System;
using GameCore.Car;
using GameCore.Scriptables;
using UI.Game.Architectural;
using UnityEngine;
using VContainer;

public class CarUpgradeInfo : Content
{
    #region Consts

    private const string CarName = "CarNameText";
    private const string CurrencyButton = "CurrencyButton";
    private const string AdButton = "AdButton";
    private const string DescriptionArea = "DescriptionArea";
    private const string Description = "DescriptionText";
    private const string Price = "PriceText";
    private const string Remaining = "RemainingText";
    private const string SegmentsArea = "Scroll View";
    private const string ButtonsArea = "ButtonsArea";
    private const string SelectButton = "SelectButton";

    #endregion


    #region Fields

    private CarManager _carManager;
    private Car _car;
    private CanvasGroup _segmentsCanvasGroup;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _segmentsCanvasGroup = GetGameObject(SegmentsArea).GetComponent<CanvasGroup>();
    }

    #endregion


    #region Public Methods

    public void SetCarInfo(Car car, IObjectResolver resolver, bool isLocked, Action<CarType> onPurchase)
    {
        _car = car;
        if (_carManager == null) _carManager = resolver.Resolve<CarManager>();
        
        var isLock = !car.CarType.Equals(CarType.Buggy) && isLocked;
        
        SetText(CarName, _car.CarName);
        SetText(Description, _car.CarBuyData.LockedMessage);
        if (_car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Coin)) SetText(Price, $"{_car.CarBuyData.Price}");
        if (_car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Ad)) SetText(Remaining, $"{_car.CarBuyData.RemainingCount}");
        SetGameObject(CurrencyButton, _car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Coin));
        SetGameObject(AdButton, _car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Ad));
        _segmentsCanvasGroup.alpha = !isLock ? 1 : 0;
        _segmentsCanvasGroup.interactable = !isLock;
        SetGameObject(DescriptionArea, isLock);
        SetGameObject(ButtonsArea, isLock);
        SetGameObject(SelectButton, !isLock);
        
        GetButton(CurrencyButton).onClick.RemoveAllListeners();
        GetButton(AdButton).onClick.RemoveAllListeners();
        GetButton(SelectButton).onClick.RemoveAllListeners();
        
        OnClickListen(CurrencyButton, ()=> onPurchase(_car.CarType));
        OnClickListen(AdButton, ()=> onPurchase(_car.CarType));
        OnClickListen(SelectButton, ()=> _carManager.SetSelectedCar(_car.CarType));
    }


    public void UpdateCarInfo(bool isLocked)
    {
        SetGameObject(CurrencyButton, _car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Coin));
        SetGameObject(AdButton, _car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Ad));
        _segmentsCanvasGroup.alpha = !isLocked ? 1 : 0;
        _segmentsCanvasGroup.interactable = !isLocked;
        SetGameObject(DescriptionArea, isLocked);
        SetGameObject(ButtonsArea, isLocked);
        SetGameObject(SelectButton, !isLocked);
    }

    #endregion
}
