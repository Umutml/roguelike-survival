using System;
using Interfaces;
using UI.Game.Architectural;
using UnityEngine.UI;
using VContainer;

public class EnergyRefillPopupContent : Content
{
    #region Consts
    
    private const string NO_BUTTON = "RefillEnergyNoButton";
    private const string WATCH_AD_BUTTON = "RefillEnergyWatchAdButton";
    private const string SLIDER_TEXT = "SliderText";
    private const string SLIDER = "Slider";

    #endregion


    #region Fields

    private Slider _slider;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _slider = GetSlider(SLIDER);
    }

    #endregion


    #region Public Methods

    public void Initialize(IEnergyService energyService,Action okAction, Action closeAction, Action watchAdAction, IObjectResolver resolver = null)
    {
        SetText(SLIDER_TEXT, $"{energyService.CurrentEnergy} / {energyService.MaxEnergy}");
        _slider.value = energyService.CurrentEnergy / (float) energyService.MaxEnergy;
        OnClickListen(NO_BUTTON, closeAction, resolver);
        OnClickListen(WATCH_AD_BUTTON, watchAdAction, resolver);
    }
    #endregion
}
