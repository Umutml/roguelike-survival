using _Scripts.Utilities;
using Editor.SuperUnityBuildCustomActions.Utilities;
using SuperUnityBuild.BuildTool;
using UnityEditor;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    public class AutomationLoggerNsSet : BuildAction, IPreBuildAction, IPreBuildPerPlatformActionCanConfigureEditor
    {
        
        [Tooltip("Enable or disable LoggerNS")]
        public bool loggerStatus = true;
    
        public override void Execute()
        {
            SetLoggerStatus();
        }
        
        [ContextMenu("Run")]
        private void SetLoggerStatus()
        {
            LoggerNS.SetLogStatus(loggerStatus);
        }
        
        [MenuItem("Tools/SuperUnityBuildSingle/Create LoggerStatus Asset")]
        public static void CreateAsset()
        {
            AutomationAssetUtility.CreateAsset<AutomationLoggerNsSet>("Assets/Editor/SuperUnityBuildCustomActions/SharedActions/SetLoggerStatus.asset");
        }
    }
}
