using System;
using System.Threading;
using _Scripts.GameCore.NPC;
using _Scripts.GameCore.Vibration.Constants;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.PopupSystem;
using GameCore.Tutorial;
using Interfaces;
using TMPro;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.NPC
{
    [RequireComponent(typeof(Collider))]
    public abstract class NpcBase : MonoBehaviour, INPCBehavior
    {
        #region Actions

        public event Action<bool> OnStateChanged;

        #endregion

        #region Serialized Fields

        [SerializeField] private SpriteRenderer outlineSpriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Collider collider;

        #endregion

        #region Private Fields

        private TimerInfoController _timerInfoController;
        private PopupManager _popupManager;
        private VibrationManager _vibrationManager;
        private CancellationTokenSource _cancellationTokenSource;
        private IAnalyticsService _iAnalyticsService;
        private TutorialSequenceController _tutorialSequenceController;
        private ITutorialService _tutorialService;
        private Camera _camera;

        protected bool IsLocked;

        #endregion

        #region Properties

        protected PopupManager PopupManager => _popupManager;
        protected VibrationManager VibrationManager => _vibrationManager;
        protected IAnalyticsService IAnalyticsService => _iAnalyticsService;
        protected TutorialSequenceController TutorialSequenceController => _tutorialSequenceController;
        protected ITutorialService TutorialService => _tutorialService;
        protected SpriteRenderer OutlineSpriteRenderer => outlineSpriteRenderer;
        protected Collider Collider => collider;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            _camera = Camera.main;
            if (collider == null)
            {
                collider = GetComponent<BoxCollider>();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            CheckCamera(_cancellationTokenSource.Token).Forget();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region Public Methods

        protected abstract void OnCompleteTimer();

        public virtual void Execute(bool isActive)
        {
            if (IsLocked) return;
            OnStateChanged?.Invoke(isActive);

            if (isActive)
            {
                if (IsLocked)
                {
                    return;
                }

                _timerInfoController.SetTimer(NpcBaseConstants.TimerDuration, OnCompleteTimer);
                _vibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.Npc);
            }
            else
            {
                _timerInfoController.StopTimer();
            }
            
            SetScaleOutline(isActive ? Vector3.one * 1.1f : Vector3.one, isActive ? Ease.OutBack : Ease.InBack);
            outlineSpriteRenderer.color = isActive ? Color.green : Color.white;
        }


        protected void OpenPopup(PopupConstants.PopupType targetPopup)
        {
            _ = _popupManager.OpenPopup(targetPopup);
        }

        #endregion

        #region Private Methods

        [Inject]
        public virtual void Init(IObjectResolver resolver)
        {
            _timerInfoController = resolver.Resolve<TimerInfoController>();
            _popupManager = resolver.Resolve<PopupManager>();
            _iAnalyticsService = resolver.Resolve<IAnalyticsService>();
            _tutorialService = resolver.Resolve<ITutorialService>();
            _vibrationManager = resolver.Resolve<VibrationManager>();
            _tutorialSequenceController = resolver.Resolve<TutorialSequenceController>();
        }

        private async UniTask CheckCamera(CancellationToken token)
        {
            try
            {
                if (animator == null || _camera == null || transform == null)
                {
                    Dispose();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(NpcBaseConstants.Delay), cancellationToken: token);

                while (!token.IsCancellationRequested)
                {
                    if (this == null || transform == null)
                    {
                        LoggerNS.LogWarning("CheckCamera stopped because the object was destroyed.");
                        return;
                    }

                    animator.enabled = _camera.IsInViewport(transform.position);
                    await UniTask.Delay(TimeSpan.FromSeconds(NpcBaseConstants.Delay), cancellationToken: token);
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

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other)) return;

            Execute(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other)) return;

            Execute(false);
        }


        private void SetScaleOutline(Vector3 endScale, Ease ease)
        {
            outlineSpriteRenderer.transform.DOScale(endScale, 0.25f).SetEase(ease);
        }
        
        
        private void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private bool IsPlayer(Collider other) => other.gameObject.layer == LayerMask.NameToLayer(NpcBaseConstants.Player);
        #endregion
    }
}