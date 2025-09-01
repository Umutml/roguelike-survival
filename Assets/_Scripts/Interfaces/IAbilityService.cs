namespace Interfaces
{
    public interface IAbilityService
    {
        public enum AbilityType { Passive, StatBooster, CrowdControl, CrowdDestruction }
        
        IAbility GetAbility(AbilityType abilityType);
        
        
    }
}
