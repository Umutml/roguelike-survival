using System.Threading.Tasks;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.AI;
using GameCore.Health;
using GameCore.Player;
using GameCore.Scriptables;
using GameCore.Spawner;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "MobsSpawnNearNpcStep",
    menuName = "ScriptableObjects/Tutorial/Steps/Mob Spawn Near Npc Step",
    order = 0)]
public class MobSpawnNearNpcStep : TutorialStep
{
    [SerializeField] private SpawnBehaviorState behaviourState;
    [SerializeField] private Bounds areaBounds;
    [SerializeField] private bool debug;
    [SerializeField] private bool skipRegistration;
    private MobManager _mobManager;

    public override async UniTask ProcessStep()
    {
        _mobManager = Resolver.Resolve<MobManager>();
        await _mobManager.PoolCreationCompletionSource.Task;
#if UNITY_EDITOR
        if (debug)
        {
            EditorHelper.DrawBounds(areaBounds, 30);
        }

        SpawnMobs();
#endif
    }

    private async void SpawnMobs()
    {
        for (var i = 0; i < behaviourState.spawnCount; i++)
        {
            var spawnPosition = GetRandomPositionInBounds(areaBounds);
            await UniTask.Delay(200);
            var mob = await _mobManager.SpawnMobAtPosition(spawnPosition, behaviourState.GetBehaviourType());
            if (mob == null)
            {
                LoggerNS.LogError("Mob is null");
                return;
            }

            if (skipRegistration)
            {
                return;
            }

            RegisterMob(mob);
        }
    }

    private Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        var x = Random.Range(bounds.min.x, bounds.max.x);
        var z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, 0, z);
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