using _Scripts.Utilities;
using UnityEngine;

namespace GameCore.AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "Obliterate", menuName = "ScriptableObjects/Abilities/Obliterate", order = 1)]
    public class Obliterate : Ability
    {
        public override void Execute()
        {
            if (isOnCooldown) return;
            base.Execute();
            LoggerNS.Log("Skill used: Obliterate");
        }
    }
}
