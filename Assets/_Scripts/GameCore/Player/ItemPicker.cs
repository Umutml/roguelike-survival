using System;
using System.Collections;
using _Scripts.GameCore.NPC;
using GameCore.Scriptables;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Player
{
    public class ItemPicker : MonoBehaviour
    {
        #region Actions

        public event Action<bool> OnCarPickup;

        #endregion

        #region Serialized Fields

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float carDetectionRadius = 1f;

        #endregion

        #region Private Fields

        private const float CarProximityDistance = 3f;
        private readonly WaitForSeconds _wait = new(0.3f);

        private string _radiusSkillId;
        private string _carDetectionRadiusSkillId;

        private PlayerController _playerController;
        private PlayerSkillController _playerSkillController;
        private CarController _carController;
        private CarManager _carManager;
        private Coroutine _overlapSphereCheckCoroutine;
        private IObjectResolver _resolver;
        private bool _isNearCar;
        private float _currentRadius;
        private Transform _carTransform;
        private bool _lockCarPickup;

        #endregion

        #region Properties

        public float Radius => radius;
        public float CarDetectionRadius => carDetectionRadius;

        public CarController CarController => _carController;
        public bool LockCarPickup
        {
            get => _lockCarPickup;
            set
            {
                _lockCarPickup = value;
                _carManager.SetEnableDoorCircles(!_lockCarPickup);
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeComponents();
            SubscribeActions();
            ConfigureRadius(PlayerMovementMode.Walk);
        }

        private void Start()
        {
            _overlapSphereCheckCoroutine = StartCoroutine(OverlapSphereCheckRoutine());
        }

        private void OnDestroy()
        {
            StopOverlapSphereCheck();
            UnSubscribeActions();
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(IObjectResolver resolver)
        {
            _resolver = resolver;
            _carManager = resolver.Resolve<CarManager>();
        }

        private void ResetItemPicker()
        {
            PlayerSkillController.ResetSkill(ref radius, _radiusSkillId);
            PlayerSkillController.ResetSkill(ref carDetectionRadius, _carDetectionRadiusSkillId);
            ConfigureRadius(_playerController.PlayerMovementMode);
        }

        private void InitializeComponents()
        {
            _playerController = GetComponent<PlayerController>();
            _playerSkillController = GetComponent<PlayerSkillController>();
        }


        private void SubscribeActions()
        {
            _playerSkillController.OnSkillUpgrade += AdjustPickup;
            _playerSkillController.OnSkillUpgrade += AdjustPickupCar;
            _playerSkillController.OnResetSkill += ResetItemPicker;
        }

        private void UnSubscribeActions()
        {
            _playerSkillController.OnSkillUpgrade -= AdjustPickup;
            _playerSkillController.OnSkillUpgrade -= AdjustPickupCar;
            _playerSkillController.OnResetSkill -= ResetItemPicker;
        }

        private void AdjustPickup(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type is not StatUpgradeType.PickupRange)
            {
                return;
            }

            PlayerSkillController.Calculate(ref radius, ref _radiusSkillId, upgradeDetail);
            ConfigureRadius(_playerController.PlayerMovementMode);
        }

        private void AdjustPickupCar(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type is not StatUpgradeType.CarPickupRange)
            {
                return;
            }

            PlayerSkillController.Calculate(ref carDetectionRadius, ref _carDetectionRadiusSkillId, upgradeDetail);
            ConfigureRadius(_playerController.PlayerMovementMode);
        }

        private IEnumerator OverlapSphereCheckRoutine()
        {
            while (true)
            {
                PerformOverlapSphereCheck();
                CheckCarProximity();
                yield return _wait;
            }
        }

        private void StopOverlapSphereCheck()
        {
            if (_overlapSphereCheckCoroutine != null)
            {
                StopCoroutine(_overlapSphereCheckCoroutine);
            }
        }

        private void PerformOverlapSphereCheck()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, _currentRadius, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                ProcessCarCollision(hitCollider);
                ProcessItemCollision(hitCollider);
                ProcessCollectableCollision(hitCollider);
            }
        }

        private void ProcessCollectableCollision(Collider hitCollider)
        {
            if (!hitCollider.TryGetComponent<ICollectableItem>(out var collectableItem))
            {
                return;
            }

            if (collectableItem.IsCollected)
            {
                return;
            }

            if (!IsWithinOptionalDistance(collectableItem.Transform.position, collectableItem.Distance))
            {
                return;
            }

            collectableItem.Collect(_resolver);
        }

        private void ProcessCarCollision(Collider hitCollider)
        {
            if (!hitCollider.TryGetComponent<CarController>(out var carController)) return;
            _carController = carController;
            _carTransform = carController.transform;
        }

        private void ProcessItemCollision(Collider hitCollider)
        {
            if (hitCollider.TryGetComponent<IDropItem>(out var dropItem) && !dropItem.IsPickedUp)
            {
                HandleItemPickup(dropItem);
            }
        }

        public void HandleItemPickup(IDropItem dropItem)
        {
            if (!dropItem.IsPickable) return;

            if (PlayerIsWalking() && ItemBeyondPickupDistance(dropItem)) return;

            dropItem.Use();
        }

        public void ConfigureRadius(PlayerMovementMode playerMovementMode)
        {
            _currentRadius = playerMovementMode switch
            {
                PlayerMovementMode.Drive => carDetectionRadius,
                _ => radius
            };
        }

        private void CheckCarProximity()
        {
            if (LockCarPickup) { return; }

            if (_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive)) return;
            if (_carController == null || _carTransform == null) return;
            var isNearCar = Vector3.Distance(transform.position, _carTransform.position) < CarProximityDistance;
            if (_isNearCar == isNearCar) return;
            if (_carController.IsDead) return;
            _isNearCar = isNearCar;
            OnCarPickup?.Invoke(isNearCar);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _currentRadius);
        }

        private bool PlayerIsWalking()
        {
            return _playerController.PlayerMovementMode == PlayerMovementMode.Walk;
        }

        private bool ItemBeyondPickupDistance(IDropItem dropItem)
        {
            return dropItem.OptionalDistance.HasValue &&
                !IsWithinOptionalDistance(dropItem.Transform.position, dropItem.OptionalDistance);
        }

        private bool IsWithinOptionalDistance(Vector3 position, float? optionalDistance)
        {
            return Vector3.Distance(transform.position, position) < (optionalDistance ?? 1f);
        }

        #endregion
    }
}
