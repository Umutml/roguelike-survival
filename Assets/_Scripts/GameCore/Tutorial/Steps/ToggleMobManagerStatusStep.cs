using Cysharp.Threading.Tasks;
using GameCore.Spawner;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial
{
    [CreateAssetMenu(fileName = "ToggleMobManagerStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Mob Manager Status Step",
        order = 0)]
    public class ToggleMobManagerStatusStep : TutorialStep
    {
        [SerializeField] private bool isLocked;
        [SerializeField] private float mobSpawnSpeed = 1;
        [SerializeField] private int mobCountPerSpawn = 4;
        [SerializeField] private int mobSpawnAngle = 180;

        private MobManager _mobManager;

        public override async UniTask ProcessStep()
        {
            _mobManager = Resolver.Resolve<MobManager>();
            _mobManager.IsLocked = isLocked;
            _mobManager.MobSpawnSpeed = mobSpawnSpeed;
            _mobManager.MobCountPerSpawn = mobCountPerSpawn;
            _mobManager.MobSpawnAngle = mobSpawnAngle;
        }
    }
}
