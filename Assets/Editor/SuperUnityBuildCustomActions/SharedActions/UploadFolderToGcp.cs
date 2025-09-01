using System;
using System.IO;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UniTools.Build;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class UploadFolderToGcp : BuildAction, IPostBuildPerPlatformAction
    {
        [Tooltip("Local relative folder path, * sign must be preserved. It means all files in that folder. More info please look gcloud cli docs.")]
        [SerializeField]
        private string sourceFolder = "ServerData/iOS/*";

        [Tooltip("Google Cloud Storage Path, this field differs based on build type")]
        [SerializeField]
        private string destinationFolder = "REPLACE THIS WITH GOOGLE CLOUD STORAGE PATH";

        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            UploadFolderToGcpAsync();
        }

        [ContextMenu("Run UploadFolderToGcp")]
        private async void UploadFolderToGcpAsync()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Uploading Folder to GCP", "Starting upload...", 0.1f);
                Debug.Log($"{nameof(UploadFolderToGcp)}: Started");

                var fastlane = Cli.Tool("gcloud");

                var argSource = $"{Directory.GetParent(Application.dataPath)}/{sourceFolder}";
                var argDestination = $"gs://{destinationFolder}";

                var command = $"storage cp {argSource} {argDestination} -n -r";
                var result = fastlane.Execute(command);

                Debug.Log($"{nameof(UploadFolderToGcp)}: {result.Output}");
                Debug.Log($"{nameof(UploadFolderToGcp)}: Completed");

                if (result.ExitCode != 0)
                {
                    await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadFolderToGcp)}: Failed! {result}");
                    EditorUtility.ClearProgressBar();
                    throw new Exception($"{nameof(UploadFolderToGcp)}: Failed! {result}");
                }

                EditorUtility.DisplayProgressBar("Uploading Folder to GCP", "Upload completed successfully", 1.0f);
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadFolderToGcp)}: Failed! {e}");
                Debug.LogError($"{nameof(UploadFolderToGcp)}: Failed! {e}");
                EditorUtility.ClearProgressBar();
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create UploadFolderDataToGcp Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<UploadFolderToGcp>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/UploadFolderToGcp.asset");
        }
    }
}