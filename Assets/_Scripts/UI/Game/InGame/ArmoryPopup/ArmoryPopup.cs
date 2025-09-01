using System.Collections.Generic;
using System.Threading;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.Player.WeaponSystem;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class ArmoryPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Animator statsArmor;
    [SerializeField] private WeaponResources weaponResources;
    [SerializeField] private ArmoryWeaponSegment weaponSegment;
    [SerializeField] private ArmoryStats armoryStats;
    [SerializeField] private Transform weaponsParent;
    [SerializeField] private Image statsBackground;
    [SerializeField] private GameObject tutorialHand;
    [SerializeField] private GameObject closeButtonPoint;
    [SerializeField] private Sprite coin;
    [SerializeField] private Sprite gem;
    [SerializeField] private Sprite ads;

    #endregion


    #region Fields

    private ArmoryWeaponSegment _weaponSegment;
    private WeaponData _weaponData;
    private PlayerWeaponController _playerWeaponController;
    private VibrationManager _vibrationManager;
    private AlertManager _alertManager;
    private PlayerWeaponData _playerWeaponData;
    private readonly List<ArmoryWeaponSegment> _weaponSegments = new();
    private List<string> _unlockedWeapons = new();
    private const string OPEN = "Open";
    private const string CLOSE = "Close";
    private readonly UniTaskCompletionSource _initSegmentCompletionSource = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    #endregion


    #region Properties

    public WeaponData WeaponData
    {
        get => _weaponData;
        set
        {
            _weaponData = value;
            InitializeStatsPopup(_weaponData);
        }
    }


    public PlayerWeaponData PlayerWeaponData => _playerWeaponData;

    #endregion


    #region Public Methods

    public override void OnOpenPopup()
    {
        _vibrationManager = Resolver.Resolve<VibrationManager>();
        _playerWeaponController = Resolver.Resolve<PlayerController>().WeaponController;
        _alertManager = Resolver.Resolve<AlertManager>();
        _playerWeaponData = GetPlayerWeaponData();
        _unlockedWeapons = _playerWeaponData.unlockedWeapons;
        CreateSegments();
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    }

    public override async void InitializeTutorial(object data)
    {
        base.InitializeTutorial(data);
        if (data is not string weaponName)
        {
            LoggerNS.LogError("Weapon Name is null");
            return;
        }

        await _initSegmentCompletionSource.Task;

        if (_weaponSegments is not {Count: > 0})
        {
            LoggerNS.LogError("Weapon Segments is null or empty");
            return;
        }

        if (scrollRect != null)
        {
            scrollRect.enabled = false;
        }

        foreach (var segment in _weaponSegments)
        {
            var isTargetWeapon = segment.WeaponData.WeaponName.Equals(weaponName);
            segment.SetActiveState(isTargetWeapon);

            if (!isTargetWeapon) continue;
            tutorialHand.SetActive(true);
            tutorialHand.transform.position = segment.SelectTransform.transform.position;
        }


        await UniTaskAsyncHelper.WaitUntil(() => _playerWeaponData.usingWeapon == weaponName,
            1000,
            true,
            _cancellationTokenSource.Token);

        tutorialHand.transform.position = closeButtonPoint.transform.position;
        tutorialHand.SetActive(true);
    }


    public void OpenStatsPopup(bool open)
    {
        statsArmor.SetTrigger(open ? OPEN : CLOSE);

        statsBackground.enabled = open;
        statsBackground.raycastTarget = open;
    }

    #endregion


    #region Private Methods

    private void BuyWeapon(WeaponData weaponData)
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        if (_unlockedWeapons.Contains(weaponData.WeaponName)) return;

        if (weaponData.PurchaseOptions.Equals(PurchaseOptions.Ad))
        {
            var mediationService = Resolver.Resolve<IMediationService>();
            mediationService.ShowRewardedAd(IMediationService.ArmoryRocketPlacementId);
            _weaponData = weaponData;
        }
        else
        {
            if (TryPurchaseUpgrade(weaponData))
            {
                SaveLoadHelper.UpdateData<PlayerWeaponData>(data =>
                {
                    data.usingWeapon = weaponData.WeaponName;
                    data.unlockedWeapons.Add(weaponData.WeaponName);
                });

                _playerWeaponData = GetPlayerWeaponData();
                _unlockedWeapons = _playerWeaponData.unlockedWeapons;
                _playerWeaponController.SwitchToWeapon(GetWeaponName(_playerWeaponData.usingWeapon),
                    WeaponSlot.SlotType.RightHand);
                UpdatedSegments();
            }
            else
            {
                _alertManager.CallAlert("Not enough coin");
            }
        }
    }

    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (ironSourcePlacement.getPlacementName().Equals(IMediationService.ArmoryRocketPlacementId))
        {
            if (_unlockedWeapons.Contains(_weaponData.WeaponName)) return;

            LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() +
                "And AdInfo " + adInfo);
            SaveLoadHelper.UpdateData<PlayerWeaponData>(data =>
            {
                data.usingWeapon = _weaponData.WeaponName;
                data.unlockedWeapons.Add(_weaponData.WeaponName);
            });

            _playerWeaponData = GetPlayerWeaponData();
            _unlockedWeapons = _playerWeaponData.unlockedWeapons;
            _playerWeaponController.SwitchToWeapon(GetWeaponName(_playerWeaponData.usingWeapon),
                WeaponSlot.SlotType.RightHand);
            UpdatedSegments();
        }
    }

    private void InitializeStatsPopup(WeaponData weaponData)
    {
        var unlocked = _unlockedWeapons.Contains(weaponData.WeaponName);
        armoryStats.InitializeStats(weaponData, unlocked);
        OpenStatsPopup(true);
    }


    private void CreateSegments()
    {
        foreach (var weaponData in weaponResources.Weapons)
        {
            _weaponSegment = Instantiate(weaponSegment, weaponsParent);
            _weaponSegment.InitializeSegment(this,
                weaponData,
                GetPriceIcon(weaponData.WeaponBuyType),
                () => BuyWeapon(weaponData),
                () => SelectWeapon(weaponData.WeaponName),
                Resolver.Resolve<IInventoryManager>().GetCurrencyBalance(PurchaseOptions.Coin) >=
                weaponData.WeaponPrice);

            _weaponSegment.name = weaponData.WeaponName;
            _weaponSegments.Add(_weaponSegment);
        }

        _initSegmentCompletionSource.TrySetResult();
    }


    private void UpdatedSegments()
    {
        for (var i = 0; i < _weaponSegments.Count; i++)
        {
            _weaponSegments[i].UpdateSegment(this,
                weaponResources.Weapons[i],
                Resolver.Resolve<IInventoryManager>().GetCurrencyBalance(PurchaseOptions.Coin) >=
                weaponResources.Weapons[i].WeaponPrice);
        }
    }


    private void SelectWeapon(string weaponName)
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        _playerWeaponData.usingWeapon = weaponName;

        SaveLoadHelper.UpdateData<PlayerWeaponData>(data =>
        {
            data.usingWeapon = weaponName;
            data.unlockedWeapons = _unlockedWeapons;
        });

        _ = _playerWeaponController.SwitchToWeapon(GetWeaponName(weaponName), WeaponSlot.SlotType.RightHand);
        UpdatedSegments();
    }


    private bool TryPurchaseUpgrade(WeaponData weaponData)
    {
        return Resolver.Resolve<IInventoryManager>().PurchaseItem(
            new PurchaseDetails((int) weaponData.WeaponPrice, weaponData.PurchaseOptions));
    }


    private Sprite GetPriceIcon(WeaponBuyType weaponBuyType) =>
        weaponBuyType switch
        {
            WeaponBuyType.Coin => coin,
            WeaponBuyType.Gem => gem,
            WeaponBuyType.Ad => ads,
            _ => null
        };

    private void OnDestroy()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    private PlayerWeaponData GetPlayerWeaponData() => SaveLoadHelper.TryLoadPersistentData<PlayerWeaponData>();
    private string GetWeaponName(string weaponName) => weaponName.Replace(" ", "");

    #endregion
}
