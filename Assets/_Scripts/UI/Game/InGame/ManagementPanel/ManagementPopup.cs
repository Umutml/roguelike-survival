using _Scripts.GameCore.NPC;
using GameCore.PopupSystem;
using Interfaces;
using UnityEngine;
using VContainer;

public class ManagementPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private ManagementPopupContent managementPopupContent;

    #endregion
    
    #region Fields

    private ManagementNpcController _managementNpcController;

    #endregion
    
    #region Public Methods

    public override void OnOpenPopup()
    {
        _managementNpcController = Resolver.Resolve<ManagementNpcController>();
        managementPopupContent.Initialize(_managementNpcController, OnClickOkButton, OnClickCloseButton, Resolver);
        Resolver.Resolve<IGameService>().PauseGame();
    }

    #endregion


    #region Private Methods
    
    private void OnClickOkButton()
    {
        var energyService = Resolver.Resolve<IEnergyService>();
        
        if (energyService.ConsumeEnergy(5))
        {
            _managementNpcController.StartManagement();
        }
        OnClickCloseButton();
    }


    private void OnClickCloseButton()
    {
        Resolver.Resolve<IGameService>().ResumeGame();
        ClosePopup();
    }

    #endregion
}
