using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Editor.iOSPostProcess;
using Editor.SuperUnityBuildCustomActions.Tools;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UniTools.Build;
using UnityEditor;
using UnityEngine;

[assembly: CliTool("fastlane")]

namespace Editor.SuperUnityBuildCustomActions.iOSActions
{
    public class IOSPostBuildActions : BuildAction, IPostBuildPerPlatformAction
    {
        private const string PlatformName = "iOS";
        [SerializeField] private string projectPath = "Builds/iOS";
        [SerializeField] private ExportMethods exportMethod = ExportMethods.AppStore;

        [Tooltip("Export and upload the IPA to the App Store.")]
        [SerializeField]
        private bool exportAndUpload = true;

        [Tooltip("If true, the build will be uploaded to the App Store after successful ipa export.")]
        [SerializeField]
        private bool uploadToAppStore = true;

        [Tooltip("Export the IPA only.")]
        [SerializeField]
        private bool exportIpa;

        [Tooltip("If true, the build will be tagged in git with version numbers after successful build.")]
        [SerializeField]
        private bool gitTagVersion = true;

        private readonly Dictionary<ExportMethods, string> _exportMethodDictionary = new()
        {
            { ExportMethods.AppStore, "app-store" },
            { ExportMethods.AdHoc, "ad-hoc" },
            { ExportMethods.Enterprise, "enterprise" },
            { ExportMethods.Development, "development" },
            { ExportMethods.MacApplication, "mac_application" }
        };

        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
            Debug.Log($"{nameof(IOSPostBuildActions)}: Started");

            if (exportAndUpload) // Export and upload via fastlane 1 command
            {
                BuildAndPush();
            }
            else if (exportIpa) // Export IPA and if successful, upload to App Store (if enabled)
            {
                ExportIpa();
            }
        }

        // Build and push to App Store via Fastlane command 'build_and_push' (1 command) Separate from ExportIpa method
        [ContextMenu("Run BuildAndPushFastlane")]
        private async void BuildAndPush()
        {
            while (EditorApplication.isCompiling) await Task.Delay(5000); // Wait for compilation to finish
            await IOSOnPostProcess.ExecutePodInstall();
            EditorUtility.DisplayProgressBar("Build-Upload IPA", "Building and Pushing...", 0.6f);
            await DiscordWebhookCommunication.SendWebhook($"{nameof(BuildAndPush)}: Started");
            Debug.Log($"{nameof(BuildAndPush)}: Started");
            var fastlane = Cli.Tool("fastlane");
            var command = "build_and_push";

            Debug.Log($"{nameof(BuildAndPush)}: Running command '{command}' in projectPath: {projectPath}");

            await SetEnvironmentVariables();

            var result = fastlane.Execute(command, projectPath);

            Debug.Log($"{nameof(BuildAndPush)}: Command Execution Completed. ExitCode: {result.ExitCode}");
            Debug.Log($"{nameof(BuildAndPush)}: Output: {result.Output}");
            Debug.Log($"{nameof(BuildAndPush)}: Error: {result.Error}");

            if (result.ExitCode != 0)
            {
                Debug.LogError($"{nameof(BuildAndPush)}: Failed! ExitCode: {result.ExitCode}, Error: {result.Error}, Output: {result.Output}");
                EditorUtility.ClearProgressBar();
                return;
            }

            Debug.Log($"{nameof(BuildAndPush)}: Completed Successfully.");
            await DiscordWebhookCommunication.SendWebhook($"{nameof(BuildAndPush)}: Completed Successfully.");
            EditorUtility.ClearProgressBar();
            await OnAutomationComplete();
        }

