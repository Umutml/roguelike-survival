using UI.Game.InGame.ShopPopup;

public class DailyOfferItem : ShopOfferItem
{
    private UITimer _uiTimer;
    
    protected override void SetupOfferTimer()
    { 
        base.SetupOfferTimer();
        SetupDailyTimer();
    }
    
    
    private void SetupDailyTimer()
    {
        if (!gameObject.TryGetComponent(out UITimer uiTimer)) return;
        
        _uiTimer = uiTimer;
        var currentDailyTime = _shopManager.DailyOffer.LastRefreshTime;
        var endTime = currentDailyTime.AddDays(1);
        
        _uiTimer.CreateTimer(GetText(ShopPopupConstants.Time), string.Empty, "FFFFFF", endTime);
    }
}
