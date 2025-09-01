using System;
using System.Collections.Generic;
using GameCore.Inventory;
using GameCore.Scriptables;
using Interfaces;
using UI.Game.Architectural;
using UI.Game.InGame.ShopPopup;
using UnityEngine;


public class ShopPopupTopbar : Content
{
    #region Fields

    private IEnergyService _energyService;
    private Dictionary<PurchaseOptions, RectTransform> _iconTransforms = new();

    #endregion


    #region Properties

    public Dictionary<PurchaseOptions, RectTransform> IconTransforms => _iconTransforms;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        InitializeIconTransforms();
    }

    #endregion
    
    
    
    #region Public Methods

    public void InitializeCurrency(GameInventoryManager gameInventoryManager, IEnergyService energyService)
    {
        _energyService = energyService;
        
        SetCoinText((int)gameInventoryManager.GetCurrencyBalance(PurchaseOptions.Coin));
        SetGemText((int)gameInventoryManager.GetCurrencyBalance(PurchaseOptions.Gem));
        SetEnergyText(_energyService.CurrentEnergy);
    }


    public void SetEnergyText(int currentEnergy)
    {
        SetText(ShopPopupConstants.EnergyAmount, $"{currentEnergy} / {_energyService.MaxEnergy}");
    }
    

    public void SetCoinText(int value = 0)
    {
        SetText(ShopPopupConstants.CoinAmount, GetFormattedIconWithText(value));
    }

    public void SetGemText(int value = 0)
    {
        SetText(ShopPopupConstants.GemAmount, GetFormattedIconWithText(value));
    }

    #endregion


    #region Private Methods
    
    private void InitializeIconTransforms()
    {
        _iconTransforms.Add(PurchaseOptions.Coin, GetGameObject(ShopPopupConstants.CoinIcon).GetComponent<RectTransform>());
        _iconTransforms.Add(PurchaseOptions.Gem, GetGameObject(ShopPopupConstants.GemIcon).GetComponent<RectTransform>());
        _iconTransforms.Add(PurchaseOptions.Energy, GetGameObject(ShopPopupConstants.EnergyIcon).GetComponent<RectTransform>());
    }

    private string GetFormattedIconWithText(int value) => $"{value}";

    #endregion
}
