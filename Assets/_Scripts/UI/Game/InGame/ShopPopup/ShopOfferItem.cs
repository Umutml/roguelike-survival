using System;
using Cysharp.Threading.Tasks;
using GameCore.Scriptables;
using UI.Game.Architectural;
using UI.Game.InGame.ShopPopup;
using VContainer;

public class ShopOfferItem : Content
{
    #region Fields

    private ShopItemResources _shopItemResources;
    private UITimer _uiTimer;
    private IObjectResolver _resolver;
    protected ShopManager _shopManager;

    #endregion

    #region Public Methods

    public void InitializeOffer(ShopItemResources shopItemResources, IObjectResolver resolver, Action infoPopupAction)
    {
        _shopItemResources = shopItemResources;
        _resolver = resolver;
        _shopManager = _resolver.Resolve<ShopManager>();
        
        SetText(ShopPopupConstants.ItemTitle, shopItemResources.ItemTitle);
        CreateSegments();
        OnClickListen(ShopPopupConstants.InfoButton, infoPopupAction);
        SetupOfferTimer();
    }


    protected virtual void SetupOfferTimer()
    {
        
    }

    #endregion


    #region Private Methods

    private async UniTask CreateSegments()
    {
        if (_shopItemResources.Slots.Count == 0) return;
    
        for (var i = 0; i < _shopItemResources.Slots.Count; i++)
        {
            var itemSegment = await _shopItemResources.GetSegmentReference();
        
            var segment = Instantiate(itemSegment, GetGameObject(ShopPopupConstants.Content).transform);
            segment.GetComponent<ShopBaseItem>().InitializeSegment(_resolver, _shopItemResources.Slots[i].ShopItemList[0], i);
        }
    }

    #endregion
}
