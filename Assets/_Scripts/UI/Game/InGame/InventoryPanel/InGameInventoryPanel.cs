using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using UI.Game.Architectural;
using UI.Game.InGame.InventoryPanel.Constants;
using UnityEngine;
using VContainer;

namespace UI.Game.InGame.InventoryPanel
{
    public class InGameInventoryPanel : Content
    {
        #region Public Fields

        public event Action<(PostMatchItem, PurchaseDetails)> OnPurchaseItem;

        #endregion

        #region Serialized Fields

        [SerializeField] private SpriteDatabase spriteDatabase;
        [SerializeField] private List<InGameWeaponSegment> weaponSegments;
        [SerializeField] private List<InGameInventoryItemSegment> inventoryItemSegments;
        [SerializeField] private List<InGameShopItemSegment> shopItemSegments;

        #endregion

        #region Private Fields

        private PlayerStatusController _playerStatusController;
        private GameInventoryManager _gameInventoryManager;
        private IGameService _gameService;
        private WaveManager _waveManager;
        private IObjectResolver _resolver;

        private bool _isInitialized;

        #endregion

        #region Unity Methods

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(WaveManager waveManager, IGameService gameService,
            PlayerStatusController playerStatusController, IInventoryManager inventoryManager, IObjectResolver resolver)
        {
            _gameInventoryManager = inventoryManager as GameInventoryManager;
            _waveManager = waveManager;
            _gameService = gameService;
            _playerStatusController = playerStatusController;
            _resolver = resolver;
            
            SubscribeToEvents();
            SetupInventoryPanel();
        }

        private void SubscribeToEvents()
        {
            if (_isInitialized) return;

            //_waveManager.OnWaveUIInitialized += SetupInventoryPanel;
            _playerStatusController.PurchasePodCountChanged += SetPurchasePodCountText;
            _isInitialized = true;
        }

        private void SetupInventoryPanel()
        {
            OnClickListen(InGameInventoryPanelConstants.START_BUTTON, StartButtonAction, _resolver);
        }

        private void SetPurchasePodCountText(int podCount)
        {
            SetText(InGameInventoryPanelConstants.POD_COUNT_TEXT, podCount.ToString());
        }

        private void SetupInventoryPanel(Wave wave)
        {
            SetActivity(true);
            SetupWeaponSegments();
            SetupInventoryItemSegments();
            SetupShopItemSegments();
        }

        private void SetupWeaponSegments()
        {
            var weapons = _gameInventoryManager.GetWeaponItems() ?? new List<PostMatchItem>();

            for (var i = 0; i < weaponSegments.Count; i++)
                weaponSegments[i].Initialize(i < weapons.Count ? weapons[i] : null);
        }

        private void SetupInventoryItemSegments()
        {
            var inventoryItems = _gameInventoryManager.GetInventoryItems() ?? new List<PostMatchItem>();

            for (var i = 0; i < inventoryItemSegments.Count; i++)
                inventoryItemSegments[i].Initialize(i < inventoryItems.Count ? inventoryItems[i] : null);
        }

        private void SetupShopItemSegments()
        {
            var shopItems = _gameInventoryManager.PostMatchStore.GetRandomPurchaseItems(shopItemSegments.Count,
                _gameInventoryManager.PodCount) ?? new List<(PostMatchItem, PurchaseDetails)>();

            for (var i = 0; i < shopItemSegments.Count; i++)
                shopItemSegments[i].Initialize(i < shopItems.Count ? shopItems[i] : (null, null), this);
        }

        private void StartButtonAction()
        {
            SetActivity(false);
            _waveManager.IncreaseWaveIndex();
        }

        private void SetActivity(bool isActive)
        {
            gameObject.SetActive(isActive);
            if (isActive)
                _gameService.PauseGame();
            else
                _gameService.ResumeGame();
        }

        #endregion

        #region Public Methods

        public void PurchaseItem((PostMatchItem, PurchaseDetails) item)
        {
            if (!IsPurchasePossible(item)) return;

            _gameInventoryManager.PurchaseItem(item.Item2);
            _gameInventoryManager.AddItem(item.Item1);

            SetupWeaponSegments();
            SetupInventoryItemSegments();

            OnPurchaseItem?.Invoke(item);
        }

        public bool IsPurchasePossible((PostMatchItem, PurchaseDetails) item)
        {
            return _gameInventoryManager.CanModifyResource(item.Item2) &&
                   !_gameInventoryManager.ContainsItem(item.Item1);
        }

        public UniTask<Sprite> GetSprite(PurchaseDetails purchaseDetails)
        {
            var spriteType = purchaseDetails.PurchaseOption switch
            {
                PurchaseOptions.Coin => SpriteType.Coin,
                PurchaseOptions.Gem => SpriteType.Gem,
                PurchaseOptions.XpPod => SpriteType.Xp,
                _ => throw new ArgumentOutOfRangeException()
            };

            return spriteDatabase.GetSpriteByType(spriteType);
        }

        #endregion
    }
}