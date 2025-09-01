using GameCore.Scriptables;
using UI.Game.InGame.ShopPopup;


public class DailyShopItem : ShopBaseItem
{
    protected override void Initialize()
    {
        base.Initialize();
        IsBuyable = ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0;
        SetGameObject(ShopPopupConstants.TickIcon, !(ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0));
        SetGameObject(ShopPopupConstants.FreeContent, ShopItem.PurchaseOptions.Equals(PurchaseOptions.Free));
        SetGameObject(ShopPopupConstants.PriceContent, !ShopItem.PurchaseOptions.Equals(PurchaseOptions.Free));
        SetGameObject(ShopPopupConstants.GemIcon, ShopItem.PurchaseOptions.Equals(PurchaseOptions.Gem));
        SetGameObject(ShopPopupConstants.CoinIcon, ShopItem.PurchaseOptions.Equals(PurchaseOptions.Coin));
        SetGameObject(ShopPopupConstants.PriceArea, (ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0));
        SetText(ShopPopupConstants.Price, $"{ShopItem.ProductPrice}");
        
        if (ShopItem.PurchaseOptions.Equals(PurchaseOptions.Ad))
        {
            SetGameObject(ShopPopupConstants.WatchArea, true);
            SetGameObject(ShopPopupConstants.WatchArea, (ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0));
            SetText(ShopPopupConstants.Count, $"{ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount}");
            GetButton(ShopPopupConstants.BuyButton).interactable =
                ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0;
        }
    }

    
    protected override void Success(PurchaseOptions productType)
    {
        base.Success(productType);
        ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount--;
        ShopManager.SaveDailyOfferData();
        SetText(ShopPopupConstants.Count, $"{ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount}");
        GetButton(ShopPopupConstants.BuyButton).interactable =
            ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0;
        SetGameObject(ShopPopupConstants.TickIcon, !(ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0));
        SetGameObject(ShopPopupConstants.PriceArea, (ShopManager.DailyOffer.DailyOfferItems[SlotIndex].StockCount > 0));
    }
}
