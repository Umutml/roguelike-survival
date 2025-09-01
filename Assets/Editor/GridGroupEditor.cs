using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class GridGroupEditor : EditorWindow
{
    private int groupSize = 2;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Group Grid Objects With Labels")]
    public static void ShowWindow()
    {
        GetWindow<GridGroupEditor>("Grid Group Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Group Grid Objects", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Group size field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Group size:", GUILayout.Width(80));
        groupSize = EditorGUILayout.IntField(groupSize);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Set Labels"))
        {
            GroupGridObjects();
        }
    }

    private float GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    private void GroupGridObjects()
    {
        // Get all addressable assets
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.groups.Find(g => g.Name == "CleanCity_Grids");
        
        if (group == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find CleanCity_Grids addressable group!", "OK");
            return;
        }

        // Get all chunk entries
        var chunkEntries = group.entries.Where(e => e.address.StartsWith("CleanCity_Chunk_")).ToList();

        // First, clear all existing labels from all chunks
        foreach (var entry in chunkEntries)
        {
            entry.labels.Clear();
        }
        
        // Dictionary to store grid positions and their entries
        var gridPositions = new Dictionary<Vector2Int, AddressableAssetEntry>();
        var allCoordinates = new HashSet<Vector2Int>();
        
        // Parse coordinates from chunk names
        foreach (var entry in chunkEntries)
        {
            var match = Regex.Match(entry.address, @"CleanCity_Chunk_(\d+)_(\d+)");
            if (match.Success)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                var pos = new Vector2Int(x, y);
                gridPositions[pos] = entry;
                allCoordinates.Add(pos);
            }
        }

        // Sort coordinates by X then Y for consistent grouping
        var remainingCoords = allCoordinates.ToList();
        int groupIndex = 1;

        while (remainingCoords.Count > 0)
        {
            var currentGroup = new List<Vector2Int>();
            var startPos = remainingCoords[0];
            currentGroup.Add(startPos);
            remainingCoords.RemoveAt(0);

            // While we haven't filled the group and there are remaining coordinates
            while (currentGroup.Count < groupSize && remainingCoords.Count > 0)
            {
                // Find the closest remaining coordinate to any point in the current group
                var bestDistance = float.MaxValue;
                var bestCoord = remainingCoords[0];
                var bestIndex = 0;

                for (int i = 0; i < remainingCoords.Count; i++)
                {
                    var coord = remainingCoords[i];
                    
                    // Find minimum distance to any point in current group
                    float minDistanceToGroup = float.MaxValue;
                    foreach (var groupCoord in currentGroup)
                    {
                        float dist = GetDistance(coord, groupCoord);
                        minDistanceToGroup = Mathf.Min(minDistanceToGroup, dist);
                    }

                    if (minDistanceToGroup < bestDistance)
                    {
                        bestDistance = minDistanceToGroup;
                        bestCoord = coord;
                        bestIndex = i;
                    }
                }

                // Add the closest coordinate to the group
                currentGroup.Add(bestCoord);
                remainingCoords.RemoveAt(bestIndex);
            }

            // Apply label to the group
            string groupLabel = $"GridGroup{groupIndex}";
            foreach (var coord in currentGroup)
            {
                var entry = gridPositions[coord];
                entry.SetLabel(groupLabel, true, true);
            }

            groupIndex++;
        }

        // Save changes
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", $"Grid objects have been grouped into {groupIndex-1} groups", "OK");
    }
} 