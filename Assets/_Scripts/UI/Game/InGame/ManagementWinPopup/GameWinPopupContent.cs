using System;
using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Level;
using GameCore.Player;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using UI.Game.Architectural;
using UI.Game.InGame.GameWinPopup.Constants;
using VContainer;
using Random = UnityEngine.Random;

public class GameWinPopupContent : Content
{
    #region Fields

    private ManagementNpcController _managementNpcController;
    private PlayerStatusController _playerStatusController;
    private PlayerController _playerController;
    private GameInventoryManager _gameInventoryManager;
    private WaveManager _waveManager;
    private WaveLevelManager _waveLevelManager;
    private int _lastIncrementedCoin;
    private int _lastIncrementedGem;

    #endregion


    #region Public Methods

    public void Initialize(IObjectResolver resolver, Action onContinue, Action onWatchAd)
    {
        _playerController = resolver.Resolve<PlayerController>();
        _playerStatusController = resolver.Resolve<PlayerStatusController>();
        _gameInventoryManager = resolver.Resolve<GameInventoryManager>();
        _waveManager = resolver.Resolve<WaveManager>();
        _waveLevelManager = resolver.Resolve<WaveLevelManager>();
        _managementNpcController = resolver.Resolve<ManagementNpcController>();

        SetText(GameWinPopupConstants.TitleText,
            _playerController.GetInitialSkinKey() == "Hattori" ? "Hattori" : "Ms Blaster");
        SetStats();
        SetWave();
        SetStage();
        SetLevel();
        SetDamageInfo();
        SetInventory();
        OnClickListen(GameWinPopupConstants.CONTINUE_BUTTON, onContinue, resolver);
        OnClickListen(GameWinPopupConstants.WatchADButton, onWatchAd, resolver);
        IronSourceRewardedVideoEvents.onAdRewardedEvent += GiveDoubleRewardAfterAd;
    }

    public void SetWave()
    {
        var wave = _waveManager.CurrentWave;
        SetText(GameWinPopupConstants.WaveText, GetValueByColor("Wave", $"{wave.level}"));
        SetText(GameWinPopupConstants.TimeText, GetValueByColor("Time", $"{wave.level * 30}sec"));
    }


    public void SetStage()
    {
        SetText(GameWinPopupConstants.StageText,
            GetValueByColor("Stage", $"{_managementNpcController.GetManagementStateDetails().Item2 + 1}"));
    }

    public void SetLevel()
    {
        SetText(GameWinPopupConstants.LevelText, $"Lv {_waveLevelManager.CurrentLevelDetails.level}");
    }

    #endregion


    #region Private Methods

    private void SetStats()
    {
        SetText(GameWinPopupConstants.MaxHpIncrementText, GetStats());
        SetText(GameWinPopupConstants.PickupRangeIncrementText, GetStats());
        SetText(GameWinPopupConstants.SpeedIncrementText, GetStats());
        SetText(GameWinPopupConstants.AttackSpeedIncrementText, GetStats());
    }


    private void SetDamageInfo()
    {
        SetText(GameWinPopupConstants.KillCountText,
            $"Kill Count\\n<color=#FDD828>{_playerStatusController.KillCount}</color>");
        SetText(GameWinPopupConstants.DamageText,
            $"Damage\\n<color=#FDD828>{_playerStatusController.GivenDamage}</color>");
    }

    private void SetInventory()
    {
        SetText(GameWinPopupConstants.GemText, $"{_gameInventoryManager.WaveGemAmount}");
        SetText(GameWinPopupConstants.CoinText, $"{_gameInventoryManager.WaveCoinAmount}");
        _lastIncrementedCoin = _gameInventoryManager.WaveCoinAmount;
        _lastIncrementedGem = _gameInventoryManager.WaveGemAmount;
    }

    private void GiveDoubleRewardAfterAd(IronSourcePlacement placement, IronSourceAdInfo adinfo)
    {
        if (placement.getPlacementName().Equals(IMediationService.DoubledPlacementId))
        {
            SetText(GameWinPopupConstants.CoinText, $"{_lastIncrementedCoin * 2}");
            SetText(GameWinPopupConstants.GemText, $"{_lastIncrementedGem * 2}");
            _gameInventoryManager.ModifyCurrencyBalance(new PurchaseDetails(_lastIncrementedGem, PurchaseOptions.Gem));
            _gameInventoryManager.ModifyCurrencyBalance(new PurchaseDetails(_lastIncrementedCoin, PurchaseOptions.Coin));
            LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + placement.getPlacementName() + "And AdInfo " + adinfo);
        }
    }

    private string GetValueByColor(string title, string value) => $"<color=#FF9D69>{title}</Color> {value}";

    private string GetStats() => $"+{Random.Range(50, 100)}%";

    #endregion
}
