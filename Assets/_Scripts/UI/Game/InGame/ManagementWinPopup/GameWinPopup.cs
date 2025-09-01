using GameCore.PopupSystem;
using Interfaces;
using UnityEngine;
using VContainer;

public class GameWinPopup : Popup
{
    #region Serializable fields

    [SerializeField] private GameWinPopupContent gameWinPopupContent;

    #endregion
    
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        Resolver.Resolve<IGameService>().PauseGame();
        gameWinPopupContent.Initialize(Resolver, OnContinueButton, OnWatchAdButton);
    }

    #endregion


    #region Private Methods

    private void OnContinueButton()
    {
        Resolver.Resolve<IGameService>().ResumeGame();
        ClosePopup();
    }
    
    private void OnWatchAdButton()
    {
        var mediationService = Resolver.Resolve<IMediationService>();
        mediationService.ShowRewardedAd(IMediationService.DoubledPlacementId);
        ClosePopup();
    }

    #endregion
}
