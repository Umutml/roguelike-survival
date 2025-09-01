using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Player.Input;
using GameCore.Player.WeaponSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Transform temporaryMeleeParticleParent;

        private Animator _animator;
        private PlayerMovementController _playerMovementController;
        private MonoBehaviour _leftArmIk;
        private MonoBehaviour _rightArmIk;

        #endregion

        #region Fields

        private MeleeWeapon _currentMeleeWeapon;

        private bool _isLeftHandEquipped, _isRightHandEquipped;
        private bool _modelTransformIsOverridden;
        private Quaternion _originalSwordEffectRotation;
        private Vector3 _originalSwordEffectPosition;

        #endregion

        #region Properties

        public bool IsInAnimation
        {
            get
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(1);
                var isEnded = stateInfo.normalizedTime <= 1.0f;

                return isEnded && (stateInfo.IsName("KatanaChainAttack1") || stateInfo.IsName("KatanaChainAttack2") ||
                                   stateInfo.IsName("KatanaChainAttack3"));
            }
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _playerMovementController = GetComponent<PlayerMovementController>();
        }

        private void OnEnable()
        {
            _playerMovementController.OnCanMoveChanged += MoveState;
        }

        private void OnDestroy()
        {
            _playerMovementController.OnCanMoveChanged -= MoveState;
        }

        #endregion

        #region Public Methods

        public void Initialize(Animator animator, MonoBehaviour leftArmIk, MonoBehaviour rightArmIk)
        {
            _animator = animator;
            _leftArmIk = leftArmIk;
            _rightArmIk = rightArmIk;
        }

        public void SetMovementBlendAnimations(Vector2 moveDelta, Transform modelTransform)
        {
            if (moveDelta.magnitude > 0)
            {
                _animator.SetBool("Walking", true);
            }
            else
            {
                _animator.SetBool("Walking", false);
            }

            if (!_modelTransformIsOverridden && moveDelta.magnitude > 0f)
                modelTransform.localRotation = Quaternion.LookRotation(new Vector3(moveDelta.x, 0, moveDelta.y));

            var modelDirection = modelTransform.forward;

            var aimDeltaV3 = new Vector3(modelDirection.x, 0, modelDirection.z);
            var moveDeltaV3 = new Vector3(moveDelta.x, 0, moveDelta.y);
            var strafeRotatedVector = Quaternion.AngleAxis(90f, Vector3.up) * aimDeltaV3;

            var aimdeltav2 = new Vector2(modelDirection.x, modelDirection.z);


            var walkDot = Vector2.Dot(moveDelta, aimdeltav2);
            var strafeDot = Vector3.Dot(strafeRotatedVector, moveDeltaV3);
            var forward = walkDot;
            var right = strafeDot;

            _animator.SetFloat("Forward", forward);
            _animator.SetFloat("Right", right);
        }

        public async UniTask PlayAttackAnimation()
        {
            _animator.SetBool("BasicAttackSkill", true);
            await UniTask.Delay(500);
            _animator.SetBool("BasicAttackSkill", false);
        }


        public void SetHandWields(bool isLeftHandEquipped, bool isRightHandEquipped)
        {
            _isRightHandEquipped = isRightHandEquipped;
            _isLeftHandEquipped = isLeftHandEquipped;

            _animator.SetBool("LeftHandEquipped", isLeftHandEquipped);
            _animator.SetBool("RightHandEquipped", isRightHandEquipped);
        }

        public void ToggleAiming(bool isAiming)
        {
            _animator.SetBool("Aiming", isAiming);

            if (isAiming)
            {
                ToggleIk(_isLeftHandEquipped, _isRightHandEquipped);
            }
            else
            {
                ToggleIk(false, false);
            }
        }

        public void ToggleDeadState(bool isDead)
        {
            _animator.SetBool("IsDead", isDead);
        }

        public void SetWalkAnimationSpeed(float speed)
        {
            _animator.SetFloat("WalkSpeed", speed);
        }

        public void SetMeleeWeapon(MeleeWeapon meleeWeapon)
        {
            _currentMeleeWeapon = meleeWeapon;
        }

        public async void ToggleMeleeState(bool isMelee, int meleeWeaponType = 0)
        {
            ToggleIk(!isMelee, !isMelee);

            _animator.SetInteger("ChainAttackStarter", Random.Range(0, 2));
            _animator.SetBool("IsMelee", isMelee);
            _animator.SetInteger("MeleeWeaponType", meleeWeaponType);

            if (isMelee)
            {
                _animator.SetTrigger("SubstateTrigger");
                await UniTask.Delay(1000);
                if (_animator == null) return;
                _animator.ResetTrigger("SubstateTrigger");
            }
            else
                _animator.ResetTrigger("SubstateTrigger");
        }


        public void SetModelTransformOverride(bool toggle)
        {
            _modelTransformIsOverridden = toggle;
        }

        public void PlayDashAnimation()
        {
            _animator.SetInteger("DashType", Random.Range(0, 1));
            _animator.SetBool("IsDashing", true);
        }

        public void RemoveDashState()
        {
            _animator.SetBool("IsDashing", false);
        }

        public async UniTask ReparentMeleeParticles(CancellationToken token)
        {
            if (token.IsCancellationRequested || !_currentMeleeWeapon || !_currentMeleeWeapon.OriginalParent ||
                !temporaryMeleeParticleParent) return;

            foreach (var reparentObject in _currentMeleeWeapon.ReparentObjects)
            {
                if (reparentObject)
                    reparentObject.SetParent(temporaryMeleeParticleParent);
            }

            await UniTask.Delay(1000);

            if (token.IsCancellationRequested) return;

            foreach (var reparentObject in _currentMeleeWeapon.ReparentObjects)
            {
                reparentObject.SetParent(_currentMeleeWeapon.OriginalParent);
                // Restore original local transforms
                var originalRotPos = _currentMeleeWeapon.OriginalLocalRotPos[reparentObject];
                reparentObject.localPosition = originalRotPos.Item1;
                reparentObject.localRotation = originalRotPos.Item2;
            }
        }

        #endregion

        #region Private Methods

        private void MoveState(bool isMoving)
        {
            if (_animator == null) return;
            _animator.SetBool("Walking", isMoving);
        }

        private void ToggleIk(bool left, bool right)
        {
            _leftArmIk.enabled = left;
            _rightArmIk.enabled = right;
        }

        #endregion
    }
}