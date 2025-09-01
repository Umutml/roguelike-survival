using System;
using System.Collections.Generic;
using GameCore.PopupSystem;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Utilities;


namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "PopupResources", menuName = "Scriptables/PopupResources", order = 1)]
    public class PopupResources : ScriptableObject
    {
        [SerializeField] private List<PanelData> popupList = new();
        
        public PanelData GetPanelData(PopupConstants.PopupType popupType)
        {
            return popupList.FirstOrDefault(panel => panel.PopupType.Equals(popupType));
        }

        public async UniTask<GameObject> GetPopup(PopupConstants.PopupType popupType)
        {
            var popup = popupList.FirstOrDefault(panel => panel.PopupType.Equals(popupType)).Popup;
            return await AssetManager<GameObject>.LoadObject(popup);
        }
        
        public AssetReference GetPopupReference(PopupConstants.PopupType popupType)
        {
            var popup = popupList.FirstOrDefault(panel => panel.PopupType.Equals(popupType)).Popup;
            return popup;
        }
    }


    [Serializable]
    public struct PanelData
    {
        [SerializeField] private PopupConstants.PopupType popupType;
        [SerializeField] private AssetReference popup;
        [SerializeField] private bool isTopBarShow;

        public PopupConstants.PopupType PopupType => popupType;
        public AssetReference Popup => popup;
        public bool IsTopBarShow => isTopBarShow;
    }
}