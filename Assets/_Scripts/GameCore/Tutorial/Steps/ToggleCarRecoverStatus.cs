using _Scripts.Utilities;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;

namespace _Scripts.GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "ToggleCarRecoverStatus",
        menuName = "ScriptableObjects/Tutorial/Steps/Toggle Car Recover Status",
        order = 0)]
    public class ToggleCarRecoverStatus : TutorialStep
    {
        [SerializeField] private bool isEnable;

        private const string CarRecoverNpc = "CarRecoverNpc";

        public override async UniTask ProcessStep()
        {
            var carRecover = await TutorialService.GetTutorialObject(CarRecoverNpc);
            if (carRecover == null)
            {
                LoggerNS.LogError("CarRecoverNPC: No CarRecoverNPC found in scene");
                return;
            }

            if (!carRecover.TryGetComponent(out CarRecoverNpcController carRecoverNpcController))
            {
                LoggerNS.LogError("CarRecoverNPC: No CarRecoverNPC found in carRecoverNPC");
                return;
            }

            carRecoverNpcController.SetState(isEnable);
        }
    }
}
