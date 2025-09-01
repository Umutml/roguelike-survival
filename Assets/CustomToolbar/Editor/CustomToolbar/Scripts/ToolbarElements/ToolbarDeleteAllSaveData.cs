using System;
using _Utilities;
using UnityEngine;
using UnityEditor;

namespace UnityToolbarExtender.ToolbarElements
{
    [Serializable]
    internal class ToolbarDeleteAllSaveData : BaseToolbarElement
    {
        private static GUIContent deleteAllSaveDataBtn;

        public override string NameInList => "[Button] Delete all save data";
        public override int SortingGroup => 4;

        public override void Init()
        {
            deleteAllSaveDataBtn = EditorGUIUtility.IconContent("CacheServerDisconnected");
            deleteAllSaveDataBtn.tooltip = "Delete all persistent save data";
        }

        protected override void OnDrawInList(Rect position)
        {
        }

        protected override void OnDrawInToolbar()
        {
            if (GUILayout.Button(deleteAllSaveDataBtn, ToolbarStyles.commandButtonStyle))
            {
                SaveLoadHelper.DeleteAllSavedData();
                PlayerPrefs.DeleteAll();
            }
        }
    } 
}