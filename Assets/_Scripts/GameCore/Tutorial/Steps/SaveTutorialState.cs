using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "SaveTutorialState",
        menuName = "ScriptableObjects/Tutorial/Steps/Save Tutorial State",
        order = 0)]
    public class SaveTutorialState : TutorialStep
    {
        [SerializeField] private bool isTutorialCompleted;

        private TutorialData _tutorialData;


        public override UniTask ProcessStep()
        {
            var tutorialSequenceController = Resolver.Resolve<TutorialSequenceController>();
            tutorialSequenceController.SetTutorialCompleted(isTutorialCompleted);
            
            return UniTask.CompletedTask;
        }
    }
}