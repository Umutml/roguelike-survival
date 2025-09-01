using System;
using System.Collections.Generic;
using _Scripts.GameCore.Shop;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Inventory;
using GameCore.Scriptables;
using GameCore.Spawner;
using Interfaces;
using Managers;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Purchasing;
using VContainer;

public class ShopManager : MonoBehaviour
{
    #region Actions
    public event Action<string> OnNotEnoughCurrency;
    public event Action<PurchaseOptions> OnSuccess;
    public event Action<PurchaseOptions> OnFailed;
    public event Action<DropPodType, int, Vector3> OnTopBarAnimationStart;
    
    #endregion

    #region Serializable Fields

    [SerializeField] private ShopItemResources shopItemResources;
    [SerializeField] private ShopItemResources iapItemResources;

    #endregion
    
    #region Fields
    
    private IInventoryManager _inventoryManager;
    private IEnergyService _energyService;
    private IMediationService _mediationService;
    private IAPurchaseManager _iaPurchaseManager;
    private ShopItem _shopItem;
    private DailyOfferData _dailyOfferData;
    private CurrencyData _currencyData;

    private Dictionary<PurchaseOptions, Action<ShopItem>> _purchaseStrategies;
    private Dictionary<PurchaseOptions, Func<ShopItem, bool>> _paymentStrategies;
    private Dictionary<PurchaseOptions, (Action success, Action failed)> _purchaseActions = new ();
    private Dictionary<PurchaseOptions, RectTransform> _iconTransforms = new();
    
    #endregion

    #region Properties

    public DailyOfferData DailyOffer => _dailyOfferData;
    public CurrencyData Currency => _currencyData;

    public Dictionary<PurchaseOptions, RectTransform> IconTransforms
    {
        private get => _iconTransforms;
        set => _iconTransforms = value;
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializePurchaseActionsDictionary();
        InitializeCurrencyData();
    }

