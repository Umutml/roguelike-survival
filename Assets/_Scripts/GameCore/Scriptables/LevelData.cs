using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        public LevelDetails[] levels;
    }
}

[System.Serializable]
public struct LevelDetails
{
    public string name;
    public int level;
    public int expPodToUnlock;
}
