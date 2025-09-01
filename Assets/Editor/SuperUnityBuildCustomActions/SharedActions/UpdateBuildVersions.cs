using System;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class UpdateBuildVersions : BuildAction, IPreBuildAction, IPreBuildPerPlatformActionCanConfigureEditor
    {
        public string bundleVersion = "1.0.0";
        public int bundleBuildVersion = 100;

        public override void Execute() // This will be executed once before or after all players are built.
        {
            UpdateVersions();
        }
        
        [ContextMenu("Run")] 
        private async void UpdateVersions()
        {
            PlayerSettings.iOS.buildNumber = bundleBuildVersion.ToString();
            PlayerSettings.Android.bundleVersionCode = bundleBuildVersion;
            PlayerSettings.bundleVersion = bundleVersion;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Updated build versions to {bundleVersion} and {bundleBuildVersion}");
            await DiscordWebhookCommunication.SendWebhook($"{nameof(UpdateBuildVersions)} to {bundleVersion}-{bundleBuildVersion} completed.");
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create UpdateBuildVersions Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<UpdateBuildVersions>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/UpdateBuildVersions.asset");
        }
    }
}
