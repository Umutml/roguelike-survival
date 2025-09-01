using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;


namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ShowStatusPromptStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Show Status Prompt Step",
        order = 0)]
    public class ShowStatusPromptStep : TutorialStep
    {
        [SerializeField] private string status;

        private TutorialSequenceController _tutorialSequenceController;

        public override UniTask ProcessStep()
        {
            _tutorialSequenceController = Resolver.Resolve<TutorialSequenceController>();
            _tutorialSequenceController.InvokeStatusPrompt(status);
            return UniTask.CompletedTask;
        }
    }

}