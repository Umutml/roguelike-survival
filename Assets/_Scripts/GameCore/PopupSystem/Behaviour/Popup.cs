using System;
using Interfaces;
using UnityEngine;
using VContainer;
#if UNITY_EDITOR
using _Scripts.Utilities;
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace GameCore.PopupSystem
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Popup : MonoBehaviour
    {
        #region Actions

        public Action ClosePopupAction;

        #endregion


        #region Serializable Fields

        [Header("Popup")] [SerializeField] private PopupConstants.PopupType popupType;
        [SerializeField] private bool isAnimationActive = true;
#if UNITY_EDITOR
        [SerializeField] private AnimatorController animatorController;
#endif

        #endregion


        #region Fields

        private IObjectResolver _resolver;
        private Animator _animator;
        private readonly string _popupAnimationPath = "Assets/_Animations/UI/Popup/Popup.controller";
        private static readonly int Show = Animator.StringToHash("Show");

        #endregion


        #region Properties

        public IObjectResolver Resolver
        {
            get => _resolver;
            set => _resolver = value;
        }

        public PopupConstants.PopupType PopupType => popupType;

        #endregion


        #region Unity Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetButtonAnimatorComponent();
        }
#endif

        private void OnEnable()
        {
            PlayAnimation();
        }

        #endregion


        #region Public Methods

        public virtual void Initialize(object data)
        {
        }

        public virtual void InitializeTutorial(object data)
        {
        }

        private void PlayAnimation()
        {
            _animator = GetComponent<Animator>();
            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            if (!isAnimationActive) return;
            _animator.SetTrigger(Show);
        }

        /// <summary>
        /// Closes the popup and resumes the game.
        /// </summary>
        public void ClosePopup()
        {
            Resolver.Resolve<IGameService>().ResumeGame();
            ClosePopupAction?.Invoke();
        }

        #endregion


        #region Private Methods

#if UNITY_EDITOR
        private void SetButtonAnimatorComponent()
        {
            if (_animator != null) return;
            _animator = GetComponent<Animator>();
            animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(_popupAnimationPath);
            if (animatorController is null)
            {
                LoggerNS.LogError($"Animator controller not found at {_popupAnimationPath}");
                return;
            }

            _animator.runtimeAnimatorController = animatorController;
        }
#endif

        #endregion


        #region Absract Methods

        public abstract void OnOpenPopup();
        //public abstract void OnClosePopup();

        #endregion
    }
}