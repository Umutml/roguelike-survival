using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameCore.Spawner;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    private Transform _viewerObject;
    private GridObject _gridObject;
    private float _mainCamViewAngle = 75;
    private readonly Dictionary<Grid, GameObject> _createdGrids = new();
    private Vector3 _viewerPosition;
    private bool _gridSystemInitialized;
    private bool _gridSystemIsReady;
    public bool isTestMode;
    private AsyncOperationHandle<GridObject> _currentGridObjectHandle;
    private AsyncOperationHandle<IList<IResourceLocation>> _currentLocationHandle;

    private Vector3 ViewerPosition
    {
        set
        {
            if (Vector3.Distance(_viewerPosition, value) > 1)
            {
                UpdateGrids(value);
                _viewerPosition = value;
            }
        }
    }

    private Vector3 _lastCamPosition;

    public UniTaskCompletionSource<bool> GridSystemInitialized { get; } = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
        ReleaseCurrentGridObject();
    }

    private void Update()
    {
        if (!_gridSystemInitialized) return;
        if (!_viewerObject) return;
        if (!_gridObject) return;
        ViewerPosition = _viewerObject.transform.position;
        var speed = isTestMode ? 30000 : (_viewerObject.transform.position - _lastCamPosition).magnitude / Time.deltaTime;
        _mainCamViewAngle = Mathf.Clamp(speed * 1.75f, _gridObject.minGridSize, _gridObject.maxGridSize);
        _lastCamPosition = _viewerObject.transform.position;
    }

    private void ReleaseCurrentGridObject()
    {
        if (_currentLocationHandle.IsValid())
        {
            Addressables.Release(_currentLocationHandle);
        }
        if (_currentGridObjectHandle.IsValid())
        {
            Addressables.Release(_currentGridObjectHandle);
        }
    }

    private void OnSceneChanged(Scene loadedScene, LoadSceneMode loadedSceneMode)
    {
        _gridSystemInitialized = false;
        ReleaseCurrentGridObject();
        if (loadedScene.isLoaded)
            LoadGridObject(loadedScene.name + "_Grid");
    }

    private void LoadGridObject(string loadedSceneName)
    {
        try
        {
            _currentLocationHandle = Addressables.LoadResourceLocationsAsync(loadedSceneName);
            _currentLocationHandle.Completed += async (locationHandle) =>
            {
                if (locationHandle.Status == AsyncOperationStatus.Succeeded && locationHandle.Result.Count > 0)
                {
                    _currentGridObjectHandle = Addressables.LoadAssetAsync<GridObject>(loadedSceneName);
                    await _currentGridObjectHandle.Task;
                    if (_currentGridObjectHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        _gridObject = _currentGridObjectHandle.Result;
                        InitGridSystem();
                    }
                    else
                    {
                        _gridSystemInitialized = false;
                    }
                }
                else
                {
                    _gridSystemInitialized = false;
                    Debug.LogWarning($"{loadedSceneName} Asset key has not finded!.");
                }
            };
        }
        catch (Exception e)
        {
            _gridSystemInitialized = false;
            Debug.LogError($"Error loading grid object: {e}");
        }
    }

    private void InitGridSystem()
    {
        try
        {
            _viewerObject = MobManager.TargetPlayer.transform;
            _viewerPosition = Vector3.one * 100;
            var calculatedPosition = new Vector3(_viewerObject.transform.position.x, 0, _viewerObject.transform.position.z);
            if (_viewerObject)
                ViewerPosition = calculatedPosition;
            _gridSystemInitialized = true;
            UpdateGrids(calculatedPosition);
        }
        catch (Exception e)
        {
            _gridSystemInitialized = false;
            Console.WriteLine(e);
            throw;
        }
    }

    private void UpdateGrids(Vector3 gridPosition)
    {
        bool isAnyChange = false;
        foreach (var grid in _gridObject.gridParts)
        {
            if (grid.IsInView(gridPosition, _mainCamViewAngle))
            {
                if (!_createdGrids.TryAdd(grid, null)) continue;
                var handle = grid.gridAsset.InstantiateAsync(grid.gridAssetPosition, Quaternion.identity);
                handle.Completed += (operationHandle) =>
                {
                    if (operationHandle.Status != AsyncOperationStatus.Succeeded) 
                    {
                        Addressables.Release(handle);
                        return;
                    }
                    _createdGrids[grid] = operationHandle.Result;
                    if(!_gridSystemIsReady)
                        SetGridSystemReady();
                };
            }
            else
            {
                if (!_createdGrids.TryGetValue(grid, out var createdGrid)) continue;
                if (createdGrid != null)
                {
                    grid.gridAsset.ReleaseInstance(createdGrid);
                    _createdGrids[grid] = null;
                }
                _createdGrids.Remove(grid);
                isAnyChange = true;
            }
        }
        // if(isAnyChange) 
        //     UnloadUnusedAssetsAsync().Forget();
    }

    private async void SetGridSystemReady()
    {
        _gridSystemIsReady = true;
        await Task.Delay(2500);
        GridSystemInitialized.TrySetResult(true);
    }

    private async UniTask UnloadUnusedAssetsAsync()
    {
        await Resources.UnloadUnusedAssets();
    }
}