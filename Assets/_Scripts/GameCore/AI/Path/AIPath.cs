using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIPath : MonoBehaviour
{
    public Transform[] pathPoints;
    public Vector3[] pathPositions;
#if UNITY_EDITOR
    private readonly Color[] _colorPalette = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f),
        Color.magenta
    };
    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length < 2)
            return;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Transform start = pathPoints[i];
            Transform end = pathPoints[i + 1];
            if (start == null || end == null)
                continue;
            Vector3 direction = (end.position - start.position).normalized;
            float distance = Vector3.Distance(start.position, end.position);
            float step = 0.5f;
            for (float j = 0; j < distance; j += step)
            {
                Vector3 spherePosition = start.position + direction * j;
                Gizmos.color = _colorPalette[(i + 1) % _colorPalette.Length];
                Gizmos.DrawSphere(spherePosition, 0.5f);
            }
        }
    }
#endif
    public void InitPathPoints()
    {
        pathPositions = new Vector3[pathPoints.Length];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPositions[i] = pathPoints[i].position;
        }
    }
}