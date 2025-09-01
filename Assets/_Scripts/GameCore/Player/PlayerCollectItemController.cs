using System;
using System.Collections.Generic;
using _Utilities;
using Cathei.LinqGen;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace GameCore.Player
{
    public class PlayerCollectItemController : MonoBehaviour
    {
        #region Actions

        public event Action<CollectableItemType, float> OnCollectItem;

        #endregion

        #region Private Fields

        private LootDropManager _lootDropManager;

        #endregion

        #region Public Methods

        public void CollectItem(ICollectableItem item)
        {
            var collectableItemData = GetCollectableItemData();
            var randomItem = GetRandomItem();

            if (collectableItemData.collectableItems is not { Count: > 0 })
            {
                collectableItemData.collectableItems = new List<CollectableItem>();
            }

            var data = collectableItemData.collectableItems.Gen().Where(x => x.type == randomItem.Item1)
                .FirstOrDefault() ?? new CollectableItem { type = randomItem.Item1 };

            if (!collectableItemData.collectableItems.Contains(data))
            {
                collectableItemData.collectableItems.Add(data);
            }

            data.count += randomItem.Item2;

            OnCollectItem?.Invoke(randomItem.Item1, data.count);
            //  _lootDropManager.DropIncrementItem(item.Transform.gameObject, randomItem.Item2, DropPodType.Scrap);

            SaveLoadHelper.SaveData(collectableItemData);
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(LootDropManager lootDropManager)
        {
            _lootDropManager = lootDropManager;
        }

        private (CollectableItemType, int) GetRandomItem()
        {
            return (CollectableItemType.Scrap, Random.Range(1, 5));
        }

        private CollectableItemData GetCollectableItemData()
        {
            return SaveLoadHelper.TryLoadRuntimeData<CollectableItemData>();
        }

        #endregion
    }
}