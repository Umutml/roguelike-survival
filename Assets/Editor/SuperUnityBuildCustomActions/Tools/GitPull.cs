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
    fileName = nameof(GitPull),
    menuName =  "GitActions/Tools/" + nameof(GitPull)
)]
    public class GitPull : ScriptableObject
    {
        [SerializeField] private bool RefreshAssets = false;
        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            Debug.Log($"{nameof(GitPull)}: started");

            var unityProjectPath = $"{Directory.GetParent(Application.dataPath)}";
            var git = Cli.Tool("git");
            var command = $"pull";
            var result = git.Execute(command, unityProjectPath);
            if (RefreshAssets)
            {
                AssetDatabase.Refresh();
            }

            await Task.CompletedTask;

            stopwatch.Stop();
            Debug.Log($"{nameof(GitPull)}: {result}");
            Debug.Log($"{nameof(GitPull)}: completed {stopwatch.Elapsed.TotalSeconds}");

            if (result.ExitCode != 0)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(GitPull)}: Failed! {result}");
                throw new BuildStepFailedException($"{nameof(GitPull)}: Failed! {result}");
            }
        }
    }
}
