using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/Toggle MobManager Ring System")]
    public class ToggleMobManagerRingSystem : TutorialStep
    {
        public bool RingSystemEnabled;

        public override UniTask ProcessStep()
        {
            var mobSpawnService = Resolver.Resolve<IMobSpawnService>();
            mobSpawnService.RingSystemEnabled = RingSystemEnabled;
            return UniTask.CompletedTask;
        }
    }
}
