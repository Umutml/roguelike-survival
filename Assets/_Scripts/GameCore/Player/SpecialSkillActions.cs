using System;
using System.Collections.Generic;
using _Scripts.GameCore.Drop;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Player
{
    public class SpecialSkillActions : MonoBehaviour
    {
        #region Private Fields

        private Dictionary<StatUpgradeType, Action<UpgradeDetail>> _upgradeActions;
        private PlayerSkillController _playerSkillController;
        private IAbilityService _abilityService;
        private ItemPicker _itemPicker;
        private LootDropManager _lootDropManager;
        private PlayerStatusController _playerStatusController;
        private MobManager _mobManager;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Setup();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion


        #region Private Methods

        private void Setup()
        {
            _upgradeActions = new Dictionary<StatUpgradeType, Action<UpgradeDetail>>
            {
                {StatUpgradeType.StunNearbyZombies, StunsAllNearbyZombies},
                {StatUpgradeType.AreaNukeDamage, AreaNukeDealing}
            };
        }


        [Inject]
        private void Initialize(PlayerSkillController playerSkillController, LootDropManager lootDropManager,
            ItemPicker itemPicker, IAbilityService abilityService, PlayerStatusController playerStatusController,
            MobManager mobManager)
        {
            _lootDropManager = lootDropManager;
            _playerSkillController = playerSkillController;
            _itemPicker = itemPicker;
            _abilityService = abilityService;
            _playerStatusController = playerStatusController;
            _mobManager = mobManager;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _playerSkillController.OnSkillUpgrade += OnSkillUpgrade;
            _playerStatusController.Refill += KillAllNearbyZombies;
        }

        private void UnsubscribeFromEvents()
        {
            _playerSkillController.OnSkillUpgrade -= OnSkillUpgrade;
            _playerStatusController.Refill -= KillAllNearbyZombies;
        }

        private void OnSkillUpgrade(UpgradeDetail upgradeDetail)
        {
            if (_upgradeActions is not {Count: > 0})
            {
                return;
            }

            if (_upgradeActions.ContainsKey(upgradeDetail.type))
            {
                _upgradeActions[upgradeDetail.type].Invoke(upgradeDetail);
            }
        }

        private void StunsAllNearbyZombies(UpgradeDetail upgradeDetail)
        {
            if (upgradeDetail.skill == null) { return; }

            var ability = _abilityService.GetAbility(IAbilityService.AbilityType.CrowdControl);
            var skillDetails = _playerSkillController.GetSkillDetail(upgradeDetail.skill);
            ability.Duration = upgradeDetail.skill.skillEventEffect.durations[skillDetails.StarLevel - 1].value;
            ability.Radius = upgradeDetail.skill.skillEventEffect.radii[skillDetails.StarLevel - 1].value;
            ability.Execute();
        }

        private void KillAllNearbyZombies()
        {
            var damage = 500f;
            var radius = 20f;

            DamageInfo damageInfo = new DamageInfo {Amount = damage};
            var mobs = _mobManager.GetMobsInRange(transform.position, radius);
            foreach (var mob in mobs)
            {
                if (mob == null || mob.IsDead) continue;
                mob.TakeDamage(damageInfo);
            }
        }

        private async void AreaNukeDealing(UpgradeDetail upgradeDetail)
        {
            try
            {
                if (upgradeDetail.skill == null) { return; }

                var bombObject = await _lootDropManager.GetDropObject(DropPodType.Bomb, transform.position);
                var skillDetails = _playerSkillController.GetSkillDetail(upgradeDetail.skill);
                var bombPodController = bombObject.GetComponent<BombPodController>();
                bombPodController.Initialize((int) upgradeDetail.value);
                bombPodController.damage =
                    upgradeDetail.skill.skillEventEffect.damages[skillDetails.StarLevel - 1].value;
                bombPodController.radius = upgradeDetail.skill.skillEventEffect.radii[skillDetails.StarLevel - 1].value;
                _itemPicker.HandleItemPickup(bombPodController);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while spawning bomb: {e}");
            }
        }

        #endregion
    }
}
