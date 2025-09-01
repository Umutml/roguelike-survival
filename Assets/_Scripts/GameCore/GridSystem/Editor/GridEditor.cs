#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(GridCamera))]
public class GridEditor : Editor
{
    private float _chunkWorldPointSize;
    private GridCamera _gridCamera;
    private int _dialogResult;
    private Vector3[] ChunkPositions(Vector3 targetPosition)
    {
        List<Vector3> chunkPositions = new List<Vector3>();
        chunkPositions.Add(targetPosition+new Vector3(-_chunkWorldPointSize,0,_chunkWorldPointSize));
        chunkPositions.Add(targetPosition+new Vector3(0,0,_chunkWorldPointSize));
        chunkPositions.Add(targetPosition+new Vector3(_chunkWorldPointSize,0,_chunkWorldPointSize));
        chunkPositions.Add(targetPosition+new Vector3(-_chunkWorldPointSize,0,0));
        chunkPositions.Add(targetPosition+new Vector3(0,0,0));
        chunkPositions.Add(targetPosition+new Vector3(_chunkWorldPointSize,0,0));
        chunkPositions.Add(targetPosition+new Vector3(-_chunkWorldPointSize,0,-_chunkWorldPointSize));
        chunkPositions.Add(targetPosition+new Vector3(0,0,-_chunkWorldPointSize));
        chunkPositions.Add(targetPosition+new Vector3(_chunkWorldPointSize,0,-_chunkWorldPointSize));
        return chunkPositions.ToArray();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        _gridCamera = (GridCamera)target;
        if (GUILayout.Button("Create Grid"))
        {
            _dialogResult = EditorUtility.DisplayDialogComplex(
                "BACKUP WARNING", 
                "What kind of backup do you want to take before starting this process?", 
                "Scene Backup", 
                "Prefab & Scene Backup", 
                "Without Backup"
            );
            CreateGrid();
        }
        if (GUILayout.Button("Show Grid"))
        {
            ShowGrid();
        }
    }

    private void ShowGrid()
    {
        var gridOrthographicSize = _gridCamera.OrthoCamera.orthographicSize;
        var gridAspectRatio = _gridCamera.OrthoCamera.aspect;
        var chunkSize = _gridCamera.chunkSize;
        _chunkWorldPointSize = gridOrthographicSize * 2 / chunkSize;
        var heightMax = _gridCamera.OrthoCamera.transform.position.z + gridOrthographicSize;
        var widthMin = _gridCamera.OrthoCamera.transform.position.x - gridOrthographicSize * gridAspectRatio;
        var targetPosition = new Vector3(widthMin,0, heightMax);
        _gridCamera.SetGizmoSettings(ChunkPositions(targetPosition),_chunkWorldPointSize);
    }
    [Obsolete("Obsolete")]
    private void CreateGrid()
    {
        var gridObject = CreateInstance<GridObject>();
        gridObject.gridName = SceneManager.GetActiveScene().name;
        var gridOrthographicSize = _gridCamera.OrthoCamera.orthographicSize;
        var gridAspectRatio = _gridCamera.OrthoCamera.aspect;
        var chunkSize = _gridCamera.chunkSize;
        var heightMin = _gridCamera.OrthoCamera.transform.position.z - gridOrthographicSize;
        var heightMax = _gridCamera.OrthoCamera.transform.position.z + gridOrthographicSize;
        var widthMin = _gridCamera.OrthoCamera.transform.position.x - gridOrthographicSize * gridAspectRatio;
        var widthMax = _gridCamera.OrthoCamera.transform.position.x + gridOrthographicSize * gridAspectRatio;
        var targetPosition = new Vector3(0,0, 0);
        _gridCamera.SetGizmoSettings(ChunkPositions(targetPosition),_chunkWorldPointSize);
        _chunkWorldPointSize = gridOrthographicSize * 2 / chunkSize;
        gridObject.minGridSize = (int)(_chunkWorldPointSize * 1.5f);
        gridObject.maxGridSize = gridObject.minGridSize * 5;
        var chunkCount = (int)(gridOrthographicSize * 2 / _chunkWorldPointSize);
        var allObjects = FindObjectsOfType<GameObject>().ToList();
        var removedObjects = allObjects.Where(currentObject => !IsChunkObject(currentObject)).ToList();
        foreach (var removedObject in removedObjects)
            allObjects.Remove(removedObject);
        switch (_dialogResult)
        {
            case 0:
                CreateSceneBackup();
                break;
            case 1:
                CreateSceneBackup();
                foreach (var currentObject in allObjects)
                    CreatePrefabBackup(currentObject.transform.root.gameObject);
                break;
        }
        foreach (var currentObject in allObjects)
            UnpackPrefab(currentObject.transform.root.gameObject);
        foreach (var currentObject in allObjects)
            currentObject.transform.SetParent(null,true);
        var staticName = SceneManager.GetActiveScene().name + "_Statics";
        var staticObject = GameObject.Find(staticName);
        if (staticObject)
            DestroyImmediate(staticObject);
        var otherObjects = new GameObject($"{SceneManager.GetActiveScene().name}_Statics") {transform = {position = Vector3.zero}};
        for (var x = 0; x < chunkCount; x++)
        {
            for (var y = 0; y < chunkCount; y++)
            {
                targetPosition = new Vector3(widthMin + x * _chunkWorldPointSize,0, heightMin + y * _chunkWorldPointSize);
                _gridCamera.SetGizmoSettings(ChunkPositions(targetPosition),_chunkWorldPointSize);
                var chunkObjects = new List<GameObject>();
                foreach (var targetObject in allObjects)
                {
                    if(targetObject)
                        if (IsInChunk(targetPosition, targetObject))
                        {
                            ConvertChunkObject(targetObject, out var newColliderObject);
                            if(newColliderObject)
                                newColliderObject.transform.SetParent(otherObjects.transform);
                            chunkObjects.Add(targetObject);
                        }
                }
                if (chunkObjects.Count <= 0) continue;
                var chunk = new GameObject($"{gridObject.gridName}_Chunk_{x}_{y}") {transform = {position = GetChunkPosition(chunkObjects.ToArray())}};
                foreach (var chunkObject in chunkObjects)
                {
                    chunkObject.transform.SetParent(chunk.transform);
                }
                var newPrefabAsset = CreateChunkPrefab(chunk);
                gridObject.AddGrid(chunk.transform.position, newPrefabAsset);
                foreach (var chunkObject in chunkObjects)
                {
                    allObjects.Remove(chunkObject);
                    DestroyImmediate(chunkObject);
                    DestroyImmediate(chunk);
                }
            }
        }
        CreateGridObject(gridObject);
        foreach (var currentObject in allObjects)
            if(currentObject)
                if(currentObject.isStatic)
                    currentObject.transform.SetParent(otherObjects.transform);
        var exitDialogResult = EditorUtility.DisplayDialogComplex(
            "CAMERA WARNING", 
            "Grid is done, what do you want to do now?", 
            "Delete Camera Object", 
            "Create Minimap", 
            "Close"
        );
        switch (exitDialogResult)
        {
            case 0:
                DestroyImmediate(_gridCamera.gameObject);
                break;
        }
    }

