using System;
using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Scriptables;
using Michsky.UI.ModernUIPack;
using UI.Game.Architectural;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;


public class CharacterUpgradeInfo : Content, IPointerEnterHandler, IDragHandler, IEndDragHandler
{
    #region Consts

    private const string CHARACTER_NAME = "CharacterNameText";
    private const string BACKGROUND_GRADIENT = "Background";
    private const string CHARACTER_IMAGE = "CharacterImage";
    private const string INFO_BUTTON = "InfoButton";
    private const string CHARACTER_MODEL = "Character3DImage";

    #endregion


    #region Fields

    private ModelVisualManager _modelVisualManager;
    private UIGradient _backgroundGradient;
    private RawImage _modelImage;
    private Transform _modelVisualParent;
    private Tween _rotateTween;
    private bool _isDragging;
    private float _lastMousePositionX;
    private const float RotationSpeed = 0.5f;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        _modelImage = GetGameObject(CHARACTER_MODEL).GetComponent<RawImage>();
        _backgroundGradient = GetGameObject(BACKGROUND_GRADIENT).GetComponent<UIGradient>();
        
        
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        _rotateTween.Kill();
    }

    #endregion
    
    
    #region Unity Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        _rotateTween.Kill();
        _isDragging = true;
        _lastMousePositionX = eventData.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        var deltaX = eventData.position.x - _lastMousePositionX;
        _lastMousePositionX = eventData.position.x;
            
        _modelVisualParent.rotation = Quaternion.Euler(
            _modelVisualParent.rotation.eulerAngles.x,
            _modelVisualParent.rotation.eulerAngles.y - deltaX * RotationSpeed,
            _modelVisualParent.rotation.eulerAngles.z
        );

        _modelVisualManager.ModelVisualContent = _modelVisualParent;
    }
    
    
    public void OnEndDrag(PointerEventData eventData)
    {
        _rotateTween = RotateCarParent();
        _rotateTween.Play();
        _isDragging = false;
    }

    #endregion


    #region Public Methods

    public async void InitializeCharacter(IObjectResolver resolver, CharacterResources characterResources, Action infoButtonAction)
    {
        if (_modelVisualManager == null)
        {
            _modelVisualManager = resolver.Resolve<ModelVisualManager>();
            _modelVisualParent = _modelVisualManager.ModelVisualContent;
        }
        _rotateTween = RotateCarParent();
        _rotateTween.Play();
        
        _backgroundGradient.EffectGradient = characterResources.CharacterGradient;
        
        var characterImage = await Addressables.LoadAssetAsync<Sprite>(characterResources.CharacterArt).BindTo(gameObject);
        var characterModel = await Addressables.LoadAssetAsync<GameObject>(characterResources.CharacterModel).BindTo(gameObject);
        
        _modelVisualManager.SetupModelVisual(characterModel, characterResources);
        _modelImage.texture = _modelVisualManager.RenderTexture;
        SetImage(CHARACTER_IMAGE, characterImage);
        SetText(CHARACTER_NAME, characterResources.CharacterName);
        OnClickListen(INFO_BUTTON, infoButtonAction);
    }

    #endregion


    #region Private Methods

    private Tween RotateCarParent()
    {
        return _modelVisualParent.DORotate(Vector3.up * 360, 10f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetRelative();
    }

    #endregion
}