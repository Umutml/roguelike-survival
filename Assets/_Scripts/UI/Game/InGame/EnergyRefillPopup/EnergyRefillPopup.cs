using GameCore.PopupSystem;
using _Scripts.Utilities;
using GameCore.Inventory;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using VContainer;

public class EnergyRefillPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private EnergyRefillPopupContent energyRefillPopupContent;
    private const int RewardedEnergyAmount = 30;
    private IMediationService _mediationService;
    private IGameService _gameService;
    private IAnalyticsService _analyticsService;

    #endregion
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        energyRefillPopupContent.Initialize(Resolver.Resolve<IEnergyService>(), OnClickOkButton, OnClickCloseButton, OnClickWatchAdButton, Resolver);
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoFailedToShowEvent;
        
        _gameService = Resolver.Resolve<IGameService>();
        _mediationService = Resolver.Resolve<IMediationService>();
        _analyticsService = Resolver.Resolve<IAnalyticsService>();
        
        _gameService.PauseGame();
    }
    #endregion


    #region Private Methods
    
    private void OnClickWatchAdButton()
    {
        _mediationService.ShowRewardedAd(IMediationService.EnergyPlacementId);
    }
    
    private void RewardedVideoFailedToShowEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        LoggerNS.LogError("RewardedVideoFailedToShowEvent With Error " + error + " And AdInfo " + adInfo);
        ClosePopup();
    }
    
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (ironSourcePlacement.getPlacementName().Equals(IMediationService.EnergyPlacementId))
        {
            ClosePopup(); // Close popup also resumes the game
            Resolver.Resolve<IEnergyService>().GiveEnergy(RewardedEnergyAmount);
            LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() + "And AdInfo " + adInfo);
        }
    }

    private void OnClickOkButton()
    {
        var purchaseDetail = Resolver.Resolve<IInventoryManager>().PurchaseItem(new PurchaseDetails(100,
            PurchaseOptions.Coin
        ));

        if (purchaseDetail)
        {
            Resolver.Resolve<IEnergyService>().GiveEnergy(RewardedEnergyAmount);
            ClosePopup();
        }
        else
        {
            ClosePopup();
        }
    }

    private void OnClickCloseButton()
    {
        ClosePopup(); // Close Energy Refill popup
    }

    private void OnDestroy()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoFailedToShowEvent;
    }

    #endregion
}
