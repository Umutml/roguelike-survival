using System;
using System.Collections.Generic;
using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using GameCore.Scriptables;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace GameCore.Player.Input
{
    public class PlayerMovementController : MonoBehaviour
    {
        #region Actions

        public event Action<bool> InBaseChanged;
        public event Action<bool> OnCanMoveChanged;

        #endregion


        #region Serializable Fields

        [SerializeField] private float movementSpeed = 5;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private Transform playerMinimapArrow;
        [SerializeField] private float movementRotationAngle = 0f;


        [Header("Physics")][SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedGravity = -0.5f;
        [SerializeField] private float maxFallSpeed = -20f;

        #endregion

        public event Action MovementInputAcquired;
        public event Action MovementInputLost;

        #region Fields

        private CharacterController _characterController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInput _playerInput;
        private PlayerSkillController _playerSkillController;
        private IAnalyticsService _analyticsService;
        private TutorialSequenceController _tutorialSequenceController;
        private PlayerController _playerController;
        private IGameService _gameService;
        private string _movementSkillId;
        private bool _canMove;
        private bool _inBase;
        private bool _isFalling;
        private float _previousInputMagnitude;
        private readonly Vector3 _spawnPosition = new(-48f, 1.08f, -49f);


        private PlayerInputActions _playerInputActions;
        private Vector3 _velocity;
        private const string Obstacles = "Obstacles";

        public Vector2 MovementInput
        {
            get
            {
                var input = _playerInputActions.Player.Move.ReadValue<Vector2>();
                float angle = movementRotationAngle * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                var rotatedInput = new Vector2(input.x * cos - input.y * sin, input.x * sin + input.y * cos);
                if (!(rotatedInput.sqrMagnitude > 0.01f)) return rotatedInput;
                var arrowRotation = Mathf.Atan2(rotatedInput.x, rotatedInput.y) * Mathf.Rad2Deg;
                playerMinimapArrow.localEulerAngles = new Vector3(0, arrowRotation + movementRotationAngle, 0);
                return rotatedInput;
            }
        }

        public bool MovementInputExists => MovementInput.magnitude > 0;
        public bool InBase => _inBase;

        public bool CanMove
        {
            get => _canMove;
            set
            {
                OnCanMoveChanged?.Invoke(value);
                _canMove = value;
            }
        }

        public Action OnFire { get; set; }
        public PlayerInputActions PlayerInputActions => _playerInputActions;
        public float MovementSpeed => movementSpeed;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerAnimationController = GetComponent<PlayerAnimationController>();
            _playerSkillController = GetComponent<PlayerSkillController>();
            _playerInputActions = new PlayerInputActions();
            _playerController = GetComponent<PlayerController>();
            _playerInputActions.Player.Enable();
            _characterController = GetComponent<CharacterController>();
            OnFire += () => _playerAnimationController.PlayAttackAnimation();
        }

        private void OnEnable()
        {
            _playerSkillController.OnSkillUpgrade += AdjustSpeed;
        }

        private void OnDisable()
        {
            _playerInputActions.Disable();
        }

        private void OnDestroy()
        {
            _playerSkillController.OnSkillUpgrade -= AdjustSpeed;
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (other.gameObject.layer != LayerMask.NameToLayer(Obstacles)) return;
            if (other.gameObject.CompareTag("Zone"))
            {
                _inBase = true;
                InBaseChanged?.Invoke(_inBase);
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Zone"))
            {
                //if (other.gameObject.layer != LayerMask.NameToLayer(Obstacles)) return;
                _inBase = false;
                InBaseChanged?.Invoke(_inBase);
            }
        }


        [Inject]
        private async void InitializeMovement(GameSceneSetupManager gameSceneSetupManager,
            TutorialSequenceController sequenceController, IAnalyticsService analyticsService,
            PlayerSkillController playerSkillController, IGameService gameService)
        {
            //player can't move before the scene setup is done
            await gameSceneSetupManager.SceneLoadTaskCompletionSource.Task;
            _gameService = gameService;
            CanMove = true;
            playerSkillController.OnResetSkill += ResetMovement;
            _gameService.OnGamePaused += () => CanMove = false;
            _gameService.OnGameResumed += () => CanMove = true;
            _tutorialSequenceController = sequenceController;
            _analyticsService = analyticsService;
        }

        private void ResetMovement()
        {
            PlayerSkillController.ResetSkill(ref movementSpeed, _movementSkillId);
        }

        private void Update()
        {
            if (!CanMove) return;
            var movementInput = MovementInput;
            if (_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive)) return;
            if (transform.position.y <= -3f)
            {
                SendFallingEvent();
                transform.position = _spawnPosition;
                _isFalling = true;
            }
            else
            {
                _isFalling = false;
            }

            ApplyGravity();

            if (MovementInputExists)
            {
                _characterController.Move((GetMovementDirection(movementInput) + _velocity) *
                                          (Time.deltaTime * movementSpeed));
            }

            _playerAnimationController.SetMovementBlendAnimations(movementInput, modelTransform);

            switch (movementInput.magnitude)
            {
                case > 0 when _previousInputMagnitude == 0:
                    MovementInputAcquired?.Invoke();
                    break;
                case 0 when _previousInputMagnitude > 0:
                    MovementInputLost?.Invoke();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void AdjustSpeed(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.type != StatUpgradeType.Speed)
            {
                return;
            }

            PlayerSkillController.Calculate(ref movementSpeed, ref _movementSkillId, upgradeDetail);
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = groundedGravity;
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
            }

            _velocity.y = Mathf.Max(_velocity.y, maxFallSpeed);
        }

        private void SendFallingEvent()
        {
            if (_isFalling)
            {
                return;
            }

            LoggerNS.Log(
                $"Send Falling Event, isTutorialCompleted: {_tutorialSequenceController.IsTutorialCompleted.ToString()}, fallPosition: {transform.position.ToString()}");
            _analyticsService.LogEventParameterArray("player_fell", new Dictionary<string, object>
            {
                { "isTutorialCompleted", _tutorialSequenceController.IsTutorialCompleted.ToString() },
                { "fallPosition", transform.position.ToString() }
            });
        }

        private Vector3 GetMovementDirection(Vector2 move)
        {
            Vector3 moveDirection = new Vector3(move.x, 0, move.y);
            return moveDirection;
        }

        #endregion

        public async UniTask DashTowardsTarget(Transform target)
        {
            if (_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive)) return;

            var direcionToTarget = target.position - transform.position;

            modelTransform.localRotation =
                Quaternion.LookRotation(new Vector3(direcionToTarget.x, 0, direcionToTarget.y));

            Vector3 startPosition = transform.position;
            Vector3 targetPosition = target.position;
            float dashDistance = 10f;
            float dashSpeed = 25f;
            float dashDuration = dashDistance / dashSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < dashDuration)
            {
                float t = elapsedTime / dashDuration;
                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
                _characterController.Move(newPosition - transform.position);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }
    }
}