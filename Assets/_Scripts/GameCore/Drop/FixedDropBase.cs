using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Player;
using Interfaces;
using Managers;
using MyBox;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Drop
{
    public class FixedDropBase : MonoBehaviour, IDropItem
    {
        #region Serialized Fields

        [SerializeField] private float optionalDistance;
        [SerializeField] protected List<GameObject> disableObjects;
        [SerializeField] private bool hasParticle;
        [SerializeField] protected string oneShotAudioKey;

        [ConditionalField(nameof(hasParticle), false)] [SerializeField]
        private new ParticleSystem particleSystem;

        [ConditionalField(nameof(hasParticle), false)] [SerializeField]
        private float particleDestroyWaitForSeconds = 3;

        #endregion

        #region Private Fields

        protected float _value;
        protected Animator _animator;
        private Camera _camera;
        private CancellationTokenSource _cancellationTokenSource;
        private float Delay = 1f;
        protected AudioManager AudioManager;
        protected PlayerController PlayerController;

        #endregion

        #region Properties

        public IObjectResolver Resolver { get; set; }
        public Transform Transform { get; private set; }
        public float? OptionalDistance => optionalDistance;
        public bool IsPickedUp { get; protected set; }
        public bool IsPickable => true;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _camera = Camera.main;
            _cancellationTokenSource = new CancellationTokenSource();
            CheckCamera(_cancellationTokenSource.Token).Forget();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region Private Methods

        private void SetDisableObjects(bool isHidden)
        {
            if (disableObjects is not { Count: > 0 })
            {
                return;
            }

            disableObjects.ForEach(x => x.SetActive(!isHidden));
        }

        private IEnumerator PlayParticle()
        {
            if (hasParticle is false)
            {
                yield break;
            }

            disableObjects.ForEach(x => x.SetActive(false));
            particleSystem.Play();
            yield return new WaitForSeconds(particleDestroyWaitForSeconds);
            Reset();
        }

        private async UniTask CheckCamera(CancellationToken token)
        {
            try
            {
                if (_camera == null || _animator == null || transform == null)
                {
                    Dispose();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(Delay), cancellationToken: token);

                while (!token.IsCancellationRequested)
                {
                    if (PlayerController != null)
                    {
                        Delay = PlayerController.PlayerMovementMode == PlayerMovementMode.Walk ? 2f : 1f;
                    }

                    if (this == null || transform == null)
                    {
                        LoggerNS.LogWarning("CheckCamera stopped because the object was destroyed.");
                        return;
                    }

                    SetDisableObjects(!_camera.IsInViewport(transform.position, 1f));
                    _animator.enabled = _camera.IsInViewport(transform.position, 1f);
                    await UniTask.Delay(TimeSpan.FromSeconds(Delay), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                LoggerNS.Log("CheckCamera Task was canceled");
            }
            catch (Exception ex)
            {
                LoggerNS.LogError($"An unexpected error occurred in CheckCamera: {ex.Message}");
            }
        }

        private void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        #endregion

        #region Public Methods

        public virtual void Initialize(int value, bool isHidden = false)
        {
            AudioManager = Resolver.Resolve<AudioManager>();
            PlayerController = Resolver.Resolve<PlayerController>();
            _value = value;
            Transform = transform;
            SetDisableObjects(isHidden);
        }

        public virtual void Use()
        {
            Dispose();
            IsPickedUp = true;
            StartCoroutine(PlayParticle());
        }

        public virtual void Reset()
        {
            gameObject.SetActive(false);
            IsPickedUp = false;
        }

        #endregion
    }
}