using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using GameCore.Scriptables;
using Michsky.UI.ModernUIPack;
using UI.Game.Architectural;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class CharacterSegment : Content
{
    #region Consts

    private const string LOCK = "LockIcon";
    private const string TICK = "TickIcon";
    private const string CHARACTER_IMAGE = "CharacterImage";
    private const string CHARACTER_TITLE = "CharacterTitleText";
    private const string GRADIENT_BACKGROUND = "GradientBackground";
    private const string SEGMENT_BUTTON = "SegmentButton";

    #endregion

    #region Serialized Fields

    [SerializeField] private Gradient grayGradient;

    #endregion

    #region Fields

    private UIGradient _characterGradient;
    private CharacterResources _characterResources;
    private bool _isActive;

    #endregion

    #region Properties
    public bool IsActive => _isActive;
    #endregion


    #region Unity Methods

    private void Awake()
    {
        _characterGradient = GetGameObject(GRADIENT_BACKGROUND).GetComponent<UIGradient>();
    }

    #endregion


    #region Public Methods

    public async void InitializeSegment(CharacterResources characterResources, Action segmentButtonAction, bool isActive)
    {
        _characterResources = characterResources;
        var characterImage = !characterResources.IsLocked ? await Addressables.LoadAssetAsync<Sprite>(_characterResources.CharacterArt).BindTo(gameObject) : await Addressables.LoadAssetAsync<Sprite>(_characterResources.CharacterGrayArt).BindTo(gameObject);
        SetImage(CHARACTER_IMAGE, characterImage);
        SetState(isActive);
        SetText(CHARACTER_TITLE, characterResources.CharacterName);
        SetGameObject(LOCK, characterResources.IsLocked);
        OnClickListen(SEGMENT_BUTTON, segmentButtonAction);
        _characterGradient.EffectGradient = !characterResources.IsLocked ? _characterResources.CharacterGradient : grayGradient;

    }

    public void SetState(bool isActive)
    {
        SetGameObject(TICK, isActive);
        _isActive = isActive;
    }

    #endregion
}
