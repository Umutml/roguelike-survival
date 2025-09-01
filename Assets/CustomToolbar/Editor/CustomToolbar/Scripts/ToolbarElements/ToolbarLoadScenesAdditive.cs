using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityToolbarExtender.ToolbarElements
{
    [Serializable]
    internal class ToolbarLoadScenesAdditive : BaseToolbarElement
    {
        private static GUIContent loadScenesBtn;
        private const string ScenePath = "Assets/Scenes/CoreScenes/";

        public override string NameInList => "[Button] Load Scenes Additive";
        public override int SortingGroup => 4;

        public override void Init()
        {
            loadScenesBtn = EditorGUIUtility.IconContent("d_AlphabeticalSorting");
            loadScenesBtn.tooltip = "Loads BaseScene, GameScene, and CleanCity";
        }

        protected override void OnDrawInList(Rect position)
        {
            // No implementation needed for this example
        }

        protected override void OnDrawInToolbar()
        {
            if (GUILayout.Button(loadScenesBtn, ToolbarStyles.commandButtonStyle))
            {
                LoadScenes();
            }
        }

        private void LoadScenes()
        {
            OpenSceneEditor("BaseScene", OpenSceneMode.Single);
            OpenSceneEditor("GameScene", OpenSceneMode.Additive);
            OpenSceneEditor("CleanCity", OpenSceneMode.Additive);
        }

        private void OpenSceneEditor(string sceneName, OpenSceneMode mode)
        {
            try
            {
                EditorSceneManager.OpenScene($"{ScenePath}{sceneName}.unity", mode);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {sceneName}: {e.Message}");
            }
        }
    }
}