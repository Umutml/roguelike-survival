using System.Threading.Tasks;

namespace UniTools.Build
{
    public abstract class BaseCliTool : ICliToolPath, ICliToolInstalled
    {
        public bool IsInstalled
        {
            get
            {
                try
                {
                    return System.IO.Path.IsPathRooted(Path);
                }
                catch
                {
                    return false;
                }
            }
        }

        public abstract string Path { get; }

        public abstract ToolResult Execute(string arguments = null, string workingDirectory = null);

        /// <summary>
        /// Execute method wrapped in an async Task for asynchronous usage.
        /// </summary>
        public async Task<ToolResult> ExecuteAsync(string arguments = null, string workingDirectory = null)
        {
            return await Task.Run(() =>
            {
                var result = Execute(arguments, workingDirectory);
                return result;
            });
        }
    }
}
