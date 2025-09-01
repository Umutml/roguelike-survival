using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.Utilities
{
    public static class AutomationAssetUtility
    {
        
        [Tooltip("Creates a ScriptableObject asset at the specified path.")]
        public static void CreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log($"{typeof(T).Name} asset created at {assetPath}");
        }
    }
}
