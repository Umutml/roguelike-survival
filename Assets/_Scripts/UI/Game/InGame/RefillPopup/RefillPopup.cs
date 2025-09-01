using System;
using GameCore.PopupSystem;
using System.Collections;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using VContainer;

public class RefillPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private RefillPopupContent refillPopupContent;
    private IAnalyticsService _analyticsService;

    #endregion
    
    
    #region Fields

    private readonly WaitForSecondsRealtime _openGameLosePopupDelay = new (10f);

    #endregion
    
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        refillPopupContent.Initialize(OnClickOkButton, OnClickCloseButton, OnClickWatchAdButton, Resolver);
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoFailedToShowEvent; 
        _analyticsService = Resolver.Resolve<IAnalyticsService>();
    }
    #endregion


    #region Private Methods
    
    private void OnClickWatchAdButton()
    {
        IMediationService mediationService = Resolver.Resolve<IMediationService>();
        mediationService.ShowRewardedAd(IMediationService.RevivePlacementId);
    }
    
    private void RewardedVideoFailedToShowEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        LoggerNS.LogError("RewardedVideoFailedToShowEvent With Error " + error + " And AdInfo " + adInfo);
    }
    
    private async void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (ironSourcePlacement.getPlacementName().Equals(IMediationService.RevivePlacementId))
        {
            await UniTask.Delay(1000); // Delay for closing the rewarded video process takes time then refill the player with actions
            ClosePopup(); // Close popup also resumes the game
            Resolver.Resolve<PlayerStatusController>().RefillPlayer();
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
            Resolver.Resolve<PlayerStatusController>().RefillPlayer();
            ClosePopup();
        }
        else
        {
            StartCoroutine(OpenGameLosePopup());
        }
    }
    
    private IEnumerator OpenGameLosePopup()
    {
        yield return _openGameLosePopupDelay;
        OnClickCloseButton();
    }

    private void OnClickCloseButton()
    {
        Resolver.Resolve<PopupManager>().OpenPopup(PopupConstants.PopupType.GameLose);
        ClosePopup(); // Close refill popup
    }

    private void OnDestroy()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    #endregion
}
