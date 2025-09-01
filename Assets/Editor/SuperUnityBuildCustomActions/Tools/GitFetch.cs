using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UniTools.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.SuperUnityBuildCustomActions.Tools
{
    [CreateAssetMenu(
    fileName = nameof(GitFetch),
    menuName =  "GitActions/Tools/" + nameof(GitFetch)
)]
    public class GitFetch : ScriptableObject
    {
        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            Debug.Log($"{nameof(GitFetch)}: started");

            var unityProjectPath = $"{Directory.GetParent(Application.dataPath)}";
            var git = Cli.Tool("git");
            var command = $"fetch";
            var result = git.Execute(command, unityProjectPath);

            await Task.CompletedTask;

            stopwatch.Stop();
            Debug.Log($"{nameof(GitFetch)}: {result}");
            Debug.Log($"{nameof(GitFetch)}: completed {stopwatch.Elapsed.TotalSeconds}");

            if (result.ExitCode != 0)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(GitFetch)}: Failed! {result}");
                throw new BuildStepFailedException($"{nameof(GitFetch)}: Failed! {result}");
            }
        }
    }

}
