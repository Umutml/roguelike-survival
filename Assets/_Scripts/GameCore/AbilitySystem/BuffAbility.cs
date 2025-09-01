using GameCore.BuffSystem;
using GameCore.Health;
using UnityEngine;

namespace GameCore.AbilitySystem
{
    public class BuffAbility : Ability
    {
        [SerializeField] protected Buff.BuffType buffType;
        [SerializeField] protected float buffValue;
        [SerializeField] protected float buffTime;
        
        public override void Execute()
        {
            if (isOnCooldown) return;
            base.Execute();
            
            var statusController = User.GetComponent<PlayerStatusController>();

            Buff buff = new Buff(buffType, buffValue, buffTime);
            statusController?.ApplyBuff(buff);
        }
    }
}
