using System;
using GameCore.Health;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using RootMotion.FinalIK;
using UnityEngine;
using static ObjectiveStructure;

namespace _Scripts.GameCore.NPC
{
    public class NpcObjectiveAnimator : ObjectiveDamageable
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AimIK _aimIk;
        [SerializeField] internal float attackRate = 1.0f;
        public RangedWeapon _rangedWeapon;
        private bool _isReloading;
        private IDamageable _currentTarget;
        protected float FindTargetCooldown = 1f;
        private DateTime _lastTargetAcquisitionTime = DateTime.MinValue;
        protected MobManager MobManager;
        private const int MagazineCapacity = 20;
        private int _currentAmmo = MagazineCapacity;
        protected void OnNpcStateChanged(NpcState npcState)
        {
            switch (npcState)
            {
                case NpcState.Idle:
                    _aimIk.enabled = false;
                    _animator.SetBool(NpcAnimationConstants.Attack, false);
                    _animator.SetBool(NpcAnimationConstants.Dead, false);
                    _animator.SetBool(NpcAnimationConstants.Run, false);
                    break;
                case NpcState.Attack:
                    FindTarget(out var isTargetFound);
                    _aimIk.enabled = isTargetFound;
                    _animator.SetBool(NpcAnimationConstants.Attack, isTargetFound);
                    _animator.SetBool(NpcAnimationConstants.Dead, false);
                    _animator.SetBool(NpcAnimationConstants.Run, false);
                    break;
                case NpcState.Dead:
                    _aimIk.enabled = false;
                    _animator.SetBool(NpcAnimationConstants.Attack, false);
                    _animator.SetBool(NpcAnimationConstants.Dead, true);
                    _animator.SetBool(NpcAnimationConstants.Run, false);
                    break;
                case NpcState.Move:
                    _aimIk.enabled = false;
                    _animator.SetBool(NpcAnimationConstants.Attack, false);
                    _animator.SetBool(NpcAnimationConstants.Dead, false);
                    _animator.SetBool(NpcAnimationConstants.Run, true);
                    break;
            }
        }
        private void TriggerReloading()
        {
            _isReloading = true;
            _aimIk.enabled = false;
            _animator.SetTrigger(NpcAnimationConstants.Reload);
        }
        public void ReloadComplete()
        {
            _isReloading = false;
            _aimIk.enabled = true;
        }
        protected void SetAttackAnimationSpeed(float value)
        {
            _animator.SetFloat(NpcAnimationConstants.AttackSpeed, attackRate);
        }
        protected void SetAnimationType(string parameterName, float value)
        {
            _animator.SetFloat(parameterName, value);
        }
        public void FireEvent()
        {
            if (_currentTarget == null)
                return;
            _currentAmmo--;
            CheckAmmo();
            if (_isReloading)
                return;
            _rangedWeapon.FireAt(_currentTarget,DamageSource.Npc);
        }
        private void CheckAmmo()
        {
            if (_currentAmmo <= 0)
            {
                TriggerReloading();
                _currentAmmo = MagazineCapacity;
            }
        }
        private void FindTarget(out bool isTargetFound)
        {
            isTargetFound = false;
            if (!MobManager) return;
            if(_isReloading) return;
            if (_lastTargetAcquisitionTime.AddSeconds(FindTargetCooldown) < DateTime.Now)
            {
                _currentTarget = GetClosestDamageable(transform.position, _rangedWeapon.Range);
                if (_currentTarget != null)
                {
                    isTargetFound = true;
                    _aimIk.solver.target = _currentTarget.Transform;
                }
                else
                {
                    isTargetFound = false;
                }
                _lastTargetAcquisitionTime = DateTime.Now;
            }
            if (_currentTarget == null) return;
            isTargetFound = true;
            var bodyLookDirection = _currentTarget.Position - transform.position;
            var targetBodyRotation = Quaternion.LookRotation(bodyLookDirection);
            var bodyRotation = Quaternion.Euler(0, targetBodyRotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                bodyRotation,
                Time.deltaTime * 480
            );
        }
        private IDamageable GetClosestDamageable(Vector3 position, float range)
        {
            var closestMob = MobManager.GetClosestMob(position, range, true);
            return closestMob;
        }
    }
}