        [ContextMenu("Run ExportIpaFastlane")]
        private async void ExportIpa()
        {
            try
            {
                while (EditorApplication.isCompiling) await Task.Delay(5000); // Wait for compilation to finish
                await IOSOnPostProcess.ExecutePodInstall();
                // Initial state
                EditorUtility.DisplayProgressBar("Export-Upload IPA", "Exporting IPA...", 0.1f);
                Debug.Log($"{nameof(ExportIpa)}: Started");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(ExportIpa)}: Started");

                // Create the Fastlane command
                var fastlane = Cli.Tool("fastlane");
                var argExportMethod = $"export_method:{_exportMethodDictionary[exportMethod]}";
                var command = $"build {argExportMethod}";

                Debug.Log($"{nameof(ExportIpa)}: Running command '{command}' in projectPath: {projectPath}");

                // Set environment variables
                await SetEnvironmentVariables();

                // Call ExecuteAsync
                var result = await fastlane.ExecuteAsync(command, projectPath);

                // Log the output
                Debug.Log($"{nameof(ExportIpa)}: Command Execution Completed. ExitCode: {result.ExitCode}");
                Debug.Log($"{nameof(ExportIpa)}: Output: {result.Output}");
                Debug.Log($"{nameof(ExportIpa)}: Error: {result.Error}");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(ExportIpa)}: Command Completed. ExitCode: {result.ExitCode}");

                // Error handling
                if (result.ExitCode != 0)
                {
                    var errorMessage = $"{nameof(ExportIpa)}: Failed! ExitCode: {result.ExitCode}, Error: {result.Error}, Output: {result.Output}";
                    Debug.LogError(errorMessage);
                    await DiscordWebhookCommunication.SendWebhook(errorMessage);
                    return;
                }

