using UnityEngine;
using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine.AddressableAssets;
using VContainer;

namespace GameCore.Spawner
{
    public class LootDropManager : MonoBehaviour
    {
        #region Actions

        public event Action<DropPodType, int> OnTopBarAnimationStart;

        #endregion

        #region Serialized Fields

        [SerializeField] private List<LootDrop> lootDrops;

        #endregion

        #region Private Fields

        private IObjectResolver _resolver;

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(IObjectResolver objectResolver)
        {
            _resolver = objectResolver;
        }

        #endregion

        #region Public Methods

        public void StartTopBarAnimation(DropPodType dropPodType, int count)
        {
            OnTopBarAnimationStart?.Invoke(dropPodType, count);
        }


        public async UniTask<GameObject> GetDropObject(DropPodType dropPodType, Vector3? spawnPosition = null,
            Quaternion? spawnRotation = null)
        {
            var lootDrop = lootDrops.Gen().Where(x => x.type == dropPodType).FirstOrDefault();
            var dropObject = await ObjectManager.GetObject(lootDrop.instance, spawnPosition, spawnRotation)
                .BindTo(gameObject, lootDrop.instance);
            if (dropObject.TryGetComponent(out IDropItem dropItem))
            {
                dropItem.Resolver = _resolver;
            }

            dropObject.SetActive(true);
            return dropObject;
        }

        #endregion
    }

    [Serializable]
    public struct LootDrop
    {
        public AssetReferenceGameObject instance;
        public DropPodType type;
    }

    public enum DropPodType
    {
        Xp,
        Gem,
        Health,
        Coin,
        Box,
        Bomb,
        Scrap,
        Weapon,
        Armor,
        Energy,
    }
}