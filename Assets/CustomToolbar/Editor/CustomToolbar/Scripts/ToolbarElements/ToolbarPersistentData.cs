using System;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace UnityToolbarExtender.ToolbarElements
{
    [Serializable]
    internal class ToolbarPersistentData : BaseToolbarElement
    {
        private static GUIContent openPersistentDataBtn;

        public override string NameInList => "[Button] Open persistent data folder";
        public override int SortingGroup => 5;

        public override void Init()
        {
            openPersistentDataBtn = EditorGUIUtility.IconContent("d_Folder Icon");
            openPersistentDataBtn.tooltip = "Open persistent data";
        }

        protected override void OnDrawInList(Rect position)
        {
        }

        protected override void OnDrawInToolbar()
        {
            if (GUILayout.Button(openPersistentDataBtn, ToolbarStyles.commandButtonStyle))
            {
                OpenPersistentDataFolder();
            }
        }

        private void OpenPersistentDataFolder()
        {
            string persistentDataPath = Application.persistentDataPath;
            UnityEngine.Debug.Log("Opening persistent data folder: " + persistentDataPath);

#if UNITY_EDITOR_OSX
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{persistentDataPath}\"",
                UseShellExecute = false
            };
            Process.Start(startInfo);
#elif UNITY_EDITOR_WIN
            var fullPath = System.IO.Path.GetFullPath(persistentDataPath);
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = fullPath,
                UseShellExecute = true
            };
            Process.Start(startInfo);
#else
    UnityEngine.Debug.LogWarning("Platform not supported for opening persistent data folder.");
#endif
        }
    }
}
