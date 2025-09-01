using GameCore.PopupSystem;
using GameCore.Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Game.InGame.ObjectFoundPopup
{
    public class ObjectFoundPopup : Popup
    {
        #region Serializable Fields

        [SerializeField] private Transform content;
        [SerializeField] private FoundedObjectResources foundedObjectResources;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image objectImage;

        #endregion


        #region Fields

        private FoundedObject _foundedObject;

        #endregion


        #region Public Methods

        public override void OnOpenPopup()
        {
        }

        public override void Initialize(object data)
        {
            base.Initialize(data);

            if (data is not FoundedObjectType foundedObjectType)
            {
                return;
            }

            InitializeFoundedObject(foundedObjectType);
        }

        #endregion


        #region Private Methods

        private void InitializeFoundedObject(FoundedObjectType targetType)
        {
            _foundedObject = foundedObjectResources.GetFoundedObject(targetType);

            SetUIElements();
        }

        private void SetUIElements()
        {
            titleText.text = _foundedObject.title;
            descriptionText.text = _foundedObject.description;
            objectImage.sprite = _foundedObject.icon;
            content.gameObject.SetActive(true);
        }

        #endregion
    }
}