using System;
using System.Collections.Generic;
using _Scripts.GameCore.Scriptables;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Inventory;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SupplyRunPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private AdLadderResources adladderResources;
    [SerializeField] private SupplyRunSegment supplyRunSegmentPrefab;
    [SerializeField] private UITimer dailyTimer;
    [SerializeField] private UITimer adTimer;
    [SerializeField] private Transform segmentsParent;
    [SerializeField] private Button claimButton;
    [SerializeField] private Sprite normalSegment;
    [SerializeField] private Sprite claimedSegment;
    [SerializeField] private Sprite normalButton;
    [SerializeField] private Sprite claimedButton;
    [SerializeField] private TMP_Text dailyTimerText;
    [SerializeField] private TMP_Text adTimerText;

    #endregion


    #region Fields
    
    private readonly List<SupplyRunSegment> _supplyRunSegments = new ();
    private SupplyRunData _supplyRunData;
    private AlertManager _alertManager;
    private IInventoryManager _inventoryManager;
    private IEnergyService _energyService;
    private IAnalyticsService _analyticsService;
    private SupplyRunSegment _supplyRunSegmentInstance;

    #endregion
    
    
    #region Public Methods

    private void Awake()
    {
        GetSupplyRunData();
    }


    private void OnDestroy()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }


    public override void OnOpenPopup()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        
        GetManagers();
        SetDailyTimer();
        CreateSegments();
        
        if (!_supplyRunData.IsClaimed) return;
        
        claimButton.interactable = false;
        claimButton.image.sprite = claimedButton;
        SetBuyButtonTimer();
    }
    
    
    public void BuyReward()
    {
        if (_supplyRunData.LastClaimedIndex >= _supplyRunSegments.Count - 1)
        {
            _alertManager.CallAlert("You have already claimed all rewards.");
            return;
        }
        
        claimButton.interactable = false;
        claimButton.image.sprite = claimedButton;
        
        var mediationService = Resolver.Resolve<IMediationService>();
        mediationService.ShowRewardedAd(IMediationService.AdLadderPlacementId);
    }

    #endregion



    #region Private Methods

    private void CreateSegments()
    {
        GetSupplyRunData();
        
        for (var i = 0; i < adladderResources.AdLadderRewardData.Count; i++)
        {
            var isClaimed = i <= _supplyRunData.LastClaimedIndex;
            _supplyRunSegmentInstance = Instantiate(supplyRunSegmentPrefab, segmentsParent);
            _supplyRunSegmentInstance.InitializeSegment(adladderResources.AdLadderRewardData[i], isClaimed ? claimedSegment : normalSegment ,i, IsLastSegment(i), isClaimed);
            _supplyRunSegments.Add(_supplyRunSegmentInstance);
        }
    }
    
    
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (!ironSourcePlacement.getPlacementName().Equals(IMediationService.AdLadderPlacementId)) return;
        
        LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() +
                     "And AdInfo " + adInfo);
        
        SendEvent();
        GiveReward();
        GetSupplyRunData();
        _supplyRunSegments[_supplyRunData.LastClaimedIndex].UpdateSegment(claimedSegment, _supplyRunData.LastClaimedIndex >= _supplyRunSegments.Count - 1);
    }


    private void GiveReward()
    {
        SaveLoadHelper.UpdateData<SupplyRunData>(data =>
        {
            data.LastClaimedTime = DateTime.Now;
            data.IsClaimed = true;
            data.LastClaimedIndex++;
        });
        
        GetSupplyRunData();
        
        
        var rewardData = adladderResources.AdLadderRewardData[_supplyRunData.LastClaimedIndex];
        
        switch (adladderResources.AdLadderRewardData[_supplyRunData.LastClaimedIndex].RewardType)
        {
            case RewardType.Energy:
                _energyService.GiveEnergy(rewardData.RewardCount);
                break;
            case RewardType.Coin:
                _inventoryManager.ModifyCurrencyBalance(new PurchaseDetails(rewardData.RewardCount, PurchaseOptions.Coin));
                break;
            case RewardType.Gem:
                _inventoryManager.ModifyCurrencyBalance(new PurchaseDetails(rewardData.RewardCount, PurchaseOptions.Gem));
                break;
        }
        
        SetBuyButtonTimer();
    }
    

    private void SendEvent()
    {
        _analyticsService.LogEvent(new EventParameters<string> { EventName = $"supply_run_{_supplyRunData.LastClaimedIndex}"});
    }


    private void SetDailyTimer()
    {
        var currentDailyTime = _supplyRunData.ResetTime;
        var now = DateTime.Now;
        
        if ((now - currentDailyTime).TotalDays >= 1)
        {
            currentDailyTime = now;

            SaveLoadHelper.UpdateData<SupplyRunData>(data =>
            {
                data.LastClaimedIndex = -1;
                data.ResetTime = currentDailyTime;
            });
        }

        var endTime = currentDailyTime.AddDays(1);
        dailyTimer.CreateTimer(dailyTimerText, string.Empty, "FFFFFF", endTime);
    }


    private void SetBuyButtonTimer()
    {
        var currentDailyTime = _supplyRunData.LastClaimedTime;
        var endTime = currentDailyTime.AddMinutes(3f);
        GetSupplyRunData();
        if (_supplyRunData.IsClaimed)
        {
            adTimer.CreateTimer(adTimerText, string.Empty, "FFFFFF", endTime, OnCompleteBuyButtonTimer, TimerUpdateType.Second);
        }
    }
    
    
    private void OnCompleteBuyButtonTimer()
    {
        SaveLoadHelper.UpdateData<SupplyRunData>(data =>
        {
            data.IsClaimed = false;
            data.LastClaimedTime = DateTime.Now;
        });
        
        claimButton.interactable = true;
        claimButton.image.sprite = normalButton;
        adTimerText.text = "Claim";
        _alertManager.CallAlert("A new supply run is available.");
    }


    private void GetManagers()
    {
        _inventoryManager = Resolver.Resolve<IInventoryManager>();
        _analyticsService = Resolver.Resolve<IAnalyticsService>();
        _energyService = Resolver.Resolve<IEnergyService>();
        _alertManager = Resolver.Resolve<AlertManager>();
    }


    private void GetSupplyRunData() => _supplyRunData = SaveLoadHelper.TryLoadPersistentData<SupplyRunData>();
    private bool IsLastSegment(int index) => index.Equals(adladderResources.AdLadderRewardData.Count - 1);

    #endregion


    private class SupplyRunData
    {
        public bool IsClaimed;
        public DateTime ResetTime;
        public DateTime LastClaimedTime = DateTime.Now;
        public int LastClaimedIndex = -1;
    }
}
