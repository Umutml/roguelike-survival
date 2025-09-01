using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class SrDebuggerControl : BuildAction, IPreBuildAction, IPreBuildPerPlatformActionCanConfigureEditor
    {
        [Tooltip("Enable or disable SRDebugger")]
        public bool enableDebugger = false;

        private const string SettingsDefaultPath = "Assets/StompyRobot/SRDebugger/usr/Resources/SRDebugger/Settings.asset";

        public override void Execute()
        {
            SetDebuggerStatus();
        }

        [ContextMenu("Run")]
        private void SetDebuggerStatus()
        {
            var settings = AssetDatabase.LoadAssetAtPath<Object>(SettingsDefaultPath);
            if (settings == null)
            {
                Debug.LogError("SRDebugger settings asset not found at " + SettingsDefaultPath);
                return;
            }

            var serializedSettings = new SerializedObject(settings);
            var enableProperty = serializedSettings.FindProperty("_isEnabled");

            if (enableProperty != null)
            {
                enableProperty.boolValue = enableDebugger;
                serializedSettings.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.Log("SRDebugger status is " + enableDebugger);
                DiscordWebhookCommunication.SendWebhook("SRDebugger status set success " + enableDebugger);
            }
            else
            {
                Debug.LogError("_isEnabled property not found in SRDebugger settings asset.");
                // Debug log to list all properties for debugging fixing
                // var prop = serializedSettings.GetIterator();
                // while (prop.NextVisible(true)) Debug.Log("Property name: " + prop.name);
            }
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create SrDebuggerControl Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<SrDebuggerControl>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/SrDebuggerControl.asset");
        }
    }
}