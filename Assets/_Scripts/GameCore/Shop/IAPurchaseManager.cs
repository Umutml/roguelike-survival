#if !UNITY_SERVER
#define SUBSCRIPTION_MANAGER
#define UNITY_PURCHASING

using System;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using VContainer;
using Product = UnityEngine.Purchasing.Product;

#if UNITY_PURCHASING

// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
namespace _Scripts.GameCore.Shop
{
    public class IAPurchaseManager : MonoBehaviour, IDetailedStoreListener
#endif
    {
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private PopupManager popupManager;

        public Action<PurchaseOptions> OnSuccess;
        
#if UNITY_PURCHASING
        private IStoreListener _storeListenerImplementation;
        private ConfigurationBuilder _builder;
        
        public ConfigurationBuilder Builder => _builder;
#endif

#if UNITY_PURCHASING
        private IAppleExtensions m_AppleExtensions;

        private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private ITransactionHistoryExtensions m_TransactionHistoryExtensions;
        
        private bool InAppPurchaseInstalled;
        
        private void OnDestroy()
        {
            _onPurchaseSuccess = null;
            _onPurchaseFailed = null;
            OnSuccess = null;
        }


        private void Init()
        {
            InitializePurchasing();
        }
        
        private Action _onPurchaseSuccess;
        private Action _onPurchaseFailed;

        private void Start()
        {
            InitializePurchasing();
        }
        

#endif
        
        public IStoreController getStoreController()
        {
#if UNITY_PURCHASING
            return m_StoreController;
#endif
        }

#if UNITY_PURCHASING

        public void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }

            _builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            var iapProduct = shopManager.GetShopIAPItems();
            foreach (var product in iapProduct)
            {
                _builder.AddProduct(product, ProductType.Consumable);
            }

            UnityPurchasing.Initialize(this, _builder);
        }

        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

#endif
        public void BuyItem(string itemIapName, Action onPurchaseSuccess = null, Action onPurchaseCanceled = null)
        {
#if UNITY_PURCHASING
            _onPurchaseSuccess = onPurchaseSuccess;
            _onPurchaseFailed = onPurchaseCanceled;
            BuyProductID(itemIapName);
#endif
        }
        

#if UNITY_PURCHASING

        public Product GetProduct(string productId)
        {
            if (m_StoreController != null)
            {
                var product = m_StoreController.products.WithID(productId);

                if (product == null)
                {
                    Debug.LogError("<color=red>Could not find a product with product id : </color> " + productId);
                }

                return product;
            }

            return null;
        }

        private void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                    popupManager.OpenPopup(PopupConstants.PopupType.Loading);
                }
                else
                {
                    popupManager.ClosePopup(PopupConstants.PopupType.Loading);
                    _onPurchaseFailed?.Invoke();
                    Debug.LogError("BuyProductID: FAIL. Not purchasing product, " + "either is not found or is not available for purchase");
                }
            }
            else
            {
                popupManager.ClosePopup(PopupConstants.PopupType.Loading);
                _onPurchaseFailed?.Invoke();
                Debug.LogError("<color=red>BuyProductID FAIL. Not initialized.</color>");
            }
        }

        public bool CheckProductHasReceipt(string productId)
        {
            if (!IsInitialized()) return false;
            var product = m_StoreController.products.WithID(productId);
            return product is { hasReceipt: true };
        }
        
        private void OnDeferred(Product item)
        {
            Debug.Log("Purchase deferred: " + item.definition.id);
        }
        

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
#if UNITY_PURCHASING
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            if (m_StoreController != null)
            {
                InAppPurchaseInstalled = true;
                Debug.Log($"<color=green> IAP has installed</color>");
            }
            else
            {
                Debug.LogError("<color=red>IAP NULL</color>");
                Init();
            }

#endif
#if UNITY_PURCHASING && UNITY_IOS
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
            m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
#endif
#if UNITY_PURCHASING && UNITY_ANDROID
            m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
#endif
#if UNITY_PURCHASING
            m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
#endif
            
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason.
            // Consider sharing this reason with the user.
            popupManager.ClosePopup(PopupConstants.PopupType.Loading);
            Debug.LogError("OnInitializeFailed InitializationFailureReason:" + error);
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("Purchase OK: " + args.purchasedProduct.definition.id);
            
            if (args == null || args.purchasedProduct == null || args.purchasedProduct.definition == null || args.purchasedProduct.metadata == null || args.purchasedProduct.receipt == null)
            {
                popupManager.ClosePopup(PopupConstants.PopupType.Loading);
                Debug.LogError("PurchaseEventArgs or its properties are null.");
                return PurchaseProcessingResult.Pending;
            }

            Debug.Log($"Purchase successful: {args.purchasedProduct.definition.id}");
            
            _onPurchaseSuccess?.Invoke();
            OnSuccess?.Invoke(PurchaseOptions.Gem);
            popupManager.ClosePopup(PopupConstants.PopupType.Loading);

            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
            Debug.LogError(failureReason.ToString());
            popupManager.ClosePopup(PopupConstants.PopupType.Loading);
        }

        private void RestorePurchases()
        {
            if (!IsInitialized())
            {
                Debug.LogError("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("RestorePurchases started ...");
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.LogError("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError("FAILED: " + message);
            popupManager.ClosePopup(PopupConstants.PopupType.Loading);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            
        }

#endif
    }
}
#endif
