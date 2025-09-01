using System;
using Cysharp.Threading.Tasks;
using GameCore.Scriptables;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;

namespace UI.Game.InGame.DropIncrement
{
    public class DropIncrementController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private SpriteDatabase spriteDatabase;

        #endregion

        #region Private Fields

        private bool _isInitialized;
        private DropIncrementManager _dropIncrementManager;

        #endregion


        [Inject]
        private void Initialize(DropIncrementManager dropIncrementManager)
        {
            _dropIncrementManager = dropIncrementManager;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_isInitialized)
            {
                return;
            }

            _dropIncrementManager.OnDropIncrementItem += SetupIncrementController;

            _isInitialized = true;
        }

        private async void SetupIncrementController(GameObject item, Tuple<int, DropPodType> dropData)
        {
            if (!item.TryGetComponent<DropIncrementUI>(out var dropIncrementUI))
            {
                return;
            }

            var sprite = await spriteDatabase.GetSpriteByValueAndType(dropData);
            dropIncrementUI.Initialize(sprite, dropData.Item1);
        }
    }
}