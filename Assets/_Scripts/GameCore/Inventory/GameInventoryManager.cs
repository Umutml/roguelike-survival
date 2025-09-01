using System;
using System.Collections.Generic;
using _Scripts.GameCore.NPC;
using Cathei.LinqGen;
using GameCore.Health;
using GameCore.Player;
using GameCore.Scriptables;
using UnityEngine;
using VContainer;

namespace GameCore.Inventory
{
    public class GameInventoryManager : InventoryBase
    {
        #region Serialized Fields

        [SerializeField] private PostMatchStore postMatchStore;

        #endregion

        #region Private Fields

        private PlayerStatusController _playerStatusController;
        private PlayerSkillController _playerSkillController;
        private ManagementNpcController _managementNpcController;
        private readonly List<PostMatchItem> _purchasedItems = new();

        #endregion

        #region Properties

        public PostMatchStore PostMatchStore => postMatchStore;
        public int PodCount => _playerStatusController.PurchasePodCount;
        public int WaveCoinAmount { get; private set; }
        public int WaveGemAmount { get; private set; }
        public int GemCount { get; private set; }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(PlayerStatusController playerStatusController,
            PlayerSkillController playerSkillController, ManagementNpcController managementNpcController)
        {
            _playerStatusController = playerStatusController;
            _playerSkillController = playerSkillController;
            _managementNpcController = managementNpcController;
            SubscribeToEvents();
        }

        #endregion

        #region Unity Methods

        private void Start()
        {
            _purchasedItems.Add(postMatchStore.postMatchItems.Gen().Where(item => item.itemName == "Pistol")
                .FirstOrDefault()); // refactore this line!
        }

        #endregion

        #region Public Methods

        public override bool ModifyCurrencyBalance(PurchaseDetails purchaseDetails)
        {
            if (!_managementNpcController.IsProgress)
            {
                return base.ModifyCurrencyBalance(purchaseDetails);
            }

            switch (purchaseDetails.PurchaseOption)
            {
                case PurchaseOptions.Coin:
                    WaveCoinAmount += purchaseDetails.Price;
                    break;
                case PurchaseOptions.Gem:
                    WaveGemAmount += purchaseDetails.Price;
                    break;
                case PurchaseOptions.XpPod:
                    break;
            }

            return base.ModifyCurrencyBalance(purchaseDetails);
        }

        public override void AddItem<T>(T item)
        {
            var postMatchItem = item as PostMatchItem;
            if (postMatchItem == null)
            {
                return;
            }

            if (postMatchItem.itemType == PostMatchItemType.Skill)
            {
                ApplySkillUpgrade(postMatchItem);
            }
            else
            {
                ApplyWeaponUsage(postMatchItem);
            }

            _purchasedItems.Add(postMatchItem);
        }

        public override bool ContainsItem<T>(T item)
        {
            return item as PostMatchItem != null && _purchasedItems.Contains(item as PostMatchItem);
        }

        public override void RemoveItem<T>(T item)
        {
            var postMatchItem = item as PostMatchItem;
            if (postMatchItem == null)
            {
                return;
            }

            if (postMatchItem.itemType == PostMatchItemType.Weapon)
            {
                ApplyWeaponUsage(postMatchItem, false);
            }

            _purchasedItems.Remove(item as PostMatchItem);
        }

        public override bool PurchaseItem(PurchaseDetails purchaseDetails)
        {
            if (purchaseDetails.PurchaseOption != PurchaseOptions.XpPod)
            {
                return base.PurchaseItem(purchaseDetails);
            }

            if (!CanModifyResource(purchaseDetails))
            {
                return false;
            }

            _playerStatusController.AdjustPurchasePodValue(-purchaseDetails.Price);
            return true;
        }


        public override bool CanModifyResource(PurchaseDetails purchaseDetails)
        {
            if (purchaseDetails.PurchaseOption != PurchaseOptions.XpPod)
            {
                return base.CanModifyResource(purchaseDetails);
            }

            return _playerStatusController.PurchasePodCount >= purchaseDetails.Price;
        }

        public List<PostMatchItem> GetWeaponItems()
        {
            return _purchasedItems?.Gen().Where(item => item.itemType == PostMatchItemType.Weapon).ToList();
        }

        public List<PostMatchItem> GetInventoryItems()
        {
            return _purchasedItems?.Gen().Where(item => item.itemType == PostMatchItemType.Skill).ToList();
        }

        #endregion

        #region Private Methods

        private void SubscribeToEvents()
        {
            _playerSkillController.OnResetSkill += ResetWaveCurrency;
        }

        private void ResetWaveCurrency()
        {
            WaveCoinAmount = 0;
            WaveGemAmount = 0;
        }

        private void ApplySkillUpgrade(PostMatchItem item)
        {
            _playerSkillController.ApplyStatUpgrade(item.skillDetail.skillCategories.Gen().First()
                .upgradeDetails); // refactore this line because of the First() method
        }

        private void ApplyWeaponUsage(PostMatchItem item, bool isAdded = true)
        {
        }

        #endregion
    }
}