using _Scripts.Utilities;
using GameCore.BuffSystem;
using GameCore.Health;
using GameCore.Player;
using UnityEngine;

namespace GameCore.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "FastGunner", menuName = "ScriptableObjects/Abilities/FastGunner", order = 1)]
    public class FastGunner: BuffAbility
    {
        
        public override void Execute()
        {
            if (isOnCooldown) return;
            base.Execute();
            
            LoggerNS.Log("Skill Used: FastGunner activated");
            
        }
    }
}

