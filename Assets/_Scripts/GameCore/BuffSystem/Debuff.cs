using System;

namespace GameCore.BuffSystem
{
    [Serializable]
    public class Debuff
    {
        /// <summary>
        /// Create a new Debuff with the given type, value, and time.
        /// </summary>
        /// <param name="type">Debuff type</param>
        /// <param name="value">Debuff value (Multiplier)</param>
        /// <param name="time">Debuff time</param>
        public Debuff(Debufftype type, float value, float time)
        {
            Type = type;
            Value = value;
            Time = time;
        }
        
        public enum Debufftype
        {
            Stun,
            Slow
        }
        
        public Debufftype Type;
        public float Value;
        public float Time;
    }
}
