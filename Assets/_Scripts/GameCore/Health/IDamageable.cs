using System;
using JetBrains.Annotations;
using UnityEngine;

namespace GameCore.Health
{
    public interface IDamageable
    {
        [CanBeNull] public string SpecificDamageType { get; }
        public BoxCollider Bounds { get; }
        public Vector3 GetClosetPoint(Vector3 targetPosition)
        {
            if(!Bounds ||Bounds.size == Vector3.zero) return Transform.position;
            return Bounds.ClosestPoint(targetPosition);
        }
        public float Health { get; }
        public Vector3 Position { get; }
        public Vector3 ForcePosition { get; }
        public float ForcePower { get; }
        public Transform RandomTransform { get; }
        public Transform Transform { get; }

        public bool IsDead { get; }
        public bool IsNotDamageable { get; }

        public void TakeDamage(DamageInfo damageInfo);
        public void TakeDOT(DamageInfo damageInfo, float duration);
        public void TakeDamageFromVehicle(Vector3 carPosition, float collisionForce);
        public event Action<DamageSource> Died;

        public void OnLoseFocus();
    }
}