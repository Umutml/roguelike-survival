using System.Collections;
using System.Collections.Generic;
using GameCore.Player;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Drop
{
    public class PickupDropBase : MonoBehaviour, IDropItem
    {
        #region Serialized Fields

        [SerializeField] private List<GameObject> disableObjects;

        #endregion

        #region Private Fields

        private readonly WaitForSeconds DestroyWaitForSeconds = new(30);
        private readonly int DestroyKey = Animator.StringToHash("Destroy");

        private PlayerController _playerController;
        protected Animator _animator;
        private bool _isMoving;

        private const float MoveSpeed = 20;
        private const float MoveDistance = 0.1f;

        protected float _value;

        #endregion

        #region Properties

        public IObjectResolver Resolver { get; set; }
        public Transform Transform { get; private set; }
        public float? OptionalDistance => null;
        public bool IsPickedUp { get; set; }
        public bool IsPickable => true;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            Move();
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

        private void Move()
        {
            if (_playerController == null)
            {
                Reset();
                return;
            }

            if (_playerController.PlayerMovementMode == PlayerMovementMode.Drive)
            {
                Reset();
                return;
            }

            if (Vector3.Distance(transform.position, _playerController.transform.position) >= MoveDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    _playerController.transform.position,
                    MoveSpeed * Time.fixedDeltaTime);

                return;
            }

            Reset();
        }

        private IEnumerator DestroyDropAfterDelay()
        {
            yield return DestroyWaitForSeconds;
            if (_animator != null) _animator.SetTrigger(DestroyKey);
        }

        #endregion

        #region Public Methods

        public virtual void Initialize(int value, bool isHidden = false)
        {
            _value = value;
            Transform = transform;
            _playerController = Resolver.Resolve<PlayerController>();
            SetDisableObjects(isHidden);
            StartCoroutine(nameof(DestroyDropAfterDelay));
        }

        public virtual void Use()
        {
            IsPickedUp = true;
            _isMoving = true;
        }

        public virtual void Reset()
        {
            gameObject.SetActive(false);
            _isMoving = false;
            IsPickedUp = false;
            StopCoroutine(nameof(DestroyDropAfterDelay));
        }

        #endregion
    }
}