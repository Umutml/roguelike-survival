using System;
using UI.Game.Architectural;

namespace UI.Game.InGame.LowPowerPopup
{
    public class LowPowerPopupContent : Content
    {
        private const string SETTINGS_BUTTON = "SettingsButton";
        private const string CONTINUE_BUTTON = "ContinueButton";
        private const string CLOSE_BUTTON = "CloseButton";

        public void Initialize(Action settingsAction, Action continueAction, Action closeAction)
        {
            OnClickListen(SETTINGS_BUTTON, settingsAction);
            OnClickListen(CONTINUE_BUTTON, continueAction);
            OnClickListen(CLOSE_BUTTON, closeAction);
        }
    }
}
