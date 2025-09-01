using Cysharp.Threading.Tasks;
using GameCore.Box;
using GameCore.Spawner;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace GameCore.Tutorial
{
    [CreateAssetMenu(fileName = "ToggleDropManagerBoxStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Drop Manager Box Status Step",
        order = 0)]
    public class ToggleDropManagerBoxStatusStep : TutorialStep
    {
        [SerializeField] private bool isLocked;

        private BoxManager _boxManager;

        public override async UniTask ProcessStep()
        {
            _boxManager = Resolver.Resolve<BoxManager>();
            _boxManager.IsBoxDropLocked = isLocked;
        }
    }
}