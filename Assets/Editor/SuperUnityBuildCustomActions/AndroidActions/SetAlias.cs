using System;
using System.Threading.Tasks;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UniTools.Build;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.AndroidActions
{
    public sealed class SetAlias : BuildAction, IPreBuildPerPlatformAction, IPreBuildPerPlatformActionCanConfigureEditor
    {
        [SerializeField] private string mAlias = default;
        [SerializeField] private string mPassword = default;

        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            SetAliasData();
        }
        
        [ContextMenu("Run ExportIpaFastlane")]
        public async void SetAliasData()
        {
            if (!PlayerSettings.Android.useCustomKeystore)
            {
                throw new BuildFailedException($"{nameof(SetAlias)}: failed due to using of the custom keystore is not selected!");
            }
        
            PlayerSettings.Android.keyaliasName = mAlias;
            PlayerSettings.Android.keyaliasPass = mPassword;
            PlayerSettings.Android.keystorePass = mPassword;
            PlayerSettings.Android.useCustomKeystore = true;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(SetAlias)}: Alias and password configured for {mAlias}");
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Set Alias")]
        public static void SetAliasCreateAsset()
        {
            AutomationAssetUtility.CreateAsset<SetAlias>("Assets/Editor/SuperUnityBuildCustomActions/AndroidActions/SetAliasBuildAction.asset");
        }
        
        
    }
}