    private void OnEnable()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent += GrantAdReward;
    }


    private void OnDisable()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= GrantAdReward;
    }


    private void OnDestroy()
    {
        OnSuccess = null;
        OnFailed = null;
    }

    #endregion

    #region Public Methods
    public void BuyShopItem(ShopItem shopItem, Action<PurchaseOptions> success, Action<PurchaseOptions> failed)
    {
        _shopItem = shopItem;
        OnSuccess = success;
        OnFailed = failed;

        
        if (_shopItem.PurchaseOptions == PurchaseOptions.Ad)
        {
            ProcessAdPurchase(_shopItem);
            return;
        }

        if (_purchaseStrategies.TryGetValue(_shopItem.ProductType, out var purchaseAction))
        {
            purchaseAction.Invoke(_shopItem);
        }
    }


    public List<string> GetShopIAPItems()
    {
        var iapItems = new List<string>();
        
        foreach (var item in iapItemResources.Slots)
        {
            iapItems.Add(item.ShopItemList[0].ProductID);
        }

        return iapItems;
    }


    public void InvokePurchaseAction(PurchaseOptions productType, bool isSuccess)
    {
        if (!_purchaseActions.ContainsKey(productType))
        {
            Debug.LogError($"Product Type Not Found! {productType}");
            return;
        }
        
        if (isSuccess)
        {
            _purchaseActions[productType].success?.Invoke();
        }
        else
        {
            _purchaseActions[productType].failed?.Invoke();
        }
    }


    public void SaveCurrencyData()
    {
        SaveLoadHelper.SaveData(_currencyData);
    }
    
    
    public void SaveDailyOfferData()
    {
        SaveLoadHelper.SaveData(_dailyOfferData);
    }
    
    #endregion

    #region Private Methods
    [Inject]
    private void Init(IInventoryManager inventoryManager, IEnergyService energyService, IMediationService mediationService, IAPurchaseManager iaPurchaseManager)
    {
        
        
        _inventoryManager = inventoryManager;
        _energyService = energyService;
        _mediationService = mediationService;
        _iaPurchaseManager = iaPurchaseManager;
        

        _purchaseStrategies = new Dictionary<PurchaseOptions, Action<ShopItem>>
        {
            { PurchaseOptions.Chest, _ => {} },
            { PurchaseOptions.Energy, BuyEnergy },
            { PurchaseOptions.Gem, ProcessCurrencyPurchase },
            { PurchaseOptions.Coin, ProcessCurrencyPurchase },
            { PurchaseOptions.Ad, ProcessAdPurchase }
        };

        _paymentStrategies = new Dictionary<PurchaseOptions, Func<ShopItem, bool>>
        {
            { PurchaseOptions.Ad, _ => true },
            { PurchaseOptions.IAP, _ => true },
            { PurchaseOptions.Free, _ => true },
            { PurchaseOptions.Gem, HasEnoughCurrency },
            { PurchaseOptions.Coin, HasEnoughCurrency }
        };
    }
    

    private void ProcessCurrencyPurchase(ShopItem shopItem)
    {
        _shopItem = shopItem;
        
        if (!_paymentStrategies.TryGetValue(shopItem.PurchaseOptions, out var paymentCheck) || !paymentCheck(shopItem))
            return;

        if (shopItem.PurchaseOptions == PurchaseOptions.IAP)
        {
            _iaPurchaseManager.BuyItem(shopItem.ProductID, ApplyPurchaseReward);
            return;
        }

       
        OnSuccess?.Invoke(_shopItem.ProductType);
    }
    
    private void ApplyPurchaseReward()
    {
        _inventoryManager.ModifyCurrencyBalance(new PurchaseDetails(_shopItem.ProductAmount, _shopItem.ProductType));
        var dropType = _shopItem.ProductType.Equals(PurchaseOptions.Coin) ? DropPodType.Coin : DropPodType.Gem;
        OnTopBarAnimationStart?.Invoke(dropType, 10, _iconTransforms[_shopItem.ProductType].position);
    } 

    private void BuyEnergy(ShopItem shopItem)
    {
        if (_energyService.CurrentEnergy >= _energyService.MaxEnergy)
        {
            //OnFailed?.Invoke();
            OnNotEnoughCurrency?.Invoke("Energy fully charged");
            return;
        }

        if (!_paymentStrategies.TryGetValue(_shopItem.PurchaseOptions, out var paymentCheck) || !paymentCheck(_shopItem))
            return;

        OnSuccess?.Invoke(shopItem.ProductType);
        OnTopBarAnimationStart?.Invoke(DropPodType.Energy, 10, _iconTransforms[PurchaseOptions.Energy].position);
    }

    private void ProcessAdPurchase(ShopItem shopItem)
    {
        _mediationService.ShowRewardedAd(IMediationService.ShopItemPlacementId);
    }

    private void GrantAdReward(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (!ironSourcePlacement.getPlacementName().Equals(IMediationService.ShopItemPlacementId)) return;
        
        LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() +
                     "And AdInfo " + adInfo);
        
        OnSuccess?.Invoke(_shopItem.ProductType);
    }
    
    
    public void GiveBonusCurrency(PurchaseOptions purchaseOptions, int amount)
    {
        _inventoryManager.ModifyCurrencyBalance(new PurchaseDetails(amount, purchaseOptions));
    }
    

    public void CheckDailyOfferData()
    {
        _dailyOfferData = new DailyOfferData();

        if (!SaveLoadHelper.IsDataExists(nameof(DailyOfferData)))
        {
            InitializeDailyOfferData();
        }
        else
        {
            _dailyOfferData = SaveLoadHelper.TryLoadPersistentData<DailyOfferData>();
            
            if ((DateTime.Now - _dailyOfferData.LastRefreshTime).TotalHours >= 24)
            {
                RefreshDailyOffers();
            }
        }
    }
    
    
    private void InitializeCurrencyData()
    {
        _currencyData = new CurrencyData();
        
        if (!SaveLoadHelper.IsDataExists(nameof(CurrencyData)))
        {
            
            _currencyData.coinsData = new List<bool>();
            _currencyData.gemsData = new List<bool>();

            for (var i = 0; i < iapItemResources.Slots.Count; i++)
            {
                _currencyData.coinsData.Add(true);
                _currencyData.gemsData.Add(true);
            }

            SaveLoadHelper.SaveData(_currencyData);
        }
        else
        {
            _currencyData = SaveLoadHelper.TryLoadPersistentData<CurrencyData>();
        }
    }
    
    
    private void InitializeDailyOfferData()
    {
        _dailyOfferData.LastRefreshTime = DateTime.Now;
        _dailyOfferData.DailyOfferItems = new List<DailyOfferItem>();

        foreach (var slot in shopItemResources.Slots)
        {
            _dailyOfferData.DailyOfferItems.Add(new DailyOfferItem
            {
                StockCount = slot.ShopItemList[0].DailyStock
            });
        }

        SaveLoadHelper.SaveData(_dailyOfferData);
    }

    
    private void RefreshDailyOffers()
    {
        _dailyOfferData.LastRefreshTime = DateTime.Now;

        for (int i = 0; i < shopItemResources.Slots.Count; i++)
        {
            _dailyOfferData.DailyOfferItems[i].StockCount = shopItemResources.Slots[i].ShopItemList[0].DailyStock;
        }

        SaveLoadHelper.SaveData(_dailyOfferData);
    }


    private void InitializePurchaseActionsDictionary()
    {
        _purchaseActions.Add(PurchaseOptions.Energy, ( ()=> _energyService.GiveEnergy(_shopItem.ProductAmount), ()=> OnNotEnoughCurrency?.Invoke("Energy Full Charged!")));
        _purchaseActions.Add(PurchaseOptions.Coin, (ApplyPurchaseReward, ()=> OnNotEnoughCurrency?.Invoke("Not Enough Coin")));
        _purchaseActions.Add(PurchaseOptions.Gem, (ApplyPurchaseReward, ()=> OnNotEnoughCurrency?.Invoke("Not Enough Gem")));
        
    }
    

    private bool HasEnoughCurrency(ShopItem shopItem)
    {
        if (shopItem.PurchaseOptions == PurchaseOptions.Ad)
            return true;

        if (_inventoryManager.PurchaseItem(new PurchaseDetails((int)shopItem.ProductPrice, shopItem.PurchaseOptions)))
            return true;

        OnNotEnoughCurrency?.Invoke($"Not enough {shopItem.PurchaseOptions}");
        //OnFailed?.Invoke();
        return false;
    }

    public class CurrencyData
    {
        public List<bool> coinsData = new ();
        public List<bool> gemsData = new();
    }

    public class DailyOfferData
    {
        public DateTime LastRefreshTime;
        public List<DailyOfferItem> DailyOfferItems = new ();
    }


    public class DailyOfferItem
    {
        public int StockCount;
    }
    
    #endregion
}
