using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UniTools.Build;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.SuperUnityBuildCustomActions.Tools
{
    [CreateAssetMenu(
    fileName = nameof(GitTag),
    menuName =  "GitActions/Tools/" + nameof(GitTag)
)]
    public class GitTag : ScriptableObject
    {
        [SerializeField]
        private bool CreateTag = true;

        [SerializeField]
        private bool PushTag = true;

        public async Task Execute()
        {
            var versionCode = $"{PlayerSettings.Android.bundleVersionCode}";

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget == BuildTarget.iOS)
            {
                versionCode = PlayerSettings.iOS.buildNumber;
            }
            var bundleVersion = PlayerSettings.bundleVersion;
            var buildType = "Release"; // TODO: get build type from version settings

            var argTagName = $"v{bundleVersion}-{versionCode}-{buildType}";

            var unityProjectPath = $"{Directory.GetParent(Application.dataPath)}";
            var git = Cli.Tool("git");

            var tagExist = TaskTagExist(argTagName, git, unityProjectPath);
            if (!tagExist)
            {
                if (CreateTag) { await CreateTagAsync(argTagName, git, unityProjectPath); }
                if (PushTag) { await PushTagAsync(argTagName, git, unityProjectPath); }
            }
            await Task.CompletedTask;
        }

        private async Task CreateTagAsync(string argTagName, BaseCliTool git, string unityProjectPath)
        {
            var commandTagCreate = $"tag {argTagName}";
            var gitCreateResult = git.Execute(commandTagCreate, unityProjectPath);

            Debug.Log($"{nameof(GitTag)}: {gitCreateResult}");

            if (gitCreateResult.ExitCode != 0)
            {
                await DiscordWebhookCommunication.SendWebhook(
                    $"{nameof(GitTag)}: Tag Create Failed! {gitCreateResult}"
                );
            }
        }

        private async Task PushTagAsync(string argTagName, BaseCliTool git, string unityProjectPath)
        {
            var commandTagPush = $"push origin {argTagName}";
            var gitPushResult = git.Execute(commandTagPush, unityProjectPath);

            Debug.Log($"{nameof(GitTag)}: {gitPushResult}");

            if (gitPushResult.ExitCode != 0)
            {
                await DiscordWebhookCommunication.SendWebhook(
                    $"{nameof(GitTag)}:Tag Push Failed! {gitPushResult}"
                );
            }
        }

        private bool TaskTagExist(string argTagName, BaseCliTool git, string unityProjectPath)
        {
            var commandTagPush = $"tag -l";
            var gitPushResult = git.Execute(commandTagPush, unityProjectPath);
            var tagContain = gitPushResult.Output.Contains(argTagName);
            return tagContain;
        }
    }
}
