using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleObjectiveManagerStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Objective Manager Status Step",
        order = 0)]
    public class ToggleObjectiveStatusStep : TutorialStep
    {
        [SerializeField] private ObjectiveStructure.ObjectiveType objectiveType;
        [SerializeField] private bool isInit;

        private ObjectiveManager _objectiveManager;

        public override async UniTask ProcessStep()
        {
            _objectiveManager = Resolver.Resolve<ObjectiveManager>();

            if (isInit)
            {
                await _objectiveManager.SpawnObjectiveByType(objectiveType);
            }
            else
            {
                await _objectiveManager.DestroyObjectiveByType(objectiveType);
            }
        }
    }
}