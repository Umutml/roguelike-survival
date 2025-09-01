using System;
using _Scripts.GameCore.NPC;
using UI.Game.Architectural;
using VContainer;


public class ManagementPopupContent : Content
{
    #region Consts

    private const string TITLE = "TitleText";
    private const string INFO = "ManagementInfoText";
    private const string CLOSE_BUTTON = "CloseButton";
    private const string OK_BUTTON = "OKButton";

    #endregion
    
    
    #region Public Methods

    public void Initialize(ManagementNpcController managementNpcController, Action okButton, Action closeButton, IObjectResolver resolver = null)
    {
        var managementState = managementNpcController.GetManagementStateDetails();
        SetText(TITLE, $"Rescue Mission {(managementState.Item2) + 1}");
        SetText(INFO, $"You need to complete {managementState.Item1.waveCount} Waves!");
        OnClickListen(OK_BUTTON, okButton, resolver);
        OnClickListen(CLOSE_BUTTON, closeButton, resolver);
    }

    #endregion
}
