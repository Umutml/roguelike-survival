using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class SelectComponentsInHierarchy : EditorWindow
{
    private string componentName = "";
    private string layerName = "";
    
    [MenuItem("Tools/Select Components In Hierarchy")]
    public static void ShowWindow()
    {
        GetWindow<SelectComponentsInHierarchy>("Select Components");
    }

    private void OnGUI()
    {
        componentName = EditorGUILayout.TextField("Component Name", componentName);
        layerName = EditorGUILayout.TextField("Layer Name", layerName);

        if (GUILayout.Button("OK"))
        {
            FindAndSelectComponents();
        }
    }

    private void FindAndSelectComponents()
    {
        if (string.IsNullOrEmpty(componentName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a component name", "OK");
            return;
        }

        var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        var selectedObjects = allGameObjects
            .Where(go =>
            {
                if (!string.IsNullOrEmpty(layerName))
                {
                    int layerIndex = LayerMask.NameToLayer(layerName);
                    if (layerIndex == -1 || go.layer != layerIndex)
                        return false;
                }

                Type componentType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase));

                return componentType != null && go.GetComponent(componentType) != null;
            })
            .ToArray();

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Result", "No GameObjects found with the specified criteria", "OK");
            return;
        }

        Selection.objects = selectedObjects;
    }
}
