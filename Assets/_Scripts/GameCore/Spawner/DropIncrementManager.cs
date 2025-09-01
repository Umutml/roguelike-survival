using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Spawner
{
    public class DropIncrementManager : MonoBehaviour
    {
        #region Public Events

        public event Action<GameObject, Tuple<int, DropPodType>> OnDropIncrementItem;

        #endregion

        #region Serialized Fields

        [SerializeField] private AssetReferenceGameObject dropIncrementPrefab;

        #endregion

        #region Private Fields

        private readonly Vector3 DropIncrementOffset = new(0, 3f, 0);

        #endregion

        #region Public Methods

        public async void DropIncrementItem(Vector3 position, int value, DropPodType type)
        {
            var dropIncrement = await ObjectManager.GetObject(dropIncrementPrefab, position + DropIncrementOffset,
                Quaternion.identity).BindTo(gameObject, dropIncrementPrefab);
            OnDropIncrementItem?.Invoke(dropIncrement, new Tuple<int, DropPodType>(value, type));
        }

        #endregion
    }
}