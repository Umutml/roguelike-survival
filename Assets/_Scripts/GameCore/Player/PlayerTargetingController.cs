using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using UnityEngine;

namespace GameCore.Player
{
    public class PlayerTargetingController
    {
        #region Fields

        private Transform _lHandTarget;
        private Transform _rHandTarget;
        private IDamageable _currentTargetL, _currentTargetR, _currentTargetMelee;
        private PlayerWeaponController _weaponController;
        private Transform _modelTransform;
        private MobManager _mobManager;
        private BoxManager _boxManager;
        private float minTimeToExitMelee = 1.3f;
        private PlayerAnimationController _playerAnimationController;

        private DateTime _lastTargetAcquisitionTimeL = DateTime.MinValue,
            _lastTargetAcquisitionTimeR = DateTime.MinValue;

        private float _findTargetCooldown;
        private Quaternion _lhandTargetOriginalRot, _rhandTargetOriginalRot;
        private Vector3 _lhandTargetOriginalLocalPos, _rhandTargetOriginalLocalPos;
        private Quaternion _targetRotation;
        private float _maxFrontAnglePerArm;
        private float _maxBackAnglePerArm;
        private IDamageableRegisterService _damageableRegisterService;
        private float findTargetCooldown = 1f;
        private Vector3 _cachedPosition;
        private Vector3 _cachedRight;
        private Vector3 _cachedModelForward;
        private Vector3 _tempVector = Vector3.zero;
        private static readonly Vector3 UpVector = Vector3.up;
        private Quaternion _cachedRotation;
        private bool _afterMeleeCooldownActive;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private IList<IDamageable> _focusedDamageables;

        #endregion


        #region Public Methods

        public PlayerTargetingController(PlayerWeaponController weaponController, Transform lHandTarget,
            Transform rHandTarget, Transform modelTransform, MobManager mobManager, BoxManager boxManager,
            PlayerAnimationController playerAnimationController, float maxFrontAnglePerArm, float maxBackAnglePerArm,
            IDamageableRegisterService damageableRegisterService)
        {
            _focusedDamageables = new List<IDamageable>();
            _damageableRegisterService = damageableRegisterService;
            _maxBackAnglePerArm = maxBackAnglePerArm;
            _maxFrontAnglePerArm = maxFrontAnglePerArm;
            _playerAnimationController = playerAnimationController;
            _boxManager = boxManager;
            _mobManager = mobManager;
            _modelTransform = modelTransform;
            _weaponController = weaponController;
            _lHandTarget = lHandTarget;
            _rHandTarget = rHandTarget;

            _lhandTargetOriginalLocalPos = lHandTarget.localPosition;
            _rhandTargetOriginalLocalPos = rHandTarget.localPosition;

            _lhandTargetOriginalRot = lHandTarget.localRotation;
            _rhandTargetOriginalRot = rHandTarget.localRotation;
            _findTargetCooldown = findTargetCooldown;
            findTargetCooldown = 0;
        }

        public void Update()
        {
            FindTarget();

            if (_weaponController.CurrentShootingMode == PlayerWeaponController.PlayerShootingMode.Melee)
                return; //we don't lock to ranged targets if it's already melee

            if (_afterMeleeCooldownActive)
                return;

            if (Quaternion.Angle(_modelTransform.localRotation, _targetRotation) > 2)
                return;

            var weaponControllerLweapon = _weaponController.Lweapon;
            if (_currentTargetL != null)
            {
                
                LockHandOnTarget(_currentTargetL, _lHandTarget, weaponControllerLweapon);
                FireAtTarget(weaponControllerLweapon, _currentTargetL);
            }
            else if(weaponControllerLweapon)
            {
                weaponControllerLweapon.transform.localRotation = weaponControllerLweapon.DefaultRotation;
            }

            var weaponControllerRweapon = _weaponController.Rweapon;
            if (_currentTargetR != null)
            {
                LockHandOnTarget(_currentTargetR, _rHandTarget, weaponControllerRweapon);
                FireAtTarget(weaponControllerRweapon, _currentTargetR);
            }
            else if(weaponControllerRweapon)
            {
                weaponControllerRweapon.transform.localRotation = weaponControllerRweapon.DefaultRotation;
            }
        }

