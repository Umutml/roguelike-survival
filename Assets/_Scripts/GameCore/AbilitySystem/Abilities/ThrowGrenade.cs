using System;
using GameCore.BuffSystem;
using GameCore.Health;
using GameCore.Player;
using UnityEngine;
using VContainer;

namespace GameCore.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "ThrowGrenade", menuName = "ScriptableObjects/Abilities/ThrowGrenade", order = 1)]
    public class ThrowGrenade: ProjectileAbility
    {
        public override void Execute()
        {
            if (isOnCooldown) return;
            base.Execute();
            
            
            
        }

        
    }
}

