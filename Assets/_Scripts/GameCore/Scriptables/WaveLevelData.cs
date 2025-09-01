using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "WaveLevelData", menuName = "ScriptableObjects/WaveLevelData", order = 0)]
    public class WaveLevelData : ScriptableObject
    {
        public LevelDetails[] levels;
    }

}
