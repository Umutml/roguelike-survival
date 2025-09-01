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
    fileName = nameof(GitReset),
    menuName =  "GitActions/Tools/" + nameof(GitReset)
)]
    public class GitReset : ScriptableObject
    {
        [SerializeField]
        private bool RefreshAssets = false;

        [SerializeField]
        [Tooltip("If you want hard reset, select this option. Use with caution!")]
        private bool HardReset = false;

        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            Debug.Log($"{nameof(GitReset)}: started");

            var unityProjectPath = $"{Directory.GetParent(Application.dataPath)}";
            var git = Cli.Tool("git");
            var argHard = $"{(HardReset ? "--hard" : "--soft")}";
            var command = $"reset {argHard} HARD";
            var result = git.Execute(command, unityProjectPath);
            if (RefreshAssets)
            {
                AssetDatabase.Refresh();
            }

            await Task.CompletedTask;

            stopwatch.Stop();
            Debug.Log($"{nameof(GitReset)}: {result}");
            Debug.Log($"{nameof(GitReset)}: completed {stopwatch.Elapsed.TotalSeconds}");

            if (result.ExitCode != 0)
            {
                await DiscordWebhookCommunication.SendWebhook($"{nameof(GitReset)}: Failed! {result}");
                throw new BuildStepFailedException($"{nameof(GitReset)}: Failed! {result}");
            }
        }
    }
}
