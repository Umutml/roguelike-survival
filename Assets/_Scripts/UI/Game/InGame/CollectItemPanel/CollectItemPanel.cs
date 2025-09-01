using GameCore.Player;
using GameCore.Spawner;
using TMPro;
using UnityEngine;
using VContainer;

namespace UI.Game.InGame.CollectItemPanel
{
    public class CollectItemPanel : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TMP_Text scrapCountText;

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(PlayerCollectItemController playerCollectItemController)
        {
            playerCollectItemController.OnCollectItem += OnCollectItem;
        }

        private void OnCollectItem(CollectableItemType type, float count)
        {
            scrapCountText.text = $"<sprite=16/> {count}";
        }

        #endregion
    }
}
