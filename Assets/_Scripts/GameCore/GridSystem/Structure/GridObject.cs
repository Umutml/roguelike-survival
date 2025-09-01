using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GridObject", menuName = "Grid/GridObject")]
public class GridObject : ScriptableObject
{
    public string gridName;
    public int minGridSize, maxGridSize;
    public List<Grid> gridParts=new List<Grid>();

    public void AddGrid(Vector3 gridPosition, AssetReferenceGameObject gridAsset)
    {
        gridParts.Add(new Grid(gridAsset,gridPosition));
    }
}

[Serializable]
public class Grid
{
    public AssetReferenceGameObject gridAsset;
    public Vector3 gridAssetPosition;
    public Grid(AssetReferenceGameObject gridAsset, Vector3 gridAssetPosition)
    {
        this.gridAsset = gridAsset;
        this.gridAssetPosition = gridAssetPosition;
    }

    public bool IsInView(Vector3 position,float viewDistance)
    {
        position.y = gridAssetPosition.y;
        return Vector3.Distance(position, gridAssetPosition) < viewDistance;
    }
}
