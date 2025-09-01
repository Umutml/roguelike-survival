using System.Collections.Generic;
using GameCore.CollectibleItem;
using UnityEngine;
using System;

namespace GameCore.Spawner
{
    public class CollectableItemManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private List<CollectableItemController> collectableItemController = new();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            //Initialize();
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            if (collectableItemController is not {Count: > 0}) { return; }

            collectableItemController.ForEach(item => item.Initialize());
        }

        #endregion
    }

    [Serializable]
    public struct CollectableItemData
    {
        public List<CollectableItem> collectableItems;
    }

    [Serializable]
    public class CollectableItem
    {
        public CollectableItemType type;
        public int count;
    }

    public enum CollectableItemType
    {
        Scrap
    }
}
