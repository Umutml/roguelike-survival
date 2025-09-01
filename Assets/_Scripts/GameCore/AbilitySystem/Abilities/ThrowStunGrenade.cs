using _Scripts.Utilities;
using UnityEngine;

namespace GameCore.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "ThrowStunGrenade",
        menuName = "ScriptableObjects/Abilities/ThrowStunGrenade",
        order = 1)]
    public class ThrowStunGrenade : ProjectileAbility
    {
        public override void Execute()
        {
            base.Execute();
            LoggerNS.Log("Skill used: Throw Stun Grenade");
        }
    }
}
