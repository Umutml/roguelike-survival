using _Scripts.GameCore.Shop;
using _Scripts.GameCore.Vibration.Constants;
using GameCore.Scriptables;
using Interfaces;
using UI.Game.Architectural;
using UI.Game.InGame.ShopPopup;
using UnityEngine;
using VContainer;


public class ShopBaseItem : Content
{
    #region Fields

    private ShopItem _shopItem;
    private ShopManager _shopManager;
    private VibrationManager _vibrationManager;
    private IAPurchaseManager _purchaseManager;
    private IAnalyticsService _analyticsService;
    private Animator _segmentAnimator;
    private int _slotIndex;
    private bool _isBuyable = true;
    
    private static readonly int Click = Animator.StringToHash("Click");

    #endregion


    #region Properties

    protected ShopManager ShopManager => _shopManager;
    protected IAPurchaseManager PurchaseManager => _purchaseManager;
    protected ShopItem ShopItem => _shopItem;
    protected bool IsBuyable
    {
         get => _isBuyable;
         set => _isBuyable = value;
    }

    protected int SlotIndex => _slotIndex;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _segmentAnimator = GetComponent<Animator>();
    }

    #endregion


    #region Public Methods

    public void InitializeSegment(IObjectResolver resolver, ShopItem shopItem, int slotIndex)
    {
        _shopItem = shopItem;
        _vibrationManager = resolver.Resolve<VibrationManager>();
        _shopManager = resolver.Resolve<ShopManager>();
        _analyticsService = resolver.Resolve<IAnalyticsService>();
        _purchaseManager = resolver.Resolve<IAPurchaseManager>();
        _slotIndex = slotIndex;
        Initialize();
    }

    #endregion


    #region Public Methods

    protected virtual async void Initialize()
    {
        var itemImage = await _shopItem.GetProductImage(); 
        SetImage(ShopPopupConstants.ItemImage, itemImage);
        SetText(ShopPopupConstants.Amount, $"{_shopItem.ProductAmount}");
        OnClickListen(ShopPopupConstants.BuyButton, BuyItem);
    }


    protected virtual void BuyItem()
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        if (!_isBuyable) return;
        
        _segmentAnimator.SetTrigger(Click);
        _shopManager.BuyShopItem(_shopItem, Success, Failed);
    }


    protected virtual void Success(PurchaseOptions productType)
    {
        _analyticsService.LogEvent(new EventParameters<string> { EventName = $"{_shopItem.EventParameter}"});
      
        _shopManager.InvokePurchaseAction(productType, true);
    }


    protected virtual void Failed(PurchaseOptions productType)
    {
        _shopManager.InvokePurchaseAction(productType, false);
    }

    #endregion
}
