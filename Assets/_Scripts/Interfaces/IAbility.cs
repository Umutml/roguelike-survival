using System;
using UnityEngine;

namespace Interfaces
{
    public interface IAbility
    {
        string AbilityName { get; }
        bool IsOnCooldown { get; }
        float MaxCooldownTime { get; }
        float CurrentCooldownTime { get; }
        float Radius { get; set; }
        float Damage { get; set; }
        float Duration { get; set; }
        Sprite Icon { get; }
        void Execute();
        public event Action AbilityUsed;
    }
}
