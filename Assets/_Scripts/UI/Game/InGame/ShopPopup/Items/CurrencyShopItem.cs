using GameCore.Scriptables;
using UI.Game.InGame.ShopPopup;
using UnityEngine;


public class CurrencyShopItem : ShopBaseItem
{
    protected override void Initialize()
    {
        base.Initialize();

        if (ShopItem.PurchaseOptions.Equals(PurchaseOptions.IAP))
        {
            PurchaseManager.OnSuccess += Success;
        }
        
        var isCoin = ShopItem.ProductType.Equals(PurchaseOptions.Coin);
        SetText(ShopPopupConstants.Price, $"{(isCoin ? "" : "$ ")}{ShopItem.ProductPrice}");
        SetText(ShopPopupConstants.BonusAmount, $"Bonus<br>+<color=#FFD300>{ShopItem.BonusAmount}</color>");
        var currencyList = ShopItem.PurchaseOptions.Equals(PurchaseOptions.Gem) ? ShopManager.Currency.coinsData : ShopManager.Currency.gemsData;
        SetGameObject(ShopPopupConstants.BonusArea, currencyList[SlotIndex]);
    }

    protected override void Success(PurchaseOptions productType)
    {
        base.Success(productType);
        
        var currencyList = ShopItem.PurchaseOptions.Equals(PurchaseOptions.Gem) ? ShopManager.Currency.coinsData : ShopManager.Currency.gemsData;
        if (currencyList[SlotIndex])
        {
            ShopManager.GiveBonusCurrency(ShopItem.ProductType, ShopItem.BonusAmount);
        }
        
        if (ShopItem.PurchaseOptions.Equals(PurchaseOptions.Gem))
        {
            ShopManager.Currency.coinsData[SlotIndex] = false;
        }
        else
        {
            ShopManager.Currency.gemsData[SlotIndex] = false;
        }
  
        ShopManager.SaveCurrencyData();
        SetGameObject(ShopPopupConstants.BonusArea, false);
    }
}