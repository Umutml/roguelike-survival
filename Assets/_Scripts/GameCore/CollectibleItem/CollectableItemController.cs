using System.Collections;
using _Utilities;
using GameCore.Helpers;
using GameCore.Player;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


namespace GameCore.CollectibleItem
{
    public class CollectableItemController : MonoBehaviour, ICollectableItem
    {
        #region Serialized Fields

        [SerializeField] private GameObject slider;
        [SerializeField] private Image fill;
        [SerializeField] private ShaderReplacer highlightShaderReplacer;
        [SerializeField] private GameObject outline;
        [SerializeField] private float progressSpeed = 2;
        [SerializeField] private float distance = 2;

        #endregion

        #region Private Fields

        private PlayerCollectItemController _playerCollectItemController;
        private Transform _playerTransform;
        private Coroutine _waitCoroutine;
        private Camera _mainCamera;

        private const float MaxProgress = 10f;

        private readonly WaitForSeconds _wait = new(30);

        #endregion

        #region Properties

        public float ProgressSpeed => progressSpeed;
        public float Distance => distance;
        public Transform Transform { get; private set; }
        public float Progress { get; set; }
        public bool IsCollected { get; set; }

        #endregion

        #region Unity Methods

        private void Update()
        {
            CollectProgress();
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            _mainCamera = Camera.main;
            Transform = transform;
            DisableOnCollect(true);
            SetSlider(0);
            LookAtCamera();
        }

        public void Collect(IObjectResolver resolver)
        {
            IsCollected = true;
            _playerTransform = resolver.Resolve<PlayerController>().transform;
            _playerCollectItemController = resolver.Resolve<PlayerCollectItemController>();
        }

        public void Reset()
        {
            Progress = 0;
            IsCollected = false;
            DisableOnCollect(true);
            SetSlider(0);
        }

        #endregion

        #region Private Methods

        private void CollectProgress()
        {
            if (_playerTransform == null) { return; }

            if (_waitCoroutine != null) { return; }

            if (!IsDistanceLess())
            {
                Reset();
                return;
            }

            Progress += ProgressSpeed * Time.deltaTime;

            SetSlider(Progress);

            if (!IsProgressComplete()) { return; }

            CompleteCollect();
        }

        private void CompleteCollect()
        {
            SetSlider(0);
            DisableOnCollect(false);
            _playerCollectItemController.CollectItem(this);
            _waitCoroutine = StartCoroutine(nameof(WaitCoroutine));
        }

        private IEnumerator WaitCoroutine()
        {
            yield return _wait;
            ResetCoroutine();
            Reset();
        }

        private void ResetCoroutine()
        {
            if (_waitCoroutine == null)
            {
                return;
            }

            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }

        private void SetSlider(float value)
        {
            slider.SetActive(value > 0);
            fill.fillAmount = Helper.Remap(value, 0, MaxProgress, 0, 1);
        }

        private void LookAtCamera()
        {
            slider.transform.LookAt(_mainCamera.transform.position);
        }

        private void DisableOnCollect(bool isActive)
        {
            outline.SetActive(isActive);
            
            if(!highlightShaderReplacer) return;
            if(isActive)
                highlightShaderReplacer.ReplaceShaders();
            else
                highlightShaderReplacer.RevertShaders();
        }

        private bool IsDistanceLess()
        {
            return Vector3.Distance(_playerTransform.position, Transform.position) < Distance;
        }

        private bool IsProgressComplete()
        {
            return Progress >= MaxProgress;
        }

        #endregion
    }
}
