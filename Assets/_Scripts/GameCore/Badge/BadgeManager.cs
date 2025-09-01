using System;
using System.Collections.Generic;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using VContainer;

public class BadgeManager : MonoBehaviour
{
    #region Actions

    public Action<bool> OnEnableBadges;

    #endregion


    #region Serializable Fields

    [SerializeField] private BadgeResources badgeResources;
    [SerializeField] private BadgeSegment badgeSegment;
    [SerializeField] private GameObject badgeParents;
    [SerializeField] private GameObject badgeDropdown;
    [SerializeField] private GameObject shopNPC;

    #endregion


    #region Fields

    private PopupManager _popupManager;
    private BadgeSegment _badgeInstance;
    private IAnalyticsService _analyticsService;
    private readonly List<BadgeSegment> _badgeSegments = new();
    private ITutorialService _tutorialService;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        InitializeAllBadges();
    }


    private void OnEnable()
    {
        OnEnableBadges += SetEnabledBadges;
    }


    private void OnDestroy()
    {
        OnEnableBadges -= SetEnabledBadges;
    }

    #endregion


    #region Private Methods

    [Inject]
    private void Init(PopupManager popupManager, IAnalyticsService analyticsService, ITutorialService tutorialService)
    {
        _popupManager = popupManager;
        _analyticsService = analyticsService;
        _tutorialService = tutorialService;
    }

    private void InitializeAllBadges()
    {
        Debug.Log($"Is Car Upgrade: {PlayerPrefs.GetInt("IsCarUpgrade")}");
        shopNPC.SetActive(PlayerPrefs.GetInt("IsCarUpgrade").Equals(1));

        badgeDropdown.SetActive(true);
        
        CreateSegments();
        SetEnabledBadges(_tutorialService.IsTutorialCompleted);
    }


    private void OpenPopup(PopupConstants.PopupType targetPopup)
    {
        _popupManager.OpenPopup(targetPopup);
        SendBadgeClickEvent(targetPopup.ToString());
    }

    private void SendBadgeClickEvent(string badgeName)
    {
        var lowerCaseBadgeName = badgeName.ToLower();
        _analyticsService?.LogEvent(new EventParameters<string> {EventName = $"badge_{lowerCaseBadgeName}"});
    }

    private void SetEnabledBadges(bool isEnabled)
    {
        badgeDropdown.SetActive(isEnabled);
        shopNPC.SetActive(isEnabled);
        CreateSegments();
    }


    private void CreateSegments()
    {
        if (_badgeSegments.Count > 0)
        {
            return;
        }

        foreach (var badge in badgeResources.Badges)
        {
            _badgeInstance = Instantiate(badgeSegment, badgeParents.transform);
            _badgeInstance.InitializeBadge(badge, () => OpenPopup(badge.PopupType));
            _badgeSegments.Add(_badgeInstance);
        }
    }

    #endregion
}
