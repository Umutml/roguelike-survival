using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class AutomationBuildStarted : BuildAction, IPreBuildAction
    {
        private string _buildPlatform;
        public override void Execute()
        {
            _buildPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
            PostBuildStarted();
        }
        
        [ContextMenu("Run AutomationBuildStarted")] 
        private async void PostBuildStarted()
        {
            Debug.Log($"<<{_buildPlatform} {nameof(AutomationBuildStarted)} >>");
            await DiscordWebhookCommunication.SendWebhook($"<<{_buildPlatform} {nameof(AutomationBuildStarted)}>>");
        }
        
        [MenuItem("Tools/SuperUnityBuildSingle/Automation Build Started")]
        public static void AutomationBuildStartedCreateAsset()
        {
            AutomationAssetUtility.CreateAsset<AutomationBuildStarted>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/AutomationBuildStarted.asset");
        }
    }
}