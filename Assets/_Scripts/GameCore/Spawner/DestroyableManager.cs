using System;
using System.Collections.Generic;
using GameCore.Health;
using UnityEngine;
using Utilities;

namespace GameCore.Spawner
{
    public class DestroyableManager : MonoBehaviour, IDamageableRegisterService
    {
        private List<IDamageable> _destroyables = new List<IDamageable>();
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void RegisterDamageable(IDamageable damageable)
        {
            _destroyables.Add(damageable);
        }

        public void UnregisterDamageable(IDamageable damageable)
        {
            _destroyables.Remove(damageable);
        }

        public IDamageable GetClosestDamageable(Vector3 position, float? customDistance = null)
        {
            IDamageable closestDamageable = null;
            float closestDistance = customDistance ?? Mathf.Infinity;
            foreach (var destroyable in _destroyables)
            {
                if(destroyable == null) continue;
                
                float distance = Vector3.Distance(destroyable.Position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = destroyable;
                }
            }
            
            if (closestDamageable != null && _camera.IsInViewport(closestDamageable.Position))
                return closestDamageable;

            return null;
        }
    }
}
