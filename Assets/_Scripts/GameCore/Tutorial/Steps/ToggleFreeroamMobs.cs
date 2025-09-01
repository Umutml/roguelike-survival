using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [Serializable]
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/Toggle Freeroam Mobs")]
    public class ToggleFreeroamMobs : TutorialStep
    {
        public bool FreeRoamEnabled;

        public override UniTask ProcessStep()
        {
            var mobSpawnService = Resolver.Resolve<IMobSpawnService>();
            mobSpawnService.FreeRoamEnabled = FreeRoamEnabled;
            return UniTask.CompletedTask;
        }
    }
}
