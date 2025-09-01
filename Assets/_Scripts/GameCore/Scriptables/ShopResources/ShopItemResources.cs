using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RootMotion;
using UnityEngine.AddressableAssets;
using Utilities;


namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "ShopItemResources", menuName = "ScriptableObjects/Shop/ShopItemResources", order = 2)]
    public class ShopItemResources : ScriptableObject
    {
        [SerializeField] private string itemTitle;
        [SerializeField] private string itemDescription;
        [SerializeField] private bool enable = true;
        [SerializeField] private string eventParameter;
        [SerializeField] private AssetReference itemReference;
        [SerializeField] private AssetReference segmentReference;
        [SerializeField] private List<Slot> slots = new();
    
        
        public string ItemTitle => itemTitle;
        public string ItemDescription => itemDescription;
        public bool Enable => enable;
        public string EventParameter => eventParameter;
        public async UniTask<GameObject> GetItemReference() => await AssetManager<GameObject>.LoadObject(itemReference);
        public async UniTask<GameObject> GetSegmentReference() => await AssetManager<GameObject>.LoadObject(segmentReference);
        public List<Slot> Slots => slots;
    }
    
    
    [Serializable]
    public struct Slot
    {
        [SerializeField] private List<ShopItem> shopItemList;
        
        public List<ShopItem> ShopItemList => shopItemList;
    }


    [Serializable]
    public struct ShopItem
    {
        [SerializeField] private int productAmount;
        [SerializeField] private int bonusAmount;
        [SerializeField] private float productPrice;
        [SerializeField] private string eventParameter;
        [SerializeField] private string productID;
        [SerializeField] private AssetReference productImage;
        [SerializeField] private PurchaseOptions productType;
        [ShowIf(nameof(productType), PurchaseOptions.Chest)] [SerializeField] private ChestType chestType;
        [SerializeField] private PurchaseOptions purchaseOptions;
        [SerializeField] private int dailyStock;
        
        
        public int ProductAmount => productAmount;
        public int BonusAmount => bonusAmount;
        public float ProductPrice => productPrice;
        public string EventParameter => eventParameter;
        public string ProductID => productID;
        public async UniTask<Sprite> GetProductImage() => await AssetManager<Sprite>.LoadObject(productImage);
        public PurchaseOptions ProductType => productType;
        public PurchaseOptions PurchaseOptions => purchaseOptions;
        public int DailyStock => dailyStock;
    }
}


