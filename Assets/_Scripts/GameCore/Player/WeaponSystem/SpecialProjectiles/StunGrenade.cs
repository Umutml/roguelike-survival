using GameCore.BuffSystem;
using GameCore.Player.WeaponSystem.GameCore.Player.Weapon;

namespace GameCore.Player.WeaponSystem.SpecialProjectiles
{
    public class StunGrenade : ParabolicProjectile
    {
        protected override void TakeAoeDamage()
        {
            base.TakeAoeDamage();

            Debuff debuff = new Debuff(Debuff.Debufftype.Stun, 0f, _ability.Duration);

            var mobs = _mobManager.GetMobsInRange(transform.position, _ability.Radius);
            foreach (var mob in mobs)
            {
                var debuffable = mob as IDebuffable;
                if (mob == null || mob.IsDead) continue;
                debuffable.ApplyDebuff(debuff);
            }
        }
    }
}
