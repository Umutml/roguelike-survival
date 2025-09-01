using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/Steps/New EnableTutorialOutline")]
    public class EnableTutorialOutline : TutorialStep
    {
        [SerializeField] private bool toggle;
        [SerializeField] private string gameObjectName;

        public override async UniTask ProcessStep()
        {
            await base.ProcessStep();

            var outlineObject = await TutorialService.GetTutorialObject(gameObjectName);
            var outline = outlineObject.GetComponent<TutorialOutline>();
            outline.ToggleOutline(toggle);
        }
    }
}