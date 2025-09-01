using System;
using UnityEngine;

namespace GameCore.BuffSystem
{
    [Serializable]
    public class Buff
    {
        /// <summary>
        /// Create a new buff with the given type, value, and time.
        /// </summary>
        /// <param name="type">Buff type</param>
        /// <param name="value">Buff value (Multiplier)</param>
        /// <param name="time">Buff time</param>
        public Buff(BuffType type, float value, float time)
        {
            Type = type;
            Value = value;
            Time = time;
        }
        
        public enum BuffType
        {
            MovementSpeed,
            Damage,
            AttackSpeed
        }
        
        public BuffType Type;
        public float Value;
        public float Time;
    }
}
