using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.GameCore.NPC;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Interfaces;
using _Scripts.Utilities;
using _Utilities;
using DG.Tweening;
using GameCore.Car;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using Interfaces;
using UI.Architectural;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class CarUpgradePopup : Popup, IPointerEnterHandler, IDragHandler, IEndDragHandler
{
    #region Serializable Fields
    
    [SerializeField] private CarType selectedCarType = CarType.Buggy;
    [SerializeField] private CarResources carResources;
    [SerializeField] private InfiniteScrollPool infiniteScrollPool;
    [SerializeField] private CarUpgradeSegment segmentPrefab;
    [SerializeField] private CarSegment carSegment;
    [SerializeField] private Transform segmentParent;
    [SerializeField] private Transform carSegmentTransform;
    [SerializeField] private GameObject tutorialHand;
    [SerializeField] private Button backButton;
    [SerializeField] private RawImage carModelImage;
    [SerializeField] private CarUpgradeInfo carUpgradeInfo;

    #endregion

    #region Fields

    private List<CarUpgradeSegment> _carUpgradeSegmentList = new();
    private List<CarSegment> _carSegmentList = new();
    private CarMetaUpgradeData _carMetaUpgradeData;
    private CarMeta _carMeta;
    private VibrationManager _vibrationManager;
    private ManagementNpcController _managementNpcController;
    private CarMetaUpgradeResources _carMetaUpgradeResources;
    private CarUpgradeSegment _carUpgradeSegment;
    private CarSegment _carSegmentInstance;
    private PlayerSkillController _playerSkillController;
    private AlertManager _alertManager;
    private ModelVisualManager _modelVisualManager;
    private const float AUTO_SCROLL_DURATION = 1.5f;
    private IGeneralOnClickManager _generalOnClickManager;

    private Transform _carVisualParent;
    private Tween _rotateTween;
    private bool _isDragging;
    private float _lastMousePositionX;
    private const float RotationSpeed = 0.5f;

    #endregion

    #region Properties
    public CarType SelectedCarType => selectedCarType;
    #endregion


    #region Unity Methods

    private void OnDestroy()
    {
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    #endregion
    

    #region Public Methods

    private void Awake()
    {
        
        _carMetaUpgradeData = GetCarMetaUpgradeData();
        _carMeta = _carMetaUpgradeData?.CarMetaList?.FirstOrDefault(x => x.CarType == CarType.Buggy) ?? new CarMeta
        {
            CarType = selectedCarType,
            UpgradeIndex = 0
        };
    }

    public override void OnOpenPopup()
    {
        _vibrationManager = Resolver.Resolve<VibrationManager>();
        _managementNpcController = Resolver.Resolve<ManagementNpcController>();
        selectedCarType = Resolver.Resolve<CarManager>().SelectedCarType;
        SetComponentValues();
        
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        CreateSegments();
        CreateCarSegments();
        Initialize();
        _rotateTween = RotateCarParent();
        _rotateTween.Play();
        _modelVisualManager.CurrentModel.GetComponent<CarModelParts>().SetCarPartsActive(
            GetCarModelPartIndex(carResources.GetCar(selectedCarType), _carMeta.UpgradeIndex));
        carUpgradeInfo.SetCarInfo(carResources.GetCar(selectedCarType), Resolver,false, BuyCar);
    }

    #endregion


    public void ApplySkill(int index)
    {
        if (_carMetaUpgradeResources is not { CarMetaUpgradeList: { Count: > 0 } })
        {
            return;
        }

        var carMetaUpgrade = _carMetaUpgradeResources.CarMetaUpgradeList[index];

        _playerSkillController.ApplyStatUpgrade(new List<UpgradeDetail>
        {
            carMetaUpgrade.UpgradeDetail
        });
    }

    public void UpdateSegments()
    {
        _carMetaUpgradeData = GetCarMetaUpgradeData();
        _carMeta = _carMetaUpgradeData?.CarMetaList?.Find(x => x.CarType == selectedCarType) ?? new CarMeta
        {
            CarType = selectedCarType,
            UpgradeIndex = 0
        };


        var lastSegment = _carUpgradeSegmentList.FirstOrDefault(x => x.Index == _carMeta.UpgradeIndex - 1);
        var newSegment = _carUpgradeSegmentList.FirstOrDefault(x => x.Index == _carMeta.UpgradeIndex);

        lastSegment?.UpdateSegment(lastSegment.Index, _carMeta.UpgradeIndex);
        newSegment?.UpdateSegment(newSegment.Index, _carMeta.UpgradeIndex);

        ScrollToLastSegment(infiniteScrollPool.ScrollRect.verticalNormalizedPosition);
        _modelVisualManager.CurrentModel.GetComponent<CarModelParts>().SetCarPartsActive(
            GetCarModelPartIndex(carResources.GetCar(CarType.Buggy), _carMeta.UpgradeIndex));
    }
    

    public void ShowTutorialHand()
    {
        tutorialHand.SetActive(true);
    }

    #region Private Methods

    private void SetComponentValues()
    {
        _generalOnClickManager = Resolver.Resolve<IGeneralOnClickManager>();
        _playerSkillController = Resolver.Resolve<PlayerSkillController>();
        _alertManager = Resolver.Resolve<AlertManager>();
        _carMetaUpgradeResources = _playerSkillController.CarMetaUpgradeResources;

        _generalOnClickManager.RegisterButton(backButton, ClosePopup);
    }


    private void BuyCar(CarType selectedCarType)
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        var car = carResources.GetCar(selectedCarType);
        
        if (car.CarBuyData.WaveCount > _managementNpcController.LoadManagementStateData().Index)
        {
            _alertManager.CallAlert("Not enough wave count");
            return;
        }

        if (car.CarBuyData.PurchaseOptions.Equals(PurchaseOptions.Coin))
        {
            if (TryPurchaseUpgrade(car.CarBuyData))
            {
                SaveLoadHelper.UpdateData<CarMetaUpgradeData>(data =>
                {
                    if (data.CarMetaList.Find(x => x.CarType.Equals(selectedCarType)) is CarMeta carMeta)
                    {
                        carMeta.UpgradeIndex = 0;
                    }
                    else
                    {
                        data.CarMetaList.Add(new CarMeta
                        {
                            CarType = selectedCarType,
                            UpgradeIndex = 0
                        });
                    }
                });
                _carMetaUpgradeData = GetCarMetaUpgradeData();
                carUpgradeInfo.UpdateCarInfo(false);

                foreach (var carSegment in _carSegmentList.Where(t => _carMetaUpgradeData.CarMetaList.Any(c => c.CarType == t.CarType)))
                {
                    carSegment.UpdateSegment(false);
                }

                return;
            }

            _alertManager.CallAlert("Not enough coin");
        }
        else
        {
            var mediationService = Resolver.Resolve<IMediationService>();
            mediationService.ShowRewardedAd(IMediationService.CarBuyPlacementId);
        }
    }
    
    
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement ironSourcePlacement, IronSourceAdInfo adInfo)
    {
        if (ironSourcePlacement.getPlacementName().Equals(IMediationService.CarBuyPlacementId))
        {
            LoggerNS.Log("RewardedVideoOnAdRewardedEvent With Placement " + ironSourcePlacement.getPlacementName() +
                         "And AdInfo " + adInfo);
            
            SaveLoadHelper.UpdateData<CarMetaUpgradeData>(data =>
            {
                if (data.CarMetaList.Find(x => x.CarType.Equals(selectedCarType)) is CarMeta carMeta)
                {
                    carMeta.UpgradeIndex = 0;
                }
                else
                {
                    data.CarMetaList.Add(new CarMeta
                    {
                        CarType = selectedCarType,
                        UpgradeIndex = 0
                    });
                }
            });
            _carMetaUpgradeData = GetCarMetaUpgradeData();
            carUpgradeInfo.UpdateCarInfo(false);

            foreach (var carSegment in _carSegmentList.Where(t => _carMetaUpgradeData.CarMetaList.Any(c => c.CarType == t.CarType)))
            {
                carSegment.UpdateSegment(false);
            }
        }
    }


    private void SelectCar(CarType carType)
    {
        _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.ButtonUI);
        selectedCarType = carType;
        _modelVisualManager.SetupModelVisual(carResources.GetCar(carType).CarModel);
        _rotateTween = RotateCarParent();
        _rotateTween.Play();
        var isLocked = true;
        
        foreach (var carMeta in _carMetaUpgradeData.CarMetaList.Where(carMeta => carMeta.CarType.Equals(carType)))
        {
            _carMeta = carMeta;
            isLocked = false;
            break;
        }
        
        carUpgradeInfo.SetCarInfo(carResources.GetCar(selectedCarType), Resolver, isLocked, BuyCar);
        
        for (var i = 0; i < _carUpgradeSegmentList.Count; i++)
        {
            _carUpgradeSegmentList[i].UpdateSegment(i, _carMeta.UpgradeIndex);
        }
    }
    

    private void CreateSegments()
    {
        for (var i = 0; i < 6; i++)
        {
            _carUpgradeSegment = Instantiate(segmentPrefab, segmentParent);
            _carUpgradeSegment.InitializeSegment(this, _carMetaUpgradeResources.CarMetaUpgradeList[i],
                GetUpgradeIcon(_carMetaUpgradeResources.CarMetaUpgradeList[i].UpgradeDetail.type), i,
                _carMeta.UpgradeIndex,
                Resolver, ShowAlertText);
            _carUpgradeSegmentList.Add(_carUpgradeSegment);
        }

        var segmentRectTransforms = _carUpgradeSegmentList.Select(x => x.GetComponent<RectTransform>()).ToList();

        infiniteScrollPool.SetupScroll(segmentRectTransforms,
            _carMetaUpgradeResources.CarMetaUpgradeList.Count, SetSegment);

        ScrollToLastSegment(0);
    }


    private void CreateCarSegments()
    {
        foreach (var car in carResources.CarList)
        {
            _carSegmentInstance = Instantiate(carSegment, carSegmentTransform);
            var isLocked = _carMetaUpgradeData.CarMetaList.All(x => x.CarType != car.CarType);
            _carSegmentInstance.InitializeSegment(car, isLocked, () => SelectCar(car.CarType));
            _carSegmentList.Add(_carSegmentInstance);
        }
    }
    

    private void SetSegment(Component segmentRectTransform, int index, bool isAnimated = false)
    {
        var segment = segmentRectTransform.GetComponent<CarUpgradeSegment>();
        segment.InitializeSegment(this, _carMetaUpgradeResources.CarMetaUpgradeList[index],
            GetUpgradeIcon(_carMetaUpgradeResources.CarMetaUpgradeList[index].UpgradeDetail.type), index,
            _carMeta.UpgradeIndex,
            Resolver, ShowAlertText);
    }

    private void ScrollToLastSegment(float startValue)
    {
        var offSet = infiniteScrollPool.IsReverseArrangement ? 0 : 1;
        var targetValue = GetTargetScrollValue(_carMeta.UpgradeIndex);
        if (targetValue.Equals(offSet))
        {
            return;
        }

        var startingValue = infiniteScrollPool.ScrollRect.verticalNormalizedPosition = startValue;
        DOTween.To(() => startingValue, x => infiniteScrollPool.ScrollRect.verticalNormalizedPosition = x, targetValue,
                AUTO_SCROLL_DURATION).SetUpdate(true)
            .OnComplete(() => infiniteScrollPool.OnScroll(Vector2.zero));
    }

    private void Initialize()
    {
        _modelVisualManager = Resolver.Resolve<ModelVisualManager>();
        _modelVisualManager.SetupModelVisual(carResources.GetCar(CarType.Buggy).CarModel);

        _carVisualParent = _modelVisualManager.ModelVisualContent;
        ClosePopupAction += _modelVisualManager.ReleaseCarRenderTexture;
        carModelImage.texture = _modelVisualManager.RenderTexture;
    }
    
    
    private bool TryPurchaseUpgrade(CarBuyData carBuyData)
    {
        return Resolver.Resolve<IInventoryManager>().PurchaseItem(
            new PurchaseDetails(carBuyData.Price, carBuyData.PurchaseOptions));
    }

    private void ShowAlertText() => _alertManager.CallAlert("Not Enough Coin");
    private Tween RotateCarParent()
    {
        return _carVisualParent.DORotate(Vector3.up * 360, 10f, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear)
            .SetUpdate(true);
    }

    private Sprite GetUpgradeIcon(StatUpgradeType statUpgradeType) => _carMetaUpgradeResources.GetUpgradeIconData(statUpgradeType).Icon;
    private CarMetaUpgradeData GetCarMetaUpgradeData() => SaveLoadHelper.TryLoadPersistentData<CarMetaUpgradeData>();
    private float GetTargetScrollValue(int index) => index / (float)_carMetaUpgradeResources.CarMetaUpgradeList.Count;
    private int GetCarModelPartIndex(Car car, int upgradeIndex)
    {
        if (upgradeIndex < 5)  return 0;
        return Mathf.Min(car.UpgradeCount * 5, upgradeIndex) / 5;
    }

    #endregion

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

        _carVisualParent.rotation = Quaternion.Euler(
            _carVisualParent.rotation.eulerAngles.x,
            _carVisualParent.rotation.eulerAngles.y - deltaX * RotationSpeed,
            _carVisualParent.rotation.eulerAngles.z
        );

        _modelVisualManager.ModelVisualContent = _carVisualParent;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _rotateTween = RotateCarParent();
        _rotateTween.Play();
        _isDragging = false;
    }
}