    private void ConvertChunkObject(GameObject targetObject, out GameObject colliderObject)
    {
        var targetObjectColliders = targetObject.GetComponentsInChildren<Collider>();
        colliderObject = new GameObject(
            $"{targetObject.name}_Collider") {
            transform = {
            position = targetObject.transform.position,
            rotation = targetObject.transform.rotation,
            localScale = targetObject.transform.localScale,
            tag = targetObject.tag
            },
            layer = targetObject.layer
        };
        foreach (var targetObjectCollider in targetObjectColliders)
        {
            var newCollider = colliderObject.AddComponent(targetObjectCollider.GetType()) as Collider;
            if (newCollider != null)
            {
                CopyColliderProperties(targetObjectCollider, newCollider);
            }
            DestroyImmediate(targetObjectCollider);
        }
        colliderObject.isStatic = true;
    }
    private void CopyColliderProperties(Collider source, Collider target)
    {
        if (source is BoxCollider srcBox && target is BoxCollider tgtBox)
        {
            tgtBox.center = srcBox.center;
            tgtBox.size = srcBox.size;
        }
        else if (source is SphereCollider srcSphere && target is SphereCollider tgtSphere)
        {
            tgtSphere.center = srcSphere.center;
            tgtSphere.radius = srcSphere.radius;
        }
        else if (source is CapsuleCollider srcCapsule && target is CapsuleCollider tgtCapsule)
        {
            tgtCapsule.center = srcCapsule.center;
            tgtCapsule.radius = srcCapsule.radius;
            tgtCapsule.height = srcCapsule.height;
            tgtCapsule.direction = srcCapsule.direction;
        }
        else if (source is MeshCollider srcMesh && target is MeshCollider tgtMesh)
        {
            tgtMesh.sharedMesh = srcMesh.sharedMesh;
            tgtMesh.convex = srcMesh.convex;
        }
        target.isTrigger = source.isTrigger;
        target.contactOffset = source.contactOffset;
    }

