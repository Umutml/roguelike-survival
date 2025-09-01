using _Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "WaitForObjective",
        menuName = "ScriptableObjects/Tutorial/Steps/Wait For Objective",
        order = 0)]
    public class WaitForObjective : TutorialStep
    {
        [SerializeField] private ObjectiveStructure.ObjectiveType objectiveType;
        [SerializeField] private ObjectiveActionType objectiveActionType;

        private ObjectiveManager _objectiveManager;

        public override async UniTask ProcessStep()
        {
            _objectiveManager = Resolver.Resolve<ObjectiveManager>();

            await UniTaskAsyncHelper.WaitUntil(ShouldContinueProcessing, 2000);
        }

        private bool ShouldContinueProcessing()
        {
            if (objectiveActionType == ObjectiveActionType.Complete)
            {
                return _objectiveManager.ObjectiveActionType == ObjectiveActionType.Complete;
            }

            var objectiveHub = _objectiveManager.ActiveObjectiveHub;
            return objectiveHub != null &&
                   objectiveHub.ObjectiveType == objectiveType &&
                   _objectiveManager.ObjectiveActionType == ObjectiveActionType.Start;
        }
    }
}