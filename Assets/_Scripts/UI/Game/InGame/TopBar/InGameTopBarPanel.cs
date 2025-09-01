using GameCore.Inventory;
using GameCore.Wave;
using UI.Game.Architectural;
using VContainer;

namespace UI.Game.InGame.TopBar
{
    public class InGameTopBarPanel : Content
    {
        private const string COIN_AMOUNT_TEXT = "CoinAmountText";
        private const string GEM_AMOUNT_TEXT = "GemAmountText";

        private IInventoryManager _inventoryManager;
        private WaveManager _waveManager;

        [Inject]
        private void Initialize(WaveManager waveManager, IInventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
            _waveManager = waveManager;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _inventoryManager.OnCoinsChanged += SetCoinText;
            _inventoryManager.OnGemsChanged += SetGemText;
            //_waveManager.OnWaveUIInitialized += _ => SetActivity(true);
            _waveManager.WaveUpdated += _ => SetActivity(false);
        }

        private void SetGemText(int gemAmount)
        {
            SetText(GEM_AMOUNT_TEXT, gemAmount.ToString());
        }

        private void SetCoinText(int coinAmount)
        {
            SetText(COIN_AMOUNT_TEXT, coinAmount.ToString());
        }

        private void SetActivity(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
