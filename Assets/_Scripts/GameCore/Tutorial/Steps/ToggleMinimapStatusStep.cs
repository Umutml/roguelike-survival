using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleMinimapStatusStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Minimap Status Step",
        order = 0)]
    public class ToggleMinimapStatusStep : TutorialStep
    {
        [SerializeField] private bool isEnable;

        public override async UniTask ProcessStep()
        {
            var minimapObject = await TutorialService.GetTutorialObject("Minimap");

            if (minimapObject == null)
            {
                LoggerNS.LogError("Minimap is not found");
                return;
            }


            if (!minimapObject.TryGetComponent(out CanvasGroup canvasGroup))
            {
                LoggerNS.LogError("MinimapManager is not found");
                return;
            }

            canvasGroup.alpha = isEnable ? 1 : 0;
        }
    }
}