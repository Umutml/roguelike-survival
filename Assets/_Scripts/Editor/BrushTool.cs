using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class BrushTool : EditorWindow
{
    private float _brushSize = 1.0f;
    private readonly List<GameObject> _selectedObjects = new List<GameObject>();
    private bool _isBrushActive;
    private int _selectedLayer;
    private string[] _layerNames;
    private bool _isDragging;

    [MenuItem("Tools/Layer Brush Tool")]
    public static void ShowWindow()
    {
        GetWindow<BrushTool>("Layer Brush Tool");
    }

    private void OnEnable()
    {
        _layerNames = GetLayerNames();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Layer Brush Tool Settings", EditorStyles.boldLabel);
        _brushSize = EditorGUILayout.Slider("Brush Size", _brushSize, 0.1f, 20.0f);
        GUILayout.Label("Select Layer", EditorStyles.label);
        _selectedLayer = EditorGUILayout.Popup(_selectedLayer, _layerNames);

        if (GUILayout.Button("Clear Selection"))
        {
            _selectedObjects.Clear();
            Selection.objects = new Object[0];
        }

        EditorGUILayout.Space();
        GUILayout.Label("Instructions:", EditorStyles.helpBox);
        GUILayout.Label("- Hold Shift to activate the brush.\n- Click and drag to select objects from the selected layer.", EditorStyles.wordWrappedLabel);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.shift)
        {
            _isBrushActive = true;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Handles.color = new Color(0, 0.5f, 1, 0.4f);
                Handles.DrawSolidDisc(hit.point, hit.normal, _brushSize);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _isDragging = true;
                    SelectObjectsInRadius(hit.point);
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && _isDragging)
                {
                    SelectObjectsInRadius(hit.point);
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && _isDragging)
                {
                    _isDragging = false;
                    e.Use();
                }
            }
        }
        else
        {
            _isBrushActive = false;
        }

        SceneView.RepaintAll();
    }

    private void SelectObjectsInRadius(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, _brushSize);
        List<Object> currentSelection = new List<Object>(Selection.objects);

        foreach (var collider in colliders)
        {
            GameObject obj = collider.gameObject;
            if (obj.layer == _selectedLayer && !_selectedObjects.Contains(obj))
            {
                _selectedObjects.Add(obj);
                currentSelection.Add(obj);
            }
        }
        Selection.objects = currentSelection.ToArray();
    }

    private string[] GetLayerNames()
    {
        List<string> layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layers.Add(layerName);
            }
        }
        return layers.ToArray();
    }
}
