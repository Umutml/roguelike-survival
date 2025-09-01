#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using _Scripts.Utilities;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MinimapCamera))]
public class MinimapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MinimapCamera splitter = (MinimapCamera)target;
        if (GUILayout.Button("Create Minimap"))
        {
            TakeSnapshotAndSplit(splitter);
        }
        if (GUILayout.Button("Check Minimap Borders"))
        {
            CheckMinimapBorders(splitter.GetComponent<Camera>());
        }
    }

    private void CheckMinimapBorders(Camera camera)
    {
        var heightMin = camera.transform.position.z - camera.orthographicSize;
        var heightMax = camera.transform.position.z + camera.orthographicSize;
        var widthMin = camera.transform.position.x - camera.orthographicSize * camera.aspect;
        var widthMax = camera.transform.position.x + camera.orthographicSize * camera.aspect;
        LoggerNS.Log($"Minimap borders: \nHeight Min: {heightMin} - Max: {heightMax}\nWidth Min: {widthMin} - Max: {widthMax}");
    }
    private void TakeSnapshotAndSplit(MinimapCamera splitter)
    {
        var camera = splitter.GetComponent<Camera>();
        if (camera == null)
        {
            LoggerNS.LogError("Camera component not found on MinimapCamera object.");
            return;
        }
        var resolution = splitter.resolution;
        var partsHorizontal = 3;
        var partsVertical = 3;

        var rt = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = rt;
        var snapshot = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        snapshot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        snapshot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        var partWidth = resolution / partsHorizontal;
        var partHeight = resolution / partsVertical;
        var sceneName = SceneManager.GetActiveScene().name;
        var folderPath = Path.Combine(Application.dataPath+"/_Graphics/Minimaps/", sceneName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        var savedFiles = new List<string>();
        for (int y = 0; y < partsVertical; y++)
        {
            for (int x = 0; x < partsHorizontal; x++)
            {
                Texture2D part = new Texture2D(partWidth, partHeight);
                part.SetPixels(snapshot.GetPixels(x * partWidth, y * partHeight, partWidth, partHeight));
                part.Apply();
                byte[] bytes = part.EncodeToPNG();
                string filePath = Path.Combine(folderPath, $"{sceneName}_Part_{y}_{x}.png");
                File.WriteAllBytes(filePath, bytes);
                savedFiles.Add(filePath);
                LoggerNS.Log($"File saved: {filePath}");
            }
        }

        LoggerNS.Log("Minimap assets created!");
        CreateAddressablesGroup(sceneName + "_Minimap", savedFiles,camera);
    }
    private void CreateAddressablesGroup(string groupName, List<string> filePaths,Camera camera)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        foreach (var file in filePaths)
        {
            var relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
            var textureImporter = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spritePixelsPerUnit = 100;
                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.SaveAndReimport();
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (!settings)
        {
            LoggerNS.LogError("Addressables settings not found. Please ensure Addressables is set up.");
            return;
        }
        var group = settings.FindGroup("Minimaps") ?? settings.CreateGroup("Minimaps", false, false, false, null);
        var label = "MinimapImages";
        if (!settings.GetLabels().Contains(label))
        {
            settings.AddLabel(label);
        }
        var assetReferences = new List<AssetReference>();
        foreach (var filePath in filePaths)
        {
            var relativePath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
            var guid = AssetDatabase.AssetPathToGUID(relativePath);
            var regexMatch = Regex.Match(relativePath, @"([^/]+)$");
            var addressableName = regexMatch.Success ? regexMatch.Groups[1].Value : relativePath;
            if (string.IsNullOrEmpty(guid))
            {
                LoggerNS.LogError($"Asset at {relativePath} could not be found in the Asset Database.");
                continue;
            }
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.SetLabel(label, true);
            entry.SetAddress(addressableName);
            assetReferences.Add(new AssetReference(guid));
        }
        LoggerNS.Log($"Addressables Group '{groupName}' oluşturuldu ve dosyalar eklendi.");
        AssetDatabase.SaveAssets();
        
        var minimapObject = CreateInstance<MinimapObject>();
        minimapObject.CreateMinimap(camera, assetReferences.ToArray());
        var minimapObjectPath = $"Assets/_Scripts/Scriptable_Objects/Minimaps/{groupName}.asset";
        AssetDatabase.CreateAsset(minimapObject, minimapObjectPath);
        EditorUtility.SetDirty(minimapObject);
        var minimapObjectEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(minimapObjectPath), group);
        var minimapObjectLabel = "MinimapObject";
        if (!settings.GetLabels().Contains(minimapObjectLabel))
        {
            settings.AddLabel(minimapObjectLabel);
        }
        minimapObjectEntry.SetLabel(minimapObjectLabel, true);
        minimapObjectEntry.SetAddress(groupName);
        AssetDatabase.SaveAssets();
        // AddressableAssetSettings.BuildPlayerContent();
    }
}
#endif
