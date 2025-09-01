using System;
using System.Threading.Tasks;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class BuildAddressableContent : BuildAction, IPreBuildPerPlatformAction
    {
        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            Console.WriteLine("BuildAddressableContent: Execute");
            BuildAddressableByProfile();
        }

        [ContextMenu("Run")]
        private async void BuildAddressableByProfile()
        {
            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    throw new Exception("AddressableAssetSettings not found.");
                }

                AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
                AddressableAssetSettings.BuildPlayerContent(out var result);
                var success = string.IsNullOrEmpty(result.Error); // If there is no error, the build was successful
                if (!success)
                {
                    Debug.LogError("Addressable build error encountered: " + result.Error);
                    throw new Exception($"{nameof(BuildAddressableContent)}: Failed! {result.Error}");
                }
                await DiscordWebhookCommunication.SendWebhook($"{nameof(BuildAddressableContent)} Builded.");
                Debug.Log("Addressable content build completed successfully.");
            }
            catch (Exception e)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(BuildAddressableContent)} Failed: {e}.");
                Console.WriteLine("Addressable Server Data Build Failed: " + e);
                EditorUtility.ClearProgressBar();
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        [MenuItem("Tools/SuperUnityBuildSingle/Create BuildAddressableContent Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<BuildAddressableContent>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/BuildAddressableContent.asset");
        }
    }
}