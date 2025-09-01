using System.Globalization;
using System.Text.RegularExpressions;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.Scriptables;
using GameCore.Tutorial;
using Interfaces;
using UI.Game.Architectural;
using UI.Game.InGame.CarUpgrade.Constants;
using UnityEngine;
using VContainer;
using System;
using _Scripts.GameCore.Vibration.Constants;

public class CarUpgradeSegment : Content
{
    #region Actions

    private Action _onFailedPurchase;

    #endregion
    
    
    #region Fields

    private CarUpgradePopup _carUpgradePopup;
    private CarMetaUpgrade _carMetaUpgrade;
    private VibrationManager _vibrationManager;
    private IAnalyticsService _analyticsService;
    private RectTransform _infoAreaTransform;
    private Animator _animator;
    private Sprite _upgradeIcon;
    private bool _isCoinEnough;
    private int _lastTakedSegmentIndex;

    #endregion

    #region Properties

    public int Index { get; private set; }

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        InitializeComponents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onFailedPurchase = null;
    }

    #endregion

    #region Public Methods

    public void InitializeSegment(CarUpgradePopup carUpgradePopup, CarMetaUpgrade carMetaUpgrade, Sprite upgradeIcon,
        int index,
        int lastTakedSegmentIndex,
        IObjectResolver resolver, Action onFailedPurchase)
    {
        Index = index;
        _carMetaUpgrade = carMetaUpgrade;
        _carUpgradePopup = carUpgradePopup;
        _lastTakedSegmentIndex = lastTakedSegmentIndex;
        _upgradeIcon = upgradeIcon;
        _onFailedPurchase ??= onFailedPurchase;

        UpdateSegment(index, _lastTakedSegmentIndex);
        SetUIElements(index);
        AddClickListener(index, resolver);
    }

    public void UpdateSegment(int index, int lastTakedSegmentIndex)
    {
        _infoAreaTransform.anchoredPosition =
            new Vector2(0, index < lastTakedSegmentIndex ? CarUpgradeConstants.INFO_AREA_Y_VALUE : 0);
        _animator.enabled = index == lastTakedSegmentIndex;
        _lastTakedSegmentIndex = lastTakedSegmentIndex;

        UpdateInteractableState(index);
        UpdateVisualState(index);
    }

    #endregion

    #region Private Methods

    private void InitializeComponents()
    {
        _infoAreaTransform = GetGameObject(CarUpgradeConstants.INFO_AREA).GetComponent<RectTransform>();
        _animator = GetGameObject(CarUpgradeConstants.UPGRADE_IMAGE).GetComponent<Animator>();
    }

    private void SetUIElements(int index)
    {
        SetImage(CarUpgradeConstants.UPGRADE_IMAGE, _upgradeIcon);
        SetText(CarUpgradeConstants.LEVEL_TEXT, $"{(index / 2) + 1}");
        SetText(CarUpgradeConstants.TITLE_TEXT, GetFormattedTitle(_carMetaUpgrade.UpgradeDetail.type));
        SetText(CarUpgradeConstants.INCREMENT_TEXT, $"+{_carMetaUpgrade.UpgradeDetail.value}%");
        SetText(CarUpgradeConstants.PRICE_TEXT, _carMetaUpgrade.Price.ToString(CultureInfo.InvariantCulture));
        SetColor(CarUpgradeConstants.PRICE_TEXT, EnoughCurrency() ? Color.white : Color.red);
    }

    private void AddClickListener(int index, IObjectResolver resolver)
    {
        OnClickListen(CarUpgradeConstants.CONTENT, () => HandleUpgradeClick(index), resolver);
    }

    private void UpdateInteractableState(int index)
    {
        GetButton(CarUpgradeConstants.CONTENT).interactable = index == _lastTakedSegmentIndex;
    }

    private void UpdateVisualState(int index)
    {
        var isCompleted = index < _lastTakedSegmentIndex;
        var isCurrent = index == _lastTakedSegmentIndex;

        SetGameObject(CarUpgradeConstants.ACTIVE_SLIDER, isCompleted);
        SetGameObject(CarUpgradeConstants.TICK_ICON, isCompleted);
        SetGameObject(CarUpgradeConstants.PRICE_AREA, index >= _lastTakedSegmentIndex);
        SetGameObject(CarUpgradeConstants.LOCK, index > _lastTakedSegmentIndex);
        SetColor(CarUpgradeConstants.PRICE_TEXT, EnoughCurrency() ? Color.white : Color.red);
        if (!_carUpgradePopup.Resolver.Resolve<TutorialSequenceController>().IsTutorialCompleted && index < 2)
        {
            SetGameObject(CarUpgradeConstants.TUTORIAL_HAND, index == _lastTakedSegmentIndex);
            if (_lastTakedSegmentIndex == 2)
            {
                _carUpgradePopup.ShowTutorialHand();
            }
        }

        SetGameObject(CarUpgradeConstants.LEVEL, index % 2 != 0);
        SetGameObject(CarUpgradeConstants.GLOW, isCurrent);
    }

    private void HandleUpgradeClick(int index)
    {
        if (_vibrationManager == null)
        {
            _vibrationManager = _carUpgradePopup.Resolver.Resolve<VibrationManager>();
        }
        
        if (index != _lastTakedSegmentIndex) return;

        _isCoinEnough = TryPurchaseUpgrade();

        if (_isCoinEnough)
        {
            FinalizeUpgrade(index);
        }
        else
        { 
            _onFailedPurchase?.Invoke();
        }
        
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
    }
    
    private bool EnoughCurrency()
    {
        return _carUpgradePopup.Resolver.Resolve<IInventoryManager>().GetCurrencyBalance(PurchaseOptions.Coin) >= _carMetaUpgrade.Price;
    }

    private bool TryPurchaseUpgrade()
    {
        return _carUpgradePopup.Resolver.Resolve<IInventoryManager>().PurchaseItem(
            new PurchaseDetails((int)_carMetaUpgrade.Price, PurchaseOptions.Coin));
    }

    private void FinalizeUpgrade(int index)
    {
        GetButton(CarUpgradeConstants.CONTENT).interactable = false;

        _carUpgradePopup.ApplySkill(index);
        _analyticsService = _carUpgradePopup.Resolver.Resolve<IAnalyticsService>();
        _analyticsService.LogEvent(new EventParameters<string> { EventName = $"car_upgrade_{index}", AdjustToken = AdjustNsEventTokens.CarUpgrade });

        SaveLoadHelper.UpdateData<CarMetaUpgradeData>(data =>
        {
            if (data.CarMetaList.Find(x => x.CarType.Equals(_carUpgradePopup.SelectedCarType)) is CarMeta carMeta)
            {
                carMeta.UpgradeIndex = index + 1;
            }
            else
            {
                data.CarMetaList.Add(new CarMeta
                {
                    CarType = _carUpgradePopup.SelectedCarType,
                    UpgradeIndex = index + 1
                });
            }
        });

        _carUpgradePopup.UpdateSegments();
    }

    private string GetFormattedTitle(StatUpgradeType type)
    {
        return Regex.Replace(type.ToString(), "(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
    }

    private string GetPriceText(float price) => $"<sprite=6> {price}";

    #endregion
}