    private bool IsInChunk(Vector3 targetPosition, GameObject targetObject)
    {
        if (!(Vector3.Distance(targetObject.transform.position, targetPosition) < _chunkWorldPointSize)) return false;
        return (GameObjectUtility.GetStaticEditorFlags(targetObject) & _gridCamera.staticLayers) == _gridCamera.staticLayers;
    }
    private bool IsChunkObject(GameObject targetObject)
    {
        return (GameObjectUtility.GetStaticEditorFlags(targetObject) & _gridCamera.staticLayers) == _gridCamera.staticLayers;
    }
    private Vector3 GetChunkPosition(GameObject[] chunkObjects)
    {
        Vector3 chunkPosition = Vector3.zero;
        foreach (var chunkObject in chunkObjects)
            chunkPosition += chunkObject.transform.position;
        return chunkPosition / chunkObjects.Length;
    }
    private void CreatePrefabBackup(GameObject chunkRoot)
    {
        if (!PrefabUtility.IsPartOfAnyPrefab(chunkRoot)) return;
        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(chunkRoot);
        if (string.IsNullOrEmpty(prefabPath)) return;
        const string folderPath = "Assets/_Prefabs/Grids/Backups";
        CreateFolder(folderPath);
        var backupPath = $"{folderPath}/{chunkRoot.name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(chunkRoot, backupPath);
        Debug.Log($"{chunkRoot.name} prefab has saved : {backupPath}");
    }
    private void UnpackPrefab(GameObject chunkRoot)
    {
        if (!PrefabUtility.IsPartOfAnyPrefab(chunkRoot)) return;
        PrefabUtility.UnpackPrefabInstance(chunkRoot, PrefabUnpackMode.Completely, InteractionMode.UserAction);
    }   
    private void CreateSceneBackup()
    {
        const string backupFolder = "Assets/Scenes/CoreScenes"; 
        var currentScenePath = SceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(currentScenePath))
        {
            Debug.LogWarning("Yedekleme başarısız: Sahne kaydedilmemiş.");
            return;
        }
        var sceneName = Path.GetFileNameWithoutExtension(currentScenePath);
        var backupPath = Path.Combine(backupFolder, sceneName + "_Backup.unity");
        if (!Directory.Exists(backupFolder))
            Directory.CreateDirectory(backupFolder);
        AssetDatabase.CopyAsset(currentScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"Scene has backup! : {backupPath}");
    }
    private AssetReferenceGameObject CreateChunkPrefab(GameObject chunkObject)
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var addressableGroupName = sceneName + "_Grids";
        const string labelName = "Grid";
        var folderPath = $"Assets/_Prefabs/Grids/{sceneName}";
        CreateFolder(folderPath);
        var filePath = $"{folderPath}/{chunkObject.name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(chunkObject, filePath);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(addressableGroupName);
        if (group == null)
        {
            group = settings.CreateGroup(addressableGroupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            Debug.Log($"Created new Addressable group: {addressableGroupName}");
        }
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(filePath), group);
        entry.address = chunkObject.name;
        if (!settings.GetLabels().Contains(labelName))
        {
            settings.AddLabel(labelName);
            Debug.Log($"Created new label: {labelName}");
        }
        if (!entry.labels.Contains(labelName))
        {
            entry.SetLabel(labelName, true);
            Debug.Log($"Added label '{labelName}' to prefab '{prefab.name}'.");
        }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"Prefab '{prefab.name}' saved and added to Addressables group '{addressableGroupName}' with address '{chunkObject.name}' and label '{labelName}'.");
        return new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(filePath));
    }
    private void CreateGridObject(GridObject gridObject)
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var scriptableObjectName = $"{sceneName}_Grid";
        var addressableGroupName = sceneName + "_Grids";
        const string labelName = "GridObject";
        const string folderPath = "Assets/_Scripts/Scriptable_Objects/Grids";
        CreateFolder(folderPath);
        var gridObjectPath = $"{folderPath}/{scriptableObjectName}.asset";
        AssetDatabase.CreateAsset(gridObject, gridObjectPath);
        Debug.Log($"Grid object created: {gridObjectPath}");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(addressableGroupName);
        if (group == null)
        {
            group = settings.CreateGroup(addressableGroupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            Debug.Log($"Created new Addressable group: {addressableGroupName}");
        }
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(gridObjectPath), group);
        entry.address = scriptableObjectName;
        if (!settings.GetLabels().Contains(labelName))
        {
            settings.AddLabel(labelName);
            Debug.Log($"Created new label: {labelName}");
        }
        if (!entry.labels.Contains(labelName))
        {
            entry.SetLabel(labelName, true);
            Debug.Log($"Added label '{labelName}' to scriptable '{scriptableObjectName}'.");
        }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"Scriptable '{scriptableObjectName}' saved and added to Addressables group '{addressableGroupName}' with address '{scriptableObjectName}' and label '{labelName}'.");
    }

    private void CreateFolder(string folderPath)
    {
        if (Directory.Exists(folderPath)) return;
        Directory.CreateDirectory(folderPath);
        Debug.Log("Folder created : " + folderPath);
    }
}
#endif