using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private MinimapObject minimapObject;
    [SerializeField] private GridLayoutGroup minimapImagesParent;
    [SerializeField] private GameObject minimapImagePrefab;

    [SerializeField] private RectTransform minimapCirclePlayerIcon, minimapSquarePlayerIcon;
    [SerializeField] private GameObject minimapSquare, minimapCircle;
    [SerializeField] private Transform minimapSquareParent, minimapCircleParent;
    [SerializeField] private TextMeshProUGUI minimapText;

    private float _minimapRefreshCounter;
    private readonly Dictionary<MinimapCursor, Dictionary<RectTransform, Transform>> _minimapCursors = new();
    private Transform _playerTransform, _playerQuaternionTransform;
    private MinimapType _minimapType = MinimapType.Circle;
    private IAnalyticsService _analyticsService;
    private ManagementNpcController _managementNpcController;
    private ObjectiveManager _objectiveManager;

    private Action<bool> _onMapDisabled;
    private Action _onMapOpened;


    private MinimapType MinimapType
    {
        get => _minimapType;
        set
        {
            OnMinimapTypeChanged(value);
            _minimapType = value;
        }
    }

    private bool _minimapInitialized;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
        _onMapDisabled = (minimapActive) =>
        {
            if(!minimapActive)
                MinimapType = MinimapType.Off;
        };
        _onMapOpened = () => { MinimapType = MinimapType.Circle; };
        _managementNpcController.OnStartManagement +=()=> _onMapDisabled.Invoke(false);
        _managementNpcController.OnCompleteManagement += _onMapOpened;
        _objectiveManager.OnObjectiveStart += _onMapDisabled;
        _objectiveManager.OnObjectiveComplete += _onMapOpened;
        _objectiveManager.OnObjectiveFailed += _onMapOpened;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
        _managementNpcController.OnCompleteManagement -= _onMapOpened;
        _objectiveManager.OnObjectiveStart -= _onMapDisabled;
        _objectiveManager.OnObjectiveComplete -= _onMapOpened;
        _objectiveManager.OnObjectiveFailed -= _onMapOpened;
    }

    [Inject]
    private void Initialize(IAnalyticsService analyticsService, ManagementNpcController managementNpcController,
        ObjectiveManager objectiveManager)
    {
        _analyticsService = analyticsService;
        _managementNpcController = managementNpcController;
        _objectiveManager = objectiveManager;
    }

    private void OnSceneChanged(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        _minimapInitialized = false;
        if (loadedScene.isLoaded)
            LoadMiniMapObject(loadedScene.name);
    }

    private async void LoadMiniMapObject(string sceneName)
    {
        try
        {
            var handle = Addressables.LoadAssetAsync<MinimapObject>(sceneName + "_Minimap");
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                minimapObject = handle.Result;
                InitMinimap();
            }
            else
            {
                MinimapType = MinimapType.Off;
            }
        }
        catch (Exception e)
        {
            MinimapType = MinimapType.Off;
        }
    }

    private async void InitMinimap()
    {
        try
        {
            await ResetMinimap();
            List<Transform> mapImages = new List<Transform>();
            foreach (var minimapImage in minimapObject.minimapParts)
            {
                var newImage = await Addressables.LoadAssetAsync<Sprite>(minimapImage);
                var imageObject = Instantiate(minimapImagePrefab, minimapImagesParent.transform);
                imageObject.name = newImage.name;
                imageObject.GetComponent<Image>().sprite = newImage;
                mapImages.Add(imageObject.transform);
            }

            MinimapType = MinimapType.Circle;
            minimapText.text = minimapObject.minimapName;
            await Task.Yield();
            LayoutRebuilder.ForceRebuildLayoutImmediate(minimapImagesParent.GetComponent<RectTransform>());
            minimapImagesParent.enabled = false;
            foreach (var mapTransform in mapImages)
                mapTransform.SetAsFirstSibling();
            _minimapInitialized = true;
        }
        catch (Exception e)
        {
            LoggerNS.LogError(e.Message);
        }
    }

    private async Task ResetMinimap()
    {
        _minimapInitialized = false;
        _minimapCursors.Clear();
        foreach (Transform componentsInChild in minimapImagesParent.transform)
            Destroy(componentsInChild.gameObject);
        minimapImagesParent.enabled = true;
        await Task.Yield();
    }

    private void Update()
    {
        if (!_minimapInitialized)
            return;
        if (MinimapType == MinimapType.Off)
            return;
        if (_playerTransform == null)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _playerQuaternionTransform = _playerTransform.GetChild(0);
            return;
        }

        var playerPos = _playerTransform.position;
        switch (MinimapType)
        {
            case MinimapType.Circle:
                minimapCirclePlayerIcon.eulerAngles = new Vector3(0, 0, -_playerQuaternionTransform.eulerAngles.y);
                break;
            case MinimapType.Square:
                minimapSquarePlayerIcon.eulerAngles = new Vector3(0, 0, -_playerQuaternionTransform.eulerAngles.y);
                break;
        }

        var newPlayerPos = new Vector2(playerPos.x, playerPos.z)
        {
            x = Helper.Remap(playerPos.x, minimapObject.widthMin, minimapObject.widthMax, -1500, 1500, true),
            y = Helper.Remap(playerPos.z, minimapObject.heightMin, minimapObject.heightMax, -1500, 1500, true)
        };
        minimapImagesParent.GetComponent<RectTransform>().anchoredPosition = newPlayerPos;

        if (minimapObject.minimapCursors.Length < 1)
            return;
        MinimapPositionsRefresh();
        if (Time.frameCount % 120 == 0)
            MinimapRefresh();
    }

    private async void MinimapRefresh()
    {
        foreach (var minimapCursor in minimapObject.minimapCursors)
        {
            if (_minimapCursors.TryGetValue(minimapCursor, out var activeCursorObjects))
            {
                for (int i = 0; i < activeCursorObjects.Count; i++)
                {
                    var cursorObject = activeCursorObjects.ElementAt(i);
                    if (cursorObject.Value == null || !cursorObject.Value.gameObject.activeSelf || !cursorObject.Value.transform.CompareTag(minimapCursor.objectTag))
                    {
                        cursorObject.Key.gameObject.SetActive(false);
                        _minimapCursors[minimapCursor].Remove(cursorObject.Key);
                    }
                }

                if (minimapCursor.cursorCount == activeCursorObjects.Count) continue;
                {
                    var cursorObject = GameObject.FindGameObjectsWithTag(minimapCursor.objectTag);
                    foreach (var cursorTransform in cursorObject)
                    {
                        if (activeCursorObjects.ContainsValue(cursorTransform.transform))
                            continue;
                        var cursorImage = await ObjectManager.GetObject(minimapCursor.cursorImage).BindTo(gameObject, minimapCursor.cursorImage);
                        cursorImage.SetActive(true);
                        var cursorRectTransform = cursorImage.GetComponent<RectTransform>();
                        cursorRectTransform.SetParent(minimapImagesParent.transform);
                        cursorRectTransform.transform.SetAsLastSibling();
                        cursorRectTransform.anchoredPosition = GetMinimapPosition(cursorTransform.transform);
                        cursorRectTransform.DOPunchScale(Vector3.one * 0.5f, 1f, 1, 0.5f).SetEase(Ease.OutBack).SetLoops(minimapCursor.punchAnimationCount);
                        _minimapCursors[minimapCursor].Add(cursorRectTransform, cursorTransform.transform);
                    }
                }
            }
            else
            {
                var cursorObject = GameObject.FindGameObjectsWithTag(minimapCursor.objectTag);
                foreach (var cursorTransform in cursorObject)
                {
                    var cursorImage = await ObjectManager.GetObject(minimapCursor.cursorImage).BindTo(gameObject, minimapCursor.cursorImage);
                    cursorImage.SetActive(true);
                    var cursorRectTransform = cursorImage.GetComponent<RectTransform>();
                    cursorRectTransform.SetParent(minimapImagesParent.transform);
                    cursorRectTransform.transform.SetAsLastSibling();
                    cursorRectTransform.anchoredPosition = GetMinimapPosition(cursorTransform.transform);
                    cursorRectTransform.DOPunchScale(Vector3.one * 0.5f, 1f, 1, 0.5f).SetLoops(minimapCursor.punchAnimationCount);
                    if (_minimapCursors.TryGetValue(minimapCursor, out var cursor))
                    {
                        if (!cursor.ContainsKey(cursorRectTransform))
                            cursor.Add(cursorRectTransform, cursorTransform.transform);
                    }
                    else
                    {
                        _minimapCursors.Add(minimapCursor,
                            new Dictionary<RectTransform, Transform>
                                { { cursorRectTransform, cursorTransform.transform } });
                    }
                }
            }
        }
    }

    private void MinimapPositionsRefresh()
    {
        foreach (var minimapCursor in minimapObject.minimapCursors)
        {
            if (_minimapCursors.TryGetValue(minimapCursor, out var activeCursorObjects))
            {
                foreach (var cursorObject in activeCursorObjects)
                {
                    if (cursorObject.Value && cursorObject.Key)
                    {
                        if (cursorObject.Value.transform.CompareTag(minimapCursor.objectTag))
                        {
                            if (minimapCursor.alwaysOnDisplay)
                            {
                                if (MinimapType == MinimapType.Circle)
                                {
                                    cursorObject.Key.anchoredPosition =
                                        GetMinimapPositionAroundTarget(cursorObject.Value.transform,
                                            _playerQuaternionTransform, out var isFar);
                                    if (!minimapCursor.multipleCursor) continue;
                                    if (cursorObject.Key.GetChild(1).gameObject.activeSelf != !isFar)
                                    {
                                        var targetScale = cursorObject.Key.GetChild(1).transform.localScale;
                                        cursorObject.Key.GetChild(1).transform.localScale = Vector3.zero;
                                        cursorObject.Key.GetChild(1).transform.DOScale(targetScale, 0.5f)
                                            .SetEase(Ease.OutBack);
                                        cursorObject.Key.GetChild(1).gameObject.SetActive(!isFar);
                                    }

                                    if (cursorObject.Key.GetChild(0).gameObject.activeSelf != isFar)
                                    {
                                        var targetScale = cursorObject.Key.GetChild(0).transform.localScale;
                                        cursorObject.Key.GetChild(0).transform.localScale = Vector3.zero;
                                        cursorObject.Key.GetChild(0).transform.DOScale(targetScale, 0.5f)
                                            .SetEase(Ease.OutBack);
                                        cursorObject.Key.GetChild(0).gameObject.SetActive(isFar);
                                    }

                                    var directionToCenter = minimapCirclePlayerIcon.position -
                                                            cursorObject.Key.GetChild(0).position;
                                    var outwardDirection = -directionToCenter;
                                    var angle = Mathf.Atan2(outwardDirection.y, outwardDirection.x) * Mathf.Rad2Deg;
                                    cursorObject.Key.GetChild(0).rotation = Quaternion.Euler(0, 0, angle);
                                }
                                else
                                {
                                    cursorObject.Key.anchoredPosition =
                                        GetMinimapPosition(cursorObject.Value.transform);
                                }
                            }
                            else
                            {
                                cursorObject.Key.anchoredPosition = GetMinimapPosition(cursorObject.Value.transform);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnMinimapTypeChanged(MinimapType value)
    {
        switch (value)
        {
            case MinimapType.Off:
                minimapCircle.SetActive(false);
                minimapSquare.SetActive(false);
                break;
            case MinimapType.Circle:
                minimapCircle.SetActive(true);
                minimapSquare.SetActive(false);
                minimapImagesParent.transform.SetParent(minimapCircleParent);
                MinimapPositionsRefresh();
                break;
            case MinimapType.Square:
                minimapCircle.SetActive(false);
                minimapSquare.SetActive(true);
                minimapImagesParent.transform.SetParent(minimapSquareParent);
                minimapImagesParent.transform.SetAsFirstSibling();
                _analyticsService?.LogEvent(new EventParameters<string>
                {
                    EventName = "click_map",
                });
                MinimapPositionsRefresh();
                break;
        }
    }

    public void OpenSquareMinimap() => MinimapType = MinimapType.Square;

    public void OpenCircleMinimap() => MinimapType = MinimapType.Circle;

    private Vector2 GetMinimapPosition(Transform objectTransform)
    {
        var newPlayerPos = new Vector2(objectTransform.position.x, objectTransform.position.z)
        {
            x = Helper.Remap(objectTransform.position.x, minimapObject.widthMin, minimapObject.widthMax, -1500, 1500,
                false),
            y = Helper.Remap(objectTransform.position.z, minimapObject.heightMin, minimapObject.heightMax, -1500, 1500,
                false)
        };
        return newPlayerPos;
    }

    private Vector2 GetMinimapPositionAroundTarget(Transform objectTransform, Transform targetTransform, out bool isFar)
    {
        var targetMinimapPosition = new Vector2(
            Helper.Remap(targetTransform.position.x, minimapObject.widthMin, minimapObject.widthMax, -1500, 1500,
                false),
            Helper.Remap(targetTransform.position.z, minimapObject.heightMin, minimapObject.heightMax, -1500, 1500,
                false)
        );
        var directionToObject = new Vector2(
            objectTransform.position.x - targetTransform.position.x,
            objectTransform.position.z - targetTransform.position.z
        ).normalized;
        var objectMinimapPosition = targetMinimapPosition + directionToObject * 205;
        var objectDistance = Vector3.Distance(objectTransform.position, targetTransform.position);
        isFar = objectDistance > 40;
        return objectDistance < 40 ? GetMinimapPosition(objectTransform) : objectMinimapPosition;
    }
}