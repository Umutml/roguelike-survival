using UnityEngine;

namespace GameCore.Health
{
    public interface IDamageableRegisterService
    {
        void RegisterDamageable(IDamageable damageable);
        
        void UnregisterDamageable(IDamageable damageable);
        
        IDamageable GetClosestDamageable(Vector3 position, float? range = null);
    }
}
