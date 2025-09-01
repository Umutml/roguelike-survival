using UI.Game.Architectural;
using UI.Game.InGame.ShopPopup;

public class ShopInfoPopup : Content
{
    #region Public Methods

    public void SetOfferInfo(string descriptionText)
    {
        SetText(ShopPopupConstants.Description ,descriptionText);

        gameObject.SetActive(true);
    }
    

    #endregion
}
