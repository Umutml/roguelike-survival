using System;
using UI.Game.Architectural;
using UnityEngine;

namespace UI.Game.InGame.DropIncrement
{
    public class DropIncrementUI : Content
    {
        #region Private Fields

        private readonly int DropIncrementKey = Animator.StringToHash("DropIncrement");
        private const string ItemValue = "ItemValue";
        private const string ItemSprite = "ItemSprite";

        private Animator _animator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        #endregion

        #region Public Methods

        public void Initialize(Sprite sprite, int dropAmount)
        {
            SetImage(ItemSprite, sprite);
            SetText(ItemValue, $"+{dropAmount}");
            _animator.SetTrigger(DropIncrementKey);
        }

        public void CompleteAnimation()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}