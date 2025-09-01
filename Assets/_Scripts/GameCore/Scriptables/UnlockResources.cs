using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "UnlockResources", menuName = "ScriptableObjects/Unlock Resources", order = 1)]
    public class UnlockResources : ScriptableObject
    {
        [SerializeField] private List<UnlockObject> unlockObjectList = new();

        public UnlockObject GetUnlockObject(UnlockObjectType type) =>
            unlockObjectList.Find(x => x.type.Equals(type));
    }

    [Serializable]
    public struct UnlockObject
    {
        public string popupTitle;
        public string title;
        public string description;
        public bool hasModel;

        [ConditionalField(nameof(hasModel), true)]
        public Sprite icon;

        [ConditionalField(nameof(hasModel), false)]
        public AssetReference model;

        [ConditionalField(nameof(hasModel), false)]
        [Range(1, 100)]
        public int modelSizeMultiplier;

        [ConditionalField(nameof(hasModel), false)]
        public Vector3 modelOffset;

        public UnlockObjectType type;


        public async UniTask<GameObject> GetModel() => await AssetManager<GameObject>.LoadObject(model);
    }

    public enum UnlockObjectType
    {
        Hattori,
        Shotgun,
        Buggy,
        Suv,
        HotRod,
        Monster,
        Ute,
    }
}