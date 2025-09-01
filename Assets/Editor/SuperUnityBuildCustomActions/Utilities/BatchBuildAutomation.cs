using Editor.SuperUnityBuildCustomActions.iOSActions;
using SuperUnityBuild.BuildTool;
using UnityEditor;

namespace Editor.SuperUnityBuildCustomActions.Utilities
{
    public static class BatchBuildAutomation
    {
        public static void PerformBuild()
        {
            BuildProject.BuildAll();
            IOSPostBuildActions.IosBatchBuildPostProcess();
            EditorApplication.Exit(0);
        }
    }
}
