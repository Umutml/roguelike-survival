using _Scripts.GameCore.Vibration.Constants;
using GameCore.PopupSystem;
using UI.Game.InGame.LowPowerPopup;
using UnityEngine;
using VContainer;

public class LowPowerPopup : Popup
{

    [SerializeField] private LowPowerPopupContent content;

    private VibrationManager _vibrationManager;
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        _vibrationManager = Resolver.Resolve<VibrationManager>();
        content.Initialize(OpenSettings, Continue, Close);
    }

    private void OpenSettings()
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
#if UNITY_IOS
        Application.OpenURL("app-settings:");
#endif
    }

    private void Continue()
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        ClosePopup();
    }

    private void Close()
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        ClosePopup();
    }

    #endregion
}
