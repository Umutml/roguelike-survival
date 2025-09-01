using _Scripts.GameCore.AI;
using _Scripts.Utilities;
using GameCore.BuffSystem;
using GameCore.Health;
using UnityEngine;

namespace GameCore.AI
{
    public class SpitterProjectile : MobParabolicProjectile
    {
        [SerializeField] private float poisonDuration = 4f;

        protected override void TakeAoeDamage()
        {
            base.TakeAoeDamage();
            var distance = Vector3.Distance(transform.position, _playerController.transform.position);
            if (distance <= radius)
                ApplyPoison();
        }

        private void ApplyPoison()
        {
            var player = _playerController.GetDamageable;
            var damageInfo = new DamageInfo(_damageInfo.Amount * 2); // Double damage for poison effect
            player?.TakeDOT(damageInfo, poisonDuration);
        }
    }
}
