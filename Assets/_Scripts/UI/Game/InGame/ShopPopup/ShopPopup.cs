using _Scripts.GameCore.Vibration.Constants;
using GameCore.Inventory;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using TMPro;
using UnityEngine;
using VContainer;

public class ShopPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private AllShopItemResources allShopItems;
    [SerializeField] private Transform itemsContent;
    [SerializeField] private ShopInfoPopup shopInfoPopup;
    [SerializeField] private ShopPopupTopbar shopPopupTopbar;

    #endregion

    
    #region Fields

    private VibrationManager _vibrationManager;
    private ShopManager _shopManager;
    private AlertManager _alertManager;
    private GameObject _instance;

    #endregion


    #region Unity Methods

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion
    
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        _vibrationManager = Resolver.Resolve<VibrationManager>();
        _shopManager = Resolver.Resolve<ShopManager>();
        _alertManager = Resolver.Resolve<AlertManager>();
        _shopManager.CheckDailyOfferData();
        shopPopupTopbar.InitializeCurrency(Resolver.Resolve<GameInventoryManager>(), Resolver.Resolve<IEnergyService>());
        _shopManager.IconTransforms = shopPopupTopbar.IconTransforms;
        
        SubscribeToEvents();
        InitializeShopItems();
    }

    #endregion


    #region Private Methods

    private async void InitializeShopItems()
    {
        foreach (var itemResource in allShopItems.ShopItemResources)
        {
            if (itemResource.Enable is false) continue;
            
            var shopItem = await itemResource.GetItemReference();
            _instance = Instantiate(shopItem, itemsContent);
            _instance.transform.name = shopItem.name;
            _instance.GetComponent<ShopOfferItem>().InitializeOffer(itemResource, Resolver, () => OpenInfoPopup(itemResource.ItemDescription));
        }
    }
    
    
    private void PlayShopInfoAnimation(string targetText)
    {
        _alertManager.CallAlert(targetText);
    }
    
    
    private void OpenInfoPopup(string description)
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        shopInfoPopup.SetOfferInfo(description);
    }
    
    
    private void SubscribeToEvents()
    {
        Resolver.Resolve<GameInventoryManager>().OnCoinsChanged += shopPopupTopbar.SetCoinText;
        Resolver.Resolve<GameInventoryManager>().OnGemsChanged += shopPopupTopbar.SetGemText;
        Resolver.Resolve<IEnergyService>().OnEnergyChanged += shopPopupTopbar.SetEnergyText;
        Resolver.Resolve<ShopManager>().OnNotEnoughCurrency += PlayShopInfoAnimation;
    }
    
    
    private void UnsubscribeFromEvents()
    {
        Resolver.Resolve<GameInventoryManager>().OnCoinsChanged -= shopPopupTopbar.SetCoinText;
        Resolver.Resolve<GameInventoryManager>().OnGemsChanged -= shopPopupTopbar.SetGemText;
        Resolver.Resolve<IEnergyService>().OnEnergyChanged -= shopPopupTopbar.SetEnergyText;
        Resolver.Resolve<ShopManager>().OnNotEnoughCurrency -= PlayShopInfoAnimation;
    }

    #endregion
}
