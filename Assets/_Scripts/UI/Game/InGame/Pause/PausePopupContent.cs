using System;
using UI.Game.Architectural;
using UnityEngine;
using System.Collections.Generic;
using _Scripts.GameCore.Vibration.Constants;
using UI.Game.InGame.PausePopup.Constants;
using VContainer;

public class PausePopupContent : Content
{
    #region Fields

    private readonly Dictionary<string, string> _buttonToUrl = new ()
    {
        {PausePopupConstants.TERMS_OF_USE, "https://nosurrenderheroes.io/term-of-use"},
        {PausePopupConstants.PRIVACY_POLICY, "https://nosurrenderheroes.io/privacy-policy"},
        {PausePopupConstants.TERM_OF_SERVICE, "https://nosurrenderheroes.io/terms-of-service"},
        {PausePopupConstants.COOKIE_POLICY, "https://nosurrenderheroes.io/cookie-policy"}
    };
    

    #endregion
    
    
    #region Public Methods
    
    public void SetRestartCheckpointButtonActive(bool isActive)
    {
        GetButton(PausePopupConstants.RESTART_CHECKPOINT).interactable = isActive;
    }
    

    public void Initialize(VibrationManager vibrationManager, Action closePopup, Action restartCheckPoint, IObjectResolver resolver = null)
    {
        
        
        OnClickListen(PausePopupConstants.TERMS_OF_USE, () => OpenURL(vibrationManager, _buttonToUrl[PausePopupConstants.TERMS_OF_USE]), resolver);
        OnClickListen(PausePopupConstants.PRIVACY_POLICY, () => OpenURL(vibrationManager, _buttonToUrl[PausePopupConstants.PRIVACY_POLICY]), resolver);
        OnClickListen(PausePopupConstants.TERM_OF_SERVICE, () => OpenURL(vibrationManager, _buttonToUrl[PausePopupConstants.TERM_OF_SERVICE]), resolver);
        OnClickListen(PausePopupConstants.COOKIE_POLICY, () => OpenURL(vibrationManager, _buttonToUrl[PausePopupConstants.COOKIE_POLICY]), resolver);
        OnClickListen(PausePopupConstants.RESTART_CHECKPOINT, restartCheckPoint, resolver);
        OnClickListen(PausePopupConstants.RESUME_BUTTON, closePopup, resolver);
        OnClickListen(PausePopupConstants.CLOSE_BUTTON, closePopup, resolver);
    }

    #endregion


    #region Private Methods

    private void OpenURL(VibrationManager vibrationManager, string targetURL)
    {
        Application.OpenURL(targetURL);
        vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
    }

    #endregion
}