                // If IPA was successfully exported
                Debug.Log($"{nameof(ExportIpa)}: IPA Exported Successfully.");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(ExportIpa)}: IPA Exported Successfully!");

                // Upload to App Store (if enabled)
                if (uploadToAppStore)
                {
                    Debug.Log($"{nameof(ExportIpa)}: Starting upload to App Store...");
                    EditorUtility.DisplayProgressBar("Export-Upload IPA", "Uploading to App Store...", 0.6f);
                    await UploadIpaAppStore();
                }
            }
            catch (Exception ex)
            {
                // Log errors in case of an exception
                Debug.LogError($"{nameof(ExportIpa)}: Exception occurred - {ex.Message}\n{ex.StackTrace}");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(ExportIpa)}: Exception occurred - {ex.Message}");
            }
            finally
            {
                // Final state
                EditorUtility.ClearProgressBar();
                Debug.Log($"{nameof(ExportIpa)}: Finished.");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(ExportIpa)}: Finished.");
            }
        }


        [ContextMenu("Run UploadIpaAppStore")]
        private async Task UploadIpaAppStore()
        {
            try
            {
                while (EditorApplication.isCompiling) await Task.Delay(5000); // Wait for compilation to finish

                // Notify Discord about the start of the upload process
                Debug.Log($"{nameof(UploadIpaAppStore)}: Started.");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadIpaAppStore)}: Started.");

                // Create the Fastlane command
                var fastlane = Cli.Tool("fastlane");
                var command = "push";

                Debug.Log($"{nameof(UploadIpaAppStore)}: Running command '{command}' in projectPath: {projectPath}");

                // Execute the command asynchronously
                var result = await fastlane.ExecuteAsync(command, projectPath);

                // Log command results
                Debug.Log($"{nameof(UploadIpaAppStore)}: Command Execution Completed. ExitCode: {result.ExitCode}");
                Debug.Log($"{nameof(UploadIpaAppStore)}: Output: {result.Output}");
                Debug.Log($"{nameof(UploadIpaAppStore)}: Error: {result.Error}");

                // Notify Discord of the completion status
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadIpaAppStore)}: Completed. ExitCode: {result.ExitCode}");

                // Error handling
                if (result.ExitCode != 0)
                {
                    var errorMessage = $"{nameof(UploadIpaAppStore)}: Failed! ExitCode: {result.ExitCode}, Error: {result.Error}, Output: {result.Output}";
                    Debug.LogError(errorMessage);
                    await DiscordWebhookCommunication.SendWebhook(errorMessage);
                    return;
                }

                // If successful, notify automation completion
                Debug.Log($"{nameof(UploadIpaAppStore)}: IPA uploaded successfully to App Store.");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadIpaAppStore)}: IPA uploaded successfully!");


                // Trigger automation completion workflow
                EditorUtility.DisplayProgressBar("Upload IPA", "Finalizing automation...", 0.9f);
                await OnAutomationComplete();
            }
            catch (Exception ex)
            {
                // Log exceptions
                Debug.LogError($"{nameof(UploadIpaAppStore)}: Exception occurred - {ex.Message}\n{ex.StackTrace}");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadIpaAppStore)}: Exception occurred - {ex.Message}");
            }
            finally
            {
                // Ensure the progress bar is cleared
                EditorUtility.ClearProgressBar();
                Debug.Log($"{nameof(UploadIpaAppStore)}: Finished.");
                await DiscordWebhookCommunication.SendWebhook($"{nameof(UploadIpaAppStore)}: Finished.");
            }
        }

        private async Task OnAutomationComplete()
        {
            var ciCdChannel = DiscordChannelWebhooks.Urls[DiscordChannelType.CiCdIos];
            await DiscordWebhookCommunication.SendWebhook($"{PlatformName} Automation Completed Successfully. Version: {VersionNames.GetIosVersionName()} BuildDate: {DateTime.Now}", ciCdChannel);
            // Create Git tag (if enabled)
            Debug.Log($"{nameof(ExportIpa)}: Tagging Git with Build Version...");
            EditorUtility.DisplayProgressBar("Export-Upload IPA", "Tagging Git Version...", 0.8f);
            await GitTagWithBuildVersion();
            EditorUtility.ClearProgressBar();
        }

        private async Task GitTagWithBuildVersion()
        {
            Debug.Log($"{nameof(GitTagWithBuildVersion)}: Started");
            var gitTag = CreateInstance<GitTag>();
            await gitTag.Execute();
            Debug.Log($"{nameof(GitTagWithBuildVersion)}: Completed");
            await DiscordWebhookCommunication.SendWebhook($"{nameof(GitTagWithBuildVersion)}: {VersionNames.GetIosVersionName()}");
            EditorUtility.ClearProgressBar();
        }

        private Task SetEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("LANG", "en_US.UTF-8");
            Environment.SetEnvironmentVariable("LC_ALL", "en_US.UTF-8");
            return Task.CompletedTask;
        }


        // This method is called from the BatchBuildAutomation.cs script for iOS builds ONLY BATCH BUILD
        public static void IosBatchBuildPostProcess()
        {
            DiscordWebhookCommunication.SendWebhook($"{nameof(IosBatchBuildPostProcess)}: Started");
            var fastlane = Cli.Tool("fastlane");
            var command = "build_and_push";

            // Set environment variables
            Environment.SetEnvironmentVariable("LANG", "en_US.UTF-8");
            Environment.SetEnvironmentVariable("LC_ALL", "en_US.UTF-8");

            var result = fastlane.Execute(command, "Builds/iOS");

            if (result.ExitCode != 0)
            {
                Debug.LogError($"{nameof(IosBatchBuildPostProcess)}: Failed! ExitCode: {result.ExitCode}, Error: {result.Error}, Output: {result.Output}");
                DiscordWebhookCommunication.SendWebhook($"{nameof(IosBatchBuildPostProcess)}: Failed! ExitCode: {result.ExitCode}");
                return;
            }

            DiscordWebhookCommunication.SendWebhook($"{nameof(IosBatchBuildPostProcess)}: Completed Successfully.");
            var ciCdChannel = DiscordChannelWebhooks.Urls[DiscordChannelType.CiCdIos];
            DiscordWebhookCommunication.SendWebhook($"{PlatformName} Batch Automation Completed. Version: {VersionNames.GetIosVersionName()} BuildDate: {DateTime.Now}", ciCdChannel);

            // Create Git tag
            var gitTag = CreateInstance<GitTag>();
            gitTag.Execute();
            DiscordWebhookCommunication.SendWebhook($"{nameof(GitTagWithBuildVersion)}: {VersionNames.GetIosVersionName()}");
        }

        [MenuItem("Tools/SuperUnityBuildSingle/Create IOSPostBuildActions Asset")]
        public static void IOSPostBuildActionCreateAsset()
        {
            AutomationAssetUtility.CreateAsset<IOSPostBuildActions>("Assets/Editor/SuperUnityBuildCustomActions/iOSActions/iOSPostBuildActions.asset");
        }

        private enum ExportMethods
        {
            AppStore,
            AdHoc,
            Enterprise,
            Development,
            MacApplication
        }
    }
}
