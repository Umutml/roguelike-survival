using System;
using GameCore.Health;
using GameCore.Player.WeaponSystem;
using GameCore.Spawner;
using RootMotion.FinalIK;
using UnityEngine;

namespace _Scripts.GameCore.NPC
{
    public class AllyBaseDefenderAnimationModule : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AimIK _aimIk;
        [SerializeField] internal float attackRate = 1.0f;
        [SerializeField] private RangedWeapon _rangedWeapon;
        public RangedWeapon rangedWeapon => _rangedWeapon;
        private bool isIdle;
        private bool isReloading;
        private IDamageable _currentTarget;
        private static readonly int IsIdle = Animator.StringToHash("IsIdle");
        private static readonly int AttackSpeed = Animator.StringToHash("attackSpeed");
        private static readonly int Reload = Animator.StringToHash("Reload");

        private const int MagazineCapacity = 20;
        private int _currentAmmo = MagazineCapacity;
        
        public bool IsReloading => isReloading;
        private void Start()
        {
            isIdle = true;
            
            SetIdle(isIdle);
            
            SetAttackSpeed(2);
        }
        
        private void SetIdle(bool value)
        {
            isIdle = value;
            _aimIk.enabled = !isIdle;
            _animator.SetBool(IsIdle, isIdle);
        }
        
        private void TriggerReloading()
        {
            isReloading = true;
            _aimIk.enabled = false;
            _animator.SetTrigger(Reload);
        }
        
        public void ReloadComplete()
        {
            isReloading = false;
            _aimIk.enabled = true;
        }
        
        private void SetAttackSpeed(float value)
        {
            attackRate = value;
            
            _animator.SetFloat(AttackSpeed, attackRate);
        }
        
        public void SetCurrentTarget(IDamageable target)
        {
            if (target == null)
            {
                SetIdle(true);
                return;
            }
            
            _currentTarget = target;
            SetIdle(false);
            SetAimTarget(_currentTarget.Transform);
        }
        
        public void FireEvent()
        {
            if (_currentTarget == null)
                return;

            _currentAmmo--;
            
            CheckAmmo();
            
            if (isReloading)
                return;
            
            _rangedWeapon.FireAt(_currentTarget,DamageSource.Npc,20);
            
            SetAttackSpeedRandomized(2,5);
        }
        
        private void CheckAmmo()
        {
            if (_currentAmmo <= 0)
            {
                TriggerReloading();
                _currentAmmo = MagazineCapacity;
            }
        }

        private void SetAttackSpeedRandomized(float min, float max)
        {
            SetAttackSpeed(UnityEngine.Random.Range(min, max));
        }
        
        private void SetAimTarget(Transform target)
        {
            _aimIk.solver.target = target;
        }
    }
}
