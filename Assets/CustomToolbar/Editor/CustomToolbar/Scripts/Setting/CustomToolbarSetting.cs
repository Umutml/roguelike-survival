using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender.ToolbarElements;

namespace UnityToolbarExtender
{
    internal class CustomToolbarSetting : ScriptableObject
    {
        const string SETTING_PATH = "Assets/Editor/CustomToolbarSettings.asset";

        [SerializeReference]
        internal List<BaseToolbarElement> elements = new List<BaseToolbarElement>()
        {
            // new ToolbarEnterPlayMode(),
            new ToolbarSceneSelection(),
            new ToolbarStartFromFirstScene(),
            new ToolbarSpace(),
            new ToolbarDeleteAllSaveData(),
            new ToolbarPersistentData(),
            new ToolbarSpace(),
            new ToolbarClearPrefs(),
            new ToolbarSpace(),

            new ToolbarSides(),

            new ToolbarTimeslider(),
            new ToolbarFPSSlider(),
            new ToolbarSpace(),

            // new ToolbarRecompile(),
            // new ToolbarReserializeSelected(),
            // new ToolbarReserializeAll(),
        };

        internal static CustomToolbarSetting GetOrCreateSetting()
        {
            var setting = AssetDatabase.LoadAssetAtPath<CustomToolbarSetting>(SETTING_PATH);
            if (setting == null)
            {
                setting = ScriptableObject.CreateInstance<CustomToolbarSetting>();
                AssetDatabase.CreateAsset(setting, SETTING_PATH);
                AssetDatabase.SaveAssets();
            }
            return setting;
        }

        internal static SerializedObject GetSerializedSetting()
        {
            return new SerializedObject(GetOrCreateSetting());
        }

        internal void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}