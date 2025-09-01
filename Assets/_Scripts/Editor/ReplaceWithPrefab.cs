using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
    private GameObject prefab;
    private GameObject[] selectedObjects;

    [MenuItem("GameObject/Replace With Prefab", false, 0)]
    static void ReplaceSelected()
    {
        ReplaceWithPrefab window = GetWindow<ReplaceWithPrefab>(true, "Replace With Prefab");
        window.selectedObjects = Selection.gameObjects;
        window.minSize = new Vector2(750, 100);
        window.maxSize = new Vector2(750, 100);
        window.TryFindMatchingPrefab();
        window.Show();
    }

    private void TryFindMatchingPrefab()
    {
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        string searchName = selectedObjects[0].name;
        string[] guids = AssetDatabase.FindAssets("t:Prefab " + searchName);
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject foundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (foundPrefab.name == searchName)
            {
                prefab = foundPrefab;
                return;
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10);
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        
        EditorGUILayout.Space(20);
        
        using (new EditorGUI.DisabledScope(prefab == null))
        {
            if (GUILayout.Button("OK"))
            {
                ReplaceObjects();
                Close();
            }
        }
    }

    private void ReplaceObjects()
    {
        foreach (GameObject selected in selectedObjects)
        {
            Transform selectedTransform = selected.transform;
            Vector3 position = selectedTransform.position;
            Quaternion rotation = selectedTransform.rotation;
            Vector3 scale = selectedTransform.localScale;
            GameObject parent = selected.transform.parent?.gameObject;

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.transform.localScale = scale;

            if (parent != null)
            {
                newObject.transform.SetParent(parent.transform, true);
            }

            Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefab");
            Undo.DestroyObjectImmediate(selected);
        }
    }
}
