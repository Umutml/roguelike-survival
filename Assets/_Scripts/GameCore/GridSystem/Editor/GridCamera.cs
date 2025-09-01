#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using _Utilities;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GridCamera : MonoBehaviour
{
    public Camera OrthoCamera => GetComponent<Camera>();
    public int chunkSize = 10;
    public StaticEditorFlags staticLayers = StaticEditorFlags.BatchingStatic;

    private Vector3[] _gizmosPositions;
    private float _gizmoSize;

    private void OnEnable()
    {
    }

    private void OnDrawGizmos()
    {
        if(_gizmosPositions==null) return;
        for (int i = 0; i < _gizmosPositions.Length; i++)
        {
            Gizmos.color = i == 4 ? Color.green : Color.blue;
            Gizmos.DrawCube(_gizmosPositions[i], Vector3.one * _gizmoSize);
        }
    }
    public void SetGizmoSettings(Vector3[] gizmosPositions,float gizmoSize)
    {
        _gizmosPositions = gizmosPositions;
        _gizmoSize = gizmoSize;
    }
}
#endif

