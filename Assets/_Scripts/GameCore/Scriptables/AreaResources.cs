using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.GameCore.NPC;
using GameCore.PopupSystem;
using RootMotion;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "AreaResources", menuName = "ScriptableObjects/AreaResources", order = 1)]
    public class AreaResources : ScriptableObject
    {
        [SerializeField] private List<Area> areaList = new();
        
        public List<Area> AreaList => areaList;
        public Area GetArea(AreaNpcType areaNpcType) => areaList.FirstOrDefault(area => area.AreaNpcType.Equals(areaNpcType));
    }


    [Serializable]
    public struct Area
    {
        [SerializeField] private AreaNpcType areaNpcType;
        [SerializeField] private bool isPopupOpening;
        [SerializeField] private bool afterTutorialLock;
        [SerializeField] private bool inTutorialLock;
        [ShowIf(nameof(isPopupOpening), true)] [SerializeField] private PopupConstants.PopupType openingPopupType;
        [SerializeField] private string areaName;
        [SerializeField] private string eventParameter;
        [SerializeField] private bool isNpcModel;
        [ShowIf(nameof(isNpcModel), true)][SerializeField] private string npcModelKey;
        
        
        public AreaNpcType AreaNpcType => areaNpcType;
        public bool AfterTutorialLock => afterTutorialLock;
        public bool InTutorialLock => inTutorialLock;
        public PopupConstants.PopupType OpeningPopupType => openingPopupType;
        public string AreaName => areaName;
        public string EventParameter => eventParameter;
        public bool IsNpcModel => isNpcModel;
        public string NpcModelKey => npcModelKey;
    }
}

