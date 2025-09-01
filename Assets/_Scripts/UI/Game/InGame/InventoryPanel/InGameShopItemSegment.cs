using GameCore.Scriptables;
using UI.Game.Architectural;

namespace UI.Game.InGame.InventoryPanel
{
    public class InGameShopItemSegment : Content
    {
        private const string WeaponNameText = "WeaponNameText";
        private const string PriceArea = "PriceArea";
        private const string PriceText = "PriceText";

        private (PostMatchItem item, PurchaseDetails details) _itemDetails;
        private InGameInventoryPanel _inventoryPanel;

        public async void Initialize((PostMatchItem, PurchaseDetails) itemDetails, InGameInventoryPanel inventoryPanel)
        {
            _itemDetails = itemDetails;
            _inventoryPanel = inventoryPanel;
            var sprite = await inventoryPanel.GetSprite(itemDetails.Item2);
            SetText(WeaponNameText, itemDetails.Item1.name);
            SetText(PriceText, FormatPriceText(itemDetails.Item2.Price));
            ConfigureBuyButton();

            _inventoryPanel.OnPurchaseItem += _ => ConfigureBuyButton();
        }

        private void ConfigureBuyButton()
        {
            var buyButton = GetButton(PriceArea);
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = _inventoryPanel.IsPurchasePossible(_itemDetails);
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        private void OnBuyButtonClicked()
        {
            _inventoryPanel.PurchaseItem(_itemDetails);
            GetButton(PriceArea).interactable = false;
        }

        private string FormatPriceText(int price)
        {
            return $"{price}";
        }
    }
}