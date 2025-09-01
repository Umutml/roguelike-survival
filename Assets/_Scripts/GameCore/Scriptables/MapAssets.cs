using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "MapAssets", menuName = "ScriptableObjects/MapAssets", order = 0)]
    public class MapAssets : ScriptableObject
    {
        #region Serializable Fields

        [SerializeField] private AssetReference[] mapReferences;

        #endregion

        #region Properties

        public AssetReference[] Maps
        {
            get => mapReferences;
            set => mapReferences = value;
        }

        #endregion
    }
}
