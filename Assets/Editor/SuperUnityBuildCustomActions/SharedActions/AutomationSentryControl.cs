using _Scripts.Utilities;
using Editor.SuperUnityBuildCustomActions.Utilities;
using Sentry.Unity;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class AutomationSentryControl : BuildAction, IPreBuildAction
    {
        public bool sentryStatus;

        public override void Execute()
        {
            ConfigureSentryOptions();
        }

        [ContextMenu("Run")]
        private void ConfigureSentryOptions()
        {
            var path = ScriptableSentryUnityOptions.GetConfigPath();
            var sentryOptions = AssetDatabase.LoadAssetAtPath<ScriptableSentryUnityOptions>(path);

            if (sentryOptions == null)
            {
                LoggerNS.LogError($"Failed to load SentryOptions at path: {path}");
                return;
            }

            sentryOptions.Enabled = sentryStatus; // Set the Sentry status

            EditorUtility.SetDirty(sentryOptions);
            AssetDatabase.SaveAssets();

            LoggerNS.Log("SentryOptions configured and saved successfully, Sentry status is " + sentryStatus);
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create AutomationSentryControl Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<AutomationSentryControl>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/AutomationSentryControl.asset");
        }
    }
}
