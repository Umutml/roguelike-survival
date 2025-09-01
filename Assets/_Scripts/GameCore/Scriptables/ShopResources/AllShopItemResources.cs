using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "AllShopItemResources", menuName = "ScriptableObjects/Shop/AllShopItemResources", order = 2)]
    public class AllShopItemResources : ScriptableObject
    {
        [SerializeField] private List<ShopItemResources> shopItemResources = new();
        
        public List<ShopItemResources> ShopItemResources => shopItemResources;
    }
}

