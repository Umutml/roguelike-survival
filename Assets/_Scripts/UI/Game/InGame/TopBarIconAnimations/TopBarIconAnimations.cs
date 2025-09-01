using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using GameCore.Player;
using GameCore.Scriptables;
using GameCore.Spawner;
using Managers;
using UI.Game.InGame.DropIncrement;
using VContainer;
using Random = UnityEngine.Random;


public class TopBarIconAnimations : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private SpriteDatabase spriteDatabase;
    [SerializeField] private List<IconAnimationData> iconAnimationDataList = new();
    [SerializeField] private List<TopBarIcon> iconImageList = new();
    [SerializeField] private DropIncrementUI dropIncrementUI;
    [SerializeField] private Ease ease;
    [SerializeField] private int iconCount;
    [SerializeField] private float duration;

    #endregion


    #region Fields

    private const float TopBarPositionByCar = 100;
    private RectTransform _rectTransform;
    private LootDropManager _lootDropManager;
    private ShopManager _shopManager;
    private PlayerController _playerController;
    private Queue<TopBarIcon> _iconsQueue = new();
    private TopBarIcon _currentIcon;
    private float _delay = 0.1f;
    private AudioManager _audioManager;
    private EnergyManager _energyManager;

    #endregion


    #region Properties

    public Queue<TopBarIcon> IconsQueue
    {
        get => _iconsQueue;
        set => _iconsQueue = value;
    }

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (TryGetComponent(out RectTransform rectTransform))
        {
            _rectTransform = rectTransform;
        }
    }

    private void Start()
    {
        foreach (var icon in iconImageList)
        {
            _iconsQueue.Enqueue(icon);
        }
    }

    private void OnEnable()
    {
        if (_lootDropManager != null)
        {
            _lootDropManager.OnTopBarAnimationStart += PlayAnimation;
        }
        
        if (_shopManager != null)
        {
            _shopManager.OnTopBarAnimationStart += PlayAnimation;
        }

        if (_playerController)
        {
            _playerController.PlayerInCarStateChanged += SetTopBarIconPosition;
        }
        
        if (_energyManager != null)
        {
            _energyManager.EnergyGiven += PlayEnergyAnimation;
        }
    }
    
    private void PlayEnergyAnimation(int count)
    {
        PlayAnimation(DropPodType.Energy, count);
    }

    private void OnDestroy()
    {
        if (_lootDropManager != null)
        {
            _lootDropManager.OnTopBarAnimationStart -= PlayAnimation;
        }

        if (_shopManager != null)
        {
            _shopManager.OnTopBarAnimationStart -= PlayAnimation;
        }

        if (_playerController)
        {
            _playerController.PlayerInCarStateChanged -= SetTopBarIconPosition;
        }
        
        if (_energyManager != null)
        {
            _energyManager.EnergyGiven -= PlayEnergyAnimation;
        }
    }

    #endregion


    #region Private Methods

    [Inject]
    private void Initialize(LootDropManager lootDropManager, PlayerController playerController,
        AudioManager audioManager, EnergyManager energyManager, ShopManager shopManager)
    {
        _lootDropManager = lootDropManager;
        _audioManager = audioManager;
        _playerController = playerController;
        _energyManager = energyManager;
        _shopManager = shopManager;
    }

    private void PlayAnimation(DropPodType targetDrop, int count)
    {
        ShowDropIncrementUI(new Tuple<int, DropPodType>(count, targetDrop));
        StartCoroutine(PlayAnimationWithDelay(targetDrop, Mathf.Min(5, count)));
    }
    
    private void PlayAnimation(DropPodType targetDrop, int count, Vector3 targetPosition)
    {
        ShowDropIncrementUI(new Tuple<int, DropPodType>(count, targetDrop));
        StartCoroutine(PlayAnimationWithDelay(targetDrop, Mathf.Min(5, count), targetPosition));
    }

    private async void ShowDropIncrementUI(Tuple<int, DropPodType> dropData)
    {
        var sprite = await spriteDatabase.GetSpriteByValueAndType(dropData);
        dropIncrementUI.gameObject.SetActive(true);
        dropIncrementUI.Initialize(sprite, dropData.Item1);
    }

    private IEnumerator PlayAnimationWithDelay(DropPodType targetDrop, int count)
    {
        for (var i = 0; i < count; i++)
        {
            _currentIcon = _iconsQueue.Dequeue();
            _currentIcon.gameObject.SetActive(true);
            _currentIcon.PlayIcon(this, GetIconData(targetDrop).IconSprite, GetIconData(targetDrop).IconTarget.position,
                duration);
            _audioManager.PlayOneShot("CoinIncrease",.3f);
            _currentIcon.PlayIcon(this, GetIconData(targetDrop).IconSprite, GetIconData(targetDrop).IconTarget.position,
                duration);

            yield return new WaitForSecondsRealtime(_delay);
        }
    }
    
    private IEnumerator PlayAnimationWithDelay(DropPodType targetDrop, int count, Vector3 targetPosition)
    {
        for (var i = 0; i < count; i++)
        {
            _currentIcon = _iconsQueue.Dequeue();
            _currentIcon.gameObject.SetActive(true);
            _currentIcon.PlayIcon(this, GetIconData(targetDrop).IconSprite, targetPosition,
                duration);
            _audioManager.PlayOneShot("CoinIncrease",.3f);
            _currentIcon.PlayIcon(this, GetIconData(targetDrop).IconSprite, targetPosition,
                duration);

            yield return new WaitForSecondsRealtime(_delay);
        }
    }

    private void SetTopBarIconPosition(bool inCar)
    {
        _rectTransform.anchoredPosition = inCar
            ? new Vector2(_rectTransform.anchoredPosition.x, TopBarPositionByCar)
            : new Vector2(_rectTransform.anchoredPosition.x, 0);
    }

    private IconAnimationData GetIconData(DropPodType dropPodType) =>
        iconAnimationDataList.Find(data => data.DropPodType.Equals(dropPodType));

    #endregion
}


[Serializable]
public struct IconAnimationData
{
    [SerializeField] private DropPodType dropPodType;
    [SerializeField] private RectTransform iconTarget;
    [SerializeField] private Sprite iconSprite;


    public DropPodType DropPodType => dropPodType;
    public RectTransform IconTarget => iconTarget;
    public Sprite IconSprite => iconSprite;
}