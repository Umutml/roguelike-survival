using Interfaces;
using UnityEngine;
using VContainer;

namespace UI.Game.InGame.Ability
{
    /// <summary>
    /// This UI panel controls the usage of Active abilities
    /// </summary>
    public class AbilityPanel : MonoBehaviour
    {
        [SerializeField] private AbilitySlotPanel statBoosterSlot;
        [SerializeField] private AbilitySlotPanel crowdControlSlot;
        [SerializeField] private AbilitySlotPanel crowdDestructionSlot;
        
        private IAbilityService _abilityService;

        [Inject]
        private void Construct(IAbilityService abilityService)
        {
            _abilityService = abilityService;
            
            statBoosterSlot.InstallAbility(_abilityService.GetAbility(IAbilityService.AbilityType.StatBooster));
            crowdControlSlot.InstallAbility(_abilityService.GetAbility(IAbilityService.AbilityType.CrowdControl));
            crowdDestructionSlot.InstallAbility(_abilityService.GetAbility(IAbilityService.AbilityType.CrowdDestruction));
        }
        
    }
}
