using Cysharp.Threading.Tasks;
using GameCore.Spawner;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleFastDespawnForBackMobs",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Fast Despawn For Back Mobs",
        order = 0)]
    public class ToggleFastDespawnForBackMobs : TutorialStep
    {
        [SerializeField] private bool isEnable;

        public override UniTask ProcessStep()
        {
            MobManager.FastDespawnForBackMobs = isEnable;
            return UniTask.CompletedTask;
        }
    }
}