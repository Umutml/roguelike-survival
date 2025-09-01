using System;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.Spawner;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Utilities;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "SpriteDatabase", menuName = "ScriptableObjects/SpriteDatabase")]
    public class SpriteDatabase : ScriptableObject
    {
        public SpriteEntry[] spriteEntries;

        public UniTask<Sprite> GetSpriteByType(SpriteType spriteType)
        {
            var spriteEntry = spriteEntries.Gen().Where(x => x.spriteType == spriteType).FirstOrDefault() ??
                              spriteEntries.Gen().First();
            return AssetManager<Sprite>.LoadObject(spriteEntry.spriteReference);
        }

        public UniTask<Sprite> GetSpriteByValueAndType(Tuple<int, DropPodType> dropData)
        {
            var spriteType = dropData.Item2 switch
            {
                DropPodType.Xp => dropData.Item1 > 1 ? SpriteType.Xps : SpriteType.Xp,
                DropPodType.Gem => dropData.Item1 > 1 ? SpriteType.Gems : SpriteType.Gem,
                _ => dropData.Item1 > 1 ? SpriteType.Coins : SpriteType.Coin,
            };

            return GetSpriteByType(spriteType);
        }

        public UniTask<Sprite> GetSpriteByType(DropPodType dropPodType)
        {
            var type = dropPodType switch
            {
                DropPodType.Gem => SpriteType.Gem,
                DropPodType.Xp => SpriteType.Xp,
                _ => SpriteType.Coin
            };

            return GetSpriteByType(type);
        }
    }

    [Serializable]
    public class SpriteEntry
    {
        [FormerlySerializedAs("sprite")] public AssetReference spriteReference;
        public SpriteType spriteType;
    }

    public enum SpriteType
    {
        Coin,
        Gem,
        Xp,
        Star,
        Scrap,
        Coins,
        Gems,
        Xps,
        NpcDialogRadio,
        NpcDialogSheriff,
        NpcDialogSoldier,
        NpcDialogHattori
    }
}