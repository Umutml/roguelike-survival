using System;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Scriptables;
using UnityEngine;

namespace GameCore.Inventory
{
    public abstract class InventoryBase : MonoBehaviour, IInventoryManager
    {
        #region Public Fields

        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        public abstract void AddItem<T>(T item);
        public abstract bool ContainsItem<T>(T item);
        public abstract void RemoveItem<T>(T item);

        #endregion

        #region Private Fields

        private InventoryItem _inventoryItem;

        #endregion


        #region Unity Methods

        private void Awake()
        {
            Load();
        }

        #endregion

        #region Public Methods

        public void Save()
        {
            if (!SaveLoadHelper.IsDataExists(nameof(InventoryItem)))
            {
                LoggerNS.LogError("Inventory data is not exists");
                return;
            }

            SaveLoadHelper.UpdateData<InventoryItem>(item =>
            {
                item.Coin = _inventoryItem.Coin;
                item.Gem = _inventoryItem.Gem;
            });
        }

        public void Load()
        {
            _inventoryItem = SaveLoadHelper.TryLoadPersistentData<InventoryItem>();

            OnCoinsChanged?.Invoke(_inventoryItem.Coin);
            OnGemsChanged?.Invoke(_inventoryItem.Gem);
        }
        
        public virtual float GetCurrencyBalance(PurchaseOptions purchaseOption)
        {
            return purchaseOption switch
            {
                PurchaseOptions.Coin => _inventoryItem.Coin,
                PurchaseOptions.Gem => _inventoryItem.Gem,
                _ => throw new ArgumentOutOfRangeException(nameof(purchaseOption), purchaseOption, null)
            };
        }

        public virtual bool ModifyCurrencyBalance(PurchaseDetails purchaseDetails)
        {
            switch (purchaseDetails.PurchaseOption)
            {
                case PurchaseOptions.Coin:
                    _inventoryItem.Coin += purchaseDetails.Price;
                    OnCoinsChanged?.Invoke(_inventoryItem.Coin);
                    break;
                case PurchaseOptions.Gem:
                    _inventoryItem.Gem += purchaseDetails.Price;
                    OnGemsChanged?.Invoke(_inventoryItem.Gem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(purchaseDetails.PurchaseOption),
                        purchaseDetails,
                        null);
            }

            Save();
            return true;
        }

        public virtual bool PurchaseItem(PurchaseDetails purchaseDetails)
        {
            if (!CanModifyResource(purchaseDetails))
            {
                return false;
            }

            switch (purchaseDetails.PurchaseOption)
            {
                case PurchaseOptions.Coin:
                    _inventoryItem.Coin -= purchaseDetails.Price;
                    OnCoinsChanged?.Invoke(_inventoryItem.Coin);
                    break;
                case PurchaseOptions.Gem:
                    _inventoryItem.Gem -= purchaseDetails.Price;
                    OnGemsChanged?.Invoke(_inventoryItem.Gem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(purchaseDetails.PurchaseOption),
                        purchaseDetails,
                        null);
            }

            Save();
            return true;
        }

        public virtual bool CanModifyResource(PurchaseDetails purchaseDetails)
        {
            return (purchaseDetails.PurchaseOption == PurchaseOptions.Coin
                ? _inventoryItem.Coin
                : _inventoryItem.Gem) >= purchaseDetails.Price;
        }

        #endregion
    }

    public class InventoryItem
    {
        public int Coin = 260;
        public int Gem;
    }
}