        public void OnMeleeAttackHappened()
        {
            if (_weaponController.CurrentShootingMode != PlayerWeaponController.PlayerShootingMode.Melee ||
                _currentTargetMelee == null) return;

            if (!_weaponController.IsDamageTypeCompatible(_currentTargetMelee))
            {
                return;
            }

            _weaponController.MeleeWeapon.FireAt(_currentTargetMelee);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void FocusDamageables(IList<IDamageable> damageables)
        {
            var validDamageables = new List<IDamageable>();
            foreach (var damageable in damageables)
            {
                if (damageable != null)
                {
                    validDamageables.Add(damageable);
                }
            }

            var currentTargets = new HashSet<IDamageable>();
            if (_currentTargetL != null) currentTargets.Add(_currentTargetL);
            if (_currentTargetR != null) currentTargets.Add(_currentTargetR);
            if (_currentTargetMelee != null) currentTargets.Add(_currentTargetMelee);

            if (_focusedDamageables?.Count > 0)
            {
                foreach (var focusedDamageable in _focusedDamageables)
                {
                    if (focusedDamageable != null && !currentTargets.Contains(focusedDamageable) &&
                        !validDamageables.Contains(focusedDamageable))
                    {
                        focusedDamageable.OnLoseFocus();
                    }
                }
            }

            _focusedDamageables = new List<IDamageable>();
            foreach (var target in currentTargets)
            {
                if (!_focusedDamageables.Contains(target))
                    _focusedDamageables.Add(target);
            }

            foreach (var target in validDamageables)
            {
                if (!_focusedDamageables.Contains(target))
                    _focusedDamageables.Add(target);
            }
        }

        #endregion

        #region Private Methods

        private void LockHandOnTarget(IDamageable damageable, Transform handTarget, Weapon weapon = null)
        {
            if (damageable == null) return;

            Vector3 oldPos = handTarget.position;

            var targetPos = Vector3.Slerp(_modelTransform.position, damageable.Transform.position, 0.3f);
            targetPos.y = _modelTransform.position.y;

            handTarget.position = Vector3.Slerp(oldPos, targetPos, Time.deltaTime * 10);

            if (weapon)
            {
                var defaultRotation = weapon.DefaultRotation;
                Quaternion targetRotation = Quaternion.LookRotation(
                    damageable.RandomTransform.position - weapon.transform.position);
                
                Quaternion localTargetRotation = Quaternion.Inverse(weapon.transform.parent.rotation) * targetRotation;
                
                //limit the angle difference of the weapon to its default rotation to 40 degrees
                float angle = Quaternion.Angle(defaultRotation, localTargetRotation);
                if (angle <= 40f)
                {
                    weapon.transform.LookAt(damageable.RandomTransform);
                }
                else
                {
                    Quaternion limitedRotation = Quaternion.RotateTowards(defaultRotation, localTargetRotation, 40f);
                    weapon.transform.localRotation = limitedRotation;
                }
            }
        }

        private void FireAtTarget(Weapon weapon, IDamageable target)
        {
            if (!_weaponController.IsDamageTypeCompatible(target))
            {
                return;
            }

            if (weapon == _weaponController.Lweapon)
            {
                if (!_weaponController.IsFirstTimeLWeapon &&
                    DateTime.Now < _weaponController.LastFireTimeL.AddSeconds(weapon.FireInterval)) return;
                weapon.FireAt(target);
                _weaponController.LastFireTimeL = DateTime.Now;
                _weaponController.IsFirstTimeLWeapon = false;
            }
            else if (weapon == _weaponController.Rweapon)
            {
                if (!_weaponController.IsFirstTimeRWeapon &&
                    DateTime.Now < _weaponController.LastFireTimeR.AddSeconds(weapon.FireInterval)) return;
                weapon.FireAt(target);
                _weaponController.LastFireTimeR = DateTime.Now;
                _weaponController.IsFirstTimeRWeapon = false;
            }
        }

        private void FindTarget()
        {
            if (ReferenceEquals(_mobManager, null) || ReferenceEquals(_boxManager, null)) return;

            HandleMeleeTargeting();

            if (_weaponController.CurrentShootingMode == PlayerWeaponController.PlayerShootingMode.Melee) return;

            HandleRangedTargeting();
            UpdateBodyRotationAndAnimation();
        }

        private void HandleMeleeTargeting()
        {
            if (!_weaponController.MeleeWeapon) return;

            UpdateMeleeTarget();
            HandleMeleeRotation();
        }

        private void UpdateMeleeTarget()
        {
            var meleeRange = _weaponController.MeleeWeapon.Range;
            var meleeSearchPosition = _modelTransform.position;
            _currentTargetMelee = GetClosestDamageable(meleeSearchPosition, meleeRange, false);

            if (_currentTargetMelee != null)
            {
                HandleMeleeTargetAcquisition(meleeRange);
            }
            else
            {
                _weaponController.ToggleMelee(false);
            }
        }

        private void HandleMeleeTargetAcquisition(float meleeRange)
        {
            float sqrRange = meleeRange * meleeRange;
            _cachedPosition = _modelTransform.position;
            float sqrDistance = Vector3.SqrMagnitude(_currentTargetMelee.Transform.position - _cachedPosition);

            if (_weaponController.CurrentShootingMode != PlayerWeaponController.PlayerShootingMode.Melee &&
                sqrDistance < sqrRange)
            {
                _weaponController.ToggleMelee(true);
            }
            else if (ShouldExitMelee())
            {
                _currentTargetMelee = null;
                _weaponController.ToggleMelee(false);
                _playerAnimationController.ReparentMeleeParticles(_cancellationTokenSource.Token);
                _afterMeleeCooldownActive = true;

                UniTask.Delay(TimeSpan.FromSeconds(1f)).ContinueWith(() => { _afterMeleeCooldownActive = false; });
            }
        }

        private bool ShouldExitMelee()
        {
            return _weaponController.LastMeleeTime + TimeSpan.FromSeconds(minTimeToExitMelee) < DateTime.Now &&
                !_playerAnimationController.IsInAnimation;
        }

        private void HandleMeleeRotation()
        {
            if (_currentTargetMelee == null) return;

            Vector3 directionToMelee = (_currentTargetMelee.Transform.position - _modelTransform.position).normalized;
            directionToMelee.y = 0;
            _playerAnimationController.SetModelTransformOverride(true);

            _modelTransform.localRotation = Quaternion.RotateTowards(_modelTransform.localRotation,
                Quaternion.LookRotation(directionToMelee),
                Time.deltaTime * 720);
        }

        private void HandleRangedTargeting()
        {
            var rightHandAbsAngle = UpdateRightHandTarget();
            UpdateLeftHandTarget(rightHandAbsAngle);
        }

        private float UpdateRightHandTarget()
        {
            float rightHandAbsAngle = 0f;
            if (!ShouldUpdateRightHandTarget()) return rightHandAbsAngle;

            var rightHandTargetSearchPos = GetRightHandSearchPosition();
            _currentTargetR = GetClosestDamageable(rightHandTargetSearchPos, _weaponController.Rweapon.Range, true);
            _lastTargetAcquisitionTimeR = DateTime.Now;

            if (_currentTargetR != null)
            {
                rightHandAbsAngle = ValidateRightHandTarget();
            }

            return rightHandAbsAngle;
        }

        private bool ShouldUpdateRightHandTarget()
        {
            return _weaponController.Rweapon &&
                _lastTargetAcquisitionTimeR.AddSeconds(findTargetCooldown) < DateTime.Now;
        }

        private Vector3 GetRightHandSearchPosition()
        {
            _cachedPosition = _modelTransform.position;
            if (_weaponController.EquippedWeaponCount == 1)
                return _cachedPosition;

            _cachedRight = _modelTransform.right;
            return _cachedPosition + (_cachedRight * 2);
        }

        private float ValidateRightHandTarget()
        {
            _cachedPosition = _modelTransform.position;
            _tempVector = _currentTargetR.Transform.position - _cachedPosition;
            _tempVector.Normalize();

            _cachedRight = _modelTransform.right;
            var signedAngleRightArm = Vector3.SignedAngle(_tempVector, _cachedRight, Vector3.up);
            var rightHandAbsAngle = Mathf.Abs(signedAngleRightArm);

            if (signedAngleRightArm > _maxFrontAnglePerArm || signedAngleRightArm < -_maxBackAnglePerArm)
            {
                ResetRightHand();
                return 0;
            }

            return rightHandAbsAngle;
        }

        public void ResetAllHands()
        {
            ResetLeftHand();
            ResetRightHand();
            ResetTargeting();
        }

        private void ResetRightHand()
        {
            _rHandTarget.localPosition = _rhandTargetOriginalLocalPos;
            _rHandTarget.localRotation = _rhandTargetOriginalRot;
            _currentTargetR = null;
        }

        private void UpdateLeftHandTarget(float rightHandAbsAngle)
        {
            if (!ShouldUpdateLeftHandTarget()) return;

            findTargetCooldown = _findTargetCooldown;
            var leftHandTargetSearchPos = GetLeftHandSearchPosition();
            _currentTargetL = GetClosestDamageable(leftHandTargetSearchPos, _weaponController.Lweapon.Range, true);
            _lastTargetAcquisitionTimeL = DateTime.Now;

            if (_currentTargetL != null)
            {
                ValidateLeftHandTarget(rightHandAbsAngle);
            }
        }

        private bool ShouldUpdateLeftHandTarget()
        {
            return _weaponController.Lweapon &&
                _lastTargetAcquisitionTimeL.AddSeconds(findTargetCooldown) < DateTime.Now;
        }

        private Vector3 GetLeftHandSearchPosition()
        {
            _cachedPosition = _modelTransform.position;
            if (_weaponController.EquippedWeaponCount == 1)
                return _cachedPosition;

            _cachedRight = _modelTransform.right;
            return _cachedPosition + (-_cachedRight * 2);
        }

        private void ValidateLeftHandTarget(float rightHandAbsAngle)
        {
            var normalizedDirectionToTarget =
                (_currentTargetL.Transform.position - _modelTransform.position).normalized;
            var signedAngleLeftArm =
                Vector3.SignedAngle(normalizedDirectionToTarget, -_modelTransform.right, Vector3.up);

            var leftHandAbsAngle = Mathf.Abs(signedAngleLeftArm);
            var maxAngleForLeft = 180 - rightHandAbsAngle;

            var armsAreCrossing = leftHandAbsAngle > maxAngleForLeft;
            if (armsAreCrossing)
            {
                LoggerNS.Log("<color=yellow>Arms are crossing</color>");
            }

            if (armsAreCrossing || signedAngleLeftArm < -_maxFrontAnglePerArm ||
                signedAngleLeftArm > _maxBackAnglePerArm)
            {
                ResetLeftHand();
            }
        }

        private void ResetLeftHand()
        {
            _lHandTarget.localPosition = _lhandTargetOriginalLocalPos;
            _lHandTarget.localRotation = _lhandTargetOriginalRot;
            _currentTargetL = null;
        }

        private void UpdateBodyRotationAndAnimation()
        {
            if (_currentTargetL == null && _currentTargetR == null)
            {
                ResetTargeting();
                return;
            }

            findTargetCooldown = _findTargetCooldown;
            Vector3 bodyLookDirection = CalculateBodyLookDirection();
            UpdateModelRotation(bodyLookDirection);
            UpdateAnimationState();
        }

        private Vector3 CalculateBodyLookDirection()
        {
            var dirToL = (_currentTargetL != null && _currentTargetL.Transform != null)
                ? (_currentTargetL.Transform.position - _modelTransform.position).normalized
                : Vector3.zero;
            var dirToR = (_currentTargetR != null && _currentTargetR.Transform != null)
                ? (_currentTargetR.Transform.position - _modelTransform.position).normalized
                : Vector3.zero;

            Vector3 bodyLookDirection;
            if (_currentTargetL != null && _currentTargetR != null)
            {
                bodyLookDirection = Vector3.Slerp(dirToL, dirToR, 0.5f);
            }
            else
            {
                bodyLookDirection = _currentTargetL != null ? dirToL : dirToR;
            }

            bodyLookDirection.y = 0;
            return bodyLookDirection;
        }

        private void UpdateModelRotation(Vector3 bodyLookDirection)
        {
            _playerAnimationController.SetModelTransformOverride(true);
            _targetRotation = Quaternion.LookRotation(bodyLookDirection);

            _cachedRotation = _modelTransform.localRotation;
            _modelTransform.localRotation = Quaternion.RotateTowards(
                _cachedRotation,
                _targetRotation,
                Time.deltaTime * 480);
        }

        private void UpdateAnimationState()
        {
            _playerAnimationController.SetHandWields(_currentTargetL != null, _currentTargetR != null);
            _playerAnimationController.ToggleAiming(true);
        }

        private void ResetTargeting()
        {
            _weaponController.IsFirstTimeLWeapon = true;
            _weaponController.IsFirstTimeRWeapon = true;
            _playerAnimationController.SetModelTransformOverride(false);
            _playerAnimationController.ToggleAiming(false);
            findTargetCooldown = 0;
        }

        private IDamageable GetClosestDamageable(Vector3 position, float range, bool excludeChests = false)
        {
            IDamageable closestBox = null;
            float boxDistance = float.MaxValue;
            float mobDistance = float.MaxValue;
            float destroyableDistance = float.MaxValue;

            if (!excludeChests)
            {
                closestBox = _boxManager.GetClosestBox(position, 3f);
                if (closestBox != null)
                {
                    boxDistance = Vector3.SqrMagnitude(position - closestBox.Position);
                }
            }

            var closestMob = _mobManager.GetClosestMob(position, range);
            if (closestMob != null)
            {
                mobDistance = Vector3.SqrMagnitude(position - closestMob.Position);
            }

            var closestDestroyable = _damageableRegisterService.GetClosestDamageable(position, range);
            if (closestDestroyable != null)
            {
                destroyableDistance = Vector3.SqrMagnitude(position - closestDestroyable.Position);
            }

            // Early returns for simple cases
            if (closestBox == null && closestMob == null && closestDestroyable == null) return null;
            if (closestBox != null && closestMob == null && closestDestroyable == null) return closestBox;
            if (closestBox == null && closestMob != null && closestDestroyable == null) return closestMob;
            if (closestBox == null && closestMob == null && closestDestroyable != null) return closestDestroyable;

            // Find the closest among available targets
            float minDistance = boxDistance;
            IDamageable closest = closestBox;

            if (mobDistance < minDistance)
            {
                minDistance = mobDistance;
                closest = closestMob;
            }

            if (destroyableDistance < minDistance)
            {
                closest = closestDestroyable;
            }

            return closest;
        }

        #endregion
    }
}
