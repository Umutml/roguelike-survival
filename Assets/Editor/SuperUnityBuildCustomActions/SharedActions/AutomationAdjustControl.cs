using System;
using AdjustSdk;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class AutomationAdjustControl : BuildAction, IPreBuildAction, IPreBuildPerPlatformActionCanConfigureEditor
    {
        [Tooltip("Enable or disable adjust")] public bool isProduction;

        private const string AdjustPrefabPath = "Assets/Adjust/Prefab/Adjust.prefab";

        public override void Execute()
        {
            SetAdjustStatus();
        }

        [ContextMenu("Run")]
        public void SetAdjustStatus()
        {
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AdjustPrefabPath);
                if (prefab != null)
                {
                    var adjustInstance = prefab.GetComponent<Adjust>();
                    if (adjustInstance != null)
                    {
                        adjustInstance.environment = isProduction ? AdjustEnvironment.Production : AdjustEnvironment.Sandbox;
                        EditorUtility.SetDirty(adjustInstance);
                        PrefabUtility.SavePrefabAsset(prefab);
                        AssetDatabase.SaveAssets();
                        Debug.Log("Adjust status is " + isProduction);
                        DiscordWebhookCommunication.SendWebhook($"Adjust status set {isProduction} success");
                    }
                    else
                    {
                        Debug.LogError("Adjust component not found in the prefab.");
                    }
                }
                else
                {
                    Debug.LogError("Prefab not found at path: " + AdjustPrefabPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to set Adjust status: " + e.Message);
                DiscordWebhookCommunication.SendWebhook($"Failed to set Adjust status: {e.Message}");
                throw;
            }
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create AutomationAdjustControl Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<AutomationAdjustControl>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/AutomationAdjustControl.asset");
        }
    }
}