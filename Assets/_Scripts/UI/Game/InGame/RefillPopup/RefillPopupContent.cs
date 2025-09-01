using System;
using UI.Game.Architectural;
using VContainer;

public class RefillPopupContent : Content
{
    #region Consts

    private const string OK_BUTTON = "OkButton";
    private const string NO_BUTTON = "NoButton";
    private const string WATCH_AD_BUTTON = "WatchAdButton";

    #endregion


    #region Public Methods

    public void Initialize(Action okAction, Action closeAction, Action watchAdAction, IObjectResolver resolver = null)
    {
        OnClickListen(OK_BUTTON, okAction, resolver);
        OnClickListen(NO_BUTTON, closeAction, resolver);
        OnClickListen(WATCH_AD_BUTTON, watchAdAction, resolver);
    }
    #endregion
}
