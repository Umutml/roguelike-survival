using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "MinimapObject", menuName = "Minimap/MinimapObject")]
public class MinimapObject : ScriptableObject
{
    public string minimapName;
    public float heightMin;
    public float heightMax;
    public float widthMin;
    public float widthMax;
    public AssetReference[] minimapParts;
    public MinimapCursor[] minimapCursors;

    public void CreateMinimap(Camera mainCamera, AssetReference[] minimapImages)
    {
        minimapName = SceneManager.GetActiveScene().name;
        minimapParts = minimapImages;
        heightMin = mainCamera.transform.position.z - mainCamera.orthographicSize;
        heightMax = mainCamera.transform.position.z + mainCamera.orthographicSize;
        widthMin = mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect;
        widthMax = mainCamera.transform.position.x + mainCamera.orthographicSize * mainCamera.aspect;
    }
}
[Serializable]
public struct MinimapCursor
{
    public string objectTag;
    public int cursorCount;
    public int punchAnimationCount;
    public bool alwaysOnDisplay;
    public bool multipleCursor;
    public AssetReferenceGameObject cursorImage;
}
public enum MinimapType
{
    Off,
    Circle,
    Square
}
