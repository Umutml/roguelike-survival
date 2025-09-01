using System;
using System.IO;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class CopyFastlaneFiles : BuildAction, IPostBuildPerPlatformAction
    {
        [SerializeField] private string sourceFolder = "FastlaneBackup/iOS";
        [SerializeField] private string destinationFolder = "Builds/iOS";
        private string _buildPlatform;
        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            Debug.Log("CopyFastlaneFiles: Execute");
            _buildPlatform = platform.platformName;
            CheckPaths();
        }

        [ContextMenu("Run CopyFastlaneFiles")]
        private async void CheckPaths()
        {
            try
            {
                var sourcePath = Path.Combine(Application.dataPath, "../" + sourceFolder);
                var destinationPath = Path.Combine(Application.dataPath, "../" + destinationFolder);

                if (!Directory.Exists(sourcePath))
                {
                    Debug.LogError("Source path does not exist: " + sourcePath);
                    return;
                }

                CopyAll(new DirectoryInfo(sourcePath), new DirectoryInfo(destinationPath));
                await DiscordWebhookCommunication.SendWebhook($"{nameof(CopyFastlaneFiles)} {_buildPlatform} Completed.");
                Debug.Log("Files copied successfully from " + sourcePath + " to " + destinationPath);
            }
            catch (Exception e)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(CopyFastlaneFiles)} Failed.");
                Debug.LogError("CopyFastlaneFiles: CheckPaths failed: " + e);
                throw;
            }
        }

        // Check all source files/folders and copy them to the target if they don't already exist
        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
            {
                var targetSubDir = target.CreateSubdirectory(dir.Name);
                CopyAll(dir, targetSubDir); // Copy all files and subdirectories recursively
            }

            foreach (var file in source.GetFiles())
            {
                var targetFilePath = Path.Combine(target.FullName, file.Name);
                if (!File.Exists(targetFilePath))
                {
                    file.CopyTo(targetFilePath, true);
                }
                else
                {
                    Debug.Log($"File already exists and will not be copied: {targetFilePath}");
                    throw new Exception($"File already exists and will not be copied: {targetFilePath}");
                }
            }
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create CopyFastlaneFiles Asset")]
        public static void CopyFastlaneFilesCreateAsset()
        {
            AutomationAssetUtility.CreateAsset<CopyFastlaneFiles>("Assets/Editor/SuperUnityBuildCustomActions/iOSActions/CopyFastlaneFiles.asset");
        }
    }
}