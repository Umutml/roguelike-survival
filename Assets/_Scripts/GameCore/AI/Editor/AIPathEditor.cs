#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AIPath))]
public class AIPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AIPath aiPath = (AIPath)target;
        if (GUILayout.Button("Create Path Point"))
        {
            CreatePathPoint(aiPath);
        }
    }

    private void CreatePathPoint(AIPath aiPath)
    {
        int index = aiPath.transform.childCount;
        GameObject newPoint = new GameObject($"Path_{index + 1:00}");
        newPoint.transform.parent = aiPath.transform;
        if(aiPath.pathPoints[index - 1])
            newPoint.transform.position = aiPath.pathPoints[index - 1].position + Vector3.right;
        else
            newPoint.transform.localPosition = Vector3.zero;
        Selection.activeGameObject = newPoint;
        ArrayUtility.Add(ref aiPath.pathPoints, newPoint.transform);
        EditorUtility.SetDirty(aiPath);
        SerializedObject serializedObject = new SerializedObject(aiPath);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

