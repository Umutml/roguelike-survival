using System.Collections.Generic;
using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.AI;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Spawner;
using UnityEngine;
using Utilities;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "MobsSpawnStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Mobs Spawn Step",
        order = 0)]
    public class MobsSpawnStep : TutorialStep
    {
        [SerializeField] private List<Vector3> spawnPosition;
        [SerializeField] private BehaviorType behaviorType;
        [SerializeField] private bool skipRegistration;
        [SerializeField] private bool isCrawlClose;
        [SerializeField] private int spawnDelay=200;


        private MobManager _mobManager;

        public override async UniTask ProcessStep()
        {
            _mobManager = Resolver.Resolve<MobManager>();
            await _mobManager.PoolCreationCompletionSource.Task;

            SpawnMobs();
        }

        private async void SpawnMobs()
        {
            foreach (var position in spawnPosition)
            {
                if(spawnDelay > 0)
                    await UniTask.Delay(spawnDelay,delayType: DelayType.UnscaledDeltaTime);
                var mob = await _mobManager.SpawnMobAtPosition(position, behaviorType);
                if (mob == null)
                {
                    LoggerNS.LogError("Mob is null");
                    return;
                }
                if (skipRegistration) continue;
                RegisterMob(mob);
            }
        }


        private void RegisterMob(Zombie mob)
        {
            if (mob.TryGetComponent(out IDamageable damageable))
            {
                _mobManager.ActiveTutorialMobs.Add(damageable);
            }
            else
            {
                LoggerNS.LogError("Mob does not have IDamageable component");
            }
        }
    }
}