using Addler.Runtime.Core.LifetimeBinding;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.PopupSystem;
using GameCore.Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace UI.Game.InGame.UnlockPopup
{
    public class UnlockPopup : Popup, IPointerEnterHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Transform content;
        [SerializeField] private UnlockResources unlockResources;
        [SerializeField] private TMP_Text popupTitle;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image objectImage;
        [SerializeField] private RawImage modelVisual;

        private UnlockObject _unlockObject;
        private ModelVisualManager _modelVisualManager;

        private Transform _modelVisualParent;
        private Tween _rotateTween;
        private bool _isDragging;
        private float _lastMousePositionX;
        private const float RotationSpeed = 0.5f;

        public override void OnOpenPopup()
        {
        }

        public override void Initialize(object data)
        {
            base.Initialize(data);

            if (data is not UnlockObjectType unlockObjectType)
            {
                return;
            }

            InitializeUnlockObject(unlockObjectType);
        }

        private void InitializeUnlockObject(UnlockObjectType unlockObjectType)
        {
            _unlockObject = unlockResources.GetUnlockObject(unlockObjectType);
            InitializeModel();
            SetUIElements();
        }

        private void SetUIElements()
        {
            popupTitle.text = _unlockObject.popupTitle;
            titleText.text = _unlockObject.title;
            descriptionText.text = _unlockObject.description;
            if (!_unlockObject.hasModel)
            {
                objectImage.sprite = _unlockObject.icon;
            }

            content.gameObject.SetActive(true);
        }

        private async void InitializeModel()
        {
            if (!_unlockObject.hasModel)
            {
                return;
            }

            var model = await Addressables.LoadAssetAsync<GameObject>(_unlockObject.model).BindTo(gameObject);

            _modelVisualManager = Resolver.Resolve<ModelVisualManager>();
            _modelVisualManager.SetupModelVisual(model, _unlockObject.modelOffset,
                Vector3.one * _unlockObject.modelSizeMultiplier);
            _modelVisualParent = _modelVisualManager.ModelVisualContent;
            ClosePopupAction += _modelVisualManager.ReleaseCarRenderTexture;
            modelVisual.texture = _modelVisualManager.RenderTexture;
            _rotateTween = RotateCarParent();
            _rotateTween.Play();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _rotateTween.Kill();
            _isDragging = true;
            _lastMousePositionX = eventData.position.x;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_modelVisualParent == null) return;
            if (!_isDragging) return;

            var deltaX = eventData.position.x - _lastMousePositionX;
            _lastMousePositionX = eventData.position.x;

            _modelVisualParent.rotation = Quaternion.Euler(
                _modelVisualParent.rotation.eulerAngles.x,
                _modelVisualParent.rotation.eulerAngles.y - deltaX * RotationSpeed,
                _modelVisualParent.rotation.eulerAngles.z
            );

            _modelVisualManager.ModelVisualContent = _modelVisualParent;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _rotateTween = RotateCarParent();
            _rotateTween.Play();
            _isDragging = false;
        }


        private Tween RotateCarParent()
        {
            return _modelVisualParent.DORotate(Vector3.up * 360, 10f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetRelative();
        }
    }
}