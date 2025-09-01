using System;
using GameCore.Scriptables;

namespace GameCore.Inventory
{
    public interface IInventoryManager
    {
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;
        public void Save();
        public void Load();
        public float GetCurrencyBalance(PurchaseOptions purchaseOption);
        public bool ModifyCurrencyBalance(PurchaseDetails purchaseDetails);
        public bool PurchaseItem(PurchaseDetails purchaseDetails);
        public void AddItem<T>(T item);
        public bool ContainsItem<T>(T item);
        public void RemoveItem<T>(T item);
        public bool CanModifyResource(PurchaseDetails purchaseDetails);
    }
}