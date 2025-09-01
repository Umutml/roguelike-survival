using System;
using System.IO;
using System.Threading.Tasks;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class DeleteAddressableFolderData : BuildAction, IPreBuildPerPlatformAction
    {
        [SerializeField] private bool Ios = false;
        [SerializeField] private bool Android = false;
        private string _buildPlatform;
        
        
        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            Console.WriteLine("DeleteAddressableFolderData: Execute");
            _buildPlatform = platform.platformName;
            DeleteAddressableData();
        }
        
        private async void DeleteAddressableData()
        {
            try
            {
                // Open the progress bar and send the start message
                await DiscordWebhookCommunication.SendWebhook($"{nameof(DeleteAddressableFolderData)} Started");
                Debug.Log($"{nameof(DeleteAddressableFolderData)}: Started");

                var addressableDataPath = Path.Combine(Application.dataPath, "../ServerData");
                if (Ios)
                {
                    FileUtil.DeleteFileOrDirectory(Path.Combine(addressableDataPath, "iOS"));
                }

                if (Android)
                {
                    FileUtil.DeleteFileOrDirectory(Path.Combine(addressableDataPath, "Android"));
                }
                
                // Build -> Clear Build Cache -> All - represents the Addressable Server Data that is stored in the cache
                AddressableAssetSettings.CleanPlayerContent();
                BuildCache.PurgeCache(false);
                Debug.Log("Addressable Server Data Deleted");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(DeleteAddressableFolderData)} {_buildPlatform} Completed.");

                // Complete the progress bar after successful completion
            }
            catch (Exception e)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(DeleteAddressableFolderData)} {_buildPlatform} Failed: {e}.");
                Console.WriteLine("Addressable Server Data Delete Failed: " + e);
                EditorUtility.ClearProgressBar();
                throw;
            }
            finally
            {
                // Clear the progress bar after the process is complete
                EditorUtility.ClearProgressBar();
            }
        }
        
        [MenuItem("Tools/SuperUnityBuildSingle/Create DeleteAddressableFolderData Asset")]
        public static void DeleteAddressableFolderDataCreateAsset()
        {
            AutomationAssetUtility.CreateAsset<DeleteAddressableFolderData>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/DeleteAddressableFolderData.asset");
        }
    }
}

