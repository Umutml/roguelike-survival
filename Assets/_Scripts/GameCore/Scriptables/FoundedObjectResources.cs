using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "FoundedObjectResources", menuName = "ScriptableObjects/FoundedObjectResources",
        order = 1)]
    public class FoundedObjectResources : ScriptableObject
    {
        [SerializeField] private List<FoundedObject> foundedObjectList = new();

        public FoundedObject GetFoundedObject(FoundedObjectType type) =>
            foundedObjectList.Find(x => x.type.Equals(type));
    }


    [Serializable]
    public struct FoundedObject
    {
        public FoundedObjectType type;
        public string title;
        public string description;
        public Sprite icon;
    }


    public enum FoundedObjectType
    {
        None,
        Radio,
        Map,
    }
}