
namespace GameCore.Health
{
    public class DamageInfo
    {
        public float Amount { get; set; }
        public DamageSource Source { get; set; }
        public DamageType Type { get; set; }

        public DamageInfo(float amount = 0, DamageSource source = DamageSource.Player, DamageType type = DamageType.Normal)
        {
            Amount = amount;
            Source = source;
            Type = type;
        }
    }

    public enum DamageSource
    {
        Player,
        Npc,
        Environment
    }

    public enum DamageType
    {
        Normal,
        Fire,
        Frost,
        Poison
    }
}