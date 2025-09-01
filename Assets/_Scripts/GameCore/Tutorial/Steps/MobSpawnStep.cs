using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.AI;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "MobSpawnStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Mob Spawn Step",
        order = 0)]
    public class MobSpawnStep : TutorialStep
    {
        [SerializeField] private Vector3 spawnPosition;
        [SerializeField] private BehaviorType behaviorType;
        [SerializeField] private bool skipRegistration;

        private MobManager _mobManager;

        public override async UniTask ProcessStep()
        {
            _mobManager = Resolver.Resolve<MobManager>();
            var mob = await _mobManager.SpawnMobAtPosition(spawnPosition, behaviorType);
            if (mob == null)
            {
                LoggerNS.LogError("Mob is null");
                return;
            }
            if (skipRegistration) return;
            RegisterMob(mob);
        }

        private void RegisterMob(Zombie mob)
        {
            if (mob.TryGetComponent(out IDamageable damageable)) { _mobManager.ActiveTutorialMobs.Add(damageable); }
            else
            {
                LoggerNS.LogError("Mob does not have IDamageable component");
            }
        }
    }
}
