using System;
using SuperUnityBuild.BuildTool;
using UnityEditor;

namespace Editor.SuperUnityBuildCustomActions.AndroidActions
{
    public class UploadAabToPlayStore : BuildAction, IPostBuildPerPlatformAction
    {

        public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildScriptingBackend scriptingBackend, BuildDistribution distribution, DateTime buildTime,
            ref BuildOptions options, string configKey, string buildPath)
        {
        }
    }
}
