using System;
using GameCore.Health;
using GameCore.PopupSystem;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using VContainer;
using System.Collections.Generic;
using _Scripts.Utilities;
using GameCore.Player;
using GameCore.Scriptables;
using TMPro;
using UI.Game.InGame.LevelUp;
using UnityEngine.UI;

public class LevelUpPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private Button WatchAdButton;
    [SerializeField] private LevelUpPopupContent levelUpContent;
    [SerializeField] private List<InGameLevelUpSkillArea> skillAreas;
    [SerializeField] private TextMeshProUGUI killsValueText, waveText;
    private ObjectiveManager _objectiveManager;
    #endregion

    #region Fields

    private PlayerSkillController _playerSkillController;
    private bool _isInitialized;
    private IAnalyticsService _iAnalyticsService;

    #endregion

    #region Unity Methods

    private void OnDestroy()
    {
        Resolver.Resolve<PlayerStatusController>().KillCountChanged -= levelUpContent.SetKillsValueText;
        Resolver.Resolve<WaveManager>().WaveUpdated -= levelUpContent.SetWaveText;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    #endregion

    #region Public Methods

    public override void OnOpenPopup()
    {
        _objectiveManager = Resolver.Resolve<ObjectiveManager>();
        _iAnalyticsService = Resolver.Resolve<IAnalyticsService>();
        Resolver.Resolve<IGameService>().PauseGame();
        _playerSkillController = Resolver.Resolve<PlayerSkillController>();
        SubscribeToEvents();
        _isInitialized = true;
        SetupSkillAreas();
        CheckAdButtonState();
        waveText.text = _objectiveManager.ObjectiveUpgradeProgress.Item1.ToString();
        killsValueText.text = _objectiveManager.ObjectiveUpgradeProgress.Item2.ToString();
    }
    public override void Initialize(object data)
    {
        base.Initialize(data);

        if (data is not Tuple<(Skill skill, int level), (Skill skill, int level), (Skill skill, int level)> skillsDetail)
        {
            LoggerNS.LogError("Data is not a tuple of skills.");
            return;
        }

        var skillList = new[] { skillsDetail.Item1, skillsDetail.Item2, skillsDetail.Item3 };

        if (skillAreas.Count < skillList.Length)
        {
            LoggerNS.LogError("Not enough skill areas to initialize.");
            return;
        }

        for (int i = 0; i < skillList.Length; i++)
        {
            InitializeSkillArea(i, skillList[i]);
        }
    }

    private void CheckAdButtonState()
    {
        if (PlayerPrefs.GetInt("WatchAdButtonUsed", 0) == 1)
        {
            WatchAdButton.gameObject.SetActive(false);
        }
        else
        {
            WatchAdButton.gameObject.SetActive(true);
            WatchAdButton.onClick.AddListener(WatchAdButtonClicked);
        }
    }

    #endregion

    #region Private Methods

    private void SetupSkillAreas()
    {
        var upgradeType = _objectiveManager.IsProgress ? _objectiveManager.ActiveObjectiveHub.upgradeType : UpgradeType.All;
        if (upgradeType == UpgradeType.None)
        {
            OnClickCloseButton();
            return;
        }
        var skills = _playerSkillController.GetRandomSkills(upgradeType);
        for (var i = 0; i < skillAreas.Count; i++)
        {
            InitializeSkillArea(i, skills[i]);
        }
    }

    private void InitializeSkillArea(int index, (Skill skill, int level) skillData)
    {
        skillAreas[index].Initialize(
            (skillData.skill, skillData.level),
            _playerSkillController.SkillColorData.Find(x => x.upgradeType.Equals(skillData.skill.upgradeType)),
            () =>
            {
                _playerSkillController.ApplySkillUpgrade(skillData.skill);
                OnClickCloseButton();
                SendOnClickUpgradeEvent(skillData.skill);
            });
    }


    private void SendOnClickUpgradeEvent(Skill skill)
    {
        var skillName = skill.name.Replace(" ", "_").ToLower(); // Replace spaces with underscores and convert to lowercase
        _iAnalyticsService.LogEvent(new EventParameters<string> { EventName = $"skill_{skillName}" });
    }

    private void OnClickCloseButton()
    {
        Resolver.Resolve<IGameService>().ResumeGame();
        ClosePopup();
    }

    private void SubscribeToEvents()
    {
        if (_isInitialized) return;

        Resolver.Resolve<PlayerStatusController>().KillCountChanged += levelUpContent.SetKillsValueText;
        Resolver.Resolve<WaveManager>().WaveUpdated += levelUpContent.SetWaveText;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    }
    #endregion

    #region Mediation-Ads
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (ironSourcePlacement.getPlacementName().Equals(IMediationService.PerkRefreshPlacementId))
        {
            Debug.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() + "And AdInfo " + adInfo);
            SetupSkillAreas();
        }
        else
        {
            LoggerNS.LogError("Placement name is not equal to Refresh Perk PlacementID.");
        }
    }
    #endregion

    public void WatchAdButtonClicked()
    {
        IMediationService mediationService = Resolver.Resolve<IMediationService>();
        mediationService.ShowRewardedAd(IMediationService.PerkRefreshPlacementId);

        // Disable the button GameObject and save the state
        WatchAdButton.gameObject.SetActive(false);
        PlayerPrefs.SetInt("WatchAdButtonUsed", 1);
        PlayerPrefs.Save();
    }
}
